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
    // Страница: Домашняя (список досок)
    //
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();

            OBJ_User currentUser = Application.Current.Properties["CurrentUser"] as OBJ_User; // текущий пользователь

            // Запрос
            List<OBJ_Board> boardList = DatabaseCommunicator.GET_Boards($"owner|=|{currentUser.Username}").ToList();
            // Доски с прямым разрешением на просмотр
            foreach (var board in DatabaseCommunicator.GET_Boards($"users_can_view|LISTCONTAINS|{currentUser.Username}"))
            {
                if (boardList.FindIndex(entry => entry.ID == board.ID) < 0) boardList.Add(board); 
            }
            // Общественные доски (которые могут смотреть все)
            foreach (var board in DatabaseCommunicator.GET_Boards($"users_can_view|LISTCONTAINS|*"))
            {
                if (boardList.FindIndex(entry => entry.ID == board.ID) < 0) boardList.Add(board);
            }

            // Создание элементов
            foreach (OBJ_Board board in boardList)
            {
                AddBoardListItem(board);
            }
        }

        public HomeBoardListItem AddBoardListItem(OBJ_Board board)
        {
            var newListItem = new HomeBoardListItem(board);
            newListItem.MouseDown += (sender, e) => ListItem_Click(sender as HomeBoardListItem, e); // Нажатие на элемент
            MainHorizontalStackPanel.Children.Add(newListItem); // Добавление элемента в список на экране
            return newListItem;
        }
        
        // Нажатие на элемент доски
        private void ListItem_Click(HomeBoardListItem sender, RoutedEventArgs e)
        {
            BoardPage tempDetailsPage = new BoardPage(sender.DataContext as OBJ_Board);
            NavigationService.Navigate(tempDetailsPage);
        }

        // Нажатие на кнопку создания доски
        private void Button_AddBoard_Click(object sender, RoutedEventArgs e)
        {
            // Создание объекта доски
            OBJ_Board tempBoard = new OBJ_Board()
            {
                Owner = Application.Current.Properties["CurrentUser"] as OBJ_User
            };
            bool success = DatabaseCommunicator.ADD_Board(tempBoard); // Запрос в БД
            if (success) {
                //AddBoardListItem(tempBoard);
                BoardPage tempDetailsPage = new BoardPage(tempBoard); // Переход на страницу просмотра доски
                NavigationService.Navigate(tempDetailsPage);
            }
        }

        // Нажатие на кнопку выхода из аккаунта
        private void Button_SignOut_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["CurrentUser"] = null;
            NavigationService.Navigate(new LoginPage()); // Переход на страницу входа
        }
    }
}
