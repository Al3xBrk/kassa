using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Kassa
{
    public partial class PrintReceiptWindow : Window
    {
        public PrintReceiptWindow(DateTime orderDate, List<DishViewModel> dishes, decimal total, string? paymentMethod = null, string? cashierName = null, DateTime? paymentTime = null, decimal? cashGiven = null, decimal? change = null)
        {
            InitializeComponent();
            OrderDateText.Text = $"Время заказа: {orderDate:dd.MM.yyyy HH:mm}";
            PrintDateText.Text = $"Время оплаты: {DateTime.Now:dd.MM.yyyy HH:mm}";
            DishesListView.ItemsSource = dishes.Select(d => new { d.Name, d.Price }).ToList();
            TotalText.Text = $"Итого: {total:N2} BYN";

            // Способ оплаты
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                PaymentMethodText.Text = $"Способ оплаты: {paymentMethod}";
                PaymentMethodText.Visibility = Visibility.Visible;
            }
            else
            {
                PaymentMethodText.Visibility = Visibility.Collapsed;
            }

            // ФИО кассира
            if (!string.IsNullOrEmpty(cashierName))
            {
                CashierText.Text = $"Кассир: {cashierName}";
                CashierText.Visibility = Visibility.Visible;
            }
            else
            {
                CashierText.Visibility = Visibility.Collapsed;
            }

            // Для наличных: получено и сдача
            if (cashGiven.HasValue)
            {
                CashGivenText.Text = $"Получено: {cashGiven:N2} BYN";
                CashGivenText.Visibility = Visibility.Visible;
            }
            else
            {
                CashGivenText.Visibility = Visibility.Collapsed;
            }
            if (change.HasValue)
            {
                ChangeText.Text = $"Сдача: {change:N2} BYN";
                ChangeText.Visibility = Visibility.Visible;
            }
            else
            {
                ChangeText.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
