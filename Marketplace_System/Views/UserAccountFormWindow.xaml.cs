using System.Windows;
using System.Windows.Controls;

namespace Marketplace_System.Views
{
    public partial class UserAccountFormWindow : Window
    {
        private readonly bool _isEditMode;
        public UserFormData FormData { get; private set; } = new();

        public UserAccountFormWindow(string title, UserFormData? data = null, bool isEditMode = false)
        {
            InitializeComponent();
            _isEditMode = isEditMode;
            FormTitleText.Text = title;

            if (data is not null)
            {
                FullNameTextBox.Text = data.FullName;
                EmailTextBox.Text = data.Email;
                MobileTextBox.Text = data.MobileNumber;
                CityTextBox.Text = data.City;
           
            }

            if (_isEditMode)
            {
                PasswordLabelText.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Collapsed;
                HelperText.Text = "Edit mode shows all account details except password for security.";
            }
            else
            {
                HelperText.Text = "Use a strong password: 8+ chars with uppercase, lowercase, number, and symbol.";
            }
        }

        private static void SelectComboItem(ComboBox comboBox, string target)
        {
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem { Content: string text } && text == target)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FormData = new UserFormData
            {
                FullName = FullNameTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                MobileNumber = MobileTextBox.Text.Trim(),
                City = CityTextBox.Text.Trim(),
                Password = _isEditMode ? string.Empty : PasswordBox.Password
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

    public sealed class UserFormData
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Role { get; set; } = "Buyer";
        public string Status { get; set; } = "Active";
        public string Password { get; set; } = string.Empty;
    }
}
