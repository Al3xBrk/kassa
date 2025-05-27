using System;
using System.Linq;
using System.Windows;

namespace Kassa
{
    public enum ModernMessageBoxType
    {
        Information,
        Warning,
        Error,
        Success,
        Question
    }

    public enum ModernMessageBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Yes = 6,
        No = 7
    }

    public partial class ModernMessageBox : Window
    {
        public ModernMessageBoxResult Result { get; private set; } = ModernMessageBoxResult.None;

        private ModernMessageBox()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            Result = (ModernMessageBoxResult)Button1.Tag;
            DialogResult = true;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Result = (ModernMessageBoxResult)Button2.Tag;
            DialogResult = false;
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            Result = (ModernMessageBoxResult)Button3.Tag;
            DialogResult = false;
        }

        public static ModernMessageBoxResult Show(string message)
        {
            return Show(message, "Сообщение", ModernMessageBoxType.Information);
        }

        public static ModernMessageBoxResult Show(string message, string title)
        {
            return Show(message, title, ModernMessageBoxType.Information);
        }
        public static ModernMessageBoxResult Show(string message, string title, ModernMessageBoxType type)
        {
            var messageBox = new ModernMessageBox();
            messageBox.SetupMessageBox(message, title, type, ModernMessageBoxResult.OK);

            // Пытаемся найти активное окно для центрирования
            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner == null)
            {
                owner = Application.Current.MainWindow;
            }

            if (owner?.IsLoaded == true && owner.IsVisible)
            {
                messageBox.Owner = owner;
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            messageBox.ShowDialog();
            return messageBox.Result;
        }
        public static ModernMessageBoxResult Show(string message, string title, ModernMessageBoxType type, ModernMessageBoxResult buttons)
        {
            var messageBox = new ModernMessageBox();
            messageBox.SetupMessageBox(message, title, type, buttons);

            // Пытаемся найти активное окно для центрирования
            var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (owner == null)
            {
                owner = Application.Current.MainWindow;
            }

            if (owner?.IsLoaded == true && owner.IsVisible)
            {
                messageBox.Owner = owner;
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            messageBox.ShowDialog();
            return messageBox.Result;
        }

        private void SetupMessageBox(string message, string title, ModernMessageBoxType type, ModernMessageBoxResult buttons)
        {
            MessageTextBlock.Text = message;
            TitleTextBlock.Text = title;
            Title = title;

            // Установка иконки в зависимости от типа
            switch (type)
            {
                case ModernMessageBoxType.Information:
                    IconTextBlock.Text = "ℹ️";
                    break;
                case ModernMessageBoxType.Warning:
                    IconTextBlock.Text = "⚠️";
                    break;
                case ModernMessageBoxType.Error:
                    IconTextBlock.Text = "❌";
                    break;
                case ModernMessageBoxType.Success:
                    IconTextBlock.Text = "✅";
                    break;
                case ModernMessageBoxType.Question:
                    IconTextBlock.Text = "❓";
                    break;
            }

            // Настройка кнопок
            SetupButtons(buttons);
        }

        private void SetupButtons(ModernMessageBoxResult buttons)
        {
            // Скрываем все кнопки по умолчанию
            Button1.Visibility = Visibility.Collapsed;
            Button2.Visibility = Visibility.Collapsed;
            Button3.Visibility = Visibility.Collapsed;

            switch (buttons)
            {
                case ModernMessageBoxResult.OK:
                    Button1.Content = "ОК";
                    Button1.Tag = ModernMessageBoxResult.OK;
                    Button1.Style = FindResource("PrimaryButtonStyle") as Style;
                    Button1.Visibility = Visibility.Visible;
                    Button1.IsDefault = true;
                    break;

                case ModernMessageBoxResult.Yes:
                    // Для YesNo
                    Button1.Content = "Да";
                    Button1.Tag = ModernMessageBoxResult.Yes;
                    Button1.Style = FindResource("SuccessButtonStyle") as Style;
                    Button1.Visibility = Visibility.Visible;
                    Button1.IsDefault = true;

                    Button2.Content = "Нет";
                    Button2.Tag = ModernMessageBoxResult.No;
                    Button2.Style = FindResource("DangerButtonStyle") as Style;
                    Button2.Visibility = Visibility.Visible;
                    Button2.IsCancel = true;
                    break;

                case ModernMessageBoxResult.Cancel:
                    // Для OKCancel
                    Button1.Content = "ОК";
                    Button1.Tag = ModernMessageBoxResult.OK;
                    Button1.Style = FindResource("PrimaryButtonStyle") as Style;
                    Button1.Visibility = Visibility.Visible;
                    Button1.IsDefault = true;

                    Button2.Content = "Отмена";
                    Button2.Tag = ModernMessageBoxResult.Cancel;
                    Button2.Style = FindResource("SecondaryButtonStyle") as Style;
                    Button2.Visibility = Visibility.Visible;
                    Button2.IsCancel = true;
                    break;
            }
        }

        // Статические методы для совместимости с MessageBox API
        public static ModernMessageBoxResult ShowInformation(string message, string title = "Информация")
        {
            return Show(message, title, ModernMessageBoxType.Information);
        }

        public static ModernMessageBoxResult ShowWarning(string message, string title = "Внимание")
        {
            return Show(message, title, ModernMessageBoxType.Warning);
        }

        public static ModernMessageBoxResult ShowError(string message, string title = "Ошибка")
        {
            return Show(message, title, ModernMessageBoxType.Error);
        }

        public static ModernMessageBoxResult ShowSuccess(string message, string title = "Успех")
        {
            return Show(message, title, ModernMessageBoxType.Success);
        }

        public static ModernMessageBoxResult ShowQuestion(string message, string title = "Вопрос")
        {
            return Show(message, title, ModernMessageBoxType.Question, ModernMessageBoxResult.Yes);
        }
    }
}
