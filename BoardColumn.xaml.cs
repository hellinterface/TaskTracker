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
        public OBJ_Column ColumnObject { get; }
        private BoardPage ParentPage;
        private Action<BoardCard, MouseButtonEventArgs> CardClickFunction; // функция, которая должна происходить при нажатии на карточку
        private bool IsViewingUser_CanEdit; // может ли текущий пользователь редактировать доску

        public BoardColumn(BoardPage parentPage, OBJ_Board board, OBJ_Column column, Action<BoardCard, MouseButtonEventArgs> cardClickFunction, bool IsViewingUser_CanEdit)
        {
            InitializeComponent();
            Board = board;
            ColumnObject = column;
            ParentPage = parentPage;
            CardClickFunction = cardClickFunction;
            this.DataContext = column;
            this.IsViewingUser_CanEdit = IsViewingUser_CanEdit;
// если пользователь может вносить изменения, то кнопки существуют, иначе кнопки не работают/исчезают
            if (IsViewingUser_CanEdit == true)
            { // назначение кнопкам функций (при нажатии на кнопку что-то происходит)
                this.TopButton_Left.Click += (sender, e) => ParentPage.MoveColumnElementLeft(this);
                this.TopButton_Right.Click += (sender, e) => ParentPage.MoveColumnElementRight(this);
                this.TopButton_DeleteColumn.Click += (sender, e) => ParentPage.DeleteColumn(this);
            }
            else
            { // скрыть и заблокировать элементы
                this.TopButtonContainer.Visibility = Visibility.Collapsed;
                this.TopButton_AddCard.IsEnabled = false;
                this.TopButton_Left.IsEnabled = false;
                this.TopButton_Right.IsEnabled = false;
                this.TopButton_DeleteColumn.IsEnabled = false;
                this.TextBox_ColumnTitle.IsEnabled = false;
            }

            // Добавление карточек в текущий создаваемый столбец
            List<OBJ_Card> allCards = ColumnObject.Cards.ToList();
            allCards = allCards.OrderBy(entry => entry.Position).ToList();
            foreach (var card in allCards)
            {
                var cardElement = AddCardElement(card); // создать элемент
            }
        }

        // Нажатие кнопки "Добавить карточку"
        private void TopButton_AddCard_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового объекта карточки
            var tempCard = new OBJ_Card()
            {
                Position = ColumnObject.Cards.Count,
                Owner = Application.Current.Properties["CurrentUser"] as OBJ_User
            };

            // Привязка карточки к столбцу и запрос на добавление
            ColumnObject.Cards.Add(tempCard);
            bool result = DatabaseCommunicator.ADD_Card(Board, ColumnObject, tempCard);
            if (result == true)
            {
                AddCardElement(tempCard); // Удалось добавить в БД, добавляем элемент карточки на страницу
            }
            else
            {
                ColumnObject.Cards.Remove(tempCard); // Не удалось
            }
        }

        // Добавление элемента карточки
        public BoardCard AddCardElement(OBJ_Card cardObject)
        {
            var newCardElement = new BoardCard(Board, cardObject);
            newCardElement.MouseDown += (sender, e) => this.CardClickFunction(newCardElement, e);
            if (IsViewingUser_CanEdit == true)
            { // назначение кнопкам функций 
                newCardElement.Button_Left.Click += (sender, e) => ParentPage.MoveCardLeft(this, newCardElement);
                newCardElement.Button_Right.Click += (sender, e) => ParentPage.MoveCardRight(this, newCardElement);
                newCardElement.Button_Up.Click += (sender, e) => this.MoveCardElementUp(newCardElement);
                newCardElement.Button_Down.Click += (sender, e) => this.MoveCardElementDown(newCardElement);
            }
            else
            {   // скрыть и заблокировать элементы
                newCardElement.Button_Left.Visibility = Visibility.Collapsed;
                newCardElement.Button_Right.Visibility = Visibility.Collapsed;
                newCardElement.Button_Up.Visibility = Visibility.Collapsed;
                newCardElement.Button_Down.Visibility = Visibility.Collapsed;
            }
            this.CardList.Children.Add(newCardElement);
            return newCardElement;
        }

        // Удаление элемента карточки
        public OBJ_Card RemoveCardElement(BoardCard cardElement)
        {
            this.CardList.Children.Remove(cardElement);
            OnCardsChange();
            return cardElement.CardObject;
        }

        // Передвинуть карточку вверх
        private bool MoveCardElementUp(BoardCard cardElement)
        {
            int previousPosition = CardList.Children.IndexOf(cardElement); // получение индекса элемента до перемещения
            if (previousPosition == 0) return false; // передвигать некуда
            CardList.Children.RemoveAt(previousPosition); // удаление элемента
            CardList.Children.Insert(previousPosition - 1, cardElement); // вставка элемента на новую позицию
            OnCardsChange(); // обновление карточек
            return true;
        }

        // Передвинуть карточку вниз
        private bool MoveCardElementDown(BoardCard cardElement)
        {
            int previousPosition = CardList.Children.IndexOf(cardElement);
            if (previousPosition == CardList.Children.Count-1) return false;
            CardList.Children.RemoveAt(previousPosition);
            CardList.Children.Insert(previousPosition + 1, cardElement);
            OnCardsChange();
            return true;
        }

        // Обновить все карточки в базе данных
        public void OnCardsChange()
        {
            foreach (BoardCard cardElement in CardList.Children)
            {
                cardElement.CardObject.Position = CardList.Children.IndexOf(cardElement);
                DatabaseCommunicator.UPDATE_Card(Board, cardElement.CardObject); // SET
            }
        }

        private void TextBox_ColumnTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            // Не используем байндинг TwoWay, потому что это событие срабатывает до обновления свойства Title.
            if (ColumnObject.Title != TextBox_ColumnTitle.Text)
            {
                ColumnObject.Title = TextBox_ColumnTitle.Text;
                DatabaseCommunicator.UPDATE_Column(Board, ColumnObject);
            }
        }
    }
}
