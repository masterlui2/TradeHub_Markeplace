using System.Windows;
using System.Windows.Controls;

namespace Marketplace_System.Views
{
    public partial class SellerSidebarView : UserControl
    {
        public SellerSidebarView()
        {
            InitializeComponent();
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs e)
        {
            Window? ownerWindow = Window.GetWindow(this);
            CreateListingModal modal = new CreateListingModal();

            if (ownerWindow is not null)
            {
                modal.Owner = ownerWindow;
            }

            modal.ShowDialog();
        }
    }
}