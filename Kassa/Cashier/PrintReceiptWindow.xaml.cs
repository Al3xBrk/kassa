using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Kassa
{
    public partial class PrintReceiptWindow : Window
    {
        public PrintReceiptWindow(DateTime orderDate, List<DishViewModel> dishes, decimal total, string? paymentMethod = null)
        {
            InitializeComponent();
            OrderDateText.Text = $"Дата заказа: {orderDate:dd.MM.yyyy HH:mm}";
            PrintDateText.Text = $"Дата печати: {DateTime.Now:dd.MM.yyyy HH:mm}";
            DishesListView.ItemsSource = dishes.Select(d => new { d.Name, d.Price }).ToList();
            TotalText.Text = $"Итого: {total:N2} BYN";

            // Добавляем информацию о способе оплаты, если она доступна
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                PaymentMethodText.Text = $"Способ оплаты: {paymentMethod}";
                PaymentMethodText.Visibility = Visibility.Visible;
            }
            else
            {
                PaymentMethodText.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
