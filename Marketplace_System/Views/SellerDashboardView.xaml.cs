using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class SellerDashboardView : UserControl
    {
        public SellerDashboardView()
        {
            InitializeComponent();
            Loaded += SellerDashboardView_Loaded;
        }

        private void SellerDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            List<SellerProductCardViewModel> listings = new();
            string sellerName = "Seller";

            try
            {
                using AppDbContext dbContext = new();

                sellerName = dbContext.Users
                    .Where(u => u.Id == SessionManager.CurrentUserId)
                    .Select(u => u.FullName)
                    .FirstOrDefault() ?? "Seller";

                listings = dbContext.ProductListings
                    .Where(p => p.SellerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new SellerProductCardViewModel
                    {
                        ProductName = p.ProductName,
                        PickupAddress = p.PickupAddress,
                        PriceText = $"₱{p.PricePerKilo:N2} per kilo",
                        StockText = $"{p.AvailableKilos} kilo(s) available",
                        ImagePath = string.IsNullOrWhiteSpace(p.ImagePath) ? "/Images/new.png" : p.ImagePath,
                        StockKilos = p.AvailableKilos,
                        PricePerKilo = p.PricePerKilo
                    })
                    .ToList();
            }
            catch
            {
                // Keep empty state if DB is unavailable.
            }

            SellerInfoTextBlock.Text = $"Welcome, {sellerName}. Monitor your products and inventory below.";
            TotalProductsTextBlock.Text = listings.Count.ToString();
            TotalStockTextBlock.Text = $"{listings.Sum(l => l.StockKilos)} kg";
            AveragePriceTextBlock.Text = listings.Count == 0
                ? "₱0.00"
                : $"₱{listings.Average(l => l.PricePerKilo):N2}";

            ListingsControl.ItemsSource = listings;
            EmptyListingsTextBlock.Visibility = listings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private sealed class SellerProductCardViewModel
        {
            public string ProductName { get; init; } = string.Empty;
            public string PickupAddress { get; init; } = string.Empty;
            public string PriceText { get; init; } = string.Empty;
            public string StockText { get; init; } = string.Empty;
            public string ImagePath { get; init; } = "/Images/new.png";
            public int StockKilos { get; init; }
            public decimal PricePerKilo { get; init; }
        }
    }
}