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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Marketplace_System.Views
{
    public partial class MyCartPanelView : UserControl, INotifyPropertyChanged
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
            string emptyStateMessage = "Your cart is currently empty";
            int currentUserId = SessionManager.CurrentUserId;

            if (currentUserId <= 0)
            {
                CartItemsControl.ItemsSource = cartLines;
                EmptyCartTextBlock.Text = "Session expired. Please login again.";
                EmptyCartBorder.Visibility = Visibility.Visible;
                CheckoutButton.IsEnabled = false;
                return;
            }

            try
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(10));
                using AppDbContext dbContext = new();

                var cartItems = await dbContext.CartItems
                    .AsNoTracking()
                    .Where(c => c.BuyerUserId == currentUserId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync(timeoutCts.Token);

                foreach (var cartItem in cartItems)
                {
                    var seller = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == cartItem.SellerUserId, timeoutCts.Token);

                    var productListing = await dbContext.ProductListings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == cartItem.ProductListingId, timeoutCts.Token);

                    cartLines.Add(new CartLineViewModel
                    {
                        CartItemId = cartItem.Id,
                        ProductName = cartItem.ProductName,
                        QuantityKilos = cartItem.QuantityKilos,
                        SellerName = seller?.FullName ?? $"User #{cartItem.SellerUserId}",
                        PickupAddress = productListing?.PickupAddress ?? "Pickup address not set",
                        UnitPrice = cartItem.UnitPrice,
                        IsSelected = true
                    });
                }
            }
            catch (OperationCanceledException)
            {
                emptyStateMessage = "Loading timed out. Please try again.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading cart: {ex.Message}");
                emptyStateMessage = "Unable to load your cart right now.";
            }

            CartItemsControl.ItemsSource = cartLines;
            EmptyCartBorder.Visibility = cartLines.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            EmptyCartTextBlock.Text = emptyStateMessage;

            UpdateCheckoutState();
        }
        private void ViewTermsButton_Click(object sender, RoutedEventArgs e)
        {
            var termsWindow = new TermsConditionsWindow
            {
                Owner = Window.GetWindow(this)
            };

            termsWindow.ShowDialog();
        }
        private void UpdateCheckoutState()
        {
            if (CartTotalTextBlock == null || CheckoutButton == null) return;

            var selectedLines = GetSelectedLines();
            decimal total = selectedLines.Sum(c => c.TotalAmount);

            CartTotalTextBlock.Text = $"₱{total:N2}";
            CheckoutButton.IsEnabled = selectedLines.Count > 0 && AcceptTncCheckBox?.IsChecked == true;
        }

        private List<CartLineViewModel> GetSelectedLines()
        {
            return (CartItemsControl.ItemsSource as IEnumerable<CartLineViewModel> ?? Enumerable.Empty<CartLineViewModel>())
                .Where(c => c?.IsSelected == true)
                .ToList();
        }
        private void AcceptTncCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateCheckoutState();
        }
        private async void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedLines = GetSelectedLines();
            if (selectedLines.Count == 0) return;
            if (AcceptTncCheckBox?.IsChecked != true)
            {
                MessageBox.Show(
                    "Please review and accept the Buyer Terms and Conditions before proceeding.",
                    "Terms & Conditions Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show(
                $"Checkout {selectedLines.Count} item(s)?\n\n" +
                $"• Pickup items: {selectedLines.Count(x => x.IsPickupSelected)}\n" +
                $"• Delivery items: {selectedLines.Count(x => x.IsDeliverySelected)}",
                "Confirm Checkout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using AppDbContext dbContext = new();
                var now = DateTime.UtcNow;
                var cartItemIds = selectedLines.Select(x => x.CartItemId).ToList();

                var cartItems = await dbContext.CartItems
                    .Where(c => cartItemIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var item in cartItems)
                {
                    var viewModel = selectedLines.First(x => x.CartItemId == item.Id);

                    var order = new Order
                    {
                        OrderNumber = $"ORD-{now:yyyyMMdd}-{item.Id:D4}",
                        ProductName = item.ProductName,
                        QuantityKilos = item.QuantityKilos,
                        UnitPrice = item.UnitPrice,
                        BuyerUserId = item.BuyerUserId,
                        SellerUserId = item.SellerUserId,
                        ProductListingId = item.ProductListingId,
                        Status = Order.StatusPendingPayment,
                        FulfillmentMethod = viewModel.IsDeliverySelected ? Order.FulfillmentDelivery : Order.FulfillmentPickup,
                        Notes = viewModel.IsDeliverySelected
                            ? $"Delivery to: {viewModel.AddressText}"
                            : $"Pickup at: {viewModel.PickupAddress}",
                        CreatedAt = now,
                        UpdatedAt = now
                    };

                    dbContext.Orders.Add(order);

                    // Update product stock
                    var listing = await dbContext.ProductListings
                        .FirstOrDefaultAsync(p => p.Id == item.ProductListingId);
                    if (listing != null)
                    {
                        listing.AvailableKilos = Math.Max(0, listing.AvailableKilos - item.QuantityKilos);
                    }
                }

                dbContext.CartItems.RemoveRange(cartItems);
                await dbContext.SaveChangesAsync();

                MessageBox.Show(
                    "Checkout submitted successfully! The seller will confirm your order shortly.",
                    "Checkout Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await LoadCartItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to complete checkout: {ex.Message}",
                    "Checkout Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed class CartLineViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isPickupSelected = true;
        private bool _isDeliverySelected;
        private string _addressText = string.Empty;
        private string _sellerName = string.Empty;
        private string _pickupAddress = string.Empty;

        public int CartItemId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int QuantityKilos { get; init; }

        public string SellerName
        {
            get => _sellerName;
            init => _sellerName = value;
        }

        public string PickupAddress
        {
            get => _pickupAddress;
            init => _pickupAddress = value;
        }

        public decimal UnitPrice { get; init; }

        public string QuantityText => $"{QuantityKilos} kilo(s)";
        public string SellerText => $"Seller: {SellerName}";
        public string PickupAddressText => $"Pickup Address: {PickupAddress}";
        public string TotalText => $"₱{TotalAmount:N2}";
        public decimal TotalAmount => QuantityKilos * UnitPrice;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPickupSelected
        {
            get => _isPickupSelected;
            set
            {
                if (_isPickupSelected != value)
                {
                    _isPickupSelected = value;
                    if (value) IsDeliverySelected = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowAddressSection));
                    OnPropertyChanged(nameof(AddressLabel));
                    RefreshAddressText();
                }
            }
        }

        public bool IsDeliverySelected
        {
            get => _isDeliverySelected;
            set
            {
                if (_isDeliverySelected != value)
                {
                    _isDeliverySelected = value;
                    if (value) IsPickupSelected = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShowAddressSection));
                    OnPropertyChanged(nameof(AddressLabel));
                    RefreshAddressText();
                }
            }
        }

        public string AddressText
        {
            get => _addressText;
            set
            {
                if (_addressText != value)
                {
                    _addressText = value ?? string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowAddressSection => IsDeliverySelected;
        public string AddressLabel => IsDeliverySelected ? "Delivery Address" : "Pickup Address";

        private void RefreshAddressText()
        {
            if (!IsDeliverySelected && string.IsNullOrEmpty(AddressText))
            {
                AddressText = PickupAddress;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}