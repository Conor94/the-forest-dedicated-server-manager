using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace TheForestDSM.Dialogs
{
    public partial class MessageDialog : Window
    {
        public double ContentBoxWidth => InfoDialogWindow.ActualWidth - (InfoDialogDockPanel.Margin.Left * InfoDialogDockPanel.Margin.Right) - (InfoDialogStackPanel.Margin.Left + InfoDialogStackPanel.Margin.Right);

        /// <summary>Initializes a <see cref="MessageDialog"/> with only a positive button.</summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="content">The content for the dialog.</param>
        /// <param name="type">The type of dialog. Refer to <see cref="MessageDialogType"/> for the available types.</param>
        /// <param name="positiveButtonText">The text for the positive button.</param>
        public MessageDialog(string title, string content, MessageDialogType type, string positiveButtonText = "Ok") : this(title, content, type, positiveButtonText, null)
        {
        }

        /// <summary>Initializes a <see cref="MessageDialog"/> with only a positive button.</summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="content">
        /// The content displayed by the dialog.<para/>
        /// For information about what content can be displayed, refer to derived classes listed here: <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.documents.inline?view=netframework-4.7.2"/>.
        /// </param>
        /// <param name="type">The type of dialog. Refer to <see cref="MessageDialogType"/> for the available types.</param>
        /// <param name="positiveButtonText">The text for the positive button.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid type is for <paramref name="content"/>.</exception>
        public MessageDialog(string title, Inline[] content, MessageDialogType type, string positiveButtonText = "Ok") : this(title, content, type, positiveButtonText, null)
        {
        }

        /// <summary>Initializes a <see cref="MessageDialog"/> with positive and negative button.</summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="content">
        /// The content displayed by the dialog.<para/>
        /// For information about what content can be displayed, refer to derived classes listed here: <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.documents.inline?view=netframework-4.7.2"/>.
        /// </param>
        /// <param name="type">The type of dialog. Refer to <see cref="MessageDialogType"/> for the available types.</param>
        /// <param name="positiveButtonText">The text for the positive button.</param>
        /// <param name="negativeButtonText">The text for the negative button.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid type is for <paramref name="content"/>.</exception>
        public MessageDialog(string title, Inline[] content, MessageDialogType type, string positiveButtonText, string negativeButtonText) : this(title, "", type, positiveButtonText, negativeButtonText)
        {
            ContentBox.Inlines.AddRange(content.Select((element) =>
            {
                if (element is Hyperlink link)
                {
                    link.RequestNavigate += Link_RequestNavigate;
                    return link;
                }

                return element;
            }));
        }

        /// <summary>Initializes a <see cref="MessageDialog"/> with positive and negative button.</summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="content">The content for the dialog.</param>
        /// <param name="type">The type of dialog. Refer to <see cref="MessageDialogType"/> for the available types.</param>
        /// <param name="positiveButtonText">The text for the positive button.</param>
        /// <param name="negativeButtonText">The text for the negative button.</param>
        public MessageDialog(string title, string content, MessageDialogType type, string positiveButtonText, string negativeButtonText)
        {
            InitializeComponent();

            Title = title;
            if (!string.IsNullOrEmpty(content))
            {
                ContentBox.Text = content;
            }

            switch (type)
            {
                case MessageDialogType.Info:
                    DialogImage.Source = new BitmapImage(new Uri($@"{AppStrings.ImagesPath}\info.png", UriKind.Relative));
                    break;
                case MessageDialogType.Question:
                    DialogImage.Source = new BitmapImage(new Uri($@"{AppStrings.ImagesPath}\question.png", UriKind.Relative));
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

            PositiveButton.Content = positiveButtonText;

            if (negativeButtonText == null)
            {
                NegativeButton.Visibility = Visibility.Collapsed;

                PositiveButton.Margin = new Thickness(150, 0, 0, 0); // Center the button when there's only one

                PositiveButton.IsDefault = true;
            }
            else
            {
                NegativeButton.Content = negativeButtonText;

                PositiveButton.Margin = new Thickness(70, 0, 0, 0);
                NegativeButton.Margin = new Thickness(0, 0, 0, 0);

                PositiveButton.IsDefault = false;
                NegativeButton.IsDefault = true;
            }
        }

        private void Link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri).Dispose();
            e.Handled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;

            Close();
        }
    }
}
