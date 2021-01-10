using DotNetExtensions;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace TheForestDedicatedServerManagerService
{
    public partial class Service : ServiceBase
    {
        #region Fields and properties
        private Timer mShutdownTimer;

        public Timer ShutdownTimer
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
                // schedule a timer for the shutdown time
                ShutdownTimer = new Timer()
                {
                    Interval = 0,
                    AutoReset = false
                };
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
            // check app configuration to see what needs to be done (shutdown server, server, or both)
            // shutdown system if needed
            // shutdown server if needed
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
        #endregion
    }
}
