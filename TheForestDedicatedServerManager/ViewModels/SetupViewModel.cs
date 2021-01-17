using AppConfiguration;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using TheForestDedicatedServerManager.Base;
using TheForestDedicatedServerManager.Events;
using Unity;

namespace TheForestDedicatedServerManager.ViewModels
{
    public class SetupViewModel : ViewModelBase
    {
        #region Fields and properties
        private string mTheForestDeducatedServerExePath;
        private DelegateCommand mSaveSetupCommand;
        private DelegateCommand mBrowseCommand;
        private string mServerArguments;
        private DelegateCommand mCancelSetupCommand;

        public string TheForestDedicatedServerExePath
        {
            get => mTheForestDeducatedServerExePath;
            set
            {
                if (SetProperty(ref mTheForestDeducatedServerExePath, value))
                {
                    SaveSetupCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string ServerArguments
        {
            get => mServerArguments;
            set => SetProperty(ref mServerArguments, value);
        }
        public DelegateCommand SaveSetupCommand
        {
            get => mSaveSetupCommand ?? (mSaveSetupCommand = new DelegateCommand(SaveSetupExecute, SaveSetupCanExecute));
            set => mSaveSetupCommand = value;
        }
        public DelegateCommand BrowseCommand
        {
            get => mBrowseCommand ?? (mBrowseCommand = new DelegateCommand(BrowseExecute));
            set => mBrowseCommand = value;
        }
        public DelegateCommand CancelSetupCommand
        {
            get => mCancelSetupCommand ?? (mCancelSetupCommand = new DelegateCommand(CancelSetupExecute));
            set => mCancelSetupCommand = value;
        }
        #endregion

        #region Constructors
        [InjectionConstructor]
        public SetupViewModel(IEventAggregator eventAggregator, IContainerProvider container) : this(eventAggregator, container, null)
        {
        }

        public SetupViewModel(IEventAggregator eventAggregator, IContainerProvider container, Dictionary<string, Func<object, string>> validators) : base(eventAggregator, container, validators)
        {
            AppConfig config = AppConfigurationManager.GetSettings();

            // Initialize properties
            Title = "Setup";
            TheForestDedicatedServerExePath = config.TheForestServerManagerExecutablePath;
            ServerArguments = config.ServerArguments;

            // Add validator methods for properties
            AddValidator(nameof(TheForestDedicatedServerExePath), ValidateTheForestDedicatedServerExePath);

            // Raise events
            SaveSetupCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Command methods
        private void SaveSetupExecute()
        {
            try
            {
                AppConfig config = AppConfigurationManager.GetSettings();
                config.TheForestServerManagerExecutablePath = TheForestDedicatedServerExePath;
                config.ServerArguments = ServerArguments;
                config.IsSetupSaved = true;
                if (AppConfigurationManager.Save())
                {
                    CloseWindow();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception occurred.\n\n" +
                                $"Message: {e.Message}\n" +
                                $"StackTrace: {e.StackTrace}");
            }
        }
        private bool SaveSetupCanExecute()
        {
            return TheForestDedicatedServerExePath != "" && ValidateTheForestDedicatedServerExePath(TheForestDedicatedServerExePath) == "";
        }

        private void CancelSetupExecute()
        {
            CloseWindow();
        }

        private void BrowseExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Application|*.exe",
                DefaultExt = "exe",
                CheckPathExists = true
            };
            if (dialog.ShowDialog() == true)
            {
                TheForestDedicatedServerExePath = dialog.FileName;
            }
        }
        #endregion

        #region Validator methods
        private string ValidateTheForestDedicatedServerExePath(object value)
        {
            string errorMessage = "";
            if (value is string exePath)
            {
                if (!File.Exists(exePath) && exePath != "")
                {
                    errorMessage = "Server executable path is not valid.";
                }

                return errorMessage;
            }
            else
            {
                throw new Exception($"Argument must be of type '{typeof(string)}'.");
            }
        }
        #endregion

        #region Helper methods
        private void CloseWindow()
        {
            EventAggregator.GetEvent<CloseWindowRequestEvent>().Publish();
        }
        #endregion
    }
}
