using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Kassa.Models;

namespace Kassa
{
    public partial class PaymentMethodWindow : Window
    {
        public PaymentMethod? SelectedPaymentMethod { get; private set; }

        public PaymentMethodWindow(List<PaymentMethod> paymentMethods)
        {
            InitializeComponent();
            PaymentMethodsListBox.ItemsSource = paymentMethods;

            // По умолчанию выбираем первый метод оплаты
            if (paymentMethods.Any())
            {
                PaymentMethodsListBox.SelectedIndex = 0;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPaymentMethod = PaymentMethodsListBox.SelectedItem as PaymentMethod;
            if (SelectedPaymentMethod == null)
            {
                MessageBox.Show("Пожалуйста, выберите способ оплаты", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
