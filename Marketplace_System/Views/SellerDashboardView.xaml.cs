using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Data;
using Marketplace_System.Models;
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
            LoadMyListings();
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs e)
        {
            Window? owner = Window.GetWindow(this);
            CreateListingModal modal = new() { Owner = owner };
            modal.ShowDialog();
            LoadMyListings();
        }

        private void ViewListing_Click(object sender, RoutedEventArgs e)
        {
            ProductListing? listing = FindListingFromButton(sender);
            if (listing is null)
            {
                return;
            }

            MessageBox.Show(
                $"{listing.ProductName}\n\nPrice: ₱{listing.PricePerKilo:N2}/kilo\nStock: {listing.AvailableKilos} kilo(s)\nAddress: {listing.PickupAddress}",
                "Listing Details",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void EditListing_Click(object sender, RoutedEventArgs e)
        {
            ProductListing? listing = FindListingFromButton(sender);
            if (listing is null)
            {
                return;
            }

            Window? owner = Window.GetWindow(this);
            CreateListingModal modal = new(listing) { Owner = owner };
            modal.ShowDialog();
            LoadMyListings();
        }

        private void DeleteListing_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: int listingId })
            {
                return;
            }

            if (MessageBox.Show("Delete this listing?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using AppDbContext dbContext = new();
                ProductListing? listing = dbContext.ProductListings.FirstOrDefault(l => l.Id == listingId && l.SellerUserId == SessionManager.CurrentUserId);
                if (listing is null)
                {
                    return;
                }

                dbContext.ProductListings.Remove(listing);
                dbContext.SaveChanges();
            }
            catch
            {
                MessageBox.Show("Unable to delete listing right now.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadMyListings();
        }

        private void LoadMyListings()
        {
            List<SellerListingViewModel> listings = new();

            try
            {
                using AppDbContext dbContext = new();
                listings = dbContext.ProductListings
                    .Where(p => p.SellerUserId == SessionManager.CurrentUserId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new SellerListingViewModel
                    {
                        ListingId = p.Id,
                        ProductName = p.ProductName,
                        PickupAddress = p.PickupAddress,
                        MetaText = $"₱{p.PricePerKilo:N2}/kilo • {p.AvailableKilos} kilo(s)"
                    })
                    .ToList();
            }
            catch
            {
                // Keep empty if DB is unavailable.
            }

            ListingsControl.ItemsSource = listings;
            EmptyListingsTextBlock.Visibility = listings.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static ProductListing? FindListingFromButton(object sender)
        {
            if (sender is not Button { Tag: int listingId })
            {
                return null;
            }

            using AppDbContext dbContext = new();
            return dbContext.ProductListings.FirstOrDefault(l => l.Id == listingId && l.SellerUserId == SessionManager.CurrentUserId);
        }

        private sealed class SellerListingViewModel
        {
            public int ListingId { get; init; }
            public string ProductName { get; init; } = string.Empty;
            public string PickupAddress { get; init; } = string.Empty;
            public string MetaText { get; init; } = string.Empty;
        }
    }
}
