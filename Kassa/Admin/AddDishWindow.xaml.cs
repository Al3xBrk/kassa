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
                ModernMessageBox.ShowWarning("Введите название блюда.", "Ошибка");
                return;
            }
            if (DishPrice == null)
            {
                ModernMessageBox.ShowWarning("Введите корректную цену.", "Ошибка");
                return;
            }
            DialogResult = true;
        }
    }
}
