using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.Win32;
using System.Linq;
using Marketplace_System.Services;
namespace Marketplace_System.Views
{
    /// <summary>
    /// Interaction logic for CreateListingModal.xaml
    /// </summary>
    public partial class CreateListingModal : Window
    {
        private readonly int? _editingListingId;
        private string? _uploadedImagePath;
        public int? CreatedListingId { get; private set; }
        public CreateListingModal()
        {
            InitializeComponent();
            CategoryComboBox.SelectedIndex = 0;
            _editingListingId = null;
        }

        public CreateListingModal(ProductListing listing) : this()
        {
            _editingListingId = listing.Id;
            ProductNameTextBox.Text = listing.ProductName;
            PricePerKiloTextBox.Text = listing.PricePerKilo.ToString(CultureInfo.InvariantCulture);
            AvailableKilosTextBox.Text = listing.AvailableKilos.ToString(CultureInfo.InvariantCulture);
            PickupAddressTextBox.Text = listing.PickupAddress;
            _uploadedImagePath = listing.ImagePath;

            ComboBoxItem? categoryItem = CategoryComboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Content?.ToString(), listing.Category, StringComparison.OrdinalIgnoreCase));

            if (categoryItem is not null)
            {
                CategoryComboBox.SelectedItem = categoryItem;
            }

            if (!string.IsNullOrWhiteSpace(listing.ImagePath))
            {
                try
                {
                    ProductPreviewImage.Source = new BitmapImage(new Uri(listing.ImagePath, UriKind.RelativeOrAbsolute));
                }
                catch
                {
                    // Ignore image preview failure for invalid path.
                }
            }

            PublishListingButton.Content = "Save Changes";
            TitleTextBlock.Text = "Edit Listing";
            SubtitleTextBlock.Text = "Update your product details";
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

          

            try
            {
                using AppDbContext dbContext = new AppDbContext();
                ProductListing listing;
                if (_editingListingId.HasValue)
                {
                    listing = dbContext.ProductListings.FirstOrDefault(x => x.Id == _editingListingId.Value && x.SellerUserId == SessionManager.CurrentUserId)
                        ?? throw new InvalidOperationException("Listing not found or not owned by the current seller.");
                }
                else
                {
                    listing = new ProductListing
                    {
                        SellerUserId = SessionManager.CurrentUserId,
                        SellerName = SessionManager.CurrentUserFullName
                    };
                    dbContext.ProductListings.Add(listing);
                }

                listing.ProductName = productName;
                listing.Category = category;
                listing.PricePerKilo = pricePerKilo;
                listing.AvailableKilos = availableKilos;
                listing.PickupAddress = pickupAddress;
                listing.ImagePath = _uploadedImagePath;
                listing.SellerName = SessionManager.CurrentUserFullName;
                dbContext.SaveChanges();
                CreatedListingId = listing.Id;
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
                 _editingListingId.HasValue ? "Listing updated successfully!" : "Listing published successfully!",
                "Listing Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            DialogResult = true;
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