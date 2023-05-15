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
    /// Interaction logic for HomeBoardListItem.xaml
    /// </summary>
    public partial class HomeBoardListItem : UserControl
    {
        public HomeBoardListItem(OBJ_Board bindedBoardObject)
        {
            InitializeComponent();
            this.DataContext = bindedBoardObject;
        }

        // Курсор над карточкой
        private void CardMainContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        // Курсор не над карточкой
        private void CardMainContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
