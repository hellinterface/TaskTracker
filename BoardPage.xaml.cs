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
    // Страница: Доска
    //
    public partial class BoardPage : Page
    {
        public BoardPage(OBJ_Board bindedBoardObject)
        {
            InitializeComponent();
            this.DataContext = bindedBoardObject;
            //  Функция, которая будет выполняться при нажатии на карточку
            Action<BoardCard, MouseButtonEventArgs> cardClickFunction = CardItem_Click;

            // Запрос на столбцы
            OBJ_Column[] allColumns = DatabaseCommunicator.GET_Columns(bindedBoardObject.ID, "*");

            foreach (var column in allColumns)
            {
                // Создание элемента столбца
                var newBoardColumnElement = new BoardColumn(bindedBoardObject, column, CardItem_Click);

                // Добавление карточек в текущий создаваемый столбец
                foreach (var cardID in column.CardIDs)
                {
                    OBJ_Card card = DatabaseCommunicator.GET_Cards(bindedBoardObject.ID, $"id|=|{cardID}")[0];
                    var cardElement = newBoardColumnElement.AddCard(card);
                }
                MainHorizontalStackPanel.Children.Add(newBoardColumnElement);
            }

        }

        private void CardItem_Click(BoardCard sender, MouseButtonEventArgs e)
        {
            CardDetailsPage tempDetailsPage = new CardDetailsPage(sender.DataContext as OBJ_Card);
            NavigationService.Navigate(tempDetailsPage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HomePage tempHomePage = new HomePage();
            NavigationService.Navigate(tempHomePage);
        }
    }
}
