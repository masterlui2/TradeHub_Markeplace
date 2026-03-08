using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Marketplace_System.Views
{
    public partial class AdminPanelWindow : Window
    {
        public AdminPanelWindow()
        {
            InitializeComponent();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDashboard();
            UpdateActiveButton(DashboardButton);
        }

        private void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Users Management View
            PageTitleText.Text = "Manage Users";
            PageSubtitleText.Text = "Manage buyer and seller accounts";
            ShowModuleContent();
            UpdateActiveButton(ManageUsersButton);
        }

        private void UserGridButton_Click(object sender, RoutedEventArgs e)
        {
            // Show User Grid View
            PageTitleText.Text = "User Grid View";
            PageSubtitleText.Text = "Search and filter all users";
            ShowModuleContent();
            UpdateActiveButton(UserGridButton);
        }

        private void ManageListingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Product Listings View
            PageTitleText.Text = "Product Listings";
            PageSubtitleText.Text = "Review and manage product listings";
            ShowModuleContent();
            UpdateActiveButton(ManageListingsButton);
        }

        private void SalesDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Sales Dashboard View
            PageTitleText.Text = "Sales Dashboard";
            PageSubtitleText.Text = "Monitor sales and performance metrics";
            ShowModuleContent();
        }

        private void ManagePaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Payment Records View
            PageTitleText.Text = "Payment Records";
            PageSubtitleText.Text = "Review payment transactions and settlements";
            ShowModuleContent();
            UpdateActiveButton(ManagePaymentsButton);
        }

        private void SendMessagesButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Send Messages View
            PageTitleText.Text = "Send Messages";
            PageSubtitleText.Text = "Send announcements to users";
            ShowModuleContent();
            UpdateActiveButton(SendMessagesButton);
        }

        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            // Show Activity Log View
            PageTitleText.Text = "Activity Log";
            PageSubtitleText.Text = "Track administrative actions";
            ShowModuleContent();
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
                // Clear session if needed
                // SessionManager.Clear();

                // Open login window
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Close admin panel
                Close();
            }
        }

        private void ShowDashboard()
        {
            PageTitleText.Text = "Dashboard";
            PageSubtitleText.Text = "Welcome back, Admin";
            DashboardGrid.Visibility = Visibility.Visible;
            ModuleContentGrid.Visibility = Visibility.Collapsed;
        }

        private void ShowModuleContent()
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ModuleContentGrid.Visibility = Visibility.Visible;

            // Here you would load the appropriate UserControl
            // Example: ModuleContentGrid.Children.Clear();
            // ModuleContentGrid.Children.Add(new ManageUsersView());
        }

        private void UpdateActiveButton(Button activeButton)
        {
            // Reset all buttons to default style
            Button[] buttons = {
                DashboardButton, ManageUsersButton, UserGridButton,
                ManageListingsButton, ManagePaymentsButton,
                SendMessagesButton, ActivityLogButton
            };

            foreach (var button in buttons)
            {
                button.Background = System.Windows.Media.Brushes.Transparent;
            }

            // Set active button
            if (activeButton != null)
            {
                activeButton.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2D3F4B"));
            }
        }

        // Handle mouse events for module cards
        private void ModuleCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                // You can identify which module was clicked and navigate accordingly
                // This will be triggered when clicking on module cards
            }
        }
    }
}