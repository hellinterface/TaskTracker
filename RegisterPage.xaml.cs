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
    /// <summary>
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Username.Text.Length < 2 || !TextBox_Username.Text.All(char.IsLetterOrDigit))
            {
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
            if (TextBox_Password.Text.Length < 3 || Utilities.ContainsBannedCharacters(TextBox_Password.Text))
            {
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
            OBJ_User user = new OBJ_User()
            {
                Username = TextBox_Username.Text,
                Password = TextBox_Password.Text
            };
            bool success = DatabaseCommunicator.ADD_User(user);
            if (success)
            {
                Application.Current.Properties["CurrentUser"] = user;
                NavigationService.Navigate(new HomePage());
            }
            else
            {
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
            // отправка на сервер
            // проверка успешности
            // подсвечение полей красным если что-то не так
            // если ок то на домашнюю страницу
        }

        private void Button_ToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}
