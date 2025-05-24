using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LiveCharts;
using LiveCharts.Wpf;

namespace Kassa
{
    public partial class AdminWindow : Window
    {
        private KassaContext _context = new KassaContext();
        private string? selectedGroup = null;

        public AdminWindow()
        {
            InitializeComponent();
            NavDishesButton.Click += NavDishesButton_Click;
            NavProfitButton.Click += NavProfitButton_Click;
            NavReservationButton.Click += NavReservationButton_Click;
            AdminContentFrame.NavigationUIVisibility = System.Windows.Navigation.NavigationUIVisibility.Hidden;
            AdminContentFrame.Navigate(new AdminDishesPage());
        }

        private void NavDishesButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminDishesPage());
        }

        private void NavProfitButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminProfitChartPage());
        }

        private void NavReservationButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminReservationPage());
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

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}
