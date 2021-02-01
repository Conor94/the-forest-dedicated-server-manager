using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Windows;
using TheForestDedicatedServerManager.Base;
using TheForestDedicatedServerManager.Events;
using Unity;

namespace TheForestDedicatedServerManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields and properties
        private ViewModelBase mSelectedViewModel;
        private DelegateCommand mQuitCommand;

        public ViewModelBase SelectedViewModel
        {
            get => mSelectedViewModel;
            set => SetProperty(ref mSelectedViewModel, value);
        }

        public DelegateCommand QuitCommand
        {
            get => mQuitCommand ?? (mQuitCommand = new DelegateCommand(QuitExecute));
        }
        #endregion

        #region Constructors
        [InjectionConstructor]
        public MainWindowViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            EventAggregator.GetEvent<QuitEvent>().Subscribe(QuitExecute);

            Title = "The Forest Dedicated Server Manager";
            SelectedViewModel = new HomePageViewModel(eventAggregator, container);
        }
        #endregion

        #region Command methods
        private void QuitExecute()
        {
            Application.Current.Shutdown();
        }
        #endregion
    }
}
