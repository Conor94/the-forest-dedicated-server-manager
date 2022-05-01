using DataAccess;
using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Schemas;
using Extensions;
using NLog;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using TheForestDSM.Utilities;

namespace TheForestDSM.ShutdownService
{
    public partial class Service : ServiceBase
    {
        private System.Timers.Timer mShutdownTimer;

        private Logger Logger => LogManager.GetCurrentClassLogger();

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
                WriteLog($"Exception occurred while instantiating {ServiceName}.", LogLevel.Error, e);
            }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                SQLiteDAO dao = new SQLiteDAO($@"Data Source={new PathResolver().GetApplicationDataPath()}\TheForestDSM.db");

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
                    Logger.Warn("Shutdown timer was not started because the provided shutdown time was in the past. " +
                                                 $"Current time = {currentTime}, Shutdown time = {shutdownTime}");
                    Stop();
                }
                ShutdownTimer.Elapsed += ShutdownTimer_OnElapsed;
                ShutdownTimer.Start();
            }
            catch (Exception e)
            {
                WriteLog($"Exception occurred while starting {ServiceName}.", LogLevel.Error, e);
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
                WriteLog($"Exception occurred while stopping {ServiceName}.", LogLevel.Error, e);
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
                WriteLog($"{ServiceName} threw an exception when the local system was shutting down.", LogLevel.Error, e);
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
                WriteLog($"{ServiceName} threw an exception when the local user logged off.", LogLevel.Error, e);
            }
        }

        private void ShutdownTimer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Shutdown server
                Process[] processes = Process.GetProcessesByName(Config.ServerProcessName);

                if (processes.Length == 0)
                {
                    Logger.Warn($"{ServiceName} attempted to shutdown the server while the server was not running.");
                }
                else
                {
                    bool multipleInstances = false; // Used to write a different log message if there's multiple dedicated server instances
                    if (processes.Length > 1)
                    {
                        Logger.Warn($"Multiple dedicated server instances were found when the {ServiceName} was terminating the dedicated server. " +
                                                     $"{ServiceName} will attempt to close all instances of the dedicated server.");

                        multipleInstances = true;
                    }

                    for (int i = 0; i < processes.Length; i++)
                    {
                        processes[i].Kill();
                        processes[i].WaitForExit(10000);

                        if (processes[i].HasExited)
                        {
                            Logger.Info($"Dedicated server{(multipleInstances ? $" instance #{i + 1}" : "")} has been shutdown.");
                        }
                        else
                        {
                            Logger.Error($"Failed to shutdown the dedicated server{(multipleInstances ? $" instance #{i + 1}" : "")}.");
                        }
                    }

                    // Shutdown the system if needed
                    if (ShutdownServiceData.IsMachineShutdown)
                    {
                        ShutdownMachine();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, "Exception occurred when the shutdown timer elapsed.");
                WriteLog("Exception occurred when the shutdown timer elapsed.", LogLevel.Error, ex);
            }
            finally
            {
                // Stop the service
                Stop();
            }
        }

        private void WriteLog(string message, LogLevel logLevel, Exception exception = null)
        {
            if (exception != null)
            {
                StringBuilder exceptionInfo = new StringBuilder();
                foreach (Exception ex in exception.GetExceptions())
                {
                    exceptionInfo.AppendLine($"Type: {ex.GetType()}");
                    exceptionInfo.AppendLine($"Message: {ex.Message}");
                    exceptionInfo.AppendLine($"StackTrace: {ex.StackTrace}\n\n");
                }

                // Write to the Windows Event Log (these logs can be viewed in Event Viewer -> Windows Logs -> Application)
                EventLog.WriteEntry($"{message}\n\n" +
                                    exceptionInfo.ToString());

                Logger.Log(logLevel, exception, message);
            }
            else
            {
                Logger.Log(logLevel, message);
            }
        }

        /// <summary>
        /// Runs a command that shuts down the system.
        /// </summary>
        private void ShutdownMachine()
        {
            try
            {
                // Use the command line to shutdown the system
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

                Logger.Info($"{ServiceName} is shutting down the system.");

                process.Start();
                process.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error occurred when the shutdown service attempted to shutdown the system.");
            }
        }
    }
}
