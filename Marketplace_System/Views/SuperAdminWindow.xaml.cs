using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Marketplace_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        private readonly SuperAdminDashboardView _dashboardView = new();
        private readonly SuperAdminManageUsersView _usersView = new();
        private readonly SuperAdminManagePaymentsView _paymentsView = new();

        private static readonly Brush ActiveBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
        private static readonly Brush InactiveBrush = Brushes.White;

        public SuperAdminWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ShowModuleAsync(_dashboardView, DashboardNavButton, "Dashboard", "Real-time marketplace statistics and alerts");
        }

        private async void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowModuleAsync(_dashboardView, DashboardNavButton, "Dashboard", "Real-time marketplace statistics and alerts");
        }

        private async void ManageUsersNavButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowModuleAsync(_usersView, ManageUsersNavButton, "Manage Users", "Create, update, suspend, and remove users");
        }

        private async void ManagePaymentsNavButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowModuleAsync(_paymentsView, ManagePaymentsNavButton, "Manage Payments", "Transaction status and payment detail tracking");
        }

        private async Task ShowModuleAsync(UserControl module, Button selectedButton, string title, string subtitle)
        {
            try
            {
                switch (module)
                {
                    case SuperAdminDashboardView dashboardView:
                        await dashboardView.RefreshDataAsync();
                        break;
                    case SuperAdminManageUsersView usersView:
                        await usersView.RefreshDataAsync();
                        break;
                    case SuperAdminManagePaymentsView paymentsView:
                        await paymentsView.RefreshDataAsync();
                        break;
                }

                ModuleHost.Content = module;
                ModuleTitleText.Text = title;
                ModuleSubtitleText.Text = subtitle;

                DashboardNavButton.Background = InactiveBrush;
                ManageUsersNavButton.Background = InactiveBrush;
                ManagePaymentsNavButton.Background = InactiveBrush;
                selectedButton.Background = ActiveBrush;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to load {title.ToLowerInvariant()} module.\n\n{ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}