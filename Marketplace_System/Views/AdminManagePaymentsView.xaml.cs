using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Views
{
    public partial class AdminManagePaymentsView : UserControl, INotifyPropertyChanged
    {
        private readonly ObservableCollection<PaymentRow> _payments = new();
        private readonly ICollectionView _paymentsView;
        private string _paymentSearchText = string.Empty;
        private string _paymentStatusFilter = "All statuses";
        private string _paymentTableSummary = "Loading payments...";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICollectionView PaymentsView => _paymentsView;

        public string PaymentSearchText
        {
            get => _paymentSearchText;
            set
            {
                if (!SetField(ref _paymentSearchText, value))
                    return;
                _paymentsView.Refresh();
            }
        }

        public string PaymentStatusFilter
        {
            get => _paymentStatusFilter;
            set
            {
                if (!SetField(ref _paymentStatusFilter, value))
                    return;
                _paymentsView.Refresh();
            }
        }

        public string PaymentTableSummary
        {
            get => _paymentTableSummary;
            set => SetField(ref _paymentTableSummary, value);
        }

        public AdminManagePaymentsView()
        {
            InitializeComponent();
            DataContext = this;
            _paymentsView = CollectionViewSource.GetDefaultView(_payments);
            _paymentsView.Filter = FilterPayments;
            Loaded += AdminManagePaymentsView_Loaded;
        }

        private async void AdminManagePaymentsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                await using var db = new AppDbContext();
                var users = await db.Users.AsNoTracking().ToListAsync();
                var orders = await db.Orders.AsNoTracking().OrderByDescending(o => o.UpdatedAt).ToListAsync();
                var payments = await db.Payments.AsNoTracking().OrderByDescending(p => p.UpdatedAt).ToListAsync();

                var usersById = users.ToDictionary(u => u.Id, u => u.FullName);
                var latestPaymentsByOrder = payments
                    .GroupBy(p => p.OrderNumber)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.UpdatedAt).First());

                _payments.Clear();
                foreach (var order in orders)
                {
                    latestPaymentsByOrder.TryGetValue(order.OrderNumber, out var payment);
                    var status = payment?.Status ?? MapOrderStatusToPaymentStatus(order.Status);

                    _payments.Add(new PaymentRow
                    {
                        OrderNumber = order.OrderNumber,
                        BuyerName = usersById.TryGetValue(order.BuyerUserId, out var buyerName) ? buyerName : "Unknown buyer",
                        SellerName = usersById.TryGetValue(order.SellerUserId, out var sellerName) ? sellerName : "Unknown seller",
                        Method = payment?.Method ?? order.FulfillmentMethod,
                        Status = NormalizePaymentStatus(status),
                        Amount = payment?.Amount ?? (order.UnitPrice * order.QuantityKilos),
                        Date = payment?.UpdatedAt ?? order.UpdatedAt
                    });
                }

                PaymentTableSummary = $"Showing {_payments.Count} order-backed payment records.";
                _paymentsView.Refresh();
            }
            catch (Exception ex)
            {
                PaymentTableSummary = $"Database load failed: {ex.Message}";
            }
        }

        private bool FilterPayments(object item)
        {
            if (item is not PaymentRow payment)
                return false;

            if (PaymentStatusFilter != "All statuses" && !string.Equals(payment.Status, PaymentStatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.IsNullOrWhiteSpace(PaymentSearchText))
                return true;

            var term = PaymentSearchText.Trim();
            return payment.OrderNumber.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || payment.BuyerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || payment.SellerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || payment.Method.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || payment.Status.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizePaymentStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Pending";

            return status.Trim() switch
            {
                "Pending payment" => "Pending",
                "Cancelled" => "Failed",
                _ => status.Trim()
            };
        }

        private static string MapOrderStatusToPaymentStatus(string orderStatus)
        {
            return orderStatus switch
            {
                Order.StatusPendingPayment => "Pending",
                Order.StatusCancelled => "Failed",
                Order.StatusPaid or Order.StatusPreparing or Order.StatusReadyForPickup or Order.StatusCompleted => "Paid",
                _ => "Pending"
            };
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public sealed class PaymentRow
        {
            public string OrderNumber { get; set; } = string.Empty;
            public string BuyerName { get; set; } = string.Empty;
            public string SellerName { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }

            public Brush StatusBackground => Status switch
            {
                "Paid" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7")),
                "Pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                "Failed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                "Refunded" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E7FF")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"))
            };

            public Brush StatusForeground => Status switch
            {
                "Paid" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534")),
                "Pending" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E")),
                "Failed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B")),
                "Refunded" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3730A3")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"))
            };
        }
    }
}