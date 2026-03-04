using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
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

        private async void MyCartPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCartItems();
        }

        private async Task LoadCartItems()
        {
            List<CartLineViewModel> cartLines = new();
            string emptyStateMessage = "Your cart is currently empty.";
            int currentUserId = SessionManager.CurrentUserId;

            if (currentUserId <= 0)
            {
                CartItemsControl.ItemsSource = cartLines;
                EmptyCartTextBlock.Text = "Your session has expired. Please login again.";
                EmptyCartTextBlock.Visibility = Visibility.Visible;
                CheckoutButton.IsEnabled = false;
                return;
            }

            SetLoadingState(isLoading: true, message: "Loading cart items...");

            try
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(10));
                using AppDbContext dbContext = new();

                cartLines = await (
                      from cartItem in dbContext.CartItems.AsNoTracking()
                      where cartItem.BuyerUserId == currentUserId
                      join seller in dbContext.Users.AsNoTracking()
                          on cartItem.SellerUserId equals seller.Id into sellerGroup
                      from seller in sellerGroup.DefaultIfEmpty()
                      orderby cartItem.CreatedAt descending
                      select new CartLineViewModel
                      {
                          CartItemId = cartItem.Id,
                          ProductName = cartItem.ProductName,
                          QuantityText = cartItem.QuantityKilos + " kilo(s)",
                          SellerText = "Seller: " + (seller != null ? seller.FullName : "User #" + cartItem.SellerUserId),
                          TotalText = "₱" + (cartItem.QuantityKilos * cartItem.UnitPrice).ToString("N2"),
                          TotalAmount = cartItem.QuantityKilos * cartItem.UnitPrice,
                          IsSelected = true,

                          // Default value - user can change it in the item ComboBox (XAML binding)
                          FulfillmentMethod = Order.FulfillmentPickup
                      })
                    .ToListAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                emptyStateMessage = "Loading timed out. Please try again.";
            }
            catch
            {
                emptyStateMessage = "Unable to load your cart right now.";
            }
            finally
            {
                SetLoadingState(isLoading: false);
            }

            CartItemsControl.ItemsSource = cartLines;
            EmptyCartTextBlock.Text = emptyStateMessage;
            EmptyCartTextBlock.Visibility = cartLines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            UpdateCheckoutState();
        }

        private void SetLoadingState(bool isLoading, string? message = null)
        {
            if (CheckoutButton is null || EmptyCartTextBlock is null)
            {
                return;
            }

            if (isLoading)
            {
                CheckoutButton.IsEnabled = false;
                EmptyCartTextBlock.Text = message ?? "Loading...";
                EmptyCartTextBlock.Visibility = Visibility.Visible;
                return;
            }

            UpdateCheckoutState();
        }

        private void CartSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateCheckoutState();
        }

        private void UpdateCheckoutState()
        {
            if (CartTotalTextBlock is null || CheckoutButton is null)
            {
                return;
            }

            List<CartLineViewModel> selectedLines = GetSelectedLines();
            CartTotalTextBlock.Text = $"₱{selectedLines.Sum(c => c.TotalAmount):N2}";
            CheckoutButton.IsEnabled = selectedLines.Count > 0;
        }

        private List<CartLineViewModel> GetSelectedLines() =>
            (CartItemsControl.ItemsSource as IEnumerable<CartLineViewModel> ?? Enumerable.Empty<CartLineViewModel>())
            .Where(c => c?.IsSelected == true)
            .ToList();

        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            List<CartLineViewModel> selectedLines = GetSelectedLines();
            if (selectedLines.Count == 0)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Place {selectedLines.Count} selected item(s)?\nEach item will use its selected fulfillment method.\nPayment will wait for seller confirmation.",
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

                // fulfillment is per item (from ComboBox binding)
                Dictionary<int, string> fulfillmentByCartItemId =
                    selectedLines.ToDictionary(line => line.CartItemId, line => line.FulfillmentMethod);

                List<int> selectedCartItemIds = fulfillmentByCartItemId.Keys.ToList();

                List<CartItem> checkoutItems = dbContext.CartItems
                    .Where(c => c.BuyerUserId == SessionManager.CurrentUserId && selectedCartItemIds.Contains(c.Id))
                    .OrderBy(c => c.CreatedAt)
                    .ToList();

                if (checkoutItems.Count == 0)
                {
                    await LoadCartItems();
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
                        FulfillmentMethod = fulfillmentByCartItemId.TryGetValue(item.Id, out string? selectedMethod)
                            ? selectedMethod
                            : Order.FulfillmentPickup,
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

                MessageBox.Show(
                    "Checkout submitted. Seller must confirm payment before the order moves forward.",
                    "Checkout Submitted",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadCartItems();
            }
            catch
            {
                MessageBox.Show(
                    "Unable to checkout right now. Please try again.",
                    "Checkout Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
            public string FulfillmentMethod { get; set; } = Order.FulfillmentPickup;
        }
    }
}