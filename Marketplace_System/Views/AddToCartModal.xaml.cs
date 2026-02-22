using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Marketplace_System.Views
{
    /// <summary>
    /// Interaction logic for AddToCartModal.xaml
    /// </summary>
    public partial class AddToCartModal : Window
    {
        private readonly decimal _unitPrice;

        public AddToCartModal(string productName, string price, string stockDetails, string sellerName)
        {
            InitializeComponent();

            ProductNameText.Text = productName;
            PriceText.Text = price;
            StockText.Text = stockDetails;
            SellerNameText.Text = sellerName;

            _unitPrice = ExtractPriceValue(price);
            UpdateTotalPayment();
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotalPayment();
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            int currentQuantity = GetValidatedQuantity();
            QuantityTextBox.Text = (currentQuantity + 1).ToString(CultureInfo.InvariantCulture);
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            int currentQuantity = GetValidatedQuantity();
            int updatedQuantity = currentQuantity > 1 ? currentQuantity - 1 : 1;
            QuantityTextBox.Text = updatedQuantity.ToString(CultureInfo.InvariantCulture);
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            int quantity = GetValidatedQuantity();
            decimal total = quantity * _unitPrice;

            MessageBox.Show(
                $"Added {quantity} kilo(s) of {ProductNameText.Text} to cart.\nTotal: {total:C2}",
                "Added to Cart",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MessageSellerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Message composer will open here so you can chat with the seller.",
                "Message Seller",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private int GetValidatedQuantity()
        {
            if (int.TryParse(QuantityTextBox.Text, out int parsedQuantity) && parsedQuantity > 0)
            {
                return parsedQuantity;
            }

            QuantityTextBox.Text = "1";
            return 1;
        }

        private void UpdateTotalPayment()
        {
            int quantity = GetValidatedQuantity();
            decimal total = quantity * _unitPrice;
            TotalPaymentText.Text = total.ToString("C2", CultureInfo.CurrentCulture);
        }

        private static decimal ExtractPriceValue(string priceText)
        {
            Match match = Regex.Match(priceText, @"\d+(\.\d+)?");
            if (match.Success && decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal priceValue))
            {
                return priceValue;
            }

            return 0m;
        }
    }
}
