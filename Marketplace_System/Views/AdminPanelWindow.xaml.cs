using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;
using Marketplace_System.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Views
{
    public partial class AdminPanelWindow : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<AdminUserRow> _users = new();
        private readonly ObservableCollection<AdminListingRow> _listings = new();
        private readonly ObservableCollection<ActivityLogItemViewModel> _activityLogs = new();

        private readonly ActivityLogService _activityLogService = new();

        private readonly ICollectionView _usersView;
        private readonly ICollectionView _listingsView;

        private string _userSearchText = "";
        private string _listingSearchText = "";
        private string _listingStatusFilter = "All statuses";
        private string _listingSortBy = "Newest";

        private int _totalUsers;
        private int _totalListings;
        private int _activeOrders;
        private decimal _revenueThisMonth;

        private string _dashboardSummary = "Loading dashboard...";
        private string _userTableSummary = "Loading users...";
        private string _listingTableSummary = "Loading listings...";
        private string _activityLogSummary = "Loading logs...";

        private AdminUserRow? _selectedUser;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int TotalUsers
        {
            get => _totalUsers;
            set => SetField(ref _totalUsers, value);
        }

        public int TotalListings
        {
            get => _totalListings;
            set => SetField(ref _totalListings, value);
        }

        public int ActiveOrders
        {
            get => _activeOrders;
            set => SetField(ref _activeOrders, value);
        }

        public decimal RevenueThisMonth
        {
            get => _revenueThisMonth;
            set => SetField(ref _revenueThisMonth, value);
        }

        public string DashboardSummary
        {
            get => _dashboardSummary;
            set => SetField(ref _dashboardSummary, value);
        }

        public string UserTableSummary
        {
            get => _userTableSummary;
            set => SetField(ref _userTableSummary, value);
        }

        public string ListingTableSummary
        {
            get => _listingTableSummary;
            set => SetField(ref _listingTableSummary, value);
        }

        public string ActivityLogSummary
        {
            get => _activityLogSummary;
            set => SetField(ref _activityLogSummary, value);
        }

        public string UserSearchText
        {
            get => _userSearchText;
            set
            {
                if (SetField(ref _userSearchText, value))
                    _usersView.Refresh();
            }
        }

        public string ListingSearchText
        {
            get => _listingSearchText;
            set
            {
                if (SetField(ref _listingSearchText, value))
                    _listingsView.Refresh();
            }
        }

        public string ListingStatusFilter
        {
            get => _listingStatusFilter;
            set
            {
                if (SetField(ref _listingStatusFilter, value))
                    _listingsView.Refresh();
            }
        }

        public string ListingSortBy
        {
            get => _listingSortBy;
            set
            {
                if (!SetField(ref _listingSortBy, value))
                    return;

                ApplyListingSort();
            }
        }

        public ObservableCollection<ActivityLogItemViewModel> ActivityLogs => _activityLogs;
         public ICollectionView UsersView => _usersView;

        public ICollectionView ListingsView => _listingsView;

        public AdminUserRow? SelectedUser
        {
            get => _selectedUser;
            set => SetField(ref _selectedUser, value);
        }

        public AdminPanelWindow()
        {
            InitializeComponent();
            DataContext = this;

            _usersView = CollectionViewSource.GetDefaultView(_users);
            _usersView.Filter = FilterUsers;

            _listingsView = CollectionViewSource.GetDefaultView(_listings);
            _listingsView.Filter = FilterListings;

            ApplyListingSort();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowSection(AdminSection.Dashboard);
            await LoadAdminDataAsync();
        }

        private async Task LoadAdminDataAsync()
        {
            try
            {
                await using var db = new AppDbContext();

                var users = await db.Users.AsNoTracking().ToListAsync();
                var listings = await db.ProductListings.AsNoTracking().ToListAsync();
                var orders = await db.Orders.AsNoTracking().ToListAsync();

                _users.Clear();

                foreach (var user in users.OrderByDescending(u => u.CreatedAt))
                {
                    _users.Add(new AdminUserRow
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        MobileNumber = user.MobileNumber,
                        City = user.City,
                      
                        CreatedAt = user.CreatedAt,
                        LastActivityLabel = "Recently active"
                    });
                }

                _listings.Clear();

                foreach (var listing in listings)
                {
                    _listings.Add(new AdminListingRow
                    {
                        Id = listing.Id,
                        ProductName = listing.ProductName,
                        Category = listing.Category,
                        SellerName = listing.SellerUserId.ToString(),
                        PricePerKilo = listing.PricePerKilo,
                        AvailableKilos = listing.AvailableKilos,
                        SoldKilos = 0,
                        Revenue = 0,
                        Status = "Active",
                        CreatedAt = listing.CreatedAt
                    });
                }

                TotalUsers = users.Count;
                TotalListings = listings.Count;
                ActiveOrders = orders.Count(o => o.Status != Order.StatusCompleted);

                var now = DateTime.UtcNow;

                RevenueThisMonth = orders
                    .Where(o => o.Status == Order.StatusCompleted &&
                                o.CompletedAt.HasValue &&
                                o.CompletedAt.Value.Month == now.Month &&
                                o.CompletedAt.Value.Year == now.Year)
                    .Sum(o => o.UnitPrice * o.QuantityKilos);

                DashboardSummary = $"{TotalUsers} users • {TotalListings} listings • {ActiveOrders} active orders";

                await LoadDashboardActivityLogsAsync();
                UserTableSummary = $"{_users.Count} users";
                ListingTableSummary = $"{_listings.Count} listings";
                _usersView.Refresh();
                _listingsView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error:\n{ex.Message}");
            }
        }

        private bool FilterUsers(object item)
        {
            if (item is not AdminUserRow user)
                return false;

            if (string.IsNullOrWhiteSpace(UserSearchText))
                return true;

            var term = UserSearchText.ToLower();

            return user.FullName.ToLower().Contains(term)
                   || user.Email.ToLower().Contains(term)
                   || user.City.ToLower().Contains(term);
        }

        private bool FilterListings(object item)
        {
            if (item is not AdminListingRow listing)
                return false;

            if (!string.IsNullOrWhiteSpace(ListingSearchText))
            {
                if (!listing.ProductName.Contains(ListingSearchText, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (ListingStatusFilter != "All statuses")
                return listing.Status == ListingStatusFilter;

            return true;
        }

        private void ApplyListingSort()
        {
            _listingsView.SortDescriptions.Clear();

            if (ListingSortBy == "Newest")
                _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.CreatedAt), ListSortDirection.Descending));
            else
                _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.CreatedAt), ListSortDirection.Ascending));
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAdminDataAsync();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardGrid.Visibility = Visibility.Visible;
            ModuleContentGrid.Visibility = Visibility.Collapsed;
            SetActiveSidebarButton(DashboardButton);
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
            UsersModuleGrid.Visibility = Visibility.Visible;
            ListingsModuleGrid.Visibility = Visibility.Collapsed;
            PaymentsModuleGrid.Visibility = Visibility.Collapsed;
            ActivityLogModuleGrid.Visibility = Visibility.Collapsed;
            SetActiveSidebarButton(ManageUsersButton);
        }

        private void ManageListingsButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
            UsersModuleGrid.Visibility = Visibility.Collapsed;
            ListingsModuleGrid.Visibility = Visibility.Visible;
            PaymentsModuleGrid.Visibility = Visibility.Collapsed;
            ActivityLogModuleGrid.Visibility = Visibility.Collapsed;
            SetActiveSidebarButton(ManageListingsButton);
        }

        private void ManagePaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
            UsersModuleGrid.Visibility = Visibility.Collapsed;
            ListingsModuleGrid.Visibility = Visibility.Collapsed;
            PaymentsModuleGrid.Visibility = Visibility.Visible;
            ActivityLogModuleGrid.Visibility = Visibility.Collapsed;
            SetActiveSidebarButton(ManagePaymentsButton);
        }

        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
            UsersModuleGrid.Visibility = Visibility.Collapsed;
            ListingsModuleGrid.Visibility = Visibility.Collapsed;
            PaymentsModuleGrid.Visibility = Visibility.Collapsed;
            ActivityLogModuleGrid.Visibility = Visibility.Visible;
            SetActiveSidebarButton(ActivityLogButton);
        }

        private void SetActiveSidebarButton(Button activeButton)
        {
            var buttons = new[]
            {
                DashboardButton,
                ManageUsersButton,
                ManageListingsButton,
                ManagePaymentsButton,
                ActivityLogButton
            };

            foreach (var button in buttons)
            {
                button.Background = button == activeButton
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3F4B"))
                    : Brushes.Transparent;
            }
        }
        private async Task LoadDashboardActivityLogsAsync()
        {
            var logs = await _activityLogService.GetRecentAsync(30);

            _activityLogs.Clear();

            foreach (var log in logs)
            {
                _activityLogs.Add(new ActivityLogItemViewModel
                {
                    TimeLabel = log.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    Category = log.Category,
                    Action = log.Action,
                    Details = log.Details
                });
            }

            ActivityLogSummary = $"{_activityLogs.Count} recent activities";
        }
        private enum AdminSection
        {
            Dashboard,
            Users,
            Listings,
            Payments,
            ActivityLogs
        }

        private void ShowSection(AdminSection section)
        {
            DashboardGrid.Visibility = section == AdminSection.Dashboard ? Visibility.Visible : Visibility.Collapsed;
            ModuleContentGrid.Visibility = section == AdminSection.Dashboard ? Visibility.Collapsed : Visibility.Visible;

            UsersModuleGrid.Visibility = section == AdminSection.Users ? Visibility.Visible : Visibility.Collapsed;
            ListingsModuleGrid.Visibility = section == AdminSection.Listings ? Visibility.Visible : Visibility.Collapsed;

            ApplySidebarSelection(section);

            switch (section)
            {
                case AdminSection.Dashboard:
                    PageTitleText.Text = "Dashboard";
                    PageSubtitleText.Text = "Live platform summary";
                    break;
                case AdminSection.Users:
                    PageTitleText.Text = "Manage Users";
                    PageSubtitleText.Text = "Browse and review registered users";
                    break;
                case AdminSection.Listings:
                    PageTitleText.Text = "Manage Listings";
                    PageSubtitleText.Text = "Monitor listing inventory and status";
                    break;
                case AdminSection.Payments:
                    PageTitleText.Text = "Payments";
                    PageSubtitleText.Text = "Payments are handled in the dedicated payments view";
                    break;
                case AdminSection.ActivityLogs:
                    PageTitleText.Text = "Activity Log";
                    PageSubtitleText.Text = "Activity history is available in the dedicated activity log view";
                    break;
            }
        }

        private void ApplySidebarSelection(AdminSection section)
        {
            DashboardButton.Background = section == AdminSection.Dashboard ? new SolidColorBrush(Color.FromRgb(0x2D, 0x3F, 0x4B)) : Brushes.Transparent;
            ManageUsersButton.Background = section == AdminSection.Users ? new SolidColorBrush(Color.FromRgb(0x2D, 0x3F, 0x4B)) : Brushes.Transparent;
            ManageListingsButton.Background = section == AdminSection.Listings ? new SolidColorBrush(Color.FromRgb(0x2D, 0x3F, 0x4B)) : Brushes.Transparent;
            ManagePaymentsButton.Background = section == AdminSection.Payments ? new SolidColorBrush(Color.FromRgb(0x2D, 0x3F, 0x4B)) : Brushes.Transparent;
            ActivityLogButton.Background = section == AdminSection.ActivityLogs ? new SolidColorBrush(Color.FromRgb(0x2D, 0x3F, 0x4B)) : Brushes.Transparent;
        }

      
        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        public sealed class AdminUserRow
        {
            public int Id { get; init; }
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string MobileNumber { get; set; } = "";
            public string City { get; set; } = "";
            public string Role { get; set; } = "";
            public string Status { get; set; } = "Active";
            public DateTime CreatedAt { get; init; }
            public string LastActivityLabel { get; set; } = "";
        }

        private sealed class AdminListingRow
        {
            public int Id { get; init; }
            public string ProductName { get; init; } = "";
            public string SellerName { get; init; } = "";
            public string Category { get; init; } = "";
            public decimal PricePerKilo { get; init; }
            public int AvailableKilos { get; init; }
            public int SoldKilos { get; init; }
            public decimal Revenue { get; init; }
            public string Status { get; init; } = "Active";
            public DateTime CreatedAt { get; init; }
            public Brush StatusBackground => Status switch
            {
                "Sold Out" => new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)),
                "Low Stock" => new SolidColorBrush(Color.FromRgb(0xFE, 0xF3, 0xC7)),
                _ => new SolidColorBrush(Color.FromRgb(0xDC, 0xFC, 0xE7))
            };
            public Brush StatusForeground => Status switch
            {
                "Sold Out" => new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1B)),
                "Low Stock" => new SolidColorBrush(Color.FromRgb(0x92, 0x4, 0x0E)),
                _ => new SolidColorBrush(Color.FromRgb(0x16, 0x65, 0x34))
            };
        }
    }
}