using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TaskTracker
{
    //
    // Страница регистрации
    //
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        // Нажатие кнопки ОК
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Username.Text.Length < 2 || !TextBox_Username.Text.All(char.IsLetterOrDigit))
            {
                // Неверное имя пользователя
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                return;
            }
            if (TextBox_Password.Text.Length < 3 || Utilities.ContainsBannedCharacters(TextBox_Password.Text))
            {
                // Неверный пароль
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                return;
            }
            // Создание объекта
            OBJ_User user = new OBJ_User()
            {
                Username = TextBox_Username.Text,
                Password = TextBox_Password.Text
            };
            bool success = DatabaseCommunicator.ADD_User(user); // Запрос в БД
            if (success)
            {
                Application.Current.Properties["CurrentUser"] = user;
                NavigationService.Navigate(new HomePage()); // Переход на домашнюю страницу
            }
            else
            {
                // Неудача
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
        }
        
        // Нажатие кнопки назад (на страницу входа)
        private void Button_ToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}
