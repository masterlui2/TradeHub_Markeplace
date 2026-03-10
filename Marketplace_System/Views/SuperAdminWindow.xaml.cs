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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowModule(_dashboardView, DashboardNavButton, "Dashboard", "Real-time marketplace statistics and alerts");
        }

        private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(_dashboardView, DashboardNavButton, "Dashboard", "Real-time marketplace statistics and alerts");
        }

        private void ManageUsersNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(_usersView, ManageUsersNavButton, "Manage Users", "Create, update, suspend, and remove users");
        }

        private void ManagePaymentsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ShowModule(_paymentsView, ManagePaymentsNavButton, "Manage Payments", "Transaction status and payment detail tracking");
        }

        private void ShowModule(UserControl module, Button selectedButton, string title, string subtitle)
        {
            ModuleHost.Content = module;
            ModuleTitleText.Text = title;
            ModuleSubtitleText.Text = subtitle;

            DashboardNavButton.Background = InactiveBrush;
            ManageUsersNavButton.Background = InactiveBrush;
            ManagePaymentsNavButton.Background = InactiveBrush;
            selectedButton.Background = ActiveBrush;
        }
    }
}