using DotNetExtensions;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using TheForestDedicatedServerManagerConfig;

namespace TheForestDedicatedServerManagerService
{
    public partial class Service : ServiceBase
    {
        #region Fields and properties
        private System.Timers.Timer mShutdownTimer;

        public System.Timers.Timer ShutdownTimer
        {
            get => mShutdownTimer;
            set => mShutdownTimer = value;
        }
        #endregion

        #region Constructor
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
        #endregion

        #region Service methods
        protected override void OnStart(string[] args)
        {
            try
            {
                // Change the current working directory to the applications base directory. This must be done
                // because the default working directory for services is %SYSTEMROOT%\System32.
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                AppConfigManager<AppConfigSection>.Init(Environment.SpecialFolder.LocalApplicationData);
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();

                // schedule a timer for the shutdown time
                DateTime currentTime = DateTime.Now;
                DateTime shutdownTime = config.ShutdownTime;
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
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
                config.IsMachineShutdownScheduled = false;
                config.ShutdownTime = DateTime.MinValue;
                AppConfigManager<AppConfigSection>.Save();
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
        #endregion

        #region Event handlers
        private void ShutdownTimer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Shutdown server
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
                Process[] processes = Process.GetProcessesByName(config.ServerProcessName);
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
                if (config.IsMachineShutdownScheduled)
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
        #endregion

        #region Helper methods
        private void LogException(string friendlyMessage, Exception e)
        {
            StringBuilder exceptionInfo = new StringBuilder();
            foreach (Exception tmpEx in e.GetExceptions())
            {
                exceptionInfo.AppendLine($"Type: {tmpEx.GetType()}");
                exceptionInfo.AppendLine($"Message: {tmpEx.Message}");
                exceptionInfo.AppendLine($"StackTrace: {tmpEx.StackTrace}\n\n");
            }
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
        #endregion
    }
}
