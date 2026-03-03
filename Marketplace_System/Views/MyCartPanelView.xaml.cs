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
                var usersById = dbContext.Users.ToDictionary(u => u.Id, u => u.FullName);

                cartLines = dbContext.CartItems
                    .Where(c => c.BuyerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CartLineViewModel
                    {
                        CartItemId = c.Id,
                        ProductName = c.ProductName,
                        QuantityText = $"{c.QuantityKilos} kilo(s)",
                        SellerText = $"Seller: {usersById.GetValueOrDefault(c.SellerUserId, $"User #{c.SellerUserId}")}",
                        TotalText = $"₱{c.QuantityKilos * c.UnitPrice:N2}",
                        TotalAmount = c.QuantityKilos * c.UnitPrice,
                        IsSelected = true
                    })
                    .ToList();
            }
            catch
            {
                // Keep empty state if DB is unreachable.
            }

            CartItemsControl.ItemsSource = cartLines;
            EmptyCartTextBlock.Visibility = cartLines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateCheckoutState();
        }

        private void CartSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckoutState();
        }

        private void FulfillmentMethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCheckoutState();
        }

        private void UpdateCheckoutState()
        {
            List<CartLineViewModel> selectedLines = GetSelectedLines();
            CartTotalTextBlock.Text = $"₱{selectedLines.Sum(c => c.TotalAmount):N2}";
            CheckoutButton.IsEnabled = selectedLines.Count > 0 && FulfillmentMethodComboBox.SelectedItem is ComboBoxItem;
        }

        private List<CartLineViewModel> GetSelectedLines() =>
            (CartItemsControl.ItemsSource as IEnumerable<CartLineViewModel> ?? Enumerable.Empty<CartLineViewModel>())
            .Where(c => c.IsSelected)
            .ToList();

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            List<CartLineViewModel> selectedLines = GetSelectedLines();
            if (selectedLines.Count == 0 || FulfillmentMethodComboBox.SelectedItem is not ComboBoxItem selectedMethod)
            {
                return;
            }

            string fulfillmentMethod = selectedMethod.Content?.ToString() ?? Order.FulfillmentPickup;
            MessageBoxResult result = MessageBox.Show(
                $"Place {selectedLines.Count} selected item(s) for {fulfillmentMethod.ToLowerInvariant()}?\nPayment will wait for seller confirmation.",
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

                List<int> selectedCartItemIds = selectedLines.Select(l => l.CartItemId).ToList();
                List<CartItem> checkoutItems = dbContext.CartItems
                    .Where(c => c.BuyerUserId == SessionManager.CurrentUserId && selectedCartItemIds.Contains(c.Id))
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                if (checkoutItems.Count == 0)
                {
                    LoadCartItems();
                    return;
                }

                DateTime now = DateTime.UtcNow;
                foreach (CartItem item in checkoutItems)
                {
                    string orderNumber = $"FH-{now:yyMMdd}-{item.Id:D4}";
                    dbContext.Orders.Add(new Order
                    {
                        OrderNumber = orderNumber,
                        ProductName = item.ProductName,
                        QuantityKilos = item.QuantityKilos,
                        UnitPrice = item.UnitPrice,
                        BuyerUserId = item.BuyerUserId,
                        SellerUserId = item.SellerUserId,
                        ProductListingId = item.ProductListingId,
                        Status = Order.StatusPendingPayment,
                        FulfillmentMethod = fulfillmentMethod,
                        Notes = "Buyer submitted checkout and payment proof. Awaiting seller confirmation.",
                        CreatedAt = now,
                        UpdatedAt = now
                    });

                    ProductListing? listing = dbContext.ProductListings.FirstOrDefault(p => p.Id == item.ProductListingId);
                    if (listing is not null)
                    {
                        listing.AvailableKilos = Math.Max(0, listing.AvailableKilos - item.QuantityKilos);
                    }
                }

                dbContext.CartItems.RemoveRange(checkoutItems);
                dbContext.SaveChanges();

                MessageBox.Show("Checkout submitted. Seller must confirm payment before the order moves forward.", "Checkout Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCartItems();
            }
            catch
            {
                MessageBox.Show("Unable to checkout right now. Please try again.", "Checkout Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private sealed class CartLineViewModel
        {
            public int CartItemId { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string QuantityText { get; init; } = string.Empty;
            public string SellerText { get; init; } = string.Empty;
            public string TotalText { get; init; } = string.Empty;
            public decimal TotalAmount { get; init; }
            public bool IsSelected { get; set; }
        }
    }
}