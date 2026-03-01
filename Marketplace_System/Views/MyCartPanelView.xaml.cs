using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class MyCartPanelView : UserControl
    {
        public MyCartPanelView()
        {
            InitializeComponent();
            Loaded += MyCartPanelView_Loaded;
        }

        private void MyCartPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            List<CartLineViewModel> cartLines = new();

            try
            {
                using AppDbContext dbContext = new();
                cartLines = dbContext.CartItems
                    .Where(c => c.BuyerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CartLineViewModel
                    {
                        ProductName = c.ProductName,
                        QuantityText = $"{c.QuantityKilos} kilo(s)",
                        SellerText = $"Seller ID: {c.SellerUserId}",
                        TotalText = $"₱{c.QuantityKilos * c.UnitPrice:N2}",
                        TotalAmount = c.QuantityKilos * c.UnitPrice
                    })
                    .ToList();
            }
            catch
            {
                // Keep empty state if DB is unreachable.
            }

            CartItemsControl.ItemsSource = cartLines;
            EmptyCartTextBlock.Visibility = cartLines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CartTotalTextBlock.Text = $"₱{cartLines.Sum(c => c.TotalAmount):N2}";
        }

        private sealed class CartLineViewModel
        {
            public string ProductName { get; init; } = string.Empty;
            public string QuantityText { get; init; } = string.Empty;
            public string SellerText { get; init; } = string.Empty;
            public string TotalText { get; init; } = string.Empty;
            public decimal TotalAmount { get; init; }
        }
    }
}