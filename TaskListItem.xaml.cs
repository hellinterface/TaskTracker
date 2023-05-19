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
    // Кастомный элемент задачи (на странице просмотра карточки)
    //
    public partial class TaskListItem : UserControl//, ICheckElement
    {
        private OBJ_Board Board;
        private OBJ_Card Card;
        public OBJ_Task TaskObject { get; }
        private CardDetailsPage ParentPage; // Родительская страница

        public TaskListItem(OBJ_Board board, OBJ_Card card, OBJ_Task taskObject, CardDetailsPage parentPage)
        {
            InitializeComponent();
            Board = board;
            Card = card;
            TaskObject = taskObject;
            ParentPage = parentPage;
            this.DataContext = taskObject;
            UpdateAppearance(); // Обновить внешний вид
        }

        // Обновление внешнего вида
        private void UpdateAppearance()
        {
            if (TaskObject.Done == true) // Отмечено как сделанное
            {
                Button_CheckBox.Style = (Style)FindResource("ButtonStyle_Main");
            }
            else // Не сделанное
            {
                Button_CheckBox.Style = (Style)FindResource("ButtonStyle_Inactive");
            }
        }

        // Нажатие на кнопку сделано/не сделано
        private void Button_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            TaskObject.Done = !TaskObject.Done; // Поменять отметку в объекте задачи
            UpdateAppearance(); // Обновить внешний вид
            DatabaseCommunicator.UPDATE_Task(Board, TaskObject); // Запрос в БД
        }

        // Нажатие на кнопку удаления задачи
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            bool success = DatabaseCommunicator.DEL_Task(Board, Card, TaskObject); // Запрос в БД
            if (success == true)
            {
                ParentPage.OnTaskDelete(this);
            }
        }

        // Событие потери фокуса на поле ввода текста задачи
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Не используем байндинг TwoWay, потому что это событие срабатывает до обновления свойства Text.
            if (TaskObject.Text != TextBox_Text.Text)
            {
                TaskObject.Text = TextBox_Text.Text; // Обновление свойства в объекте задачи
                DatabaseCommunicator.UPDATE_Task(Board, TaskObject); // Запрос в БД
            }
        }
    }
}
