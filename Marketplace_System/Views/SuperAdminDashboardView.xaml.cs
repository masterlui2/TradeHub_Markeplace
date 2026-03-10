using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Marketplace_System.Services;
using Marketplace_System.ViewModels;

namespace Marketplace_System.Views
{
    public partial class SuperAdminDashboardView : UserControl
    {
        private readonly SuperAdminService _service = new();

        public SuperAdminDashboardData? DashboardData { get; private set; }

        public SuperAdminDashboardView()
        {
            InitializeComponent();
            Loaded += SuperAdminDashboardView_Loaded;
        }

        private async void SuperAdminDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            var data = await _service.GetDashboardDataAsync();
            var maxRevenue = data.SalesTrends.Any() ? data.SalesTrends.Max(x => x.Revenue) : 1;
            data.SalesTrends = new(data.SalesTrends.Select(x => new SalesTrendPoint
            {
                Label = x.Label,
                Revenue = x.Revenue,
                Height = maxRevenue == 0 ? 4 : Math.Max(4, (double)(x.Revenue / maxRevenue) * 120)
            }));

            DashboardData = data;
            DataContext = DashboardData;
        }
    }
}