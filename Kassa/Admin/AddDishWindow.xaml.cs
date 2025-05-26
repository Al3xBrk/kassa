using System.Windows;

namespace Kassa
{
    public partial class AddDishWindow : Window
    {
        public string DishName => DishNameTextBox.Text.Trim();
        public decimal? DishPrice => decimal.TryParse(DishPriceTextBox.Text, out var price) ? price : null;

        public AddDishWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DishName))
            {
                MessageBox.Show("Введите название блюда.");
                return;
            }
            if (DishPrice == null)
            {
                MessageBox.Show("Введите корректную цену.");
                return;
            }
            DialogResult = true;
        }
    }
}
