using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LiveCharts;
using LiveCharts.Wpf;
using Kassa.Models;

namespace Kassa
{
    public partial class AdminWindow : Window
    {
        private KassaContext _context = new KassaContext();
        private string? selectedGroup = null;

        private System.Windows.Threading.DispatcherTimer _autoLogoutTimer;
        private DateTime _lastActivityTime;
        private int _autoLogoutMinutes = 10;
        private User? _currentUser;

        public AdminWindow(User? user = null)
        {
            InitializeComponent();
            _currentUser = user;
            AdminFullNameTextBlock.Text = _currentUser?.FullName ?? "";
            NavDishesButton.Click += NavDishesButton_Click;
            NavProfitButton.Click += NavProfitButton_Click;
            NavReservationButton.Click += NavReservationButton_Click;
            NavOrderHistoryButton.Click += NavOrderHistoryButton_Click;
            NavUsersButton.Click += NavUsersButton_Click;
            AdminContentFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            AdminContentFrame.Navigate(new AdminDishesPage());
            InitAutoLogout();
        }

        private void InitAutoLogout()
        {
            if (_currentUser != null)
                _autoLogoutMinutes = _currentUser.AutoLogoutMinutes;
            else
                _autoLogoutMinutes = 10;
            _lastActivityTime = DateTime.Now;
            _autoLogoutTimer = new System.Windows.Threading.DispatcherTimer();
            _autoLogoutTimer.Interval = TimeSpan.FromSeconds(1);
            _autoLogoutTimer.Tick += new EventHandler(AutoLogoutTimer_Tick);
            _autoLogoutTimer.Start();
            this.PreviewMouseMove -= ActivityDetected; // Отключено: движение мыши не сбрасывает таймер
            this.PreviewKeyDown += ActivityDetected;
            this.PreviewMouseDown += ActivityDetected; // Только клик мыши сбрасывает таймер
            UpdateLogoutTimerText();
        }

        private void ActivityDetected(object sender, EventArgs e)
        {
            _lastActivityTime = DateTime.Now;
            UpdateLogoutTimerText();
        }

        private void AutoLogoutTimer_Tick(object? sender, EventArgs e)
        {
            UpdateLogoutTimerText();
            if ((DateTime.Now - _lastActivityTime).TotalMinutes >= _autoLogoutMinutes)
            {
                _autoLogoutTimer.Stop();
                ModernMessageBox.ShowInformation("Время неактивности истекло. Возврат на главную страницу.", "Авто-выход");
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
        }

        private void UpdateLogoutTimerText()
        {
            if (LogoutTimerTextBlock != null)
            {
                var left = _autoLogoutMinutes * 60 - (DateTime.Now - _lastActivityTime).TotalSeconds;
                if (left < 0) left = 0;
                var min = (int)left / 60;
                var sec = (int)left % 60;
                LogoutTimerTextBlock.Text = $"{min:D2}:{sec:D2}";
            }
        }

        private void NavDishesButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminDishesPage());
            SetActiveNavButton(NavDishesButton);
        }

        private void NavProfitButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminProfitChartPage());
            SetActiveNavButton(NavProfitButton);
        }

        private void NavReservationButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminReservationPage());
            SetActiveNavButton(NavReservationButton);
        }
        private void NavOrderHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminOrderHistoryPage());
            SetActiveNavButton(NavOrderHistoryButton);
        }

        private void NavUsersButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminUsersPage());
            SetActiveNavButton(NavUsersButton);
        }

        private void NavHomeButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        public void NavigateToDishesPage()
        {
            AdminContentFrame.Navigate(new AdminDishesPage());
        }

        private void SetActiveNavButton(Button activeButton)
        {
            // Сброс стилей всех кнопок навигации
            var navButtons = new[] { NavHomeButton, NavDishesButton, NavProfitButton,
                                   NavReservationButton, NavOrderHistoryButton, NavUsersButton };

            foreach (var button in navButtons)
            {
                button.Style = (Style)FindResource("NavButtonStyle");
            }

            // Установка активного стиля для выбранной кнопки
            activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}
