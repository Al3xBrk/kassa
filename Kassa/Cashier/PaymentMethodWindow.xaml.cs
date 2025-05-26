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
        public PaymentMethod? SelectedPaymentMethod { get; private set; }
        public decimal? CashGiven { get; private set; }
        public decimal? Change { get; private set; }

        public PaymentMethodWindow(List<PaymentMethod> paymentMethods, Order order)
        {
            InitializeComponent();
            _paymentMethods = paymentMethods;
            _order = order;

            // Заполнение информации о заказе
            HallTextBlock.Text = order.Hall?.Name ?? "Неизвестный зал";
            TableNumberTextBlock.Text = order.TableNumber.ToString();
            TotalAmountTextBlock.Text = $"{order.TotalAmount:N2} BYN";
        }

        private void CardPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Выбираем метод оплаты "Карта" (Id = 2)
            SelectedPaymentMethod = _paymentMethods.FirstOrDefault(p => p.Id == 2);

            // Скрываем панель ввода наличных
            CashInputPanel.Visibility = Visibility.Collapsed;

            // Активируем кнопку подтверждения
            ConfirmButton.IsEnabled = true;

            // Визуально отмечаем выбранную кнопку
            CardPaymentButton.Background = System.Windows.Media.Brushes.DarkGreen;
            CashPaymentButton.Background = System.Windows.Media.Brushes.DodgerBlue;
        }

        private void CashPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            // Выбираем метод оплаты "Наличные" (Id = 1)
            SelectedPaymentMethod = _paymentMethods.FirstOrDefault(p => p.Id == 1);

            // Показываем панель ввода наличных
            CashInputPanel.Visibility = Visibility.Visible;

            // Активируем кнопку подтверждения только если введена сумма
            ConfirmButton.IsEnabled = !string.IsNullOrWhiteSpace(CashGivenTextBox.Text);

            // Визуально отмечаем выбранную кнопку
            CashPaymentButton.Background = System.Windows.Media.Brushes.DarkBlue;
            CardPaymentButton.Background = System.Windows.Media.Brushes.LimeGreen;

            // Фокус на поле ввода
            CashGivenTextBox.Focus();
        }

        private void CashGivenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
            if (SelectedPaymentMethod == null)
            {
                MessageBox.Show("Пожалуйста, выберите способ оплаты", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Если выбрана оплата наличными, проверяем введенную сумму
            if (SelectedPaymentMethod.Id == 1) // Наличные
            {
                if (!CashGiven.HasValue || CashGiven.Value < _order.TotalAmount)
                {
                    MessageBox.Show("Внесенная сумма должна быть не меньше суммы заказа", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
