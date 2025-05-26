using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace Kassa
{
    public partial class AdminOrderHistoryPage : Page
    {
        private readonly KassaContext _context = new KassaContext();
        public AdminOrderHistoryPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadOrders();
        }

        private void LoadFilters()
        {
            var years = _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct().OrderByDescending(y => y).ToList();
            YearComboBox.ItemsSource = years;
            if (years.Count > 0) YearComboBox.SelectedItem = years[0];
            DatePickerFilter.SelectedDate = DateTime.Today;
        }

        private void DatePickerFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            var ordersAll = _context.Orders.Include(o => o.Status).ToList();
            IEnumerable<Order> filtered = ordersAll;
            if (YearComboBox.SelectedItem is int year)
                filtered = filtered.Where(o => o.OrderDate.Year == year);
            if (DatePickerFilter.SelectedDate is DateTime date)
                filtered = filtered.Where(o => o.OrderDate.Date == date.Date);
            var orders = filtered.OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderHistoryVM
                {
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    StatusName = o.Status.Name
                }).ToList();
            OrderHistoryListView.ItemsSource = orders;
        }

        public class OrderHistoryVM
        {
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string StatusName { get; set; } = string.Empty;
        }
    }
}
