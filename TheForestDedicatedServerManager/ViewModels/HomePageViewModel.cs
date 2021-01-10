using AppConfiguration;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using TheForestDedicatedServerManager.Base;
using TheForestDedicatedServerManager.Events;

namespace TheForestDedicatedServerManager.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        #region Fields and properties
        // Private only fields
        private string mServerProcessName;
        // Fields with public properties
        private string mServerOutputText;
        // Commands
        private DelegateCommand mCheckStatusCommand;
        private DelegateCommand mStartServerCommand;
        private DelegateCommand mShutdownServerCommand;
        private DelegateCommand mScheduleShutdownCommand;
        private DelegateCommand mCancelShutdownCommand;
        private DelegateCommand mQuitCommand;

        // Public properties
        public string ServerOutputText
        {
            get => mServerOutputText;
            set => SetProperty(ref mServerOutputText, value);
        }
        // Commands
        public DelegateCommand CheckStatusCommand
        {
            get => mCheckStatusCommand ?? (mCheckStatusCommand = new DelegateCommand(CheckStatusExecute, CheckStatusCanExecute));
        }
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
            get => mQuitCommand ?? (mQuitCommand = new DelegateCommand(QuitExecute, QuitCanExecute));
            private set => mQuitCommand = value;
        }
        #endregion

        #region Constructors
        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public HomePageViewModel(IEventAggregator eventAggregator) : this(eventAggregator, null)
        {
        }

        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        /// <param name="eventAggregator"><inheritdoc cref="ViewModelBase()"/></param>
        /// <param name="validators"><inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/></param>
        public HomePageViewModel(IEventAggregator eventAggregator, Dictionary<string, Func<object, string>> validators) : base(eventAggregator, validators)
        {
            AppConfig appConfig = AppConfigurationManager.GetSettings();
            mServerProcessName = Path.GetFileNameWithoutExtension(appConfig.TheForestServerManagerExecutablePath);
            ServerOutputText = "";
        }
        #endregion

        #region Command methods
        private void CheckStatusExecute()
        {
            if (CheckServerStatus())
            {
                AppendServerOutputText("The server is running.");
            }
            else
            {
                AppendServerOutputText("The server is not running.");
            }
        }
        private bool CheckStatusCanExecute()
        {
            return true;
        }

        private void StartServerExecute()
        {
            AppConfig appConfig = AppConfigurationManager.GetSettings();
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo(appConfig.TheForestServerManagerExecutablePath, appConfig.ServerArguments)
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
                }
                else
                {
                    MessageBox.Show($"Dedicated server did not start successfully.");
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
            Process[] processes = Process.GetProcessesByName(mServerProcessName);
            if (processes.Length == 1)
            {
                processes[0].Kill();
                Thread.Sleep(100);
                if (processes[0].HasExited)
                {
                    ShutdownServerCommand.RaiseCanExecuteChanged();
                    StartServerCommand.RaiseCanExecuteChanged();
                    MessageBox.Show("Dedicated server has been shutdown.");
                }
                else
                {
                    MessageBox.Show("Dedicated server failed to shutdown.");
                }
            }
            else if (processes.Length == 0)
            {
                MessageBox.Show("Dedicated server is not running.");
            }
            else if (processes.Length > 1)
            {
                throw new Exception("Multiple processes with the same name found.");
            }
        }
        private bool ShutdownServerCanExecute()
        {
            return CheckServerStatus();
        }

        private void ScheduleShutdownExecute()
        {
            throw new NotImplementedException();
        }
        private bool ScheduleShutdownCanExecute()
        {
            return CheckServerStatus();
        }

        private void CancelShutdownExecute()
        {
            throw new NotImplementedException();
        }
        private bool CancelShutdownCanExecute()
        {
            return false;
        }

        private void QuitExecute()
        {
            EventAggregator.GetEvent<QuitEvent>().Publish();
        }
        private bool QuitCanExecute()
        {
            return true;
        }
        #endregion

        #region Helper methods
        private bool CheckServerStatus()
        {
            // Throw an exception if process name field is blank
            if (string.IsNullOrWhiteSpace(mServerProcessName))
            {
                throw new Exception($"{nameof(mServerProcessName)} cannot be null, empty, or only whitespace.");
            }
            else
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
        }
        private void AppendServerOutputText(string text)
        {
            ServerOutputText += $"{text}\n";
        }
        #endregion
    }
}
