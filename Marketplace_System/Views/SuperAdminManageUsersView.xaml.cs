using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Models;
using Marketplace_System.Services;
using Marketplace_System.ViewModels;

namespace Marketplace_System.Views
{
    public partial class SuperAdminManageUsersView : UserControl
    {
        private readonly SuperAdminService _service = new();
        public ObservableCollection<SuperAdminUserRow> Users { get; } = new();

        public SuperAdminManageUsersView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (_, _) => await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            Users.Clear();
            var users = await _service.GetUsersAsync();
            foreach (var user in users)
                Users.Add(user);
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var form = new UserAccountFormWindow("Create User Account") { Owner = Window.GetWindow(this) };
            if (form.ShowDialog() != true)
                return;

            var data = form.FormData;
            await _service.CreateUserAsync(new User
            {
                FullName = data.FullName,
                Email = data.Email,
                MobileNumber = data.MobileNumber,
                City = data.City,
                PasswordHash = PasswordHasher.Hash(data.Password),
                CreatedAt = DateTime.UtcNow
            });
            await RefreshDataAsync();
        }

        private void ViewUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: SuperAdminUserRow row }) return;
            var profile = new UserProfileViewWindow(new AdminPanelWindow.AdminUserRow
            {
                Id = row.Id,
                FullName = row.FullName,
                Email = row.Email,
                MobileNumber = row.MobileNumber,
                City = row.City,
                Status = row.Status,
                Role = "User",
                CreatedAt = row.CreatedAt,
                LastActivityLabel = "N/A"
            })
            { Owner = Window.GetWindow(this) };
            profile.ShowDialog();
        }

        private async void EditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: SuperAdminUserRow row }) return;
            var form = new UserAccountFormWindow("Edit User Account", new UserFormData
            {
                FullName = row.FullName,
                Email = row.Email,
                MobileNumber = row.MobileNumber,
                City = row.City
            }, true)
            { Owner = Window.GetWindow(this) };

            if (form.ShowDialog() != true)
                return;

            var data = form.FormData;
            row.FullName = data.FullName;
            row.Email = data.Email;
            row.MobileNumber = data.MobileNumber;
            row.City = data.City;
            await _service.UpdateUserAsync(row);
            await RefreshDataAsync();
        }

        private async void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: SuperAdminUserRow row }) return;
            if (MessageBox.Show($"Delete {row.FullName}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            await _service.DeleteUserAsync(row.Id);
            await RefreshDataAsync();
        }

        private async void SuspendToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: SuperAdminUserRow row })
                return;

            var nextStatus = !row.IsSuspended;
            await _service.ToggleSuspendAsync(row.Id, nextStatus);
            await RefreshDataAsync();
        }
    }
}