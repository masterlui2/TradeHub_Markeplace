using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Services;

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
                        Status = o.Status,
                        TotalText = $"₱{o.QuantityKilos * o.UnitPrice:N2}",
                        LastUpdatedText = $"Updated: {o.UpdatedAt.ToLocalTime():MMM dd, yyyy hh:mm tt}"
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

        private sealed class OrderLineViewModel
        {
            public string OrderNumber { get; init; } = string.Empty;
            public string ProductSummary { get; init; } = string.Empty;
            public string SellerName { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public string TotalText { get; init; } = string.Empty;
            public string LastUpdatedText { get; init; } = string.Empty;
        }
    }
}