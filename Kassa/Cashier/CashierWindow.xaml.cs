using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kassa;
using Kassa.Models;

namespace Kassa
{
    public partial class CashierWindow : Window
    {
        private readonly KassaContext _context = new KassaContext();
        private int? selectedTable = null;

        private System.Windows.Threading.DispatcherTimer? _autoLogoutTimer;
        private DateTime _lastActivityTime;
        private int _autoLogoutMinutes = 10; private User? _currentUser;
        private Shift? _currentShift;

        public CashierWindow(User? user = null)
        {
            InitializeComponent();
            _currentUser = user;
            UserFullNameTextBlock.Text = _currentUser?.FullName ?? "";
            LoadHalls();
            if (HallComboBox.Items.Count > 0)
                HallComboBox.SelectedIndex = 0;

            // Загружаем текущую смену, если она есть
            if (_currentUser != null)
            {
                _currentShift = _context.Shifts.FirstOrDefault(s => s.CashierId == _currentUser.Id && s.ClosedAt == null);
            }

            InitAutoLogout();
            UpdateShiftButtons();
        }

        private void LoadHalls()
        {
            HallComboBox.Items.Clear();
            foreach (var hall in _context.Halls.ToList())
            {
                HallComboBox.Items.Add(
                    new ComboBoxItem { Content = hall.Name, Tag = hall }
                );
            }
        }
        private void UpdateTables()
        {
            TablesPanel.Children.Clear();
            selectedTable = null;
            if (HallComboBox.SelectedItem is ComboBoxItem item && item.Tag is Hall hall)
            {
                int count = hall.TableCount;

                // Обновляем заголовок с количеством столов
                TableCountTextBlock.Text = $"({count} столов)";

                var tableSums = _context.Orders
                    .Where(o => o.HallId == hall.Id && o.StatusId == 1)
                    .GroupBy(o => o.TableNumber)
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

                for (int i = 1; i <= count; i++)
                {
                    // Основной текст стола
                    var tableNumberBlock = new TextBlock
                    {
                        Text = $"Стол #{i}",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var stack = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    stack.Children.Add(tableNumberBlock);

                    // Проверяем резервацию
                    var today = DateTime.Today;
                    var nearestReservation = _context.TableReservations
                        .Where(r => r.HallId == hall.Id && r.TableNumber == i && r.Date == today && r.FromTime >= TimeOnly.FromDateTime(DateTime.Now))
                        .OrderBy(r => r.FromTime)
                        .FirstOrDefault();

                    if (nearestReservation != null)
                    {
                        var reservationBlock = new TextBlock
                        {
                            Text = $"🕒 {nearestReservation.FromTime:HH:mm}",
                            FontSize = 12,
                            FontWeight = FontWeights.Normal,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)) // Красный для резерва
                        };
                        stack.Children.Add(reservationBlock);
                    }

                    // Проверяем сумму заказа
                    if (tableSums.TryGetValue(i, out decimal sum) && sum > 0)
                    {
                        var sumBlock = new TextBlock
                        {
                            Text = $"💰 {sum:N2} BYN",
                            FontSize = 12,
                            FontWeight = FontWeights.SemiBold,
                            TextAlignment = TextAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)) // Зеленый для суммы
                        };
                        stack.Children.Add(sumBlock);
                    }

                    // Создаем карточку стола с современным дизайном
                    var border = new Border
                    {
                        Style = (Style)FindResource("TableCardStyle"),
                        Tag = i,
                        Child = stack
                    };

                    border.MouseLeftButtonUp += TableCard_Click;
                    TablesPanel.Children.Add(border);
                }
            }
            else
            {
                TableCountTextBlock.Text = "";
            }
        }

        private void HallComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTables();
        }
        private void TableCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Сбрасываем стили всех столов на обычные
            foreach (var child in TablesPanel.Children)
            {
                if (child is Border b)
                {
                    b.Style = (Style)FindResource("TableCardStyle");

                    // Обновляем стили текста для обычных столов
                    if (b.Child is StackPanel stack)
                    {
                        foreach (var textChild in stack.Children)
                        {
                            if (textChild is TextBlock tb)
                            {
                                tb.Style = (Style)FindResource("TableTextStyle");
                            }
                        }
                    }
                }
            }

            // Применяем выбранный стиль к кликнутому столу
            if (sender is Border border)
            {
                border.Style = (Style)FindResource("SelectedTableCardStyle");
                selectedTable = (int)border.Tag;

                // Обновляем стили текста для выбранного стола
                if (border.Child is StackPanel stack)
                {
                    foreach (var textChild in stack.Children)
                    {
                        if (textChild is TextBlock tb)
                        {
                            tb.Style = (Style)FindResource("SelectedTableTextStyle");
                        }
                    }
                }

                if (HallComboBox.SelectedItem is ComboBoxItem hallItem && hallItem.Tag is Hall hallObj)
                {
                    var order = _context.Orders.FirstOrDefault(o => o.HallId == hallObj.Id && o.TableNumber == selectedTable && o.StatusId == 1);
                    if (order != null)
                    {
                        if (_currentUser == null || !_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
                        {
                            ModernMessageBox.ShowWarning("Необходимо открыть смену для работы с заказами.", "Смена не открыта");
                            return;
                        }
                        var orderWindow = new OrderWindow(order, ResetAutoLogout) { Owner = this };
                        orderWindow.ShowDialog();
                        UpdateTables();
                    }
                }
            }
        }
        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, открыта ли смена у текущего кассира            if (_currentUser == null || !_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
            {
                ModernMessageBox.ShowWarning("Необходимо открыть смену перед созданием заказов.", "Смена не открыта");
                return;
            }

            if (HallComboBox.SelectedItem is ComboBoxItem hallItem && hallItem.Tag is Hall hallObj && selectedTable != null)
            {
                var order = _context.Orders.FirstOrDefault(o => o.HallId == hallObj.Id && o.TableNumber == selectedTable && o.StatusId == 1);
                if (order != null)
                {
                    var orderWindow = new OrderWindow(order, ResetAutoLogout) { Owner = this };
                    orderWindow.ShowDialog();
                    UpdateTables();
                }
                else
                {
                    var orderWindow = new OrderWindow(hallObj.Id, selectedTable.Value, _currentUser?.Id, ResetAutoLogout) { Owner = this };
                    orderWindow.ShowDialog();
                    UpdateTables();
                }
            }
            else
            {
                ModernMessageBox.ShowWarning("Выберите зал и столик.");
            }
        }

        private void GoHome_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
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
            _autoLogoutTimer.Tick += AutoLogoutTimer_Tick;
            _autoLogoutTimer.Start();
            this.PreviewKeyDown += ActivityDetected;
            this.PreviewMouseDown += ActivityDetected; // Добавлено: только клик мыши сбрасывает таймер
            UpdateLogoutTimerText();
        }

        private void ActivityDetected(object sender, EventArgs e)
        {
            _lastActivityTime = DateTime.Now;
            UpdateLogoutTimerText();
        }
        private void AutoLogoutTimer_Tick(object? sender, EventArgs e)
        {
            UpdateLogoutTimerText(); if ((DateTime.Now - _lastActivityTime).TotalMinutes >= _autoLogoutMinutes)
            {
                _autoLogoutTimer?.Stop();
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
                LogoutTimerTextBlock.Text = $"Автовыход через: {min:D2}:{sec:D2}";
            }
        }

        private void OpenShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return; if (_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
            {
                ModernMessageBox.ShowWarning("Смена уже открыта.");
                return;
            }
            var shift = new Shift
            {
                CashierId = _currentUser.Id,
                OpenedAt = DateTime.Now
            };
            _context.Shifts.Add(shift);
            _context.SaveChanges(); _currentShift = shift;
            ModernMessageBox.ShowSuccess("Смена открыта.");
            UpdateShiftButtons();
        }
        private void CloseShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            var shift = _context.Shifts.FirstOrDefault(s => s.CashierId == _currentUser.Id && s.ClosedAt == null); if (shift == null)
            {
                ModernMessageBox.ShowWarning("Нет открытой смены.");
                return;
            }

            // Проверяем, остались ли неоплаченные заказы, созданные этим кассиром
            var unpaidOrders = _context.Orders
                .Where(o => o.UserId == _currentUser.Id && o.StatusId == 1) // StatusId == 1 означает "Создан" и не оплачен
                .ToList(); if (unpaidOrders.Any())
            {
                ModernMessageBox.ShowWarning($"Невозможно закрыть смену. У вас есть {unpaidOrders.Count} неоплаченных заказов.", "Неоплаченные заказы");
                return;
            }
            shift.ClosedAt = DateTime.Now;
            _context.SaveChanges();
            _currentShift = null;
            ModernMessageBox.ShowSuccess("Смена закрыта.");
            UpdateShiftButtons();
        }
        private void UpdateShiftButtons()
        {
            if (_currentUser == null) return;
            var openShift = _context.Shifts.FirstOrDefault(s => s.CashierId == _currentUser.Id && s.ClosedAt == null);
            OpenShiftButton.IsEnabled = openShift == null;
            CloseShiftButton.IsEnabled = openShift != null;

            // Обновляем текст статуса смены
            if (openShift != null)
            {
                ShiftStatusTextBlock.Text = "Смена открыта";
                ShiftStatusTextBlock.Foreground = Brushes.Green;
                // Показываем рабочую область, скрываем историю
                WorkAreaGrid.Visibility = Visibility.Visible;
                ShiftHistoryGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShiftStatusTextBlock.Text = "Смена закрыта";
                ShiftStatusTextBlock.Foreground = Brushes.Red;
                // Скрываем рабочую область, показываем историю последней смены
                WorkAreaGrid.Visibility = Visibility.Collapsed;
                ShiftHistoryGrid.Visibility = Visibility.Visible;
                ShowLastShiftOrders();
            }
        }
        private void ShowLastShiftOrders()
        {
            if (_currentUser == null) return;

            // Получаем последнюю закрытую смену текущего кассира
            var lastClosedShift = _context.Shifts
                .Where(s => s.CashierId == _currentUser.Id && s.ClosedAt != null)
                .OrderByDescending(s => s.ClosedAt)
                .FirstOrDefault();

            if (lastClosedShift == null)
            {
                OrderHistoryListView.ItemsSource = null;
                return;
            }

            // Получаем заказы за эту смену
            var shiftOrders = _context.Orders
                .Where(o => o.UserId == _currentUser.Id &&
                           o.OrderDate >= lastClosedShift.OpenedAt &&
                           o.OrderDate <= lastClosedShift.ClosedAt &&
                           o.StatusId >= 2) // Только завершенные/оплаченные заказы
                .Select(o => new
                {
                    Id = o.Id,
                    HallName = o.Hall.Name,
                    TableNumber = o.TableNumber,
                    CreatedAt = o.OrderDate,
                    CompletedAt = o.PaymentTime ?? o.OrderDate, // Используем время оплаты или время создания
                    StatusName = o.Status.Name,
                    TotalAmount = o.TotalAmount
                })
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            OrderHistoryListView.ItemsSource = shiftOrders;
        }

        public void ResetAutoLogout()
        {
            ActivityDetected(this, EventArgs.Empty);
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}
