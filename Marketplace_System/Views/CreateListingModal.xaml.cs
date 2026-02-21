using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Marketplace_System.Views
{
    /// <summary>
    /// Interaction logic for CreateListingModal.xaml
    /// </summary>
    public partial class CreateListingModal : Window
    {
        public CreateListingModal()
        {
            InitializeComponent();
        }

        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.webp",
                Title = "Select Product Image"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            ProductPreviewImage.Source = new BitmapImage(new Uri(dialog.FileName));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PublishListingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Your listing details are ready. You can connect this action to your backend save flow.",
                "Listing Draft Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
    }
}