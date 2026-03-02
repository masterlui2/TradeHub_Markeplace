using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class MyCartPanelView : UserControl
    {
        private List<CartItem> _loadedCartItems = new();
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
            _loadedCartItems = new();

            try
            {
                using AppDbContext dbContext = new();
                var usersById = dbContext.Users.ToDictionary(u => u.Id, u => u.FullName);

                _loadedCartItems = dbContext.CartItems.Where(c => c.BuyerUserId == SessionManager.CurrentUserId)
                      .ToList();

                cartLines = _loadedCartItems
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CartLineViewModel
                    {
                        ProductName = c.ProductName,
                        QuantityText = $"{c.QuantityKilos} kilo(s)",
                        SellerText = $"Seller: {usersById.GetValueOrDefault(c.SellerUserId, $"User #{c.SellerUserId}")}",
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
            CheckoutButton.IsEnabled = _loadedCartItems.Count > 0;
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedCartItems.Count == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Place {_loadedCartItems.Count} item(s) as orders now?",
                "Confirm Checkout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using AppDbContext dbContext = new();

                List<CartItem> checkoutItems = dbContext.CartItems
                    .Where(c => c.BuyerUserId == SessionManager.CurrentUserId)
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                if (checkoutItems.Count == 0)
                {
                    LoadCartItems();
                    return;
                }

                foreach (CartItem item in checkoutItems)
                {
                    string orderNumber = $"FH-{DateTime.UtcNow:yyMMdd}-{item.Id:D4}";
                    dbContext.Orders.Add(new Order
                    {
                        OrderNumber = orderNumber,
                        ProductName = item.ProductName,
                        QuantityKilos = item.QuantityKilos,
                        UnitPrice = item.UnitPrice,
                        BuyerUserId = item.BuyerUserId,
                        SellerUserId = item.SellerUserId,
                        ProductListingId = item.ProductListingId,
                        Status = "Pending payment",
                        CreatedAt = DateTime.UtcNow
                    });

                    ProductListing? listing = dbContext.ProductListings.FirstOrDefault(p => p.Id == item.ProductListingId);
                    if (listing is not null)
                    {
                        listing.AvailableKilos = Math.Max(0, listing.AvailableKilos - item.QuantityKilos);
                    }
                }

                dbContext.CartItems.RemoveRange(checkoutItems);
                dbContext.SaveChanges();

                MessageBox.Show("Checkout complete. Your orders are now in My Orders.", "Checkout Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCartItems();
            }
            catch
            {
                MessageBox.Show("Unable to checkout right now. Please try again.", "Checkout Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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