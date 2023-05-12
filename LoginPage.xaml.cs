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
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            foreach (var username in DatabaseCommunicator.KEYS_Users())
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

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            OBJ_User[] recievedUsers = DatabaseCommunicator.GET_Users(new string[] { $"username|=|{TextBox_Username.Text}", $"password|=|{TextBox_Password.Text}"});
            if (recievedUsers.Length == 1)
            {
                Application.Current.Properties["CurrentUser"] = recievedUsers[0];
                NavigationService.Navigate(new HomePage());
            }
            else
            {
                TextBox_Username.Style = (Style)FindResource("TextBoxStyle_Incorrect");
                TextBox_Password.Style = (Style)FindResource("TextBoxStyle_Incorrect");
            }
        }

        private void Button_ToRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}
