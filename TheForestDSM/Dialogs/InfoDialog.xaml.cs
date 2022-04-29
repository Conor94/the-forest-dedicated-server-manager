using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TheForestDSM.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class InfoDialog : Window
    {
        public double ContentBoxWidth => InfoDialogWindow.ActualWidth - (InfoDialogDockPanel.Margin.Left * InfoDialogDockPanel.Margin.Right) - (InfoDialogStackPanel.Margin.Left + InfoDialogStackPanel.Margin.Right);

        public InfoDialog(string title, string content)
        {
            InitializeComponent();

            Title = title;
            ContentBox.Text = content;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
