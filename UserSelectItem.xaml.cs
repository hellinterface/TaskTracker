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
    /// Interaction logic for UserSelectItem.xaml
    /// </summary>
    public partial class UserSelectItem : UserControl
    {
        public OBJ_User User { get; }
        private bool isChecked = false;
        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                UpdateAppearance();
            }
        }

        public UserSelectItem(OBJ_User user, bool isChecked = false)
        {
            InitializeComponent();
            this.DataContext = user;
            User = user;
            IsChecked = isChecked;
            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (IsChecked == true)
            {
                CheckBox.Background = (SolidColorBrush)FindResource("ButtonBackgroundColor_Normal");
                CheckBoxIcon.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                CheckBox.Background = new SolidColorBrush(Colors.White);
                CheckBoxIcon.Foreground = (SolidColorBrush)FindResource("ColoredText");
            }
        }

        private void CheckBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }
}
