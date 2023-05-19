using System;
using System.Collections.Generic;
using System.Data.Common;
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
        public OBJ_Board Board { get; }

        public bool IsViewingUser_Owner { get; protected set; } = false; //истина, если текущий пользователь-владелец
        public bool IsViewingUser_CanEdit { get; protected set; } = false; //истина, если текущий пользователь может редактировать доску

        public BoardPage(OBJ_Board bindedBoardObject)
        {
            InitializeComponent();
            this.DataContext = bindedBoardObject;
            Board = bindedBoardObject;
            //  Функция, которая будет выполняться при нажатии на карточку
            //Action<BoardCard, MouseButtonEventArgs> cardClickFunction = CardItem_Click;

            OBJ_User currentUser = Application.Current.Properties["CurrentUser"] as OBJ_User; // текущий пользователь



            if (currentUser.Username == Board.Owner.Username) // владелец или нет
            {
                IsViewingUser_Owner = true;
                IsViewingUser_CanEdit = true;
            }
            else if (Board.UsersCanEdit.FindIndex(entry => (entry.Username == currentUser.Username) || (entry.Username == "*")) >= 0) // имеет права на редактирование или нет
            {
                IsViewingUser_Owner = false;
                IsViewingUser_CanEdit = true;
            }
            else // может только просматривать
            {
                IsViewingUser_Owner = false;
                IsViewingUser_CanEdit = false;
            }

            // Запрос на столбцы
            List<OBJ_Column> allColumns = DatabaseCommunicator.GET_Columns(bindedBoardObject.ID, "*").ToList();
            allColumns = allColumns.OrderBy(entry => entry.Position).ToList(); // сортировка списка столбцов по свойству Position

            foreach (var column in allColumns)
            {
                AddColumn(column); // создание элемента столбца
            }
            // блокировка элементов в зависимости от прав пользователя
            if (IsViewingUser_Owner == false)
            {
                TopButton_AccessSettings.IsEnabled = false;
            }
            if (IsViewingUser_CanEdit == false)
            {
                TopButton_AccessSettings.IsEnabled = false;
                TopButton_AddColumn.IsEnabled = false;
                this.TextBox_BoardTitle.IsEnabled = false;
            }
        }

        // Назад
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HomePage tempHomePage = new HomePage();
            NavigationService.Navigate(tempHomePage);
        }

        private void TopButton_AddColumn_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового объекта столбца
            var tempColumn = new OBJ_Column()
            {
                Position = MainHorizontalStackPanel.Children.Count
            };

            // Привязка столбца к доске и запрос на добавление
            bool result = DatabaseCommunicator.ADD_Column(Board, tempColumn);
            if (result == true)
            {
                AddColumn(tempColumn); // Удалось добавить в БД, добавляем элемент колонки на страницу
            }
            else
            {
                // Не удалось
            }
        }

        // Обновление данных столбцов
        private void OnColumnsChange()
        {
            foreach (BoardColumn columnElement in MainHorizontalStackPanel.Children)
            {
                columnElement.ColumnObject.Position = MainHorizontalStackPanel.Children.IndexOf(columnElement);
                DatabaseCommunicator.UPDATE_Column(Board, columnElement.ColumnObject);
            }
        }

        // Переместить столбец влево
        public bool MoveColumnElementLeft(BoardColumn columnElement)
        {
            int previousPosition = MainHorizontalStackPanel.Children.IndexOf(columnElement);
            if (previousPosition == 0) return false;
            MainHorizontalStackPanel.Children.RemoveAt(previousPosition);
            MainHorizontalStackPanel.Children.Insert(previousPosition - 1, columnElement);
            OnColumnsChange();
            return true;
        }

        // Переместить столбец вправо
        public bool MoveColumnElementRight(BoardColumn columnElement)
        {
            int previousPosition = MainHorizontalStackPanel.Children.IndexOf(columnElement);
            if (previousPosition == MainHorizontalStackPanel.Children.Count-1) return false;
            MainHorizontalStackPanel.Children.RemoveAt(previousPosition);
            MainHorizontalStackPanel.Children.Insert(previousPosition + 1, columnElement);
            OnColumnsChange();
            return true;
        }
        // удаление столбца
        public bool DeleteColumn(BoardColumn column)
        {
            bool success = DatabaseCommunicator.DEL_Column(Board, column.ColumnObject);
            if (success == true)
            {
                MainHorizontalStackPanel.Children.Remove(column);
                OnColumnsChange();
                return true;
            }
            else return false;
        }

        // Добавление элемента столбца
        public BoardColumn AddColumn(OBJ_Column columnObject)
        {
            // Создание элемента столбца
            Action<BoardCard, MouseButtonEventArgs> cardClickFunction = (sender, args) =>
            {
                CardDetailsPage tempDetailsPage = new CardDetailsPage(sender.CardObject, columnObject, this, sender as BoardCard);
                NavigationService.Navigate(tempDetailsPage);
            };

            var newBoardColumnElement = new BoardColumn(this, Board, columnObject, cardClickFunction, IsViewingUser_CanEdit);
            if (IsViewingUser_CanEdit == true)
            {
            }
            MainHorizontalStackPanel.Children.Add(newBoardColumnElement);

            OnColumnsChange();
            return newBoardColumnElement;
        }
// перейти на стр. Настроек
        private void TopButton_AccessSettings_Click(object sender, RoutedEventArgs e)
        {
            BoardAccessSettingsPage tempSettingsPage = new BoardAccessSettingsPage(Board);
            NavigationService.Navigate(tempSettingsPage);
        }

        private void TextBox_BoardTitle_LostFocus(object sender, RoutedEventArgs e)// при потере фокуса обновляются данные о доске
        {
            // Не используем байндинг TwoWay, потому что это событие срабатывает до обновления свойства Title.
            if (Board.Title != TextBox_BoardTitle.Text)
            {
                Board.Title = TextBox_BoardTitle.Text;
                DatabaseCommunicator.UPDATE_Board(Board);
            }
        }

        // Перемещение карточки влево
        public bool MoveCardLeft(BoardColumn oldColumn, BoardCard card)
        {
            int columnPosition = MainHorizontalStackPanel.Children.IndexOf(oldColumn);
            if (columnPosition == 0) return false;
            BoardColumn newColumn = MainHorizontalStackPanel.Children[columnPosition - 1] as BoardColumn;
            OBJ_Card cardObject = oldColumn.RemoveCardElement(card);
            newColumn.AddCardElement(cardObject);
            oldColumn.ColumnObject.Cards.Remove(cardObject);
            newColumn.ColumnObject.Cards.Add(cardObject);
            oldColumn.OnCardsChange();
            newColumn.OnCardsChange();
            OnColumnsChange();
            return true;
        }

        // Перемещение карточки вправо
        public bool MoveCardRight(BoardColumn oldColumn, BoardCard card)
        {
            int columnPosition = MainHorizontalStackPanel.Children.IndexOf(oldColumn);
            if (columnPosition == MainHorizontalStackPanel.Children.Count - 1) return false;
            BoardColumn newColumn = MainHorizontalStackPanel.Children[columnPosition + 1] as BoardColumn;
            OBJ_Card cardObject = oldColumn.RemoveCardElement(card);
            newColumn.AddCardElement(cardObject);
            oldColumn.ColumnObject.Cards.Remove(cardObject);
            newColumn.ColumnObject.Cards.Add(cardObject);
            oldColumn.OnCardsChange();
            newColumn.OnCardsChange();
            OnColumnsChange();
            return true;
        }
    }
}
