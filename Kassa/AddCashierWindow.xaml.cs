using Kassa.Models;
using System.Linq;
using System.Windows;

namespace Kassa
{
    public partial class AddCashierWindow : Window
    {
        public AddCashierWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password))
            {
                ErrorTextBlock.Text = "Введите ФИО и пароль.";
                return;
            }
            using (var db = new KassaContext())
            {
                if (db.Users.Any(u => u.FullName == fullName))
                {
                    ErrorTextBlock.Text = "Пользователь с таким ФИО уже существует.";
                    return;
                }
                var user = new User
                {
                    FullName = fullName,
                    Role = UserRole.Cashier,
                    PasswordHash = LoginWindow.HashPassword(password)
                };
                db.Users.Add(user);
                db.SaveChanges();
            }
            DialogResult = true;
            Close();
        }
    }
}
