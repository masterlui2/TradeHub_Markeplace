using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Marketplace_System.Views;

namespace Marketplace_System
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Brush InactiveForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F4E3D"));
        private static readonly Brush ActiveForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D5B"));
        private static readonly Brush ActiveBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6F4EC"));

        private object? _browseProductsContent;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search products")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search products";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _browseProductsContent = MainContentHost.Content;
            ActivateBrowseProductsView();
        }

        private void ActivateBrowseProductsView()
        {
            if (_browseProductsContent is not null)
            {
                MainContentHost.Content = _browseProductsContent;
            }

            SetActiveNav("browse");
        }

        private void SetActiveNav(string activeItem)
        {
            List<Button> topButtons = new()
            {
                TopHomeNavButton,
                TopOrdersNavButton,
                TopCartNavButton,
                TopInboxNavButton
            };

            foreach (Button button in topButtons)
            {
                button.Background = Brushes.Transparent;
                button.Foreground = InactiveForeground;
            }

            List<Button> sidebarButtons = new()
            {
                BrowseProductsNavButton,
                MyOrdersNavButton,
                MyCartNavButton,
                InboxNavButton
            };

            foreach (Button button in sidebarButtons)
            {
                button.Background = Brushes.Transparent;
                button.Foreground = InactiveForeground;
            }

            switch (activeItem)
            {
                case "browse":
                    TopHomeNavButton.Background = ActiveBackground;
                    TopHomeNavButton.Foreground = ActiveForeground;
                    BrowseProductsNavButton.Background = ActiveBackground;
                    BrowseProductsNavButton.Foreground = ActiveForeground;
                    break;
                case "orders":
                    TopOrdersNavButton.Background = ActiveBackground;
                    TopOrdersNavButton.Foreground = ActiveForeground;
                    MyOrdersNavButton.Background = ActiveBackground;
                    MyOrdersNavButton.Foreground = ActiveForeground;
                    break;
                case "cart":
                    TopCartNavButton.Background = ActiveBackground;
                    TopCartNavButton.Foreground = ActiveForeground;
                    MyCartNavButton.Background = ActiveBackground;
                    MyCartNavButton.Foreground = ActiveForeground;
                    break;
                case "inbox":
                    TopInboxNavButton.Background = ActiveBackground;
                    TopInboxNavButton.Foreground = ActiveForeground;
                    InboxNavButton.Background = ActiveBackground;
                    InboxNavButton.Foreground = ActiveForeground;
                    break;
            }
        }

        private void SellingNavButton_Click(object sender, RoutedEventArgs e)
        {
            SidebarHost.Content = new SellerSidebarView();
            MainContentHost.Content = new SellerDashboardView();
        }

        private void BrowseProductsNavButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBrowseProductsView();
        }

        private void LogoButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBrowseProductsView();
        }

        private void TopHomeNavButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBrowseProductsView();
        }

        private void TopOrdersNavButton_Click(object sender, RoutedEventArgs e)
        {
            MyOrdersNavButton_Click(sender, e);
        }

        private void TopCartNavButton_Click(object sender, RoutedEventArgs e)
        {
            MyCartNavButton_Click(sender, e);
        }

        private void TopInboxNavButton_Click(object sender, RoutedEventArgs e)
        {
            InboxNavButton_Click(sender, e);
        }

        private void InboxNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new InboxPanelView();
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
   

private void MyOrdersNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new MyOrdersPanelView();
            SetActiveNav("orders");
        }

        private void MyCartNavButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentHost.Content = new MyCartPanelView();
            SetActiveNav("cart");
        }


        private void CreateListingNavButton_Click(object sender, RoutedEventArgs e)
        {
            CreateListingModal modal = new CreateListingModal
            {
                Owner = this
            };

            modal.ShowDialog();
        }

        
        
    }
}