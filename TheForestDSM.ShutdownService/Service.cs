using DataAccess;
using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Schemas;
using Extensions;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;

namespace TheForestDSM.ShutdownService
{
    public partial class Service : ServiceBase
    {
        private System.Timers.Timer mShutdownTimer;

        public System.Timers.Timer ShutdownTimer
        {
            get => mShutdownTimer;
            set => mShutdownTimer = value;
        }
        public ShutdownServiceDataRepository ShutdownServiceDataRepo { get; private set; }
        public ShutdownServiceData ShutdownServiceData { get; private set; }
        public ConfigurationRepository ConfigurationRepo { get; private set; }
        public Configuration Config { get; private set; }

        public Service()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception e)
            {
                LogException($"Exception occurred while instantiating {ServiceName}.", e);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string dbFilePath = $@"{Environment.GetEnvironmentVariable("PROGRAMDATA")}\TheForestDSM";
                SQLiteDAO dao = new SQLiteDAO($@"Data Source={dbFilePath}\TheForestDSM.db");

                ShutdownServiceDataRepo = new ShutdownServiceDataRepository(dao);
                ShutdownServiceData = ShutdownServiceDataRepo.Read(ShutdownServiceDataSchema.ID_DEFAULT_VALUE);

                ConfigurationRepo = new ConfigurationRepository(dao);
                Config = ConfigurationRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE);

                // schedule a timer for the shutdown time
                DateTime currentTime = DateTime.Now;
                DateTime shutdownTime = Convert.ToDateTime(ShutdownServiceData.ShutdownTime);
                if (shutdownTime > currentTime)
                {
                    ShutdownTimer = new System.Timers.Timer()
                    {
                        Interval = (shutdownTime - currentTime).TotalMilliseconds,
                        AutoReset = false
                    };
                }
                else
                {
                    // Shutdown time was in the past (the GUI application should prevent this), so the timer can't be started.
                    // Log a message and then stop the service.
                    EventLog.WriteEntry("Shutdown timer was not started because the specified shutdown time was in the past.\n" +
                                            $"Current time: {currentTime}\n" +
                                            $"Shutdown time: {shutdownTime}");
                    Stop();
                }
                ShutdownTimer.Elapsed += ShutdownTimer_OnElapsed;
                ShutdownTimer.Start();
            }
            catch (Exception e)
            {
                LogException($"Exception occurred while starting {ServiceName}.", e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                ShutdownServiceData.IsShutdownScheduled = false;
                ShutdownServiceData.IsMachineShutdown = false;
                ShutdownServiceData.ShutdownTime = "";

                ShutdownServiceDataRepo.Update(ShutdownServiceData);
            }
            catch (Exception e)
            {
                LogException($"Exception occurred while stopping {ServiceName}.", e);
            }
        }

        protected override void OnShutdown()
        {
            try
            {
                // system was shutdown
                // abort the scheduled tasks (shutting down server and/or shutting down system)
                Stop();
            }
            catch (Exception e)
            {
                LogException($"{ServiceName} threw an exception when the local system was shutting down.", e);
            }
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            try
            {
                if (changeDescription.Reason == SessionChangeReason.SessionLogoff)
                {
                    // user logged off
                    // abort the scheduled tasks (shutting down server and/or shutting down system)
                    Stop();
                }
            }
            catch (Exception e)
            {
                LogException($"{ServiceName} threw an exception when the local user logged off.", e);
            }
        }

        private void ShutdownTimer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Shutdown server
                Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);
                if (processes.Length == 1)
                {
                    processes[0].Kill();
                    Thread.Sleep(100);
                    if (processes[0].HasExited)
                    {
                        EventLog.WriteEntry("Dedicated server has been shutdown.");
                    }
                    else
                    {
                        EventLog.WriteEntry("Dedicated server failed to shutdown.");
                    }
                }
                else if (processes.Length == 0)
                {
                    EventLog.WriteEntry($"{ServiceName} attempted to shutdown the server while the server was not running.");
                }
                else if (processes.Length > 1)
                {
                    throw new Exception("Multiple processes with the same name found.");
                }

                // Shutdown the system if needed
                if (ShutdownServiceData.IsMachineShutdown)
                {
                    ShutdownMachine();
                }
            }
            catch (Exception ex)
            {
                LogException("Exception occurred when the shutdown timer elapsed.", ex);
            }
            finally
            {
                // Stop the service
                Stop();
            }
        }

        private void LogException(string friendlyMessage, Exception e)
        {
            StringBuilder exceptionInfo = new StringBuilder();
            foreach (Exception exception in e.GetExceptions())
            {
                exceptionInfo.AppendLine($"Type: {exception.GetType()}");
                exceptionInfo.AppendLine($"Message: {exception.Message}");
                exceptionInfo.AppendLine($"StackTrace: {exception.StackTrace}\n\n");
            }

            // Write to the Windows Event Log (these logs can be viewed in Event Viewer -> Windows Logs -> Application)
            EventLog.WriteEntry($"{friendlyMessage}\n\n" +
                                exceptionInfo.ToString());
        }

        /// <summary>
        /// Runs a command that shuts down the system.
        /// </summary>
        private void ShutdownMachine()
        {
            // Test creating a process to call a command
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "cmd.exe",
                    Arguments = "/C shutdown /s",
                    WindowStyle = ProcessWindowStyle.Hidden
                },
            };
            EventLog.WriteEntry($"{ServiceName} is shutting down the system.");
            process.Start();
            process.Dispose();
        }
    }
}
