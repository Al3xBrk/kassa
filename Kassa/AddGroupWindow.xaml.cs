using System.Windows;

namespace Kassa
{
    public partial class AddGroupWindow : Window
    {
        public string GroupName => GroupNameTextBox.Text.Trim();

        public AddGroupWindow(string? initialName = null)
        {
            InitializeComponent();
            if (!string.IsNullOrWhiteSpace(initialName))
                GroupNameTextBox.Text = initialName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupName))
            {
                MessageBox.Show("Введите название группы.");
                return;
            }
            DialogResult = true;
        }
    }
}
