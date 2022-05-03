using DataAccess.Models;
using System.Windows;
using TheForestDSM.Events;
using TheForestDSM.ViewModels.ScheduleShutdown;

namespace TheForestDSM.Views.ScheduleShutdown
{
    public partial class ScheduleShutdownView : Window
    {
        public ScheduleShutdownView(ScheduleShutdownViewModel viewModel, ShutdownServiceData data)
        {
            InitializeComponent();

            DataContext = viewModel;

            viewModel.Data = data;
            viewModel.EventAggregator.GetEvent<ScheduleShutdownViewCloseRequest>().Subscribe(() =>
            {
                Close();
            });
        }
    }
}
