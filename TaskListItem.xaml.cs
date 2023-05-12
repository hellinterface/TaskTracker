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
    /// Interaction logic for TaskListItem.xaml
    /// </summary>
    public partial class TaskListItem : UserControl//, ICheckElement
    {
        private OBJ_Board Board;
        private OBJ_Card Card;
        public OBJ_Task TaskObject { get; }
        private CardDetailsPage ParentPage;

        public TaskListItem(OBJ_Board board, OBJ_Card card, OBJ_Task taskObject, CardDetailsPage parentPage)
        {
            InitializeComponent();
            Board = board;
            Card = card;
            TaskObject = taskObject;
            ParentPage = parentPage;
            this.DataContext = taskObject;
            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (TaskObject.Done == true)
            {
                Button_CheckBox.Style = (Style)FindResource("ButtonStyle_Main");
            }
            else
            {
                Button_CheckBox.Style = (Style)FindResource("ButtonStyle_Inactive");
            }
        }

        private void Button_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            TaskObject.Done = !TaskObject.Done;
            UpdateAppearance();
            DatabaseCommunicator.UPDATE_Task(Board, TaskObject);
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            bool success = DatabaseCommunicator.DEL_Task(Board, Card, TaskObject);
            if (success == true)
            {
                ParentPage.OnTaskDelete(this);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Не используем байндинг TwoWay, потому что это событие срабатывает до обновления свойства Title.
            if (TaskObject.Text != TextBox_Text.Text)
            {
                TaskObject.Text = TextBox_Text.Text;
                DatabaseCommunicator.UPDATE_Task(Board, TaskObject);
            }
        }
    }
}
