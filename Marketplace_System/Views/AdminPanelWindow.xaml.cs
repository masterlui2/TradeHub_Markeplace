using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Views
{
    public partial class AdminPanelWindow : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<AdminUserRow> _users = new();
        private readonly ICollectionView _usersView;
        private string _userSearchText = string.Empty;
        private int _totalUsers;
        private int _totalListings;
        private int _activeOrders;
        private decimal _revenueThisMonth;
        private string _dashboardSummary = "Loading dashboard...";
        private string _userTableSummary = "Loading users...";

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

        public ICollectionView UsersView => _usersView;

        public string UserSearchText
        {
            get => _userSearchText;
            set
            {
                if (!SetField(ref _userSearchText, value))
                {
                    return;
                }

                _usersView.Refresh();
            }
        }

        public AdminPanelWindow()
        {
            InitializeComponent();
            DataContext = this;
            _usersView = System.Windows.Data.CollectionViewSource.GetDefaultView(_users);
            _usersView.Filter = FilterUsers;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAdminDataAsync();
            ShowDashboard();
            UpdateActiveButton(DashboardButton);
        }

        private async Task LoadAdminDataAsync()
        {
            try
            {
                await using var db = new AppDbContext();

                var users = await db.Users.AsNoTracking().ToListAsync();
                var listings = await db.ProductListings.AsNoTracking().ToListAsync();
                var orders = await db.Orders.AsNoTracking().ToListAsync();

                var sellerIds = listings.Select(l => l.SellerUserId).ToHashSet();
                var userLastActivity = listings
                    .GroupBy(l => l.SellerUserId)
                    .ToDictionary(g => g.Key, g => g.Max(x => x.CreatedAt));

                foreach (var order in orders)
                {
                    if (!userLastActivity.TryGetValue(order.BuyerUserId, out var buyerActivity) || buyerActivity < order.UpdatedAt)
                    {
                        userLastActivity[order.BuyerUserId] = order.UpdatedAt;
                    }

                    if (!userLastActivity.TryGetValue(order.SellerUserId, out var sellerActivity) || sellerActivity < order.UpdatedAt)
                    {
                        userLastActivity[order.SellerUserId] = order.UpdatedAt;
                    }
                }

                _users.Clear();
                foreach (var user in users.OrderByDescending(u => u.CreatedAt))
                {
                    userLastActivity.TryGetValue(user.Id, out var lastActivity);
                    _users.Add(new AdminUserRow
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        City = user.City,
                        Role = sellerIds.Contains(user.Id) ? "Seller" : "Buyer",
                        CreatedAt = user.CreatedAt,
                        LastActivityLabel = lastActivity == default
                            ? "No activity yet"
                            : lastActivity.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    });
                }

                TotalUsers = users.Count;
                TotalListings = listings.Count;
                ActiveOrders = orders.Count(o => o.Status != Order.StatusCompleted && o.Status != Order.StatusCancelled);

                var now = DateTime.UtcNow;
                RevenueThisMonth = orders
                    .Where(o => o.Status == Order.StatusCompleted && o.CompletedAt.HasValue && o.CompletedAt.Value.Month == now.Month && o.CompletedAt.Value.Year == now.Year)
                    .Sum(o => o.UnitPrice * o.QuantityKilos);

                DashboardSummary = $"{TotalUsers} users are registered. {TotalListings} listings are posted and {ActiveOrders} orders are currently active.";
                UserTableSummary = $"Showing {_users.Count} users from the database.";
                _usersView.Refresh();
            }
            catch (Exception ex)
            {
                DashboardSummary = "Could not load dashboard data from the database.";
                UserTableSummary = $"Database load failed: {ex.Message}";
                MessageBox.Show($"Failed to load admin data.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool FilterUsers(object item)
        {
            if (item is not AdminUserRow user)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(UserSearchText))
            {
                return true;
            }

            var term = UserSearchText.Trim();
            return user.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.City.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.Role.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadAdminDataAsync();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            UpdateActiveButton(DashboardButton);
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUsersModule("Manage Users", "View and search all registered users");
            UpdateActiveButton(ManageUsersButton);
        }

        private void UserGridButton_Click(object sender, RoutedEventArgs e)
        {
            ShowUsersModule("User Grid", "Database-backed user records with filtering");
            UpdateActiveButton(UserGridButton);
        }

        private void ManageListingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            PageTitleText.Text = "Manage Listings";
            PageSubtitleText.Text = "Listing management will be connected next";
            UpdateActiveButton(ManageListingsButton);
        }

        private void ManagePaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            PageTitleText.Text = "Payments";
            PageSubtitleText.Text = "Payment controls will be connected next";
            UpdateActiveButton(ManagePaymentsButton);
        }

        private void SendMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            PageTitleText.Text = "Messages";
            PageSubtitleText.Text = "Message center will be connected next";
            UpdateActiveButton(SendMessagesButton);
        }

        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            PageTitleText.Text = "Activity Log";
            PageSubtitleText.Text = "Audit timeline will be connected next";
            UpdateActiveButton(ActivityLogButton);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?",
                                        "Confirm Logout",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }

        private void ShowDashboard()
        {
            PageTitleText.Text = "Dashboard";
            PageSubtitleText.Text = "Live platform summary";
            DashboardGrid.Visibility = Visibility.Visible;
            ModuleContentGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowUsersModule(string title, string subtitle)
        {
            PageTitleText.Text = title;
            PageSubtitleText.Text = subtitle;
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
        }

        private void UpdateActiveButton(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardButton,
                ManageUsersButton,
                UserGridButton,
                ManageListingsButton,
                ManagePaymentsButton,
                SendMessagesButton,
                ActivityLogButton
            };

            foreach (var button in buttons)
            {
                button.Background = System.Windows.Media.Brushes.Transparent;
            }

            activeButton.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2D3F4B"));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private sealed class AdminUserRow
        {
            public int Id { get; init; }
            public string FullName { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string City { get; init; } = string.Empty;
            public string Role { get; init; } = string.Empty;
            public DateTime CreatedAt { get; init; }
            public string LastActivityLabel { get; init; } = string.Empty;
        }
    }
}