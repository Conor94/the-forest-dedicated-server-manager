using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using PrismMvvmBase.Bindable;
using System.Windows;
using TheForestDSM.Events;
using Unity;

namespace TheForestDSM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
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

        [InjectionConstructor]
        public MainWindowViewModel(IEventAggregator eventAggregator, IContainerProvider container) : base(eventAggregator, container)
        {
            EventAggregator.GetEvent<QuitEvent>().Subscribe(QuitExecute);

            Title = "The Forest Dedicated Server Manager";
            SelectedViewModel = new HomePageViewModel(eventAggregator, container);
        }

        private void QuitExecute()
        {
            Application.Current.Shutdown();
        }
    }
}
