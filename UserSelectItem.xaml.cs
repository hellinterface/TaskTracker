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
    // Кастомный элемент пользователя (на странице выбора пользователей)
    //
    public partial class UserSelectItem : UserControl
    {
        public OBJ_User User { get; } // Объект пользователя
        private bool isChecked = false; // Отмечено?
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                UpdateAppearance(); // Обновить внешний вид
            }
        }
        
        public UserSelectItem(OBJ_User user, bool isChecked = false)
        {
            InitializeComponent();
            this.DataContext = user;
            User = user;
            IsChecked = isChecked;
            UpdateAppearance(); // Обновить внешний вид
        }

        // Функция обновления внешнего вида элемента
        private void UpdateAppearance()
        {
            if (IsChecked == true) // Отмечено выбранным
            {
                CheckBox.Background = (SolidColorBrush)FindResource("ButtonBackgroundColor_Normal");
                CheckBoxIcon.Foreground = new SolidColorBrush(Colors.White);
            }
            else // Не отмечено
            {
                CheckBox.Background = new SolidColorBrush(Colors.White);
                CheckBoxIcon.Foreground = (SolidColorBrush)FindResource("ColoredText");
            }
        }

        // Нажатие на чекбокс (кнопка выбрано/не выбрано)
        private void CheckBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }
}
