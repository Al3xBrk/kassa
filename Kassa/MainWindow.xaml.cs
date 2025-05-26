using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Kassa.Models;

namespace Kassa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowLogin(UserRole role)
        {
            var loginWindow = new LoginWindow(role) { Owner = this };
            if (loginWindow.ShowDialog() == true)
            {
                var user = loginWindow.AuthenticatedUser;
                if (user == null) return;
                if (role == UserRole.Administrator)
                {
                    var adminWindow = new AdminWindow(user);
                    adminWindow.Show();
                }
                else
                {
                    var cashierWindow = new CashierWindow(user);
                    cashierWindow.Show();
                }
                this.Close();
            }
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLogin(UserRole.Administrator);
        }

        private void CashierButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLogin(UserRole.Cashier);
        }
    }
}