using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;
using static Marketplace_System.MainWindow;
namespace Marketplace_System.Views
{

    public partial class AddToCartModal : Window
    {
        private readonly BrowseProductCard _product;

        public AddToCartModal(BrowseProductCard product)


        {
            InitializeComponent();

            Loaded += AddToCartModal_Loaded;
            _product = product;
            ProductNameText.Text = product.ProductName;
            PriceText.Text = product.PriceText;
            StockText.Text = product.StockText;
            SellerNameText.Text = product.SellerName;
        }

        private void AddToCartModal_Loaded(object sender, RoutedEventArgs e)
        {
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
            if (quantity > _product.StockKilos && _product.StockKilos > 0)
            {
                MessageBox.Show($"Only {_product.StockKilos} kilo(s) are available.", "Not Enough Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using AppDbContext dbContext = new();
                CartItem? existing = dbContext.CartItems.FirstOrDefault(c =>
                    c.BuyerUserId == SessionManager.CurrentUserId && c.ProductListingId == _product.ProductId);

                if (existing is null)
                {
                    dbContext.CartItems.Add(new CartItem
                    {
                        ProductName = _product.ProductName,
                        QuantityKilos = quantity,
                        UnitPrice = _product.PricePerKilo,
                        BuyerUserId = SessionManager.CurrentUserId,
                        SellerUserId = _product.SellerId,
                        ProductListingId = _product.ProductId
                    });
                }
                else
                {
                    existing.QuantityKilos += quantity;
                }

                dbContext.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Unable to save this cart item right now.", "Add to Cart Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal total = quantity * _product.PricePerKilo;
            MessageBox.Show($"Added {quantity} kilo(s) of {_product.ProductName} to cart.\nTotal: ₱{total:N2}", "Added to Cart", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MessageSellerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Message composer will open here so you can chat with the seller.", "Message Seller", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (TotalPaymentText == null)
          

            {
                return;
            }

            int quantity = GetValidatedQuantity();
            decimal total = quantity * _product.PricePerKilo;
            TotalPaymentText.Text = $"₱{total:N2}";
        }
    }
}
