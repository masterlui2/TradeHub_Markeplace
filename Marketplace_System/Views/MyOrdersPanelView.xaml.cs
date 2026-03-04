using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Services;
using Marketplace_System.Models;
namespace Marketplace_System.Views
{
    public partial class MyOrdersPanelView : UserControl
    {
        public MyOrdersPanelView()
        {
            InitializeComponent();
            Loaded += MyOrdersPanelView_Loaded;
        }

        private void MyOrdersPanelView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            List<OrderLineViewModel> orders = new();

            try
            {
                using AppDbContext dbContext = new();
                var usersById = dbContext.Users.ToDictionary(u => u.Id, u => u.FullName);

                orders = dbContext.Orders
                    .Where(o => o.BuyerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OrderLineViewModel
                    {
                        OrderNumber = o.OrderNumber,
                        ProductSummary = $"{o.ProductName} • {o.QuantityKilos} kilo(s)",
                        SellerName = $"Seller: {usersById.GetValueOrDefault(o.SellerUserId, $"User #{o.SellerUserId}")}",
                        FulfillmentText = $"Method: {o.FulfillmentMethod}",
                        Status = o.Status,
                        TotalText = $"₱{o.QuantityKilos * o.UnitPrice:N2}",
                        LastUpdatedText = $"Updated: {o.UpdatedAt.ToLocalTime():MMM dd, yyyy hh:mm tt}",
                        AddressText = BuildAddressText(o.FulfillmentMethod, o.Notes)
                    })
                    .ToList();
            }
            catch
            {
                // Empty state fallback.
            }

            OrdersItemsControl.ItemsSource = orders;
            EmptyOrdersTextBlock.Visibility = orders.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private static string BuildAddressText(string fulfillmentMethod, string notes)
        {
            string defaultLabel = fulfillmentMethod == Order.FulfillmentDelivery
                ? "Delivery address: Not provided"
                : "Pickup address: Not provided";

            if (string.IsNullOrWhiteSpace(notes))
            {
                return defaultLabel;
            }

            string key = fulfillmentMethod == Order.FulfillmentDelivery
                ? "Delivery address:"
                : "Pickup address:";

            int keyIndex = notes.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (keyIndex < 0)
            {
                return notes;
            }

            string value = notes[(keyIndex + key.Length)..].Trim();
            int sentenceEnd = value.IndexOf('.');
            if (sentenceEnd >= 0)
            {
                value = value[..sentenceEnd].Trim();
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = "Not provided";
            }

            return $"{key} {value}";
        }
        private sealed class OrderLineViewModel
        {
            public string OrderNumber { get; init; } = string.Empty;
            public string ProductSummary { get; init; } = string.Empty;
            public string SellerName { get; init; } = string.Empty;
            public string FulfillmentText { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public string TotalText { get; init; } = string.Empty;
            public string LastUpdatedText { get; init; } = string.Empty;
            public string AddressText { get; init; } = string.Empty;
        }
    }
}