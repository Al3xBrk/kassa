using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Kassa
{
    using Kassa.Models;
    public partial class AdminReservationPage : Page
    {
        private readonly KassaContext _context = new KassaContext();
        public AdminReservationPage()
        {
            InitializeComponent();
            LoadHalls();
            LoadTimelineHalls();
        }

        private void LoadHalls()
        {
            HallComboBox.ItemsSource = _context.Halls.ToList();
            HallComboBox.DisplayMemberPath = "Name";
            HallComboBox.SelectedIndex = 0;
            HallComboBox_SelectionChanged(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));
        }

        private void HallComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HallComboBox.SelectedItem is Hall hall)
            {
                TableComboBox.ItemsSource = Enumerable.Range(1, hall.TableCount).ToList();
                TableComboBox.SelectedIndex = 0;
            }
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            if (HallComboBox.SelectedItem is Hall hall && TableComboBox.SelectedItem is int tableNumber && ReservationDatePicker.SelectedDate != null)
            {
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    ModernMessageBox.Show("Введите имя для резервации.");
                    return;
                }
                if (FromTimePicker.Value == null || ToTimePicker.Value == null)
                {
                    ModernMessageBox.Show("Выберите время начала и конца.");
                    return;
                }
                var fromTime = TimeOnly.FromDateTime(FromTimePicker.Value.Value);
                var toTime = TimeOnly.FromDateTime(ToTimePicker.Value.Value);
                if (fromTime >= toTime)
                {
                    ModernMessageBox.Show("Время начала должно быть меньше времени конца.");
                    return;
                }
                // Проверка на пересечение с существующими бронями
                var date = ReservationDatePicker.SelectedDate.Value.Date;
                var overlaps = _context.TableReservations.Any(r =>
                    r.HallId == hall.Id &&
                    r.TableNumber == tableNumber &&
                    r.Date == date &&
                    // Пересечение интервалов: (A < D) && (C < B)
                    fromTime < r.ToTime && r.FromTime < toTime
                );
                if (overlaps)
                {
                    ModernMessageBox.ShowError("В это время стол уже зарезервирован.", "Ошибка");
                    return;
                }
                var reservation = new TableReservation
                {
                    HallId = hall.Id,
                    TableNumber = tableNumber,
                    Date = date,
                    FromTime = fromTime,
                    ToTime = toTime,
                    Name = NameTextBox.Text.Trim()
                };
                _context.TableReservations.Add(reservation);
                _context.SaveChanges();
                ModernMessageBox.ShowInformation("Стол успешно зарезервирован!", "Успех");
                UpdateTimeline();
            }
            else
            {
                MessageBox.Show("Заполните все поля.");
            }
        }

        private void LoadTimelineHalls()
        {
            TimelineHallComboBox.ItemsSource = _context.Halls.ToList();
            TimelineHallComboBox.DisplayMemberPath = "Name";
            TimelineHallComboBox.SelectedIndex = 0;
            TimelineDatePicker.SelectedDate = DateTime.Today;
            UpdateTimeline();
        }

        private void TimelineDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeline();
        }

        private void TimelineHallComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeline();
        }
        private void UpdateTimeline()
        {
            if (TimelineHallComboBox.SelectedItem is Hall hall && TimelineDatePicker.SelectedDate is DateTime date)
            {
                var reservations = _context.TableReservations
                    .Where(r => r.HallId == hall.Id && r.Date == date.Date)
                    .ToList();
                TimelineDataGrid.ItemsSource = reservations;
            }
        }

        private void GoHomeButton_Click(object sender, RoutedEventArgs e)
        {
            var adminWindow = Window.GetWindow(this) as AdminWindow;
            if (adminWindow != null)
            {
                adminWindow.NavigateToDishesPage();
            }
        }

        private void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int reservationId)
            {
                var reservation = _context.TableReservations.FirstOrDefault(r => r.Id == reservationId);
                if (reservation != null)
                {
                    if (ModernMessageBox.ShowQuestion($"Отменить резерв для стола {reservation.TableNumber} ({reservation.Name})?", "Подтверждение") == ModernMessageBoxResult.Yes)
                    {
                        _context.TableReservations.Remove(reservation);
                        _context.SaveChanges();
                        UpdateTimeline();
                    }
                }
            }
        }
    }
}
