using System;
using System.Windows;
using System.Windows.Controls;
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
            SidebarHost.Content = new SellerSidebarView();
            MainContentHost.Content = new SellerDashboardView();
        }

        private void InboxNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new InboxPanelView();
        }

        private void MyOrdersNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new MyOrdersPanelView();
        }

        private void MyCartNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new MyCartPanelView();
        }


        private void CreateListingNavButton_Click(object sender, RoutedEventArgs e)
        {
            CreateListingModal modal = new CreateListingModal
            {
                Owner = this
            };

            modal.ShowDialog();
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string rawDetails)
            {
                return;
            }

            string[] details = rawDetails.Split('|');
            if (details.Length < 4)
            {
                return;
            }

            AddToCartModal modal = new AddToCartModal(details[0], details[1], details[2], details[3])
            {
                Owner = this
            };

            modal.ShowDialog();
        }
    }
}
