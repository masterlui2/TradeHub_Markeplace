using System;
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
                        QuantityText = $"{o.QuantityKilos} kilo(s)",
                        UnitPriceText = $"₱{o.UnitPrice:N2}",
                        TotalText = $"₱{o.QuantityKilos * o.UnitPrice:N2}",
                        LastUpdatedText = $"Updated: {o.UpdatedAt.ToLocalTime():MMM dd, yyyy hh:mm tt}",
                        AddressText = BuildAddressText(o.FulfillmentMethod, o.Notes),
                        StatusBackground = GetStatusBackground(o.Status),
                        StatusBorderBrush = GetStatusBorderBrush(o.Status),
                        StatusForeground = GetStatusForeground(o.Status)
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
        private static string GetStatusBackground(string status)
        {
            return status == Order.StatusPendingPayment ? "#FFF8E6" : "#EEF6F2";
        }

        private static string GetStatusBorderBrush(string status)
        {
            return status == Order.StatusPendingPayment ? "#F2E2B3" : "#CFE5DA";
        }

        private static string GetStatusForeground(string status)
        {
            return status == Order.StatusPendingPayment ? "#856404" : "#2E7D5B";
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
            public string QuantityText { get; init; } = string.Empty;
            public string UnitPriceText { get; init; } = string.Empty;
            public string TotalText { get; init; } = string.Empty;
            public string LastUpdatedText { get; init; } = string.Empty;
            public string AddressText { get; init; } = string.Empty;
            public string StatusBackground { get; init; } = "#EEF6F2";
            public string StatusBorderBrush { get; init; } = "#CFE5DA";
            public string StatusForeground { get; init; } = "#2E7D5B";
        }
    }
}