using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.Win32;

namespace Marketplace_System.Views
{
    /// <summary>
    /// Interaction logic for CreateListingModal.xaml
    /// </summary>
    public partial class CreateListingModal : Window
    {
        private string? _uploadedImagePath;

        public CreateListingModal()
        {
            InitializeComponent();
            CategoryComboBox.SelectedIndex = 0;
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
            _uploadedImagePath = dialog.FileName;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PublishListingButton_Click(object sender, RoutedEventArgs e)
        {
            string productName = NormalizeWhitespace(ProductNameTextBox.Text);
            string pickupAddress = NormalizeWhitespace(PickupAddressTextBox.Text);
            string category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

            ProductNameTextBox.Text = productName;
            PickupAddressTextBox.Text = pickupAddress;

            if (string.IsNullOrWhiteSpace(productName))
            {
                ShowValidationError("Please enter a product name.", ProductNameTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                ShowValidationError("Please choose a category for your product.", CategoryComboBox);
                return;
            }

            if (!decimal.TryParse(PricePerKiloTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal pricePerKilo) || pricePerKilo <= 0)
            {
                ShowValidationError("Price per kilo must be a valid amount greater than 0.", PricePerKiloTextBox);
                return;
            }

            if (!int.TryParse(AvailableKilosTextBox.Text.Trim(), out int availableKilos) || availableKilos <= 0)
            {
                ShowValidationError("Available kilos must be a whole number greater than 0.", AvailableKilosTextBox);
                return;
            }

            if (string.IsNullOrWhiteSpace(pickupAddress))
            {
                ShowValidationError("Please enter a pickup address so buyers know where to collect the product.", PickupAddressTextBox);
                return;
            }

            ProductListing listing = new ProductListing
            {
                ProductName = productName,
                Category = category,
                PricePerKilo = pricePerKilo,
                AvailableKilos = availableKilos,
                PickupAddress = pickupAddress,
                ImagePath = _uploadedImagePath
            };

            try
            {
                using AppDbContext dbContext = new AppDbContext();
                dbContext.ProductListings.Add(listing);
                dbContext.SaveChanges();
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "We couldn't save your listing to the database right now. Please check your database connection and try again.",
                    "Save Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            MessageBox.Show(
                $"Listing published successfully!\n\n" +
                $"Product: {productName}\n" +
                $"Category: {category}\n" +
                $"Price: ₱{pricePerKilo:N2} / kilo\n" +
                $"Stock: {availableKilos} kilo(s)",
                "Listing Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }

        private static string NormalizeWhitespace(string value)
        {
            return string.Join(" ", value.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static void ShowValidationError(string message, Control controlToFocus)
        {
            MessageBox.Show(message, "Incomplete Listing", MessageBoxButton.OK, MessageBoxImage.Warning);
            controlToFocus.Focus();
        }
    }
}