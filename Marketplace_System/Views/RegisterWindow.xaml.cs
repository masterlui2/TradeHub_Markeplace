using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService = new();

        public RegisterWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string mobile = txtMobile.Text.Trim();
            string city = txtCity.Text.Trim();
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(mobile) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(
                    "Please complete all required fields.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains('@'))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show(
                    "Password and confirm password do not match.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!IsStrongPassword(password))
            {
                MessageBox.Show(
                    "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = await _authService.RegisterAsync(name, email, mobile, city, password);
                if (!result.Success)
                {
                    MessageBox.Show(result.Message, "Registration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                MessageBox.Show(
                    "Account created successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoginWindow loginWindow = new();
                loginWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to register right now.\n\n{ex.Message}",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
            Application.Current.Shutdown();
        }
        private static bool IsStrongPassword(string password)
        {
            if (password.Length < 8)
            {
                return false;
            }

            bool hasUppercase = Regex.IsMatch(password, "[A-Z]");
            bool hasLowercase = Regex.IsMatch(password, "[a-z]");
            bool hasDigit = Regex.IsMatch(password, "\\d");
            bool hasSpecialCharacter = Regex.IsMatch(password, "[^a-zA-Z0-9]");

            return hasUppercase && hasLowercase && hasDigit && hasSpecialCharacter;
        }
    }
}