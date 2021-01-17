using System.Windows;
using TheForestDedicatedServerManager.Events;
using TheForestDedicatedServerManager.ViewModels;

namespace TheForestDedicatedServerManager.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SetupView : Window
    {
        #region Constructors
        public SetupView()
        {
            InitializeComponent();
        }

        public SetupView(SetupViewModel viewModel) : this()
        {
            DataContext = viewModel;
            viewModel.EventAggregator.GetEvent<CloseWindowRequestEvent>().Subscribe(CloseWindowExecute);
        }
        #endregion

        private void CloseWindowExecute()
        {
            Close();
        }
    }
}
