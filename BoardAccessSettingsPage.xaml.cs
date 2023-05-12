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
    /// Interaction logic for BoardAccessSettingsPage.xaml
    /// </summary>
    public partial class BoardAccessSettingsPage : Page, IPageWithUserSelect
    {
        public TextBox UserListTextBox { get; set; }
        private OBJ_Board Board;

        private string usersCanViewString;
        private string usersCanEditString;

        public BoardAccessSettingsPage(OBJ_Board board)
        {
            InitializeComponent();
            this.DataContext = board;
            this.Board = board;
            UserListTextBox = UserSelect_TextBox;
            usersCanViewString = stringifyUserList(Board.UsersCanView);
            usersCanEditString = stringifyUserList(Board.UsersCanEdit);
            UserSelect_TextBox.Text = usersCanViewString;
        }

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

        private string stringifyUserList(List<OBJ_User> users)
        {
            List<string> usernames = new List<string>();
            foreach (var user in users)
            {
                usernames.Add(user.Username);
            }
            return String.Join(",", usernames);
        }

        private List<OBJ_User> getUsersFromString(string str)
        {
            List<OBJ_User> result = new List<OBJ_User>();
            List<OBJ_User> recievedUserList = DatabaseCommunicator.GET_Users("*").ToList();
            foreach (var username in str.Split(','))
            {
                if (username == "*")
                {
                    result.Add(Application.Current.Properties["AllUsers"] as OBJ_User);
                    continue;
                }
                OBJ_User foundUser = recievedUserList.Find(entry => entry.Username == username);
                if (foundUser != null) { result.Add(foundUser); }
            }
            return result;
        }

        private void Button_ModeSelect_View_Click(object sender, RoutedEventArgs e)
        {
            UserSelect_Label.Text = "Могут просматривать";
            UserSelect_TextBox.Text = usersCanViewString;
            Button_ModeSelect_Edit.Style = (Style)FindResource("ButtonStyle_Inactive");
            Button_ModeSelect_View.Style = (Style)FindResource("ButtonStyle_Main");
        }

        private void Button_ModeSelect_Edit_Click(object sender, RoutedEventArgs e)
        {
            UserSelect_Label.Text = "Могут редактировать";
            UserSelect_TextBox.Text = usersCanEditString;
            Button_ModeSelect_Edit.Style = (Style)FindResource("ButtonStyle_Main");
            Button_ModeSelect_View.Style = (Style)FindResource("ButtonStyle_Inactive");
        }

        private void Button_DeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            bool success = DatabaseCommunicator.DEL_Board(Board);
            if (success)
            {
                NavigationService.Navigate(new HomePage());
            }
        }

        private void Button_GoToUserSelect_Click(object sender, RoutedEventArgs e)
        {
            OBJ_User currentUser = Application.Current.Properties["CurrentUser"] as OBJ_User;
            List<OBJ_User> userList = DatabaseCommunicator.GET_Users("*").ToList();
            int foundUserIndex = userList.FindIndex(entry => entry.Username == currentUser.Username);
            if (foundUserIndex >= 0) userList.RemoveAt(foundUserIndex);
            UserSelectPage newUserSelectPage = new UserSelectPage(userList, UserSelect_TextBox.Text.Split(','), this);
            NavigationService.Navigate(newUserSelectPage);
        }

        private void TopButton_GoBack_Click(object sender, RoutedEventArgs e)
        {
            Board.UsersCanView = getUsersFromString(usersCanViewString);
            Board.UsersCanEdit = getUsersFromString(usersCanEditString);
            DatabaseCommunicator.UPDATE_Board(Board);
            NavigationService.Navigate(new BoardPage(Board));
        }
    }
}
