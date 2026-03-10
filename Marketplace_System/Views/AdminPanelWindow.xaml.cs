using System;
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
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Views
{
    public partial class AdminPanelWindow : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<AdminUserRow> _users = new();
        private readonly ObservableCollection<AdminListingRow> _listings = new();
        private readonly ICollectionView _usersView;
        private readonly ICollectionView _listingsView;
        private string _userSearchText = string.Empty;
        private string _listingSearchText = string.Empty;
        private string _listingStatusFilter = "All statuses";
        private string _listingSortBy = "Newest";
        private int _totalUsers;
        private int _totalListings;
        private int _activeOrders;
        private decimal _revenueThisMonth;
        private string _dashboardSummary = "Loading dashboard...";
        private string _userTableSummary = "Loading users...";
        private string _listingTableSummary = "Loading listings...";
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

        public ICollectionView UsersView => _usersView;
        public ICollectionView ListingsView => _listingsView;

        public AdminUserRow? SelectedUser
        {
            get => _selectedUser;
            set => SetField(ref _selectedUser, value);
        }

        public string UserSearchText
        {
            get => _userSearchText;
            set
            {
                if (!SetField(ref _userSearchText, value))
                    return;
                _usersView.Refresh();
            }
        }

        public string ListingSearchText
        {
            get => _listingSearchText;
            set
            {
                if (!SetField(ref _listingSearchText, value))
                    return;
                _listingsView.Refresh();
            }
        }

        public string ListingStatusFilter
        {
            get => _listingStatusFilter;
            set
            {
                if (!SetField(ref _listingStatusFilter, value))
                    return;
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

        public string ListingTableSummary
        {
            get => _listingTableSummary;
            set => SetField(ref _listingTableSummary, value);
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
                        userLastActivity[order.BuyerUserId] = order.UpdatedAt;

                    if (!userLastActivity.TryGetValue(order.SellerUserId, out var sellerActivity) || sellerActivity < order.UpdatedAt)
                        userLastActivity[order.SellerUserId] = order.UpdatedAt;
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
                        MobileNumber = user.MobileNumber,
                        City = user.City,
                        Role = sellerIds.Contains(user.Id) ? "Seller" : "Buyer",
                        Status = "Active",
                        CreatedAt = user.CreatedAt,
                        LastActivityLabel = lastActivity == default
                            ? "No activity yet"
                            : lastActivity.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                    });
                }

                var listingOrders = orders
                    .GroupBy(o => o.ProductListingId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                _listings.Clear();
                foreach (var listing in listings)
                {
                    listingOrders.TryGetValue(listing.Id, out var matchedOrders);
                    matchedOrders ??= new System.Collections.Generic.List<Order>();

                    var soldKilos = matchedOrders
                        .Where(o => o.Status != Order.StatusCancelled)
                        .Sum(o => o.QuantityKilos);

                    var listingRevenue = matchedOrders
                        .Where(o => o.Status == Order.StatusCompleted)
                        .Sum(o => o.UnitPrice * o.QuantityKilos);

                    var status = listing.AvailableKilos <= 0
                        ? "Sold Out"
                        : listing.AvailableKilos <= 20
                            ? "Low Stock"
                            : "Active";

                    _listings.Add(new AdminListingRow
                    {
                        Id = listing.Id,
                        ProductName = listing.ProductName,
                        SellerName = listing.SellerName,
                        Category = listing.Category,
                        PricePerKilo = listing.PricePerKilo,
                        AvailableKilos = listing.AvailableKilos,
                        SoldKilos = soldKilos,
                        Revenue = listingRevenue,
                        Status = status,
                        CreatedAt = listing.CreatedAt,
                        LastOrderAt = matchedOrders.OrderByDescending(o => o.UpdatedAt).FirstOrDefault()?.UpdatedAt
                    });
                }

                TotalUsers = users.Count;
                TotalListings = listings.Count;
                ActiveOrders = orders.Count(o => o.Status != Order.StatusCompleted && o.Status != Order.StatusCancelled);

                var now = DateTime.UtcNow;
                RevenueThisMonth = orders
                    .Where(o => o.Status == Order.StatusCompleted
                             && o.CompletedAt.HasValue
                             && o.CompletedAt.Value.Month == now.Month
                             && o.CompletedAt.Value.Year == now.Year)
                    .Sum(o => o.UnitPrice * o.QuantityKilos);

                DashboardSummary = $"{TotalUsers} users are registered. {TotalListings} listings are posted and {ActiveOrders} orders are currently active.";

                int activeCount = _users.Count(u => u.Status == "Active");
                UserTableSummary = $"Showing {_users.Count} users ({activeCount} active).";
                ListingTableSummary = $"Showing {_listings.Count} listings from the marketplace.";

                _usersView.Refresh();
                _listingsView.Refresh();
            }
            catch (Exception ex)
            {
                DashboardSummary = "Could not load dashboard data from the database.";
                UserTableSummary = $"Database load failed: {ex.Message}";
                ListingTableSummary = $"Database load failed: {ex.Message}";
                MessageBox.Show($"Failed to load admin data.\n\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool FilterUsers(object item)
        {
            if (item is not AdminUserRow user)
                return false;

            if (string.IsNullOrWhiteSpace(UserSearchText))
                return true;

            var term = UserSearchText.Trim();
            return user.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.City.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.Role.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || user.Status.Contains(term, StringComparison.OrdinalIgnoreCase);
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

        private void ManageListingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowListingsModule("Manage Listings", "Clean listing overview with status and inventory filters");
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
                SessionManager.Clear();
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
            UsersModuleGrid.Visibility = Visibility.Visible;
            ListingsModuleGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowListingsModule(string title, string subtitle)
        {
            PageTitleText.Text = title;
            PageSubtitleText.Text = subtitle;
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;
            UsersModuleGrid.Visibility = Visibility.Collapsed;
            ListingsModuleGrid.Visibility = Visibility.Visible;
        }

        private bool FilterListings(object item)
        {
            if (item is not AdminListingRow listing)
                return false;

            if (ListingStatusFilter != "All statuses" && !string.Equals(listing.Status, ListingStatusFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.IsNullOrWhiteSpace(ListingSearchText))
                return true;

            var term = ListingSearchText.Trim();
            return listing.ProductName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || listing.SellerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || listing.Category.Contains(term, StringComparison.OrdinalIgnoreCase)
                   || listing.Status.Contains(term, StringComparison.OrdinalIgnoreCase);
        }

        private void ApplyListingSort()
        {
            _listingsView.SortDescriptions.Clear();

            switch (ListingSortBy)
            {
                case "Oldest":
                    _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.CreatedAt), ListSortDirection.Ascending));
                    break;
                case "Highest Price":
                    _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.PricePerKilo), ListSortDirection.Descending));
                    break;
                case "Lowest Stock":
                    _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.AvailableKilos), ListSortDirection.Ascending));
                    break;
                default:
                    _listingsView.SortDescriptions.Add(new SortDescription(nameof(AdminListingRow.CreatedAt), ListSortDirection.Descending));
                    break;
            }
        }

        private void UpdateActiveButton(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardButton,
                ManageUsersButton,
                ManageListingsButton,
                ManagePaymentsButton,
                SendMessagesButton,
                ActivityLogButton
            };

            foreach (var button in buttons)
                button.Background = Brushes.Transparent;

            activeButton.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#2D3F4B"));
        }

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var form = new UserAccountFormWindow("Add Account");
            if (form.ShowDialog() != true)
                return;

            await CreateUserAsync(form.FormData);
        }

        private async Task CreateUserAsync(UserFormData formData)
        {
            if (!ValidateUserForm(formData, true, out var validationMessage))
            {
                MessageBox.Show(validationMessage, "Invalid User Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await using var db = new AppDbContext();

            string normalizedEmail = formData.Email.Trim().ToLowerInvariant();
            bool exists = await db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);

            if (exists)
            {
                MessageBox.Show("Email already exists.", "Add Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new Models.User
            {
                FullName = formData.FullName.Trim(),
                Email = normalizedEmail,
                MobileNumber = formData.MobileNumber.Trim(),
                City = formData.City.Trim(),
                PasswordHash = PasswordHasher.Hash(formData.Password.Trim()),
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            var newRow = new AdminUserRow
            {
                Id = newUser.Id,
                FullName = newUser.FullName,
                Email = newUser.Email,
                MobileNumber = newUser.MobileNumber,
                City = newUser.City,
                Role = formData.Role,
                Status = formData.Status,
                CreatedAt = newUser.CreatedAt,
                LastActivityLabel = "Just added"
            };

            _users.Insert(0, newRow);
            SelectedUser = newRow;

            TotalUsers = _users.Count;
            UserTableSummary = $"Added account for {newRow.FullName}.";
            _usersView.Refresh();
        }

        private async void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser is null)
            {
                MessageBox.Show("Select a user to edit first.", "Manage Users", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await EditSelectedUserAsync(SelectedUser);
        }

        private void DeactivateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser is null)
            {
                MessageBox.Show("Select a user to deactivate first.", "Manage Users", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ToggleStatus(SelectedUser);
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedUser is null)
            {
                MessageBox.Show("Select a user to delete first.", "Manage Users", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await DeleteUserAsync(SelectedUser);
        }

        private async void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: AdminUserRow row })
            {
                SelectedUser = row;
                await EditSelectedUserAsync(row);
            }
        }

        private void DeactivateRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: AdminUserRow row })
            {
                SelectedUser = row;
                ToggleStatus(row);
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: AdminUserRow row })
            {
                SelectedUser = row;
                await DeleteUserAsync(row);
            }
        }

        private async Task EditSelectedUserAsync(AdminUserRow row)
        {
            var formData = new UserFormData
            {
                FullName = row.FullName,
                Email = row.Email,
                MobileNumber = row.MobileNumber,
                City = row.City,
                Role = row.Role,
                Status = row.Status
            };

            var form = new UserAccountFormWindow("Edit Account", formData, true);
            if (form.ShowDialog() != true)
                return;

            if (!ValidateUserForm(form.FormData, false, out var validationMessage))
            {
                MessageBox.Show(validationMessage, "Invalid User Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await using var db = new AppDbContext();
            string normalizedEmail = form.FormData.Email.Trim().ToLowerInvariant();
            bool takenByOther = await db.Users.AnyAsync(u => u.Id != row.Id && u.Email.ToLower() == normalizedEmail);

            if (takenByOther)
            {
                MessageBox.Show("Email is already used by another account.", "Edit Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Id == row.Id);
            if (dbUser is null)
            {
                MessageBox.Show("User no longer exists in database.", "Edit Account", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dbUser.FullName = form.FormData.FullName.Trim();
            dbUser.Email = normalizedEmail;
            dbUser.MobileNumber = form.FormData.MobileNumber.Trim();
            dbUser.City = form.FormData.City.Trim();
            await db.SaveChangesAsync();

            row.FullName = dbUser.FullName;
            row.Email = dbUser.Email;
            row.MobileNumber = dbUser.MobileNumber;
            row.City = dbUser.City;
            row.Role = form.FormData.Role;
            row.Status = form.FormData.Status;
            row.LastActivityLabel = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            UserTableSummary = $"Updated account details for {row.FullName}.";
            _usersView.Refresh();
        }

        private static bool ValidateUserForm(UserFormData formData, bool requirePassword, out string message)
        {
            if (string.IsNullOrWhiteSpace(formData.FullName)
                || string.IsNullOrWhiteSpace(formData.Email)
                || string.IsNullOrWhiteSpace(formData.MobileNumber)
                || string.IsNullOrWhiteSpace(formData.City)
                || string.IsNullOrWhiteSpace(formData.Role)
                || string.IsNullOrWhiteSpace(formData.Status))
            {
                message = "All fields are required.";
                return false;
            }

            if (!formData.Email.Contains('@') || !formData.Email.Contains('.'))
            {
                message = "Please enter a valid email address.";
                return false;
            }

            if (formData.MobileNumber.Trim().Length < 7)
            {
                message = "Please enter a valid mobile number.";
                return false;
            }

            if (requirePassword)
            {
                if (string.IsNullOrWhiteSpace(formData.Password))
                {
                    message = "Password is required for new accounts.";
                    return false;
                }

                if (!IsStrongPassword(formData.Password.Trim()))
                {
                    message = "Password must be 8+ chars with uppercase, lowercase, number and special symbol.";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        private static bool IsStrongPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasNumber = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            return hasUpper && hasLower && hasNumber && hasSpecial;
        }

        private void ToggleStatus(AdminUserRow row)
        {
            row.Status = row.Status == "Active" ? "Deactivated" : "Active";
            UserTableSummary = $"Updated status for {row.FullName} to {row.Status}.";
            _usersView.Refresh();
        }

        private async Task DeleteUserAsync(AdminUserRow row)
        {
            var confirm = MessageBox.Show($"Delete {row.FullName}?", "Delete User", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
                return;

            await using var db = new AppDbContext();
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Id == row.Id);
            if (dbUser is not null)
            {
                db.Users.Remove(dbUser);
                await db.SaveChangesAsync();
            }

            _users.Remove(row);
            TotalUsers = _users.Count;

            if (ReferenceEquals(SelectedUser, row))
                SelectedUser = null;

            UserTableSummary = $"Deleted user {row.FullName}.";
            _usersView.Refresh();
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
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Status { get; set; } = "Active";
            public DateTime CreatedAt { get; init; }
            public string LastActivityLabel { get; set; } = string.Empty;
        }

        private sealed class AdminListingRow
        {
            public int Id { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string SellerName { get; init; } = string.Empty;
            public string Category { get; init; } = string.Empty;
            public decimal PricePerKilo { get; init; }
            public int AvailableKilos { get; init; }
            public int SoldKilos { get; init; }
            public decimal Revenue { get; init; }
            public string Status { get; init; } = "Active";
            public DateTime CreatedAt { get; init; }
            public DateTime? LastOrderAt { get; init; }

            public Brush StatusBackground => Status switch
            {
                "Sold Out" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2")),
                "Low Stock" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"))
            };

            public Brush StatusForeground => Status switch
            {
                "Sold Out" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B")),
                "Low Stock" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"))
            };
        }
    }
}