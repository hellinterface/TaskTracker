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

            // Запрос
            OBJ_Board[] allBoards = DatabaseCommunicator.GET_Boards("*");

            // Создание элементов
            foreach(OBJ_Board board in allBoards)
            {
                AddBoardListItem(board);
            }
        }

        public HomeBoardListItem AddBoardListItem(OBJ_Board board)
        {
            var newListItem = new HomeBoardListItem(board);
            newListItem.MouseDown += (sender, e) => ListItem_Click(sender as HomeBoardListItem, e);
            MainHorizontalStackPanel.Children.Add(newListItem);
            return newListItem;
        }

        private void ListItem_Click(HomeBoardListItem sender, RoutedEventArgs e)
        {
            BoardPage tempDetailsPage = new BoardPage(sender.DataContext as OBJ_Board);
            NavigationService.Navigate(tempDetailsPage);
        }

        private void Button_AddBoard_Click(object sender, RoutedEventArgs e)
        {
            OBJ_Board tempBoard = new OBJ_Board()
            {
                Owner = Application.Current.Properties["CurrentUser"] as OBJ_User
            };
            bool success = DatabaseCommunicator.ADD_Board(tempBoard);
            if (success) { AddBoardListItem(tempBoard); }
        }
    }
}
