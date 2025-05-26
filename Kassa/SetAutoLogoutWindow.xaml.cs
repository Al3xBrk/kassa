using System.Windows;

namespace Kassa
{
    public partial class SetAutoLogoutWindow : Window
    {
        public int SelectedMinutes { get; private set; }
        public SetAutoLogoutWindow(int currentMinutes)
        {
            InitializeComponent();
            MinutesTextBox.Text = currentMinutes.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinutesTextBox.Text, out int minutes) && minutes > 0)
            {
                SelectedMinutes = minutes;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите положительное целое число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
