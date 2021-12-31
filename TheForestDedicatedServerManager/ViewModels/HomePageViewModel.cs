﻿using AppConfigurationManager;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using TheForestDedicatedServerManager.Events;
using TheForestDedicatedServerManager.Views;
using TheForestDedicatedServerManagerConfig;

namespace TheForestDedicatedServerManager.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        #region Fields and properties
        // Private only fields
        private string mServerProcessName;
        // Fields with public properties
        private string mServerOutputText;
        private string mShutdownTime;
        private bool mIsMachineShutdown;
        private Brush mServerStatusColour;
        // Commands
        private DelegateCommand mStartServerCommand;
        private DelegateCommand mShutdownServerCommand;
        private DelegateCommand mScheduleShutdownCommand;
        private DelegateCommand mCancelShutdownCommand;
        private DelegateCommand mQuitCommand;
        private DelegateCommand mEditSetupCommand;

        // Public properties
        public string ServerOutputText
        {
            get => mServerOutputText;
            set => SetProperty(ref mServerOutputText, value);
        }
        public string ShutdownTime
        {
            get => mShutdownTime;
            set
            {
                SetProperty(ref mShutdownTime, value);
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
                CancelShutdownCommand.RaiseCanExecuteChanged();
            }
        }
        public bool IsMachineShutdown
        {
            get => mIsMachineShutdown;
            set
            {
                SetProperty(ref mIsMachineShutdown, value);
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
                config.IsMachineShutdownScheduled = value;
                AppConfigManager<AppConfigSection>.Save();
            }
        }
        public Brush ServerStatusColour
        {
            get => mServerStatusColour;
            set => SetProperty(ref mServerStatusColour, value);
        }
        // Commands
        public DelegateCommand StartServerCommand
        {
            get => mStartServerCommand ?? (mStartServerCommand = new DelegateCommand(StartServerExecute, StartServerCanExecute));
        }
        public DelegateCommand ShutdownServerCommand
        {
            get => mShutdownServerCommand ?? (mShutdownServerCommand = new DelegateCommand(ShutdownServerExecute, ShutdownServerCanExecute));
        }
        public DelegateCommand ScheduleShutdownCommand
        {
            get => mScheduleShutdownCommand ?? (mScheduleShutdownCommand = new DelegateCommand(ScheduleShutdownExecute, ScheduleShutdownCanExecute));
        }
        public DelegateCommand CancelShutdownCommand
        {
            get => mCancelShutdownCommand ?? (mCancelShutdownCommand = new DelegateCommand(CancelShutdownExecute, CancelShutdownCanExecute));
        }
        public DelegateCommand QuitCommand
        {
            get => mQuitCommand ?? (mQuitCommand = new DelegateCommand(QuitExecute));
            private set => mQuitCommand = value;
        }
        public DelegateCommand EditSetupCommand
        {
            get => mEditSetupCommand ?? (mEditSetupCommand = new DelegateCommand(EditSetupExecute));
            set => mEditSetupCommand = value;
        }
        #endregion

        #region Events
        public event EventHandler ServerStatusChanged;
        protected virtual void RaiseServerStatusChanged()
        {
            ServerStatusChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Constructors
        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public HomePageViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            // Initialize local fields and properties
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
            mServerProcessName = config.ServerProcessName;
            ServerOutputText = "";
            ShutdownTime = null;

            // Add validator methods for properties
            AddValidator(nameof(ShutdownTime), new DataErrorValidator<string>(ValidateShutdownTime));

            // Subscribe to events
            ServerStatusChanged += HomePageViewModel_OnServerStatusChange;

            // Raise events
            StartServerCommand.RaiseCanExecuteChanged();
            ShutdownServerCommand.RaiseCanExecuteChanged();
            ScheduleShutdownCommand.RaiseCanExecuteChanged();
            CancelShutdownCommand.RaiseCanExecuteChanged();
            RaiseServerStatusChanged();
        }
        #endregion

        #region Command methods
        private void StartServerExecute()
        {
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(config.TheForestServerManagerExecutablePath, config.ServerArguments)
                {
                    Verb = "runas"
                }
            };

            try
            {
                // Start the dedicated server
                if (process.Start())
                {
                    AppendServerOutputText("Dedicated server has been started.");
                    ShutdownServerCommand.RaiseCanExecuteChanged();
                    StartServerCommand.RaiseCanExecuteChanged();
                    ScheduleShutdownCommand.RaiseCanExecuteChanged();
                    CancelShutdownCommand.RaiseCanExecuteChanged();
                    RaiseServerStatusChanged();
                }
                else
                {
                    AppendServerOutputText("Dedicated server did not start successfully.");
                }
            }
            catch (Win32Exception)
            {
                AppendServerOutputText("Server could not be started because administrative privileges were not granted.");
            }
        }
        private bool StartServerCanExecute()
        {
            if (!CheckServerStatus())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ShutdownServerExecute()
        {
            if (MessageBox.Show("Are you sure you want to shutdown the server?", "Shutdown Server", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process[] processes = Process.GetProcessesByName(mServerProcessName);
                if (processes.Length == 1)
                {
                    processes[0].Kill();
                    Thread.Sleep(200);
                    if (processes[0].HasExited)
                    {
                        AppendServerOutputText("Dedicated server has been shutdown.");
                        RaiseServerStatusChanged();
                    }
                    else
                    {
                        AppendServerOutputText("Dedicated server failed to shutdown.");
                    }
                }
                else if (processes.Length == 0)
                {
                    AppendServerOutputText("Dedicated server is not running.");
                }
                else if (processes.Length > 1)
                {
                    throw new Exception("Multiple processes with the same name found.");
                }

                ShutdownServerCommand.RaiseCanExecuteChanged();
                StartServerCommand.RaiseCanExecuteChanged();
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
                CancelShutdownCommand.RaiseCanExecuteChanged();
            }
        }
        private bool ShutdownServerCanExecute()
        {
            return CheckServerStatus();
        }

        private void ScheduleShutdownExecute()
        {
            try
            {
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
                ServiceController controller = new ServiceController(config.ServiceName);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    // Save the shutdown time to the shared file
                    config.ShutdownTime = DateTime.Parse(ShutdownTime);
                    AppConfigManager<AppConfigSection>.Save();

                    // Start the service
                    controller.Start();
                    Thread.Sleep(100);
                    CancelShutdownCommand.RaiseCanExecuteChanged();
                    if (config.IsMachineShutdownScheduled)
                    {
                        AppendServerOutputText($"Shutdown scheduled for {config.ShutdownTime}. A machine shutdown is also scheduled.");
                    }
                    else
                    {
                        AppendServerOutputText($"Shutdown scheduled for {config.ShutdownTime}.");
                    }
                }
                else
                {
                    string errorMsg = "Application attemped to schedule a shutdown when the shutdown scheduler service was already started.";
                    AppendServerOutputText(errorMsg);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException.Message == "The system cannot find the file specified")
                {
                    AppendServerOutputText("Failed to schedule shutdown because the required service was not installed.");
                }
            }
        }
        private bool ScheduleShutdownCanExecute()
        {
            return CheckServerStatus()
                && ValidateShutdownTime(ShutdownTime, out string _)
                && !string.IsNullOrWhiteSpace(ShutdownTime);
        }

        private void CancelShutdownExecute()
        {
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
            ServiceController controller = new ServiceController(config.ServiceName);
            if (controller.Status == ServiceControllerStatus.Running)
            {
                controller.Stop();
                Thread.Sleep(100);
                AppendServerOutputText("Scheduled shutdown has been cancelled.");
                ShutdownServerCommand.RaiseCanExecuteChanged();
                StartServerCommand.RaiseCanExecuteChanged();
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
                CancelShutdownCommand.RaiseCanExecuteChanged();
            }
        }
        private bool CancelShutdownCanExecute()
        {
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
            ServiceController controller = new ServiceController(config.ServiceName);
            return controller.Status == ServiceControllerStatus.Running;
        }

        private void QuitExecute()
        {
            EventAggregator.GetEvent<QuitEvent>().Publish();
        }

        private void EditSetupExecute()
        {
            // open the setup window on top of the main window
            Container.Resolve<SetupView>().ShowDialog();
        }
        #endregion

        #region Validators
        private bool ValidateShutdownTime(string shutdownTime, out string errorMessage)
        {
            bool isValid = false;

            errorMessage = "";
            if (!string.IsNullOrWhiteSpace(shutdownTime))
            {
                if (!DateTime.TryParse(shutdownTime, out DateTime tmpShutdownTime))
                {
                    errorMessage = "Shutdown time is not valid.";
                }
                else if (tmpShutdownTime < DateTime.Now)
                {
                    errorMessage = "Shutdown time cannot be in the past.";
                }
                else
                {
                    isValid = true;
                }
            };

            return isValid;
        }
        #endregion

        #region Event handlers
        private void HomePageViewModel_OnServerStatusChange(object sender, EventArgs e)
        {
            // Check if the server is running
            if (CheckServerStatus())
            {
                ServerStatusColour = new SolidColorBrush(Colors.Lime);
            }
            else
            {
                ServerStatusColour = new SolidColorBrush(Colors.Red);
            }
        }
        #endregion

        #region Helper methods
        private bool CheckServerStatus()
        {
            // Return true if process is found, and false if it wasn't
            Process[] processes = Process.GetProcessesByName(mServerProcessName);
            if (processes.Length == 1)
            {
                return true;
            }
            else if (processes.Length < 1)
            {
                return false;
            }
            else if (processes.Length > 1)
            {
                throw new Exception($"Multiple processes with the name '{mServerProcessName}' were found.");
            }
            else
            {
                throw new Exception($"Error occurred while resolving process '{mServerProcessName}'.");
            }
        }
        private void AppendServerOutputText(string text)
        {
            ServerOutputText += $"{text}\n";
        }
        #endregion
    }
}
