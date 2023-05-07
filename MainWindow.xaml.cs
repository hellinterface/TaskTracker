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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            SocketClient.StartClient();

            // Если поставить это, то права на редактирование/просмотр будут у всех пользователей
            Application.Current.Properties["AllUsers"] = new OBJ_User() { Username = "*", Password = "*" };

            // Задание текущего пользователя
            // То есть запись его в некую глобальную переменную Application.Current.Properties["CurrentUser"]
            Application.Current.Properties["CurrentUser"] = new OBJ_User() { Username = "user1", Password = "pass" };

        }
    }
}
