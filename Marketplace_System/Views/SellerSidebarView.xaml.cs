using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Marketplace_System.Views
{
    public partial class SellerSidebarView : UserControl
    {
        private static readonly Brush ActiveBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6F4EC"));

        public SellerSidebarView()
        {
            InitializeComponent();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSellerButton(DashboardButton);
            ShowSellerSection("dashboard");
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSellerButton(CreateListingButton);
            ShowSellerSection("create-listing");
        }

        private void ManageOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSellerButton(ManageOrdersButton);
            ShowSellerSection("orders");
        }

        private void MessagesButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSellerButton(MessagesButton);
            ShowSellerSection("messages");
        }

        private void SalesInsightsButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveSellerButton(SalesInsightsButton);
            ShowSellerSection("insights");
        }

        private void SetActiveSellerButton(Button activeButton)
        {
            CreateListingButton.Background = Brushes.Transparent;
            ManageOrdersButton.Background = Brushes.Transparent;
            MessagesButton.Background = Brushes.Transparent;
            SalesInsightsButton.Background = Brushes.Transparent;
            DashboardButton.Background = Brushes.Transparent;
            activeButton.Background = ActiveBackground;
        }
        private void BrowseProductsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ReturnToBuyerBrowse();
            }
        }
        private void ReturnToBuyerButton_Click(object sender, RoutedEventArgs e)
        {
            BrowseProductsButton_Click(sender, e);
        }
        private void ShowSellerSection(string section)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.ShowSellerSection(section);
            }
        }
    }
}