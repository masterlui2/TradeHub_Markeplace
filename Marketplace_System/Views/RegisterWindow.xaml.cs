using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Marketplace_System.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();

            // Allow dragging the borderless window
            MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();            string password = txtPassword.Password;
            string city = txtCity.Text.Trim();

            // Basic validation
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(city))
            {
                MessageBox.Show(
                    "Please complete all required fields.",
                    "Registration Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

         

            // ✅ STEP 1 SUCCESS
            MessageBox.Show(
                "Step 1 completed successfully!\nProceeding to the next step.",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // TODO:
            // Navigate to Step 2 window
            // var step2 = new RegisterStep2Window();
            // step2.Show();
            // Close();
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
    }
}
