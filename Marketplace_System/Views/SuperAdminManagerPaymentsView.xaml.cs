using System.Collections.ObjectModel;
using System.Windows;
using Marketplace_System.Services;
using Marketplace_System.ViewModels;
using System.Threading.Tasks;
using System.Windows.Controls;  // for UserControl and Button
namespace Marketplace_System.Views
{
    public partial class SuperAdminManagePaymentsView : UserControl
    {
        private readonly SuperAdminService _service = new();
        public ObservableCollection<SuperAdminPaymentRow> Payments { get; } = new();

        public SuperAdminManagePaymentsView()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += async (_, _) => await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            Payments.Clear();
            var payments = await _service.GetPaymentsAsync();
            foreach (var payment in payments)
                Payments.Add(payment);
        }

        private void OnViewDetailsClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: SuperAdminPaymentRow row })
                return;

            MessageBox.Show(
                $"Reference: {row.ReferenceNumber}\nOrder: {row.OrderNumber}\nPayer: {row.PayerName}\nRecipient: {row.RecipientName}\nAmount: {row.Amount:C}\nMethod: {row.Method}\nStatus: {row.Status}\nNotes: {row.Notes}",
                "Payment Details",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}