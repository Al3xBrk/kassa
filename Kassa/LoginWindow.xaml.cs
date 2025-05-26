using Kassa.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace Kassa
{
    public partial class LoginWindow : Window
    {
        private readonly UserRole _role;
        public User? AuthenticatedUser { get; private set; }

        public LoginWindow(UserRole role)
        {
            InitializeComponent();
            _role = role;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
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
                var user = db.Users.AsNoTracking().FirstOrDefault(u => u.FullName == fullName && u.Role == _role);
                if (user == null || user.PasswordHash != HashPassword(password))
                {
                    ErrorTextBlock.Text = "Неверные ФИО или пароль.";
                    return;
                }
                AuthenticatedUser = user;
                DialogResult = true;
                Close();
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                // Возвращаем hex-строку, чтобы совпадало с тем, что в базе
                var sb = new StringBuilder();
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
