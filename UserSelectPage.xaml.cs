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
    /// Interaction logic for UserSelectPage.xaml
    /// </summary>
    public partial class UserSelectPage : Page
    {
        private IPageWithUserSelect PreviousPage; // Страница

        public UserSelectPage(IEnumerable<OBJ_User> users, IEnumerable<string> selectedUsernames, IPageWithUserSelect previousPage)
        {
            InitializeComponent();
            PreviousPage = previousPage;

            UserSelectItem allUsersItem = new UserSelectItem(Application.Current.Properties["AllUsers"] as OBJ_User);
            allUsersItem.TextBlock_Username.Text = "Все пользователи";
            if (selectedUsernames.Contains("*")) { allUsersItem.IsChecked = true; } else { allUsersItem.IsChecked = false; }
            MainStackPanel.Children.Add(allUsersItem);

            foreach (var user in users)
            {
                UserSelectItem newListItem = new UserSelectItem(user);
                if (selectedUsernames.Contains(user.Username)) newListItem.IsChecked = true;
                MainStackPanel.Children.Add(newListItem);
            }
        }

        public string[] GetSelectedUsernames()
        {
            List<string> result = new List<string>();
            foreach (UserSelectItem element in MainStackPanel.Children)
            {
                if (element.IsChecked == true) result.Add(element.User.Username);
            }
            return result.ToArray();
        }

        private void TopButton_GoBack_Click(object sender, RoutedEventArgs e)
        {
            //PreviousPage.UserListTextBox.Text = String.Join(",", GetSelectedUsernames());
            PreviousPage.SetUsersFromUserSelectPage(this);
            NavigationService.Navigate(PreviousPage);
        }
    }
}
