using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Marketplace_System.Views
{
    public partial class SuperAdminWindow : Window
    {
        private static readonly Brush ActiveNavBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3F4B"));
        private static readonly Brush InactiveNavBrush = Brushes.Transparent;

        public SuperAdminWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowModule(ManageUsersModule, ManageUsersNavButton, "Manage Users", "View and manage system users.");
        }

        private void ManageUsersNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(ManageUsersModule, ManageUsersNavButton, "Manage Users", "View and manage system users.");
        }

        private void SalesDashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(SalesDashboardModule, SalesDashboardNavButton, "View Sales Dashboard", "Display an overview of sales data and analytics.");
        }

        private void ManagePaymentsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(ManagePaymentsModule, ManagePaymentsNavButton, "Manage Payments", "Monitor and manage payment transactions.");
        }

        private void ShowModule(Border module, Button navButton, string title, string subtitle)
        {
            ManageUsersModule.Visibility = Visibility.Collapsed;
            SalesDashboardModule.Visibility = Visibility.Collapsed;
            ManagePaymentsModule.Visibility = Visibility.Collapsed;

            ManageUsersNavButton.Background = InactiveNavBrush;
            SalesDashboardNavButton.Background = InactiveNavBrush;
            ManagePaymentsNavButton.Background = InactiveNavBrush;

            module.Visibility = Visibility.Visible;
            navButton.Background = ActiveNavBrush;

            ModuleTitleText.Text = title;
            ModuleSubtitleText.Text = subtitle;
        }
    }
}