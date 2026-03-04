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
    public partial class SellerManageOrdersView : UserControl
    {
        public SellerManageOrdersView()
        {
            InitializeComponent();
            Loaded += SellerManageOrdersView_Loaded;
        }

        private void SellerManageOrdersView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            List<SellerOrderViewModel> orderCards = new();

            try
            {
                using AppDbContext dbContext = new();
                Dictionary<int, string> usersById = dbContext.Users.ToDictionary(u => u.Id, u => u.FullName);

                List<Order> sellerOrders = dbContext.Orders
                    .Where(o => o.SellerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(o => o.UpdatedAt)
                    .ToList();

                PendingCountText.Text = sellerOrders.Count(o => o.Status == Order.StatusPendingPayment).ToString();
                PreparingCountText.Text = sellerOrders.Count(o => o.Status == Order.StatusPreparing).ToString();
                ReadyCountText.Text = sellerOrders.Count(o => o.Status == Order.StatusReadyForPickup).ToString();
                CompletedCountText.Text = sellerOrders.Count(o => o.Status == Order.StatusCompleted).ToString();

                orderCards = sellerOrders.Select(o => new SellerOrderViewModel
                {
                    OrderId = o.Id,
                    Status = o.Status,
                    OrderLine = $"{o.OrderNumber} • {o.ProductName} • {o.QuantityKilos}kg • ₱{o.QuantityKilos * o.UnitPrice:N2}",
                    BuyerLine = $"Buyer: {usersById.GetValueOrDefault(o.BuyerUserId, $"User #{o.BuyerUserId}")}",
                    FulfillmentLine = $"Method: {o.FulfillmentMethod}",
                    TimelineLine = $"Last update: {o.UpdatedAt.ToLocalTime():MMM dd, yyyy hh:mm tt}",
                    AddressLine = o.Notes,
                    ActionText = GetActionText(o.Status),
                    ActionVisibility = string.IsNullOrEmpty(GetActionText(o.Status)) ? Visibility.Collapsed : Visibility.Visible
                }).ToList();
            }
            catch
            {
                PendingCountText.Text = "0";
                PreparingCountText.Text = "0";
                ReadyCountText.Text = "0";
                CompletedCountText.Text = "0";
            }

            OrdersItemsControl.ItemsSource = orderCards;
            EmptyOrdersTextBlock.Visibility = orderCards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void AdvanceStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int orderId })
            {
                return;
            }

            try
            {
                using AppDbContext dbContext = new();
                Order? order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId && o.SellerUserId == SessionManager.CurrentUserId);
                if (order is null)
                {
                    return;
                }

                DateTime now = DateTime.UtcNow;
                switch (order.Status)
                {
                    case Order.StatusPendingPayment:
                        order.Status = Order.StatusPaid;
                        order.PaidAt = now;
                        order.Notes = "Seller confirmed buyer payment.";
                        break;
                    case Order.StatusPaid:
                        order.Status = Order.StatusPreparing;
                        order.PreparingAt = now;
                        order.Notes = "Seller started preparing the order.";
                        break;
                    case Order.StatusPreparing:
                        order.Status = Order.StatusReadyForPickup;
                        order.ReadyForPickupAt = now;
                        order.Notes = order.FulfillmentMethod == Order.FulfillmentDelivery
                                                  ? "Order is packed and out for delivery scheduling."
                                                  : "Order is ready for pickup.";
                        break;
                    case Order.StatusReadyForPickup:
                        order.Status = Order.StatusCompleted;
                        order.CompletedAt = now;
                        order.Notes = order.FulfillmentMethod == Order.FulfillmentDelivery
                                                   ? "Order was delivered and completed."
                                                   : "Order was picked up and completed."; 
                        break;
                    default:
                        return;
                }

                order.UpdatedAt = now;
                dbContext.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Unable to update order right now.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadOrders();
        }

        private static string GetActionText(string status) => status switch
        {
            Order.StatusPendingPayment => "Confirm payment",
            Order.StatusPaid => "Start preparing",
            Order.StatusPreparing => "Mark ready",
            Order.StatusReadyForPickup => "Complete order",
            _ => string.Empty
        };

        private sealed class SellerOrderViewModel
        {
            public int OrderId { get; init; }
            public string OrderLine { get; init; } = string.Empty;
            public string BuyerLine { get; init; } = string.Empty;
            public string FulfillmentLine { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public string TimelineLine { get; init; } = string.Empty;
            public string ActionText { get; init; } = string.Empty;
            public string AddressLine { get; init; } = string.Empty;
            public Visibility ActionVisibility { get; init; }
        }
    }
}