using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class SellerSalesInsightsView : UserControl
    {
        public SellerSalesInsightsView()
        {
            InitializeComponent();
            Loaded += SellerSalesInsightsView_Loaded;
        }

        private async void SellerSalesInsightsView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadInsights();
        }

        private async Task LoadInsights()
        {
            SetLoadingState(true);

            try
            {
                using CancellationTokenSource timeoutCts = new(TimeSpan.FromSeconds(10));
                using AppDbContext dbContext = new();

                var sellerOrders = await dbContext.Orders
                    .Where(order => order.SellerUserId == SessionManager.CurrentUserId)
                    .ToListAsync(timeoutCts.Token);

                int totalOrders = sellerOrders.Count;
                var completedOrders = sellerOrders
                    .Where(order => order.Status == Order.StatusCompleted)
                    .ToList();

                decimal totalRevenue = completedOrders.Sum(order => order.UnitPrice * order.QuantityKilos);
                double completionRate = totalOrders == 0
                    ? 0
                    : (double)completedOrders.Count / totalOrders * 100;

                TotalRevenueTextBlock.Text = $"₱{totalRevenue:N2}";
                TotalOrdersTextBlock.Text = totalOrders.ToString();
                CompletionRateTextBlock.Text = $"{completionRate:N0}%";

                var topProduct = completedOrders
                    .GroupBy(order => order.ProductName)
                    .Select(group => new
                    {
                        ProductName = group.Key,
                        KilosSold = group.Sum(order => order.QuantityKilos),
                        Revenue = group.Sum(order => order.UnitPrice * order.QuantityKilos)
                    })
                    .OrderByDescending(item => item.Revenue)
                    .FirstOrDefault();

                if (topProduct is null)
                {
                    TopProductNameTextBlock.Text = "No completed orders yet";
                    TopProductDetailsTextBlock.Text = "Once orders are completed, your best seller appears here.";
                }
                else
                {
                    TopProductNameTextBlock.Text = topProduct.ProductName;
                    TopProductDetailsTextBlock.Text = $"{topProduct.KilosSold} kg sold • ₱{topProduct.Revenue:N2} revenue";
                }

                var bestDay = completedOrders
                    .GroupBy(order => order.CompletedAt?.Date ?? order.UpdatedAt.Date)
                    .Select(group => new
                    {
                        Day = group.Key,
                        Orders = group.Count(),
                        Revenue = group.Sum(order => order.UnitPrice * order.QuantityKilos)
                    })
                    .OrderByDescending(item => item.Revenue)
                    .FirstOrDefault();

                if (bestDay is null)
                {
                    BestDayTextBlock.Text = "No sales day yet";
                    BestDayDetailsTextBlock.Text = "We'll highlight your strongest day by revenue.";
                }
                else
                {
                    BestDayTextBlock.Text = bestDay.Day.ToString("dddd");
                    BestDayDetailsTextBlock.Text =
                        $"{bestDay.Day:MMM dd, yyyy} • ₱{bestDay.Revenue:N2} across {bestDay.Orders} completed order(s)";
                }

                InsightsStatusTextBlock.Visibility = Visibility.Collapsed;
            }
            catch (OperationCanceledException)
            {
                ApplyFallbackInsights("Loading timed out. Showing placeholders.");
            }
            catch
            {
                ApplyFallbackInsights("Unable to load insights right now.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            InsightsRefreshButton.IsEnabled = !isLoading;
            InsightsStatusTextBlock.Text = isLoading ? "Loading..." : InsightsStatusTextBlock.Text;
            InsightsStatusTextBlock.Visibility = Visibility.Visible;
        }

        private void ApplyFallbackInsights(string statusMessage)
        {
            TotalRevenueTextBlock.Text = "₱0.00";
            TotalOrdersTextBlock.Text = "0";
            CompletionRateTextBlock.Text = "0%";
            TopProductNameTextBlock.Text = "No completed orders yet";
            TopProductDetailsTextBlock.Text = "Once orders are completed, your best seller appears here.";
            BestDayTextBlock.Text = "No sales day yet";
            BestDayDetailsTextBlock.Text = "We'll highlight your strongest day by revenue.";
            InsightsStatusTextBlock.Text = statusMessage;
            InsightsStatusTextBlock.Visibility = Visibility.Visible;
        }

        private async void InsightsRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadInsights();
        }
    }
}