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
    // Кастомный элемент: Колонка/столбец доски
    //
    public partial class BoardColumn : UserControl
    {
        private OBJ_Board Board;
        private OBJ_Column Column;
        private Action<BoardCard, MouseButtonEventArgs> CardClickFunction;

        public BoardColumn(OBJ_Board board, OBJ_Column column, Action<BoardCard, MouseButtonEventArgs> cardClickFunction)
        {
            InitializeComponent();
            Board = board;
            Column = column;
            CardClickFunction = cardClickFunction;
        }

        // Нажатие кнопки "Добавить карточку"
        private void TopButton_AddCard_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового объекта карточки
            var tempCard = new OBJ_Card()
            {
                Position = Column.CardIDs.Count,
                Owner = Application.Current.Properties["CurrentUser"] as OBJ_User
            };

            // Привязка карточки к столбцу и запрос на добавление
            Column.CardIDs.Add(tempCard.ID);
            bool result = DatabaseCommunicator.ADD_Card(Board, Column, tempCard);
            if (result == true)
            {
                AddCard(tempCard); // Удалось добавить в БД, добавляем элемент карточки на страницу
            }
            else
            {
                Column.CardIDs.Remove(tempCard.ID); // Не удалось, тогда отменяем привязку
            }
        }

        // Добавление элемента карточки
        public BoardCard AddCard(OBJ_Card cardObject)
        {
            var newCardElement = new BoardCard(cardObject);
            newCardElement.MouseDown += (sender, e) => this.CardClickFunction(sender as BoardCard, e);
            this.CardList.Children.Add(newCardElement);
            return newCardElement;
        }
    }
}
