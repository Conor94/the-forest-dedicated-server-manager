using System.Windows;
using TheForestDSM.Events;
using TheForestDSM.ViewModels.ScheduleShutdown;
using Prism.Ioc;
using Prism.Unity;

namespace TheForestDSM.Views.ScheduleShutdown
{
    public partial class ScheduleShutdownView : Window
    {
        public ScheduleShutdownView(ScheduleShutdownViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            viewModel.EventAggregator.GetEvent<ScheduleShutdownViewCloseRequest>().Subscribe(() =>
            {
                Close();
            });
        }
    }
}
