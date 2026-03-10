
using System;
using System.Windows;

namespace Marketplace_System.Views
{
    public partial class UserProfileViewWindow : Window
    {
        public UserProfileViewWindow(AdminPanelWindow.AdminUserRow user)
        {
            InitializeComponent();
            IdText.Text = user.Id.ToString();
            FullNameText.Text = user.FullName;
            EmailText.Text = user.Email;
            MobileText.Text = user.MobileNumber;
            CityText.Text = user.City;
            CreatedAtText.Text = user.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}