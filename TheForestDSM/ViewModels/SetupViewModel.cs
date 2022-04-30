using DataAccess.Models;
using DataAccess.Repositories;
using DataAccess.Schemas;
using DataAccess.Validators;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System;
using System.ComponentModel;
using System.Windows;
using TheForestDSM.Dialogs;
using TheForestDSM.Events;
using TheForestDSM.Views;
using Unity;

namespace TheForestDSM.ViewModels
{
    public class SetupViewModel : ViewModelBase
    {
        private readonly ConfigurationRepository mAppConfigRepo;
        private Configuration mAppConfig;

        private DelegateCommand mSaveSetupCommand;
        private DelegateCommand mCancelSetupCommand;
        private DelegateCommand mBrowseCommand;
        private DelegateCommand mRefreshIntervalInfoCommand;

        public Configuration Config
        {
            get => mAppConfig;
            set => SetProperty(ref mAppConfig, value);
        }
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
        public DelegateCommand RefreshIntervalInfoCommand
        {
            get => mRefreshIntervalInfoCommand ?? (mRefreshIntervalInfoCommand = new DelegateCommand(RefreshIntervalInfoExecute));
            set => mRefreshIntervalInfoCommand = value;
        }

        [InjectionConstructor]
        public SetupViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            mAppConfigRepo = container.Resolve<ConfigurationRepository>();
            Config = mAppConfigRepo.Read(ConfigurationSchema.ID_DEFAULT_VALUE);
            Config.PropertyChanged += AppConfig_PropertyChanged;

            Title = "Setup";

            // Raise events
            SaveSetupCommand.RaiseCanExecuteChanged();
        }

        private void AppConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Config.ServerExecutablePath))
            {
                SaveSetupCommand.RaiseCanExecuteChanged();
            }
        }

        private void SaveSetupExecute()
        {
            try
            {
                // Save to the database
                Config.IsSetup = true;
                mAppConfigRepo.Update(Config);

                EventAggregator.GetEvent<ConfigurationSavedEvent>().Publish(Config);

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
            return Config.ServerExecutablePath != "" && AppConfigurationValidator.ValidateServerExePath(Config.ServerExecutablePath, out string _);
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
                Config.ServerExecutablePath = dialog.FileName;
            }
        }

        private void RefreshIntervalInfoExecute()
        {
            new InfoDialog(AppStrings.RefreshIntervalDialog_Title, AppStrings.RefreshIntervalDialog_Content)
                          .ShowDialog();
        }

        private void CloseWindow()
        {
            EventAggregator.GetEvent<SetupViewCloseRequest>().Publish();
        }
    }
}
