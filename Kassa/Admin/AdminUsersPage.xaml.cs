using Kassa.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kassa
{
    public partial class AdminUsersPage : Page
    {
        private ObservableCollection<User>? _users;
        public AdminUsersPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var db = new KassaContext())
            {
                _users = new ObservableCollection<User>(db.Users.AsNoTracking().ToList());
                UsersDataGrid.ItemsSource = _users;
            }
        }

        private void AddCashierButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddCashierWindow();
            if (dialog.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                if (user.Role == UserRole.Administrator)
                {
                    ModernMessageBox.ShowWarning("Нельзя удалить администратора.", "Ошибка");
                    return;
                }
                if (ModernMessageBox.ShowQuestion($"Удалить пользователя {user.FullName}?", "Подтверждение") == ModernMessageBoxResult.Yes)
                {
                    using (var db = new KassaContext())
                    {
                        var toDelete = db.Users.FirstOrDefault(u => u.Id == user.Id);
                        if (toDelete != null)
                        {
                            db.Users.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
        }

        private void EditAutoLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                var dialog = new SetAutoLogoutWindow(user.AutoLogoutMinutes);
                if (dialog.ShowDialog() == true)
                {
                    using (var db = new KassaContext())
                    {
                        var dbUser = db.Users.FirstOrDefault(u => u.Id == user.Id);
                        if (dbUser != null)
                        {
                            dbUser.AutoLogoutMinutes = dialog.SelectedMinutes;
                            db.SaveChanges();
                        }
                    }
                    LoadUsers();
                }
            }
        }
    }
}
