using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;
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
        private object? _buyerSidebarContent;
        private readonly string _currentUserName;
        public ObservableCollection<BrowseProductCard> BrowseProducts { get; } = new();

        public MainWindow(string? currentUserName = null)
        {
            InitializeComponent();

            _currentUserName = string.IsNullOrWhiteSpace(currentUserName)
                ? SessionManager.CurrentUserFullName
                : currentUserName;

            DataContext = this;
            LoadBrowseProducts();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _browseProductsContent = MainContentHost.Content;
            _buyerSidebarContent = SidebarHost.Content;
            TopProfileNameTextBlock.Text = _currentUserName;

            ActivateBrowseProductsView();
            ActivateBuyerSidebar();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search products")
            {
                SearchTextBox.Text = string.Empty;
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

        private void ActivateBuyerSidebar()
        {
            if (_buyerSidebarContent is not null)
            {
                SidebarHost.Content = _buyerSidebarContent;
            }
        }

        private void ActivateBrowseProductsView()
        {
            ActivateBuyerSidebar();

            if (_browseProductsContent is not null)
            {
                MainContentHost.Content = _browseProductsContent;
            }
            LoadBrowseProducts();
            SetActiveNav("browse");
        }

        public void ReturnToBuyerBrowse() => ActivateBrowseProductsView();
        private void SetActiveNav(string activeItem)
        {
            List<Button> topButtons = new() { TopHomeNavButton, TopOrdersNavButton, TopCartNavButton, TopInboxNavButton };

            foreach (Button button in topButtons)
            {
                button.Background = Brushes.Transparent;
                button.Foreground = InactiveForeground;
            }

            List<Button> sidebarButtons = new() { BrowseProductsNavButton, MyOrdersNavButton, MyCartNavButton, InboxNavButton };

            foreach (Button button in sidebarButtons)
            {
                button.Background = Brushes.Transparent;
                button.Foreground = InactiveForeground;
            }

            // Activate selected
            switch (activeItem)
            {
                case "home":
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
            ShowSellerSection("dashboard");
        }

        public void ShowSellerSection(string section)
        {
            MainContentHost.Content = section switch
            {
                "orders" => new SellerManageOrdersView(),
                "messages" => new SellerMessagesView(),
                "insights" => new SellerSalesInsightsView(),
                "create-listing" => new SellerCreateListingsView(),
                _ => new SellerDashboardView()
            };
        }

        private void BrowseProductsNavButton_Click(object sender, RoutedEventArgs e) => ActivateBrowseProductsView();
        private void LogoButton_Click(object sender, RoutedEventArgs e) => ActivateBrowseProductsView();
        private void TopHomeNavButton_Click(object sender, RoutedEventArgs e) => ActivateBrowseProductsView();
        private void TopOrdersNavButton_Click(object sender, RoutedEventArgs e) => MyOrdersNavButton_Click(sender, e);
        private void TopCartNavButton_Click(object sender, RoutedEventArgs e) => MyCartNavButton_Click(sender, e);
        private void TopInboxNavButton_Click(object sender, RoutedEventArgs e) => InboxNavButton_Click(sender, e);

        private void InboxNavButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBuyerSidebar();
            MainContentHost.Content = new InboxPanelView();
            SetActiveNav("inbox");
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { DataContext: BrowseProductCard product })
            {
                return;
            }
            if (product.IsMyProduct)
            {
                MessageBox.Show("This is your own listing, so it can't be added to your cart.", "Own Product", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AddToCartModal modal = new(product)
            {
                Owner = this
            };

            bool? result = modal.ShowDialog();
            if (result == true)
            {
                MainContentHost.Content = new MyCartPanelView();
                SetActiveNav("cart");
            }
        }

        private void MyOrdersNavButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBuyerSidebar();
            MainContentHost.Content = new MyOrdersPanelView();
            SetActiveNav("orders");
        }

        private void MyCartNavButton_Click(object sender, RoutedEventArgs e)
        {
            ActivateBuyerSidebar();
            MainContentHost.Content = new MyCartPanelView();
            SetActiveNav("cart");
        }

        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            SessionManager.Clear();
            LoginWindow loginWindow = new();
            loginWindow.Show();
            Close();
        }

     

        private void LoadBrowseProducts()
        {
            BrowseProducts.Clear();

            List<ProductListing> listings;
            try
            {
                using AppDbContext dbContext = new();
                listings = dbContext.ProductListings
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(24)
                    .ToList();
            }
            catch
            {
                listings = new List<ProductListing>();
            }

            if (listings.Count == 0)
            {
                foreach (BrowseProductCard fallback in GetFallbackProducts())
                {
                    BrowseProducts.Add(fallback);
                }

                return;
            }

            foreach (ProductListing listing in listings)
            {
                bool isMyProduct = listing.SellerUserId != 0 && listing.SellerUserId == SessionManager.CurrentUserId;
                BrowseProducts.Add(new BrowseProductCard
                {
                    ProductId = listing.Id,
                    ProductName = listing.ProductName,
                    PricePerKilo = listing.PricePerKilo,
                    StockKilos = listing.AvailableKilos,
                    PriceText = $"₱{listing.PricePerKilo:N2} per kilo",
                    StockText = $"{listing.AvailableKilos} kilo(s) available • In Stock",
                    SellerId = listing.SellerUserId,
                    SellerName = string.IsNullOrWhiteSpace(listing.SellerName) ? "FarmHub Seller" : listing.SellerName,
                    SellerLocation = listing.PickupAddress,
                    ImagePath = ResolveListingImagePath(listing.ImagePath),
                    IsMyProduct = isMyProduct
                });
            }
        }

        private static string ResolveListingImagePath(string? imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                return imagePath;
            }

            return "/Images/new.png";
        }

        private static List<BrowseProductCard> GetFallbackProducts() => new()
        {
            new() { ProductId = 0, ProductName = "Fresh Carrots", PricePerKilo = 210m, StockKilos = 12, PriceText = "₱210 per kilo", StockText = "Approx. 12 pcs • In Stock", SellerId = 0, SellerName = "Juan Dela Cruz", SellerLocation = "Davao City", ImagePath = "/Images/carrot.jpg", IsMyProduct = false },
            new() { ProductId = 0, ProductName = "Fresh Asparagus", PricePerKilo = 210m, StockKilos = 12, PriceText = "₱210 per kilo", StockText = "Approx. 12 pcs • In Stock", SellerId = 0, SellerName = "Juan Dela Cruz", SellerLocation = "Davao City", ImagePath = "/Images/asp.jpg", IsMyProduct = false },
            new() { ProductId = 0, ProductName = "Fresh Potatoes", PricePerKilo = 210m, StockKilos = 12, PriceText = "₱210 per kilo", StockText = "Approx. 12 pcs • In Stock", SellerId = 0, SellerName = "Juan Dela Cruz", SellerLocation = "Davao City", ImagePath = "/Images/potato.jpg", IsMyProduct = false },
            new() { ProductId = 0, ProductName = "Fresh Beets", PricePerKilo = 210m, StockKilos = 12, PriceText = "₱210 per kilo", StockText = "Approx. 12 pcs • In Stock", SellerId = 0, SellerName = "Juan Dela Cruz", SellerLocation = "Davao City", ImagePath = "/Images/beets.jpg", IsMyProduct = false }
        };

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public sealed class BrowseProductCard
    {
        public int ProductId { get; init; }
        public int SellerId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal PricePerKilo { get; init; }
        public int StockKilos { get; init; }
        public string PriceText { get; init; } = string.Empty;
        public string StockText { get; init; } = string.Empty;
        public string SellerName { get; init; } = string.Empty;
        public string SellerLocation { get; init; } = string.Empty;
        public string ImagePath { get; init; } = "/Images/new.png";
        public bool IsMyProduct { get; init; }
        public string ActionButtonText => IsMyProduct ? "Own product" : "Add to Cart";
    }
}
