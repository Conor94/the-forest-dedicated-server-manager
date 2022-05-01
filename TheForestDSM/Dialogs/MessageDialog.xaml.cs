using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TheForestDSM.Dialogs
{
    /// <summary>
    /// Interaction logic for InfoDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window
    {
        public double ContentBoxWidth => InfoDialogWindow.ActualWidth - (InfoDialogDockPanel.Margin.Left * InfoDialogDockPanel.Margin.Right) - (InfoDialogStackPanel.Margin.Left + InfoDialogStackPanel.Margin.Right);

        public MessageDialog(string title, string content, MessageDialogType type)
        {
            InitializeComponent();

            Title = title;
            ContentBox.Text = content;

            switch (type)
            {
                case MessageDialogType.Info:
                    BitmapImage img = new BitmapImage(new Uri($@"{AppStrings.ImagesPath}\info.png", UriKind.Relative));
                    DialogImage.Source = img;
                    break;
                case MessageDialogType.Warn:
                    DialogImage.Source = new BitmapImage(new Uri($@"{AppStrings.ImagesPath}\warn.png", UriKind.Relative));
                    break;
                case MessageDialogType.Error:
                    DialogImage.Source = new BitmapImage(new Uri($@"{AppStrings.ImagesPath}\error.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
