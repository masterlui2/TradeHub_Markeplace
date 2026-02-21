using System.Windows;

namespace Marketplace_System.Views
{
    /// <summary>
    /// Interaction logic for AddToCartModal.xaml
    /// </summary>
    public partial class AddToCartModal : Window
    {

        public AddToCartModal(string productName, string price, string stockDetails, string sellerName)
        {
            InitializeComponent();

            ProductNameText.Text = productName;
            PriceText.Text = price;
            StockText.Text = stockDetails;
            SellerText.Text = $"Seller: {sellerName}";
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
    }
}