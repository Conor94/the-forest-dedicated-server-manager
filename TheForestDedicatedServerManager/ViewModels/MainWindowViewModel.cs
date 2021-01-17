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

        /// <summary>
        /// Constructor for the <see cref="MainWindowViewModel"/>.
        /// </summary>
        public MainWindowViewModel(IEventAggregator eventAggregator, IContainerProvider container, Dictionary<string, Func<object, string>> validators) : base(eventAggregator, container, validators)
        {
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
