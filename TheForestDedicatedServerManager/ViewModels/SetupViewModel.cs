using AppConfigurationManager;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System;
using System.IO;
using System.Windows;
using TheForestDedicatedServerManager.Events;
using TheForestDedicatedServerManagerConfig;
using Unity;

namespace TheForestDedicatedServerManager.ViewModels
{
    public class SetupViewModel : ViewModelBase
    {
        #region Fields and properties
        private string mTheForestDeducatedServerExePath;
        private string mServerArguments;
        // Commands
        private DelegateCommand mSaveSetupCommand;
        private DelegateCommand mCancelSetupCommand;
        private DelegateCommand mBrowseCommand;

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
        // Commands
        public DelegateCommand SaveSetupCommand
        {
            get => mSaveSetupCommand ?? (mSaveSetupCommand = new DelegateCommand(SaveSetupExecute, SaveSetupCanExecute));
            set => mSaveSetupCommand = value;
        }
        public DelegateCommand CancelSetupCommand
        {
            get => mCancelSetupCommand ?? (mCancelSetupCommand = new DelegateCommand(CancelSetupExecute));
            set => mCancelSetupCommand = value;
        }
        public DelegateCommand BrowseCommand
        {
            get => mBrowseCommand ?? (mBrowseCommand = new DelegateCommand(BrowseExecute));
            set => mBrowseCommand = value;
        }
        #endregion

        #region Constructors
        [InjectionConstructor]
        public SetupViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();

            // Initialize properties
            Title = "Setup";
            TheForestDedicatedServerExePath = config.TheForestServerManagerExecutablePath;
            ServerArguments = config.ServerArguments;

            // Add validator methods for properties
            AddValidator(nameof(TheForestDedicatedServerExePath), new DataErrorValidator<string>(ValidateTheForestDedicatedServerExePath));

            // Raise events
            SaveSetupCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Command methods
        private void SaveSetupExecute()
        {
            try
            {
                AppConfigSection config = AppConfigManager<AppConfigSection>.GetSection();
                config.TheForestServerManagerExecutablePath = TheForestDedicatedServerExePath;
                config.ServerArguments = ServerArguments;
                config.IsSetupSaved = true;
                AppConfigManager<AppConfigSection>.Save();
                CloseWindow();
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
            return TheForestDedicatedServerExePath != "" && ValidateTheForestDedicatedServerExePath(TheForestDedicatedServerExePath, out string _);
        }

        private void CancelSetupExecute()
        {
            CloseWindow();
        }

        private void BrowseExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Find and select TheForestDedicatedServer.exe",
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
        private bool ValidateTheForestDedicatedServerExePath(string exePath, out string errorMessage)
        {
            errorMessage = "";

            if (!File.Exists(exePath) && exePath != "")
            {
                errorMessage = "Server executable path is not valid.";
                return false;
            }

            return true;
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
