using System;
using System.Collections.Generic;
using System.IO;
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
    // Страница входа в аккаунт
    //
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            foreach (var username in DatabaseCommunicator.KEYS_Users()) // получает список пользователей и делает каждому кнопку
            {
                var newButton = new Button();
                newButton.Style = (Style)FindResource("ButtonStyle_Main");
                newButton.Content = username;
                newButton.Margin = new Thickness(0, 0, 6, 6);
                newButton.Click += (sender, e) => { 
                    TextBox_Username.Text = username;
                    TextBox_Password.Focus();
                };
                UsernamesList.Children.Add(newButton);
            }
        }

        // Нажатие на кнопку ОК
        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            // Запрос в БД
            OBJ_User[] recievedUsers = DatabaseCommunicator.GET_Users(new string[] { $"username|=|{TextBox_Username.Text}", $"password|=|{TextBox_Password.Text}"});
            if (recievedUsers.Length == 1) // Пользователь нашёлся
            {
                Application.Current.Properties["CurrentUser"] = recievedUsers[0];
                NavigationService.Navigate(new HomePage()); // Перейти на домашнюю страницу
            }
            else // Пользователь не найден
            {
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
        }
        
        // Перейти на страницу регистрации
        private void Button_ToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}
