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

        private System.Windows.Threading.DispatcherTimer _autoLogoutTimer;
        private DateTime _lastActivityTime;
        private int _autoLogoutMinutes = 10;
        private User? _currentUser;
        private Shift? _currentShift; public CashierWindow(User? user = null)
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
                var tableSums = _context.Orders
                    .Where(o => o.HallId == hall.Id && o.StatusId == 1)
                    .GroupBy(o => o.TableNumber)
                    .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
                for (int i = 1; i <= count; i++)
                {
                    string tableText = $"Стол #{i}";
                    // Найти ближайшую резервацию на сегодня для этого стола
                    var today = DateTime.Today;
                    var nearestReservation = _context.TableReservations
                        .Where(r => r.HallId == hall.Id && r.TableNumber == i && r.Date == today && r.FromTime >= TimeOnly.FromDateTime(DateTime.Now))
                        .OrderBy(r => r.FromTime)
                        .FirstOrDefault();
                    if (nearestReservation != null)
                    {
                        tableText += "\nРезерв: " + nearestReservation.FromTime.ToString("HH:mm");
                    }
                    if (tableSums.TryGetValue(i, out decimal sum) && sum > 0)
                        tableText += $"\n({sum:N2} BYN)";

                    var textBlock = new TextBlock
                    {
                        Text = tableText,
                        FontSize = 16,
                        Foreground = Brushes.Black,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    var stack = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    stack.Children.Add(textBlock);

                    var border = new Border
                    {
                        Width = 140,
                        Height = 70,
                        Margin = new Thickness(8),
                        Background = Brushes.White,
                        BorderBrush = Brushes.SteelBlue,
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(10),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = i,
                        Child = stack,
                        Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            Color = Colors.Gray,
                            Direction = 320,
                            ShadowDepth = 2,
                            Opacity = 0.3
                        }
                    };
                    border.MouseLeftButtonUp += TableCard_Click;
                    TablesPanel.Children.Add(border);
                }
            }
        }

        private void HallComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTables();
        }
        private void TableCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            foreach (var child in TablesPanel.Children)
                if (child is Border b)
                    b.Background = Brushes.White;
            if (sender is Border border)
            {
                border.Background = Brushes.LightBlue;
                selectedTable = (int)border.Tag;

                if (HallComboBox.SelectedItem is ComboBoxItem hallItem && hallItem.Tag is Hall hallObj)
                {
                    var order = _context.Orders.FirstOrDefault(o => o.HallId == hallObj.Id && o.TableNumber == selectedTable && o.StatusId == 1);
                    if (order != null)
                    {
                        // Для просмотра существующего заказа также требуется открытая смена
                        if (_currentUser == null || !_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
                        {
                            MessageBox.Show("Необходимо открыть смену для работы с заказами.",
                                "Смена не открыта", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var orderWindow = new OrderWindow(order) { Owner = this };
                        orderWindow.ShowDialog();
                        UpdateTables();
                    }
                }
            }
        }
        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, открыта ли смена у текущего кассира
            if (_currentUser == null || !_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
            {
                MessageBox.Show("Необходимо открыть смену перед созданием заказов.", "Смена не открыта", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HallComboBox.SelectedItem is ComboBoxItem hallItem && hallItem.Tag is Hall hallObj && selectedTable != null)
            {
                var order = _context.Orders.FirstOrDefault(o => o.HallId == hallObj.Id && o.TableNumber == selectedTable && o.StatusId == 1);
                if (order != null)
                {
                    var orderWindow = new OrderWindow(order) { Owner = this };
                    orderWindow.ShowDialog();
                    UpdateTables();
                }
                else
                {
                    var orderWindow = new OrderWindow(hallObj.Id, selectedTable.Value, _currentUser?.Id) { Owner = this };
                    orderWindow.ShowDialog();
                    UpdateTables();
                }
            }
            else
            {
                MessageBox.Show("Выберите зал и столик.");
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

        private void AutoLogoutTimer_Tick(object sender, EventArgs e)
        {
            UpdateLogoutTimerText();
            if ((DateTime.Now - _lastActivityTime).TotalMinutes >= _autoLogoutMinutes)
            {
                _autoLogoutTimer.Stop();
                MessageBox.Show("Время неактивности истекло. Возврат на главную страницу.", "Авто-выход", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (_currentUser == null) return;
            if (_context.Shifts.Any(s => s.CashierId == _currentUser.Id && s.ClosedAt == null))
            {
                MessageBox.Show("Смена уже открыта.");
                return;
            }
            var shift = new Shift
            {
                CashierId = _currentUser.Id,
                OpenedAt = DateTime.Now
            };
            _context.Shifts.Add(shift);
            _context.SaveChanges();
            _currentShift = shift;
            MessageBox.Show("Смена открыта.");
            UpdateShiftButtons();
        }
        private void CloseShiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;
            var shift = _context.Shifts.FirstOrDefault(s => s.CashierId == _currentUser.Id && s.ClosedAt == null);
            if (shift == null)
            {
                MessageBox.Show("Нет открытой смены.");
                return;
            }

            // Проверяем, остались ли неоплаченные заказы, созданные этим кассиром
            var unpaidOrders = _context.Orders
                .Where(o => o.UserId == _currentUser.Id && o.StatusId == 1) // StatusId == 1 означает "Создан" и не оплачен
                .ToList();

            if (unpaidOrders.Any())
            {
                MessageBox.Show($"Невозможно закрыть смену. У вас есть {unpaidOrders.Count} неоплаченных заказов.",
                    "Неоплаченные заказы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            shift.ClosedAt = DateTime.Now;
            _context.SaveChanges();
            _currentShift = null;
            MessageBox.Show("Смена закрыта.");
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
            }
            else
            {
                ShiftStatusTextBlock.Text = "Смена закрыта";
                ShiftStatusTextBlock.Foreground = Brushes.Red;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _context.Dispose();
            base.OnClosed(e);
        }
    }
}
