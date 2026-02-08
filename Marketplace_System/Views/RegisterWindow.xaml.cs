using System.Windows;

namespace Marketplace_System.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password)
                || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Please complete all fields before registering.",
                    "Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.Equals(password, confirmPassword, System.StringComparison.Ordinal))
            {
                MessageBox.Show("Passwords do not match. Please re-enter them.",
                    "Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
        }

        private void linkLogin_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
