using System.Windows;
using System.Windows.Controls;

namespace Marketplace_System.Views
{
    public partial class SellerDashboardView : UserControl
    {
        public SellerDashboardView()
        {
            InitializeComponent();
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs e)
        {
            Window? owner = Window.GetWindow(this);
            CreateListingModal modal = new CreateListingModal
            {
                Owner = owner
            };

            modal.ShowDialog();
        }
    }
}
