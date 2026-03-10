using System.Windows;

using System.Windows;

namespace Marketplace_System.Views
{
    public partial class TermsConditionsWindow : Window
    {
        public TermsConditionsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}