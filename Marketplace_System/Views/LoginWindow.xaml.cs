using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Marketplace_System.Services;

namespace Marketplace_System.Views
{
    public partial class LoginWindow : Window
    {
        private bool _isPasswordVisible = false;
        private readonly AuthService _authService = new();
              private readonly AdminCredentialService _adminCredentialService = new();
        public LoginWindow()
        {
            InitializeComponent();
            MouseLeftButtonDown += (s, e) => DragMove();
            btnShowPassword.Click += btnShowPassword_Click;
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password!",
                    "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SessionManager.Clear();
                var adminRole = _adminCredentialService.Validate(username, password);
                if (adminRole == AdminRole.SuperAdmin)
                {
                    SessionManager.SetCurrentUser(-2, "Super Administrator");
                    MessageBox.Show("Super Admin login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    SuperAdminWindow superAdminWindow = new();
                    superAdminWindow.Show();
                    Close();
                    return;
                }

                if (adminRole == AdminRole.Admin)
                {
                    SessionManager.SetCurrentUser(-1, "Administrator");
                    MessageBox.Show("Admin login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    AdminPanelWindow adminPanelWindow = new();
                    adminPanelWindow.Show();
                    Close();
                    return;
                }
                var user = await _authService.LoginAsync(username, password);
                if (user is null)
                {
                    MessageBox.Show("Invalid username/email or password!",
                        "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SessionManager.SetCurrentUser(user.Id, user.FullName);

                MessageBox.Show("Login Successful!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow mainWindow = new(user.FullName);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to login right now.\n\n{ex.Message}",
                    "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void linkRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
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

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                var visiblePasswordTextBox = new TextBox
                {
                    Text = txtPassword.Password,
                    FontSize = txtPassword.FontSize,
                    Padding = new Thickness(10, 12, 10, 12),
                    BorderThickness = new Thickness(0),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                var parent = txtPassword.Parent as Grid;
                if (parent != null)
                {
                    parent.Children.Remove(txtPassword);
                    Grid.SetColumn(visiblePasswordTextBox, 1);
                    parent.Children.Add(visiblePasswordTextBox);
                    parent.Tag = txtPassword;
                    ((Button)sender).Content = new TextBlock { Text = "🔒", FontSize = 14 };
                }
            }
            else
            {
                var parent = txtPassword.Parent as Grid;
                if (parent != null && parent.Tag is PasswordBox originalPasswordBox)
                {
                    TextBox? visiblePasswordTextBox = null;
                    foreach (var child in parent.Children)
                    {
                        if (child is TextBox textBox && Grid.GetColumn(textBox) == 1)
                        {
                            visiblePasswordTextBox = textBox;
                            break;
                        }
                    }

                    if (visiblePasswordTextBox != null)
                    {
                        originalPasswordBox.Password = visiblePasswordTextBox.Text;
                        parent.Children.Remove(visiblePasswordTextBox);
                        parent.Children.Add(originalPasswordBox);
                        ((Button)sender).Content = new TextBlock { Text = "👁️", FontSize = 14 };
                    }
                }
            }
        }
    }
}