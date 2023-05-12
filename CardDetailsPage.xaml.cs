using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
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
using static System.Net.Mime.MediaTypeNames;

namespace TaskTracker
{
    /// <summary>
    /// Interaction logic for CardDetailsPage.xaml
    /// </summary>
    public partial class CardDetailsPage : Page, IPageWithUserSelect
    {
        public TextBox UserListTextBox { get; set; }

        private BoardPage SenderPage;
        private BoardCard SenderElement;
        private OBJ_Card Card;
        private OBJ_Column Column;
        private OBJ_Board Board;
        public CardDetailsPage(OBJ_Card bindedCardObject, OBJ_Column column, BoardPage previousPage, BoardCard senderElement)
        {
            InitializeComponent();
            this.DataContext = bindedCardObject;
            SenderPage = previousPage;
            SenderElement = senderElement;
            Card = bindedCardObject;
            Column = column;
            Board = previousPage.Board;
            UserListTextBox = TextBox_UsersCanEdit;
            TextBox_Owner.Text = Card.Owner.Username;
            TextBox_UsersCanEdit.Text = stringifyUserList(Card.UsersCanEdit);

            OBJ_User currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as OBJ_User;

            if (Card.Owner.Username == currentUser.Username)
            {
            }
            else if (Card.UsersCanEdit.FindIndex(entry => (entry.Username == currentUser.Username) || (entry.Username == "*")) >= 0)
            {
                TopButton_Remove.IsEnabled = false;
                Button_GoToUserSelect.IsEnabled = false;
            }
            else
            {
                TextBox_HeaderTitle.IsEnabled = false;
                TextBox_Title.IsEnabled = false;
                TextBox_Color.IsEnabled = false;
                TextBox_Description.IsEnabled = false;
                TopButton_Remove.IsEnabled = false;
                Button_AddTask.IsEnabled = false;
                Button_AddImage.IsEnabled = false;
                Button_GoToUserSelect.IsEnabled = false;
                Container_Tasks.IsEnabled = false;
                Container_Images.IsEnabled = false;
            }

            // Цвет
            TextBox_Color.Text = new System.Windows.Media.ColorConverter().ConvertToString(Card.Color);
            SetBorderColor(TextBox_Color.Text);

            // Задачи
            Card.Tasks = DatabaseCommunicator.GET_TasksFromCard(Board, Card).ToList();
            foreach (var task in Card.Tasks)
            {
                TaskListContainer.Children.Add( new TaskListItem(Board, Card, task, this) );
            }

            // Изображения
            Card.Images = DatabaseCommunicator.GET_ImagesFromCard(Board, Card).ToList();
            foreach (var image in Card.Images)
            {
                System.Windows.Controls.Image newImageElement = new System.Windows.Controls.Image();
                newImageElement.Source = image.BitmapImage;
                newImageElement.MouseRightButtonDown += (sender, e) => RemoveImage(newImageElement);
                ImagesContainer.Children.Add(newImageElement);
            }
        }

        private bool RemoveImage(System.Windows.Controls.Image imageElement)
        {
            int foundIndex = Card.Images.FindIndex(entry => entry.BitmapImage == imageElement.Source);
            if (foundIndex >= 0)
            {
                bool success = DatabaseCommunicator.DEL_Image(Board, Card, Card.Images[foundIndex]);
                if (success)
                {
                    ImagesContainer.Children.Remove(imageElement);
                    return true;
                }
            }
            return false;
        }

        private List<OBJ_User> getUsersFromString(string str)
        {
            List<OBJ_User> result = new List<OBJ_User>();
            List<OBJ_User> recievedUserList = DatabaseCommunicator.GET_Users("*").ToList();
            foreach (var username in str.Split(','))
            {
                if (username == "*")
                {
                    result.Add(System.Windows.Application.Current.Properties["AllUsers"] as OBJ_User);
                    continue;
                }
                OBJ_User foundUser = recievedUserList.Find(entry => entry.Username == username);
                if (foundUser != null) { result.Add(foundUser); }
            }
            return result;
        }

        // Поставить пользователей,выбранных на странице выбора пользователей, в текстовое поле
        void IPageWithUserSelect.SetUsersFromUserSelectPage(UserSelectPage page)
        {
            UserListTextBox.Text = String.Join(",", page.GetSelectedUsernames());
            Card.UsersCanEdit = getUsersFromString(UserListTextBox.Text);
            DatabaseCommunicator.UPDATE_Card(Board, Card);
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

        // Обновление цвета границы
        private void SetBorderColor(string hex)
        {
            try
            {
                SolidColorBrush brush = new BrushConverter().ConvertFrom(hex) as SolidColorBrush;
                MainContainer.BorderBrush = brush;
                Card.Color = brush.Color;
            }
            catch { }
        }

        // Кнопка удаления
        private void TopButton_Remove_Click(object sender, RoutedEventArgs e)
        {
            Column.Cards.Remove(Card);
            bool success = DatabaseCommunicator.DEL_Card(Board, Column, Card);
            if (success == true)
            {
                NavigationService.Navigate(
                    new BoardPage(Board)
                );
            }
            else
            {
                Column.Cards.Remove(Card);
            }
        }

        // Кнопка назад
        private void TopButton_GoBack_Click(object sender, RoutedEventArgs e)
        {
            DatabaseCommunicator.UPDATE_Card(Board, Card);
            SenderElement.UpdateColor(Card.Color);
            NavigationService.Navigate(SenderPage);
        }

        // Кнопка выбора пользователей
        private void Button_GoToUserSelect_Click(object sender, RoutedEventArgs e)
        {
            OBJ_User currentUser = System.Windows.Application.Current.Properties["CurrentUser"] as OBJ_User;
            List<OBJ_User> userList = DatabaseCommunicator.GET_Users("*").ToList();
            int foundUserIndex = userList.FindIndex(entry => entry.Username == currentUser.Username);
            if (foundUserIndex >= 0) userList.RemoveAt(foundUserIndex);
            UserSelectPage newUserSelectPage = new UserSelectPage(userList, UserListTextBox.Text.Split(','), this);
            NavigationService.Navigate(newUserSelectPage);
        }

        // Событие ввода цвета в поле
        private void TextBox_Color_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetBorderColor(TextBox_Color.Text);
        }

        // Событие удаления задачи
        public void OnTaskDelete(TaskListItem senderElement)
        {
            TaskListContainer.Children.Remove(senderElement);
        }

        // Кнопка добавления задачи
        private void Button_AddTask_Click(object sender, RoutedEventArgs e)
        {
            OBJ_Task task = new OBJ_Task() { };
            Card.Tasks.Add(task);
            bool success = DatabaseCommunicator.ADD_Task(Board, Card, task);
            if (success == true)
            {
                TaskListItem newTaskElement = new TaskListItem(Board, Card, task, this);
                TaskListContainer.Children.Add(newTaskElement);
            }
            else
            {
                Card.Tasks.Remove(task);
            }
        }

        // Кнопка добавления изображения
        private void Button_AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string base64string = Convert.ToBase64String(File.ReadAllBytes(openFileDialog.FileName));
                OBJ_Image img = new OBJ_Image() {
                    Base64 = base64string
                };
                Card.Images.Add(img);

                bool success = DatabaseCommunicator.ADD_Image(Board, Card, img);
                if (success == true)
                {
                    System.Windows.Controls.Image newImageElement = new System.Windows.Controls.Image();
                    newImageElement.Source = img.BitmapImage;
                    ImagesContainer.Children.Add(newImageElement);
                }
                else
                {
                    Card.Images.Remove(img);
                }
            }
        }
    }
}
