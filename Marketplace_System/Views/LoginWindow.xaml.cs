using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Marketplace_System.Views
{
    public partial class LoginWindow : Window
    {
        // Variable to track password visibility
        private bool _isPasswordVisible = false;

        public LoginWindow()
        {
            InitializeComponent();

            // Add event handler for window dragging
            this.MouseLeftButtonDown += (s, e) => this.DragMove();

            // Initialize password visibility button
            btnShowPassword.Click += btnShowPassword_Click;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Simple validation for now
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password!",
                    "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Hardcoded login for testing (we'll connect to database later)
            if (username == "admin" && password == "admin123")
            {
                MessageBox.Show("Login Successful!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Open main window
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password!",
                    "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void linkRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            Close();
        }

        // New event handlers for the modern design

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                // Show the password in a TextBox
                var visiblePasswordTextBox = new TextBox
                {
                    Text = txtPassword.Password,
                    FontSize = txtPassword.FontSize,
                    Padding = new Thickness(10, 12, 10, 12),
                    BorderThickness = new Thickness(0),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Foreground = System.Windows.Media.Brushes.Black
                };

                // Replace the PasswordBox with the TextBox
                var parent = txtPassword.Parent as Grid;
                if (parent != null)
                {
                    // Remove the PasswordBox
                    parent.Children.Remove(txtPassword);

                    // Add the TextBox in the same position
                    Grid.SetColumn(visiblePasswordTextBox, 1);
                    parent.Children.Add(visiblePasswordTextBox);

                    // Store the TextBox reference in a tag or variable for switching back
                    // We'll store the original PasswordBox in the Tag of the parent Grid
                    parent.Tag = txtPassword;

                    // Update the button text/icon
                    ((Button)sender).Content = new TextBlock { Text = "🔒", FontSize = 14 };
                }
            }
            else
            {
                // Switch back to PasswordBox
                var parent = txtPassword.Parent as Grid;
                if (parent != null && parent.Tag is PasswordBox originalPasswordBox)
                {
                    // Find the TextBox that replaced the PasswordBox
                    TextBox visiblePasswordTextBox = null;
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
                        // Update the password in the original PasswordBox
                        originalPasswordBox.Password = visiblePasswordTextBox.Text;

                        // Remove the TextBox
                        parent.Children.Remove(visiblePasswordTextBox);

                        // Add the original PasswordBox back
                        parent.Children.Add(originalPasswordBox);

                        // Update the button text/icon
                        ((Button)sender).Content = new TextBlock { Text = "👁️", FontSize = 14 };
                    }
                }
            }
        }

       
    }
}