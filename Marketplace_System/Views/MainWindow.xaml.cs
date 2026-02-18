using System.Windows;
using Marketplace_System.Views;

namespace Marketplace_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SellingNavButton_Click(object sender, RoutedEventArgs e)
        {

            MainContentHost.Content = new SellerDashboardView();

        }
    }
}