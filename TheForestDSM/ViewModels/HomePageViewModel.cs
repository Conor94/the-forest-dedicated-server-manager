using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Schemas;
using DataAccess.Validators;
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
using TheForestDSM.Events;
using TheForestDSM.Views;

namespace TheForestDSM.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly ConfigurationRepository mConfigRepo;
        private readonly ShutdownServiceDataRepository mShutdownServiceDataRepo;

        public Configuration Config { get; }
        public ShutdownServiceData ShutdownServiceData { get; }

        // Private only fields
        private readonly string mServiceName = AppStrings.ResourceManager.GetString("ServiceName");

        // Fields with public properties
        private string mServerOutputText;
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

        public event EventHandler ServerStatusChanged;
        protected virtual void RaiseServerStatusChanged()
        {
            ServerStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public HomePageViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            // Get repositories
            mConfigRepo = container.Resolve<ConfigurationRepository>();
            mShutdownServiceDataRepo = container.Resolve<ShutdownServiceDataRepository>();

            // Get data from repositories
            Config = mConfigRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE);
            ShutdownServiceData = mShutdownServiceDataRepo.Read(ShutdownServiceDataSchema.ID_DEFAULT_VALUE);

            ServerOutputText = "";

            // Subscribe to events
            ServerStatusChanged += HomePageViewModel_OnServerStatusChange;
            ShutdownServiceData.PropertyChanged += ShutdownServiceData_PropertyChanged;

            // Raise events
            StartServerCommand.RaiseCanExecuteChanged();
            ShutdownServerCommand.RaiseCanExecuteChanged();
            ScheduleShutdownCommand.RaiseCanExecuteChanged();
            CancelShutdownCommand.RaiseCanExecuteChanged();
            RaiseServerStatusChanged();
        }

        private void ShutdownServiceData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShutdownServiceData.ShutdownTime))
            {
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
            }
        }

        private void StartServerExecute()
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(Config.ServerExecutablePath, Config.ServerArguments)
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
            if (MessageBox.Show("Are you sure you want to shutdown the server?\n\nShutting down the server will also cancel any scheduled shutdowns.", "Shutdown Server", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);
                if (processes.Length == 1)
                {
                    AppendServerOutputText("Attempting to shutdown the dedicated server...");

                    processes[0].Kill();

                    processes[0].WaitForExit(10000);

                    if (processes[0].HasExited)
                    {
                        AppendServerOutputText("Dedicated server has been shutdown.");
                        RaiseServerStatusChanged();
                    }
                    else
                    {
                        AppendServerOutputText("Timed out attempting to shutdown the dedicated server. Try shutting down the dedicated server manually or restarting your computer.");
                    }

                    // Update the database
                    ShutdownServiceData.IsShutdownScheduled = false;
                    ShutdownServiceData.ShutdownTime = "";
                    mShutdownServiceDataRepo.Update(ShutdownServiceData);

                    // Stop the shutdown service if it's running
                    ServiceController controller = new ServiceController(mServiceName);
                    if (controller.Status == ServiceControllerStatus.Running)
                    {
                        controller.Stop();
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
                ServiceController controller = new ServiceController(mServiceName);
                if (controller.Status != ServiceControllerStatus.Running)
                {
                    // Save the shutdown info
                    ShutdownServiceData.IsShutdownScheduled = true;
                    mShutdownServiceDataRepo.Update(ShutdownServiceData);

                    // Start the service
                    controller.Start();

                    Thread.Sleep(500);

                    ScheduleShutdownCommand.RaiseCanExecuteChanged();
                    CancelShutdownCommand.RaiseCanExecuteChanged();

                    if (ShutdownServiceData.IsMachineShutdown)
                    {
                        AppendServerOutputText($"Shutdown scheduled for {ShutdownServiceData.ShutdownTime}. A machine shutdown is also scheduled.");
                    }
                    else
                    {
                        AppendServerOutputText($"Shutdown scheduled for {ShutdownServiceData.ShutdownTime}.");
                    }
                }
                else
                {
                    AppendServerOutputText("Application attemped to schedule a shutdown when the shutdown scheduler service was already started.");
                }
            }
            catch (Exception e)
            {
                if (e.InnerException?.GetType() == typeof(Win32Exception))
                {
                    AppendServerOutputText("Failed to start the shutdown service. This likely occurred because the service is not installed.");
                }
                else
                {
                    AppendServerOutputText("Failed to start the shutdown service due to an unknown error.");
                }
            }
        }
        private bool ScheduleShutdownCanExecute()
        {
            return CheckServerStatus() // Make sure the dedicated server is running
                   && !ShutdownServiceData.IsShutdownScheduled // Check if a shutdown has already been scheduled
                   && ShutdownServiceDataValidator.ValidateShutdownTime(ShutdownServiceData.ShutdownTime, out string _)
                   && !string.IsNullOrWhiteSpace(ShutdownServiceData.ShutdownTime);
        }

        private void CancelShutdownExecute()
        {
            try
            {
                ServiceController controller = new ServiceController(mServiceName);
                if (controller.Status == ServiceControllerStatus.Running)
                {
                    ShutdownServiceData.IsShutdownScheduled = false;
                    ShutdownServiceData.ShutdownTime = "";

                    controller.Stop();
                    Thread.Sleep(100);
                    AppendServerOutputText("Scheduled shutdown has been cancelled.");

                    ShutdownServerCommand.RaiseCanExecuteChanged();
                    StartServerCommand.RaiseCanExecuteChanged();
                    ScheduleShutdownCommand.RaiseCanExecuteChanged();
                    CancelShutdownCommand.RaiseCanExecuteChanged();
                }
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("The service for shutting down the server is not installed on this system.",
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
        private bool CancelShutdownCanExecute()
        {
            try
            {
                ServiceController controller = new ServiceController(mServiceName);
                return controller.Status == ServiceControllerStatus.Running;
            }
            catch (InvalidOperationException) // This exception occurs if the shutdown service is not installed on the local system
            {
                return false;
            }
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

        private bool CheckServerStatus()
        {
            // Return true if process is found, and false if it wasn't
            Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);
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
                throw new Exception($"Multiple processes with the name '{Config.ServerProcessName}' were found.");
            }
            else
            {
                throw new Exception($"Error occurred while resolving process '{Config.ServerProcessName}'.");
            }
        }
        private void AppendServerOutputText(string text)
        {
            ServerOutputText += $"{text}\n";
        }
    }
}
