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
            this.InitializeComponent();
            LoadFilters();
            LoadOrdersByCashier();
            LoadShifts();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private void LoadFilters()
        {
            var years = _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct().OrderByDescending(y => y).ToList();
            YearComboBox.ItemsSource = years;
            if (years.Count > 0) YearComboBox.SelectedItem = years[0];
            // Инициализируем диапазон дат
            if (DateFromPicker != null)
                DateFromPicker.SelectedDate = DateTime.Today.AddDays(-7);
            if (DateToPicker != null)
                DateToPicker.SelectedDate = DateTime.Today;

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

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadOrdersByCashier();
            LoadShifts();
        }
        private void DateRangeFilter_Changed(object sender, SelectionChangedEventArgs e)
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

            if (DateFromPicker.SelectedDate is DateTime dateFrom)
                shifts = shifts.Where(s => s.OpenedAt.Date >= dateFrom.Date || (s.ClosedAt.HasValue && s.ClosedAt.Value.Date >= dateFrom.Date));
            if (DateToPicker.SelectedDate is DateTime dateTo)
                shifts = shifts.Where(s => s.OpenedAt.Date <= dateTo.Date || (s.ClosedAt.HasValue && s.ClosedAt.Value.Date <= dateTo.Date));

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
            if (DateFromPicker.SelectedDate is DateTime dateFrom)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date >= dateFrom.Date);
            if (DateToPicker.SelectedDate is DateTime dateTo)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date <= dateTo.Date);
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

        private void SaveReceiptsButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем заказы с учётом текущих фильтров
            var ordersQuery = _context.Orders
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.Hall)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Items).ThenInclude(i => i.Dish)
                .Where(o => o.StatusId == 2)
                .AsQueryable();

            if (YearComboBox.SelectedItem is int year)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Year == year);
            if (DateFromPicker.SelectedDate is DateTime dateFrom)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date >= dateFrom.Date);
            if (DateToPicker.SelectedDate is DateTime dateTo)
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date <= dateTo.Date);
            if (CashierComboBox.SelectedItem is User selectedCashier && selectedCashier.Id != -1)
                ordersQuery = ordersQuery.Where(o => o.UserId == selectedCashier.Id);

            var orders = ordersQuery.OrderBy(o => o.OrderDate).ToList();
            if (orders.Count == 0)
            {
                MessageBox.Show("Нет заказов для сохранения.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Формируем текст чеков
            var lines = new List<string>();
            foreach (var order in orders)
            {
                lines.Add($"Чек заказа #{order.Id}");
                lines.Add($"Время заказа: {order.OrderDate:dd.MM.yyyy HH:mm}");
                lines.Add($"Время оплаты: {order.PaymentTime:dd.MM.yyyy HH:mm}");
                lines.Add($"Кассир: {order.User?.FullName ?? "-"}");
                lines.Add($"Зал: {order.Hall?.Name ?? "-"}");
                lines.Add($"Стол: {order.TableNumber}");
                lines.Add($"Статус: {order.Status?.Name ?? "-"}");
                lines.Add($"Сумма заказа: {order.TotalAmount:N2} BYN");
                lines.Add($"Способ оплаты: {order.PaymentMethod?.Name ?? "-"}");
                if (order.PaymentMethodId == 1 && order.CashGiven.HasValue)
                {
                    lines.Add($"Получено: {order.CashGiven:N2} BYN");
                    lines.Add($"Сдача: {order.Change:N2} BYN");
                }


                lines.Add("Блюда:");
                foreach (var item in order.Items)
                    lines.Add($"  - {item.Dish?.Name ?? "-"} ({item.Price:N2} BYN)");
                lines.Add(new string('-', 40));
            }

            // Сохраняем в файл
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Сохранить чеки",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = $"receipts_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (dialog.ShowDialog() == true)
            {
                System.IO.File.WriteAllLines(dialog.FileName, lines);
                MessageBox.Show($"Чеки успешно сохранены в файл:\n{dialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveShiftsButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные о сменах, отображаемых в ListView
            var shifts = ShiftsListView.ItemsSource as IEnumerable<ShiftViewModel>;
            if (shifts == null || !shifts.Any())
            {
                MessageBox.Show("Нет данных о сменах для сохранения.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"shifts_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new System.IO.StreamWriter(dialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        writer.WriteLine("Информация о сменах");
                        writer.WriteLine($"Экспортировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        writer.WriteLine();
                        foreach (var shift in shifts)
                        {
                            writer.WriteLine($"Кассир: {shift.CashierName}");
                            writer.WriteLine($"Открыта: {shift.OpenedAt:dd.MM.yyyy HH:mm}");
                            writer.WriteLine($"Закрыта: {(shift.ClosedAt.HasValue ? shift.ClosedAt.Value.ToString("dd.MM.yyyy HH:mm") : "-")}");
                            writer.WriteLine($"Продолжительность: {shift.Duration}");
                            writer.WriteLine(new string('-', 40));
                        }
                    }
                    MessageBox.Show("Информация о сменах успешно сохранена.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
