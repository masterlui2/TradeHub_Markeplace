using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class SellerCreateListingsView : UserControl
    {
        public ObservableCollection<SellerListingRow> Listings { get; } = new();

        public SellerCreateListingsView()
        {
            InitializeComponent();
            ListingsDataGrid.ItemsSource = Listings;
            LoadListings();
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs e)
        {
            CreateListingModal modal = new() { Owner = Window.GetWindow(this) };
            if (modal.ShowDialog() == true)
            {
                LoadListings();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int listingId })
            {
                return;
            }

            using AppDbContext dbContext = new();
            ProductListing? listing = dbContext.ProductListings
                .FirstOrDefault(p => p.Id == listingId && p.SellerUserId == SessionManager.CurrentUserId);

            if (listing is null)
            {
                MessageBox.Show("Listing was not found.", "Edit Listing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreateListingModal modal = new(listing) { Owner = Window.GetWindow(this) };
            if (modal.ShowDialog() == true)
            {
                LoadListings();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int listingId })
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this listing?",
                "Delete Listing",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            using AppDbContext dbContext = new();
            ProductListing? listing = dbContext.ProductListings
                .FirstOrDefault(p => p.Id == listingId && p.SellerUserId == SessionManager.CurrentUserId);

            if (listing is null)
            {
                MessageBox.Show("Listing was not found.", "Delete Listing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dbContext.ProductListings.Remove(listing);
            dbContext.SaveChanges();
            LoadListings();
        }

        private void LoadListings()
        {
            Listings.Clear();

            using AppDbContext dbContext = new();
            var listings = dbContext.ProductListings
                .Where(p => p.SellerUserId == SessionManager.CurrentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            foreach (ProductListing listing in listings)
            {
                Listings.Add(new SellerListingRow
                {
                    Id = listing.Id,
                    ProductName = listing.ProductName,
                    Category = listing.Category,
                    PriceText = $"₱{listing.PricePerKilo:N2}",
                    StockText = $"{listing.AvailableKilos} kg",
                    UpdatedText = listing.CreatedAt.ToLocalTime().ToString("MMM dd, yyyy")
                });
            }

            bool hasListings = Listings.Count > 0;
            ListingsDataGrid.Visibility = hasListings ? Visibility.Visible : Visibility.Collapsed;
            EmptyStateTextBlock.Visibility = hasListings ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public sealed class SellerListingRow
    {
        public int Id { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string PriceText { get; init; } = string.Empty;
        public string StockText { get; init; } = string.Empty;
        public string UpdatedText { get; init; } = string.Empty;
    }
}