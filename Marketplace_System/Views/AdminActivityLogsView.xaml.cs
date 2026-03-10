using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Views
{
    public partial class AdminActivityLogsView : UserControl, INotifyPropertyChanged
    {
        private readonly ObservableCollection<ActivityLogRow> _activityLogRows = new();
        private readonly ICollectionView _activityLogsView;
        private readonly ActivityLogService _activityLogService = new();
        private string _activitySearchText = string.Empty;
        private string _activityRoleFilter = "All";
        private DateTime? _activityStartDate;
        private DateTime? _activityEndDate;
        private string _activityLogSummary = "Loading activity logs...";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICollectionView ActivityLogsView => _activityLogsView;

        public string ActivitySearchText
        {
            get => _activitySearchText;
            set
            {
                if (!SetField(ref _activitySearchText, value))
                    return;
                _activityLogsView.Refresh();
                UpdateActivitySummary();
            }
        }

        public string ActivityRoleFilter
        {
            get => _activityRoleFilter;
            set
            {
                if (!SetField(ref _activityRoleFilter, value))
                    return;
                _activityLogsView.Refresh();
                UpdateActivitySummary();
            }
        }

        public DateTime? ActivityStartDate
        {
            get => _activityStartDate;
            set
            {
                if (!SetField(ref _activityStartDate, value))
                    return;
                _activityLogsView.Refresh();
                UpdateActivitySummary();
            }
        }

        public DateTime? ActivityEndDate
        {
            get => _activityEndDate;
            set
            {
                if (!SetField(ref _activityEndDate, value))
                    return;
                _activityLogsView.Refresh();
                UpdateActivitySummary();
            }
        }

        public string ActivityLogSummary
        {
            get => _activityLogSummary;
            set => SetField(ref _activityLogSummary, value);
        }

        public AdminActivityLogsView()
        {
            InitializeComponent();
            DataContext = this;
            _activityLogsView = CollectionViewSource.GetDefaultView(_activityLogRows);
            _activityLogsView.Filter = FilterActivityLogs;
            Loaded += AdminActivityLogsView_Loaded;
        }

        private async void AdminActivityLogsView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                await using var db = new AppDbContext();
                var users = await db.Users.AsNoTracking().ToListAsync();
                var listings = await db.ProductListings.AsNoTracking().ToListAsync();
                var orders = await db.Orders.AsNoTracking().ToListAsync();
                var systemLogs = await _activityLogService.GetRecentAsync(150);

                var usersById = users.ToDictionary(u => u.Id, u => u.FullName);
                var sellerIds = listings.Select(x => x.SellerUserId).ToHashSet();
                var entries = new List<ActivityLogRow>();

                foreach (var listing in listings)
                {
                    entries.Add(new ActivityLogRow
                    {
                        Timestamp = listing.CreatedAt,
                        Role = "Seller",
                        Username = usersById.TryGetValue(listing.SellerUserId, out var sellerName) ? sellerName : listing.SellerName,
                        ActionDescription = $"Added product listing: {listing.ProductName}."
                    });
                }

                foreach (var order in orders)
                {
                    entries.Add(new ActivityLogRow
                    {
                        Timestamp = order.CreatedAt,
                        Role = "Buyer",
                        Username = usersById.TryGetValue(order.BuyerUserId, out var buyerName) ? buyerName : "Unknown buyer",
                        ActionDescription = $"Placed order {order.OrderNumber} for {order.ProductName} ({order.QuantityKilos} kg)."
                    });

                    if (order.Status == Order.StatusPaid || order.Status == Order.StatusCompleted || order.PaidAt.HasValue)
                    {
                        entries.Add(new ActivityLogRow
                        {
                            Timestamp = order.PaidAt ?? order.UpdatedAt,
                            Role = "Buyer",
                            Username = usersById.TryGetValue(order.BuyerUserId, out buyerName) ? buyerName : "Unknown buyer",
                            ActionDescription = $"Made payment for order {order.OrderNumber}."
                        });
                    }

                    if (order.Status == Order.StatusPreparing || order.PreparingAt.HasValue)
                        entries.Add(BuildSellerLog("Marked as preparing", order, usersById));

                    if (order.Status == Order.StatusReadyForPickup || order.ReadyForPickupAt.HasValue)
                        entries.Add(BuildSellerLog("Marked as ready for pickup", order, usersById));

                    if (order.Status == Order.StatusCompleted || order.CompletedAt.HasValue)
                        entries.Add(BuildSellerLog("Fulfilled order", order, usersById));
                }

                foreach (var log in systemLogs)
                {
                    var role = log.UserId.HasValue && sellerIds.Contains(log.UserId.Value) ? "Seller" : "Buyer";
                    entries.Add(new ActivityLogRow
                    {
                        Timestamp = log.CreatedAt,
                        Role = role,
                        Username = log.UserId.HasValue && usersById.TryGetValue(log.UserId.Value, out var name) ? name : "System",
                        ActionDescription = $"{log.Action}: {log.Details}"
                    });
                }

                var ordered = entries.OrderByDescending(x => x.Timestamp).Take(300).ToList();
                _activityLogRows.Clear();
                foreach (var entry in ordered)
                    _activityLogRows.Add(entry);

                _activityLogsView.Refresh();
                UpdateActivitySummary();
            }
            catch (Exception ex)
            {
                ActivityLogSummary = $"Database load failed: {ex.Message}";
            }
        }

        private ActivityLogRow BuildSellerLog(string action, Order order, Dictionary<int, string> usersById)
        {
            return new ActivityLogRow
            {
                Timestamp = order.UpdatedAt,
                Role = "Seller",
                Username = usersById.TryGetValue(order.SellerUserId, out var sellerName) ? sellerName : "Unknown seller",
                ActionDescription = $"{action} for order {order.OrderNumber}."
            };
        }

        private bool FilterActivityLogs(object item)
        {
            if (item is not ActivityLogRow row)
                return false;

            if (ActivityRoleFilter != "All" && !string.Equals(row.Role, ActivityRoleFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (ActivityStartDate.HasValue && row.Timestamp.Date < ActivityStartDate.Value.Date)
                return false;

            if (ActivityEndDate.HasValue && row.Timestamp.Date > ActivityEndDate.Value.Date)
                return false;

            if (string.IsNullOrWhiteSpace(ActivitySearchText))
                return true;

            var term = ActivitySearchText.Trim();
            return row.Username.Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.Role.Contains(term, StringComparison.OrdinalIgnoreCase)
                || row.ActionDescription.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateActivitySummary()
        {
            var visibleCount = _activityLogsView.Cast<object>().Count();
            ActivityLogSummary = visibleCount == 0
                ? "No buyer/seller activities match the selected filters."
                : $"Showing {visibleCount} buyer and seller activity entries.";
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public sealed class ActivityLogRow
        {
            public DateTime Timestamp { get; init; }
            public string Role { get; init; } = string.Empty;
            public string Username { get; init; } = string.Empty;
            public string ActionDescription { get; init; } = string.Empty;
        }
    }
}