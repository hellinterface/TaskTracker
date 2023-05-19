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
    /// страница настройки доски
    public partial class BoardAccessSettingsPage : Page, IPageWithUserSelect
    {
        public TextBox UserListTextBox { get; set; } /*поле ввода, в котором находятся нужные имена пользователей*/
        private OBJ_Board Board;

        private string usersCanViewString;/*список пользователей с доступом на просмотр через запятую*/
        private string usersCanEditString;/*список пользователей с доступом на редактирование через запятую*/

        public BoardAccessSettingsPage(OBJ_Board board)
        {
            InitializeComponent();
            this.DataContext = board;
            this.Board = board;
            UserListTextBox = UserSelect_TextBox;
            // преобразование списков пользователей в строки
            usersCanViewString = stringifyUserList(Board.UsersCanView);
            usersCanEditString = stringifyUserList(Board.UsersCanEdit);
            UserSelect_TextBox.Text = usersCanViewString;
        }
        // получение выбранных на странице UserSelectPage пользователей
        void IPageWithUserSelect.SetUsersFromUserSelectPage(UserSelectPage page)
        {
            UserListTextBox.Text = String.Join(",", page.GetSelectedUsernames());
            if (UserSelect_Label.Text == "Могут просматривать")
            {
                usersCanViewString = UserListTextBox.Text;
            }
            else
            {
                usersCanEditString = UserListTextBox.Text;
            }
        }
        //  преобразование списков пользователей в строки
        private string stringifyUserList(List<OBJ_User> users)
        {
            List<string> usernames = new List<string>();
            foreach (var user in users)
            {
                usernames.Add(user.Username);
            }
            return String.Join(",", usernames);
        }
        // преобразование строки в список пользователей
        private List<OBJ_User> getUsersFromString(string str)
        {
            List<OBJ_User> result = new List<OBJ_User>();
            List<OBJ_User> recievedUserList = DatabaseCommunicator.GET_Users("*").ToList(); // обращение в бд
            foreach (var username in str.Split(','))
            {
                if (username == "*")
                {
                    result.Add(Application.Current.Properties["AllUsers"] as OBJ_User);
                    continue;
                }
                OBJ_User foundUser = recievedUserList.Find(entry => entry.Username == username); // поиск пользователей из бд с нужным именем
                if (foundUser != null) { result.Add(foundUser); }
            }
            return result;
        }
        // событие нажатия кнопки переключения на изменение прав на просмотр
        private void Button_ModeSelect_View_Click(object sender, RoutedEventArgs e)
        {
            UserSelect_Label.Text = "Могут просматривать";
            UserSelect_TextBox.Text = usersCanViewString;
            Button_ModeSelect_Edit.Style = (Style)FindResource("ButtonStyle_Inactive");
            Button_ModeSelect_View.Style = (Style)FindResource("ButtonStyle_Main");
        }
        // событие нажатия кнопки переключения на изменение прав на редактирование
        private void Button_ModeSelect_Edit_Click(object sender, RoutedEventArgs e)
        {
            UserSelect_Label.Text = "Могут редактировать";
            UserSelect_TextBox.Text = usersCanEditString;
            Button_ModeSelect_Edit.Style = (Style)FindResource("ButtonStyle_Main");
            Button_ModeSelect_View.Style = (Style)FindResource("ButtonStyle_Inactive");
        }
        // событие нажатие кнопки удаления доски
        private void Button_DeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            bool success = DatabaseCommunicator.DEL_Board(Board);
            if (success)
            {
                NavigationService.Navigate(new HomePage());
            }
        }
        // событие нажатия кнопки переключения на страницу выбора пользователей
        private void Button_GoToUserSelect_Click(object sender, RoutedEventArgs e)
        {
            OBJ_User currentUser = Application.Current.Properties["CurrentUser"] as OBJ_User; // текущий пользователь
            List<OBJ_User> userList = DatabaseCommunicator.GET_Users("*").ToList(); // получение всех пользователей из бд
            int foundUserIndex = userList.FindIndex(entry => entry.Username == currentUser.Username);
            if (foundUserIndex >= 0) userList.RemoveAt(foundUserIndex); // удаление из этого списка текущего пользователя
            UserSelectPage newUserSelectPage = new UserSelectPage(userList, UserSelect_TextBox.Text.Split(','), this); // создание страницы с нужным списком пользователей
            NavigationService.Navigate(newUserSelectPage);
        }
        // событие нажатия кнопки возвращения на предыдущую страницу
        private void TopButton_GoBack_Click(object sender, RoutedEventArgs e)
        {
        // преобразование строки в список пользователей
            Board.UsersCanView = getUsersFromString(usersCanViewString);
            Board.UsersCanEdit = getUsersFromString(usersCanEditString);
            DatabaseCommunicator.UPDATE_Board(Board); // обновление доски в бд
            NavigationService.Navigate(new BoardPage(Board)); // переход на страницу доски
        }
    }
}
