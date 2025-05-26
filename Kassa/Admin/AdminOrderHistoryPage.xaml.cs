using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Kassa.Models;

namespace Kassa
{
    public partial class AdminOrderHistoryPage : Page
    {
        private readonly KassaContext _context = new KassaContext();
        public AdminOrderHistoryPage()
        {
            InitializeComponent();
            LoadFilters();
            LoadOrdersByCashier();
            LoadShifts();
        }

        private void LoadFilters()
        {
            var years = _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct().OrderByDescending(y => y).ToList();
            YearComboBox.ItemsSource = years;
            if (years.Count > 0) YearComboBox.SelectedItem = years[0];
            DatePickerFilter.SelectedDate = DateTime.Today;

            // Загрузка кассиров
            var cashiers = _context.Users
                .Where(u => u.Role == UserRole.Cashier)
                .OrderBy(u => u.FullName)
                .ToList();

            // Добавляем пункт "Все кассиры" в начало списка
            var allCashiers = new User { Id = -1, FullName = "Все кассиры" };
            cashiers.Insert(0, allCashiers);

            CashierComboBox.ItemsSource = cashiers;
            CashierComboBox.SelectedIndex = 0;
        }

        private void DatePickerFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrdersByCashier();
            LoadShifts();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadOrdersByCashier();
            LoadShifts();
        }
        private void LoadShifts()
        {
            IQueryable<Shift> shifts = _context.Shifts
                .Include(s => s.Cashier);

            // Применяем фильтры
            if (YearComboBox.SelectedItem is int year)
                shifts = shifts.Where(s => s.OpenedAt.Year == year);

            if (DatePickerFilter.SelectedDate is DateTime date)
                shifts = shifts.Where(s => s.OpenedAt.Date == date.Date ||
                                        (s.ClosedAt.HasValue && s.ClosedAt.Value.Date == date.Date));

            if (CashierComboBox.SelectedItem is User selectedCashier && selectedCashier.Id != -1)
                shifts = shifts.Where(s => s.CashierId == selectedCashier.Id);

            // Сначала извлекаем данные из базы, чтобы избежать ошибки с instance методом в EF Core
            var shiftsData = shifts.OrderByDescending(s => s.OpenedAt).ToList();

            // Затем преобразуем в модель представления в памяти
            var shiftsVm = shiftsData.Select(s => new ShiftViewModel
            {
                CashierName = s.Cashier.FullName,
                OpenedAt = s.OpenedAt,
                ClosedAt = s.ClosedAt,
                // Используем статический метод для вычисления продолжительности
                Duration = s.ClosedAt.HasValue ?
                        CalculateDurationString(s.OpenedAt, s.ClosedAt.Value) :
                        "Активная смена"
            })
                .ToList();

            ShiftsListView.ItemsSource = shiftsVm;
        }

        // Сделаем метод статическим для избежания захвата контекста экземпляра в LINQ-запросе
        private static string CalculateDurationString(DateTime start, DateTime end)
        {
            var duration = end - start;
            return $"{(int)duration.TotalHours}ч {duration.Minutes}м";
        }
        private void LoadOrdersByCashier()
        {
            // Получаем все заказы с информацией о статусе и кассире
            var ordersQuery = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.PaymentMethod)
                .AsQueryable();

            // Применяем фильтры
            if (YearComboBox.SelectedItem is int year)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Year == year);

            if (DatePickerFilter.SelectedDate is DateTime date)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date == date.Date);

            if (CashierComboBox.SelectedItem is User selectedCashier && selectedCashier.Id != -1)
                ordersQuery = ordersQuery.Where(o => o.UserId == selectedCashier.Id);

            // Важно: полная загрузка данных из базы перед любыми преобразованиями
            var orders = ordersQuery.ToList();

            // Создаем плоский список заказов для отображения, с указанием кассира
            var ordersList = new List<OrderWithCashierVM>();

            foreach (var order in orders.OrderByDescending(o => o.OrderDate))
            {
                string cashierName = "Неизвестный кассир";
                if (order.UserId != null && order.User != null)
                {
                    cashierName = order.User.FullName;
                }

                ordersList.Add(new OrderWithCashierVM
                {
                    CashierName = cashierName,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.TotalAmount,
                    StatusName = order.Status.Name,
                    PaymentMethod = order.PaymentMethod?.Name ?? "Не указан"
                });
            }

            OrdersListView.ItemsSource = ordersList;
        }

        public class CashierWithOrdersViewModel
        {
            public int CashierId { get; set; }
            public string CashierName { get; set; } = string.Empty;
            public int OrdersCount { get; set; }
            public decimal TotalAmount { get; set; }
            public ObservableCollection<OrderHistoryVM> Orders { get; set; } = new ObservableCollection<OrderHistoryVM>();
        }

        public class OrderHistoryVM
        {
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string StatusName { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
        }

        public class ShiftViewModel
        {
            public string CashierName { get; set; } = string.Empty;
            public DateTime OpenedAt { get; set; }
            public DateTime? ClosedAt { get; set; }
            public string Duration { get; set; } = string.Empty;
        }

        public class OrderWithCashierVM
        {
            public string CashierName { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string StatusName { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
        }
    }
}
