using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Marketplace_System.Views
{
    public partial class PaymentFormWindow : Window
    {
        public PaymentFormData FormData { get; private set; } = new();

        public PaymentFormWindow(string title, PaymentFormData? formData = null)
        {
            InitializeComponent();
            FormTitleText.Text = title;

            if (formData is null)
            {
                MethodComboBox.SelectedIndex = 0;
                StatusComboBox.SelectedIndex = 0;
                return;
            }

            ReferenceTextBox.Text = formData.ReferenceNumber;
            OrderTextBox.Text = formData.OrderNumber;
            PayerTextBox.Text = formData.PayerName;
            RecipientTextBox.Text = formData.RecipientName;
            AmountTextBox.Text = formData.Amount.ToString("0.00", CultureInfo.InvariantCulture);
            SelectComboItem(MethodComboBox, formData.Method);
            SelectComboItem(StatusComboBox, formData.Status);
        }

        private static void SelectComboItem(ComboBox comboBox, string target)
        {
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem { Content: string value } && value == target)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AmountTextBox.Text.Trim(), out var amount))
            {
                MessageBox.Show("Please enter a valid amount.", "Payment", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            FormData = new PaymentFormData
            {
                ReferenceNumber = ReferenceTextBox.Text.Trim(),
                OrderNumber = OrderTextBox.Text.Trim(),
                PayerName = PayerTextBox.Text.Trim(),
                RecipientName = RecipientTextBox.Text.Trim(),
                Method = (MethodComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cash",
                Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pending",
                Amount = amount
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public sealed class PaymentFormData
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string PayerName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Method { get; set; } = "Cash";
        public string Status { get; set; } = "Pending";
        public decimal Amount { get; set; }
    }
}