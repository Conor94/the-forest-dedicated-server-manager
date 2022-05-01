using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Schemas;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using TheForestDSM.Events;
using TheForestDSM.Views;
using TheForestDSM.Views.ScheduleShutdown;

namespace TheForestDSM.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly ConfigurationRepository mConfigRepo;
        private readonly ShutdownServiceDataRepository mShutdownServiceDataRepo;

        private Logger Logger => LogManager.GetCurrentClassLogger();

        public Configuration Config { get; private set; }
        public ShutdownServiceData ShutdownServiceData { get; private set; }
        public bool UpdateUiThreadIsRunning { get; private set; }

        // Private only fields
        private readonly string mServiceName = AppStrings.ServiceName;

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
        public CancellationTokenSource RefreshUIThreadCancellationTokenSource { get; private set; }

        public event EventHandler ServerStatusChanged;
        protected virtual void RaiseServerStatusChanged()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ServerStatusChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public HomePageViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            // Get repositories
            mConfigRepo = container.Resolve<ConfigurationRepository>();
            mShutdownServiceDataRepo = container.Resolve<ShutdownServiceDataRepository>();

            // Get data from repositories
            Config = mConfigRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE);
            ShutdownServiceData = mShutdownServiceDataRepo.Read(ShutdownServiceDataSchema.ID_DEFAULT_VALUE);

            ServerOutputText = "";
            ServerStatusColour = new SolidColorBrush();

            // Subscribe to events
            ServerStatusChanged += OnServerStatusChange;
            ShutdownServiceData.PropertyChanged += ShutdownServiceData_PropertyChanged;
            EventAggregator.GetEvent<ConfigurationSavedEvent>().Subscribe(OnConfigurationSaved);
            EventAggregator.GetEvent<ShutdownScheduledEvent>().Subscribe(OnShutdownScheduled);

            // Raise events
            StartServerCommand.RaiseCanExecuteChanged();
            ShutdownServerCommand.RaiseCanExecuteChanged();
            ScheduleShutdownCommand.RaiseCanExecuteChanged();
            CancelShutdownCommand.RaiseCanExecuteChanged();
            RaiseServerStatusChanged();

            // Start a thread to poll the shutdown service (parts of the UI need to be updated based on whether the server is running)
            RefreshUIThreadCancellationTokenSource = new CancellationTokenSource();
            new Thread(RefreshUI)
            {
                IsBackground = true
            }.Start();
        }

        private void OnConfigurationSaved(Configuration newConfig)
        {
            Config = newConfig;

            RefreshUIThreadCancellationTokenSource.Cancel(); // Cancel the delay in the thread that refreshes the UI

            RefreshUIThreadCancellationTokenSource = new CancellationTokenSource(); // Renew the cancellation token to allow the delay to be restarted
        }

        private void ShutdownServiceData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShutdownServiceData.ShutdownTime))
            {
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
            }
        }

        private void OnServerStatusChange(object sender, EventArgs e)
        {
            // Check if the server is running
            if (IsDedicatedServerRunning())
            {
                ServerStatusColour = new SolidColorBrush(Colors.Lime);
            }
            else
            {
                ServerStatusColour = new SolidColorBrush(Colors.Red);
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
                    WriteOutput("Dedicated server has been started.", LogLevel.Info);

                    ShutdownServerCommand.RaiseCanExecuteChanged();
                    StartServerCommand.RaiseCanExecuteChanged();
                    ScheduleShutdownCommand.RaiseCanExecuteChanged();
                    CancelShutdownCommand.RaiseCanExecuteChanged();

                    RaiseServerStatusChanged();
                }
                else
                {
                    WriteOutput("Failed to start dedicated server.", LogLevel.Warn);
                }
            }
            catch (Win32Exception e)
            {
                WriteOutput("Server could not be started because administrative privileges were not granted.", LogLevel.Error, e);
            }
        }
        private bool StartServerCanExecute()
        {
            return !IsDedicatedServerRunning();
        }

        private void ShutdownServerExecute()
        {
            if (MessageBox.Show("Are you sure you want to shutdown the server?\n\nShutting down the server will also cancel any scheduled shutdowns.", "Shutdown Server", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);
                if (processes.Length == 0)
                {
                    WriteOutput("Dedicated server is not running.", "Attempted to shutdown the server while the server was not running.", LogLevel.Warn);
                }
                else
                {
                    bool multipleInstances = false; // Used to write a different log message if there's multiple dedicated server instances
                    if (processes.Length > 1)
                    {
                        WriteOutput($"Multiple dedicated server instances were found. Attempting to shutdown all instances...", LogLevel.Warn);

                        multipleInstances = true;
                    }

                    for (int i = 0; i < processes.Length; i++)
                    {
                        processes[i].Kill();
                        processes[i].WaitForExit(10000);

                        if (processes[i].HasExited)
                        {
                            WriteOutput($"Dedicated server{(multipleInstances ? $" instance #{i + 1}" : "")} has been shutdown.", LogLevel.Info);
                        }
                        else
                        {
                            WriteOutput($"Failed to shutdown the dedicated server{(multipleInstances ? $" instance #{i + 1}" : "")}. " +
                                         "Try shutting down the dedicated server manually or restarting your computer.", LogLevel.Error);
                        }
                    }

                    ShutdownServerCommand.RaiseCanExecuteChanged();
                    StartServerCommand.RaiseCanExecuteChanged();
                    ScheduleShutdownCommand.RaiseCanExecuteChanged();
                    RaiseServerStatusChanged();

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

                    CancelShutdownCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private bool ShutdownServerCanExecute()
        {
            return IsDedicatedServerRunning();
        }

        private void ScheduleShutdownExecute()
        {
            Container.Resolve<ScheduleShutdownView>().ShowDialog();
        }
        private void OnShutdownScheduled(ShutdownServiceData data)
        {
            ShutdownServiceData = data;

            try
            {
                ServiceController controller = new ServiceController(mServiceName);

                // Start the service
                string output = "";
                if (controller.Status == ServiceControllerStatus.Running)
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));

                    output = $"Cancelled the previously scheduled shutdown. Scheduling a new shutdown for {ShutdownServiceData.ShutdownTime}.";
                }
                else
                {
                    output = $"Shutdown scheduled for {ShutdownServiceData.ShutdownTime}.";
                }

                if (ShutdownServiceData.IsMachineShutdown)
                {
                    output += " A machine shutdown is also scheduled.";
                }

                // Save the shutdown info
                ShutdownServiceData.IsShutdownScheduled = true;
                mShutdownServiceDataRepo.Update(ShutdownServiceData);

                controller.Start();

                Thread.Sleep(500);

                ScheduleShutdownCommand.RaiseCanExecuteChanged();
                CancelShutdownCommand.RaiseCanExecuteChanged();

                WriteOutput(output, LogLevel.Info);
            }
            catch (Exception e)
            {
                if (e.InnerException?.GetType() == typeof(Win32Exception))
                {
                    WriteOutput("Failed to start the shutdown service. This likely occurred because the service is not installed.", LogLevel.Error, e);
                }
                else
                {
                    WriteOutput("Failed to start the shutdown service due to an unknown error.", LogLevel.Error, e);
                }

                if (e.GetType() == typeof(System.ServiceProcess.TimeoutException))
                {
                    WriteOutput("Timeout occurred while waiting for the shutdown service to stop before scheduling a shutdown.", LogLevel.Error, e);
                }
            }
        }
        private bool ScheduleShutdownCanExecute()
        {
            return IsDedicatedServerRunning(); // Make sure the dedicated server is running
                   //&& ShutdownServiceDataValidator.ValidateShutdownTime(ShutdownServiceData.ShutdownTime, out string _)
                   //&& !string.IsNullOrWhiteSpace(ShutdownServiceData.ShutdownTime);
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
                    WriteOutput("Scheduled shutdown has been cancelled.", LogLevel.Info);

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


        private async void RefreshUI()
        {
            while (true)
            {
                // ContinueWith prevents an exception being thrown when the task is cancelled. The task is
                // cancelled so that the refresh interval can be updated instantly when it's changed by the user.
                await Task.Delay(Config.RefreshIntervalInSeconds * 1000, RefreshUIThreadCancellationTokenSource.Token)
                      .ContinueWith(task => { }); 

                StartServerCommand.RaiseCanExecuteChanged();
                ShutdownServerCommand.RaiseCanExecuteChanged();
                ScheduleShutdownCommand.RaiseCanExecuteChanged();
                CancelShutdownCommand.RaiseCanExecuteChanged();

                ServerStatusColour.Dispatcher.Invoke(() => RaiseServerStatusChanged());
            }
        }

        private bool IsDedicatedServerRunning()
        {
            // Return true if process is found, and false if it wasn't
            Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);
            if (processes.Length >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void WriteOutput(string output, LogLevel logLevel, Exception e = null)
        {
            WriteOutput(output, output, logLevel, e);
        }

        private void WriteOutput(string appOutput, string logOutput, LogLevel logLevel, Exception e = null)
        {
            ServerOutputText += $"{appOutput}\n";

            if (e == null)
            {
                Logger.Log(logLevel, logOutput);
            }
            else
            {
                Logger.Log(logLevel, e, logOutput);
            }
        }
    }
}
