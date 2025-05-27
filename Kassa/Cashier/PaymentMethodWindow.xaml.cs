using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Kassa.Models;

namespace Kassa
{
    public partial class PaymentMethodWindow : Window
    {
        private readonly List<PaymentMethod> _paymentMethods;
        private readonly Order _order;
        private readonly Action? _resetAutoLogoutAction;
        public PaymentMethod? SelectedPaymentMethod { get; private set; }
        public decimal? CashGiven { get; private set; }
        public decimal? Change { get; private set; }

        public PaymentMethodWindow(List<PaymentMethod> paymentMethods, Order order, Action? resetAutoLogoutAction = null)
        {
            InitializeComponent();
            _paymentMethods = paymentMethods;
            _order = order;
            _resetAutoLogoutAction = resetAutoLogoutAction;

            // Заполнение информации о заказе
            HallTextBlock.Text = order.Hall?.Name ?? "Неизвестный зал";
            TableNumberTextBlock.Text = order.TableNumber.ToString();
            TotalAmountTextBlock.Text = $"{order.TotalAmount:N2} BYN";
        }
        private void CardPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            _resetAutoLogoutAction?.Invoke();
            // Выбираем метод оплаты "Карта" (Id = 2)
            SelectedPaymentMethod = _paymentMethods.FirstOrDefault(p => p.Id == 2);

            // Скрываем панель ввода наличных
            CashInputPanel.Visibility = Visibility.Collapsed;

            // Активируем кнопку подтверждения
            ConfirmButton.IsEnabled = true;

            // Применяем стили для выбранной кнопки
            CardPaymentButton.Style = (Style)FindResource("SelectedPaymentButtonStyle");
            CashPaymentButton.Style = (Style)FindResource("PaymentButtonStyle");
        }

        private void CashPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            _resetAutoLogoutAction?.Invoke();
            // Выбираем метод оплаты "Наличные" (Id = 1)
            SelectedPaymentMethod = _paymentMethods.FirstOrDefault(p => p.Id == 1);

            // Показываем панель ввода наличных
            CashInputPanel.Visibility = Visibility.Visible;

            // Активируем кнопку подтверждения только если введена сумма
            ConfirmButton.IsEnabled = !string.IsNullOrWhiteSpace(CashGivenTextBox.Text);

            // Применяем стили для выбранной кнопки
            CashPaymentButton.Style = (Style)FindResource("SelectedPaymentButtonStyle");
            CardPaymentButton.Style = (Style)FindResource("PaymentButtonStyle");

            // Фокус на поле ввода
            CashGivenTextBox.Focus();
        }

        private void CashGivenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _resetAutoLogoutAction?.Invoke();
            // Проверяем корректность ввода и рассчитываем сдачу
            if (decimal.TryParse(CashGivenTextBox.Text, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal cashGiven))
            {
                CashGiven = cashGiven;

                // Рассчитываем сдачу
                decimal change = Math.Max(0, cashGiven - _order.TotalAmount);
                Change = change;

                // Отображаем сдачу
                ChangeTextBlock.Text = $"{change:N2} BYN";

                // Активируем кнопку подтверждения, если введенная сумма достаточна
                ConfirmButton.IsEnabled = cashGiven >= _order.TotalAmount;

                // Устанавливаем цвет в зависимости от достаточности суммы
                ChangeTextBlock.Foreground = cashGiven >= _order.TotalAmount
                    ? System.Windows.Media.Brushes.Green
                    : System.Windows.Media.Brushes.Red;
            }
            else
            {
                // Если ввод некорректный
                ChangeTextBlock.Text = "0.00 BYN";
                ChangeTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                ConfirmButton.IsEnabled = false;
                CashGiven = null;
                Change = null;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _resetAutoLogoutAction?.Invoke(); if (SelectedPaymentMethod == null)
            {
                ModernMessageBox.ShowWarning("Пожалуйста, выберите способ оплаты", "Внимание");
                return;
            }

            // Если выбрана оплата наличными, проверяем введенную сумму
            if (SelectedPaymentMethod.Id == 1) // Наличные
            {
                if (!CashGiven.HasValue || CashGiven.Value < _order.TotalAmount)
                {
                    ModernMessageBox.ShowWarning("Внесенная сумма должна быть не меньше суммы заказа", "Внимание");
                    return;
                }
            }

            // Показать чек сразу после подтверждения оплаты
            ShowReceipt();

            DialogResult = true;
        }

        private void ShowReceipt()
        {
            // Получаем ФИО кассира
            string? cashierName = _order.User?.FullName;
            if (string.IsNullOrWhiteSpace(cashierName))
            {
                // Пробуем загрузить пользователя из базы, если не подгружен
                var dbUser = new KassaContext().Users.FirstOrDefault(u => u.Id == _order.UserId);
                cashierName = dbUser?.FullName ?? "-";
            }
            // Время создания заказа
            DateTime orderDate = _order.OrderDate;
            // Время оплаты
            DateTime? paymentTime = DateTime.Now;
            // Список блюд
            var dishes = _order.Items?.Select(i => new DishViewModel { Name = i.Dish.Name, Price = i.Price }).ToList() ?? new List<DishViewModel>();
            // Сумма заказа
            decimal total = _order.TotalAmount;
            // Способ оплаты
            string paymentMethod = SelectedPaymentMethod?.Name ?? "-";
            // Получено и сдача (только для наличных)
            decimal? cashGiven = CashGiven;
            decimal? change = Change;

            var wnd = new PrintReceiptWindow(
                orderDate,
                dishes,
                total,
                paymentMethod,
                cashierName,
                paymentTime,
                cashGiven,
                change)
            {
                Owner = this
            };
            wnd.Title = "Чек заказа";
            wnd.ShowDialog();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _resetAutoLogoutAction?.Invoke();
            DialogResult = false;
        }
    }
}
