using System.Windows;
using TheForestDSM.Events;
using TheForestDSM.ViewModels;

namespace TheForestDSM.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SetupView : Window
    {
        public SetupView()
        {
            InitializeComponent();
        }

        public SetupView(SetupViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.EventAggregator.GetEvent<SetupViewCloseRequest>().Subscribe(CloseWindowExecute);
        }

        private void CloseWindowExecute()
        {
            Close();
        }
    }
}
