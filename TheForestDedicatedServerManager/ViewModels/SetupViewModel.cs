using AppConfiguration;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private DelegateCommand mSaveSettings;
        private DelegateCommand mBrowseCommand;
        private string mServerArguments;

        public string TheForestDedicatedServerExePath
        {
            get => mTheForestDeducatedServerExePath;
            set
            {
                if (SetProperty(ref mTheForestDeducatedServerExePath, value))
                {
                    SaveSettingsCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public string ServerArguments
        {
            get => mServerArguments;
            set => SetProperty(ref mServerArguments, value);
        }
        public DelegateCommand SaveSettingsCommand
        {
            get => mSaveSettings ?? (mSaveSettings = new DelegateCommand(SaveSettingsExecute, SaveSettingsCanExecute));
            set => mSaveSettings = value;
        }
        public DelegateCommand BrowseCommand
        {
            get => mBrowseCommand ?? (mBrowseCommand = new DelegateCommand(BrowseExecute));
            set => mBrowseCommand = value;
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
            SaveSettingsCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Command methods
        private void SaveSettingsExecute()
        {
            try
            {
                AppConfig config = AppConfigurationManager.GetSettings();
                config.TheForestServerManagerExecutablePath = TheForestDedicatedServerExePath;
                config.ServerArguments = ServerArguments;
                config.IsSetupSaved = true;
                if (AppConfigurationManager.Save())
                {
                    EventAggregator.GetEvent<CloseWindowRequestEvent>().Publish();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception occurred.\n\n" +
                                $"Message: {e.Message}\n" +
                                $"StackTrace: {e.StackTrace}");
            }
        }
        private bool SaveSettingsCanExecute()
        {
            return TheForestDedicatedServerExePath != "" && ValidateTheForestDedicatedServerExePath(TheForestDedicatedServerExePath) == "";
        }

        private void BrowseExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Application|*.exe",
                DefaultExt = "exe"
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
                if (!File.Exists(exePath))
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
    }
}
