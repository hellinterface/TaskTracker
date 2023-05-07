﻿using System;
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
    // Кастомный элемент: Карточка
    //
    public partial class BoardCard : UserControl
    {
        public BoardCard(OBJ_Card bindedCardObject)
        {
            InitializeComponent();
            this.DataContext = bindedCardObject;

            Color tempColor;
            if (bindedCardObject.Color != null)
            {
                tempColor = new Color();
                tempColor.A = 255;
                tempColor.R = bindedCardObject.Color.R;
                tempColor.G = bindedCardObject.Color.G;
                tempColor.B = bindedCardObject.Color.B;
            }
            else // Цвет должен быть задан, но на всякий случай поставим серый
            {
                tempColor = new Color();
                tempColor.A = 255;
                tempColor.R = 160;
                tempColor.G = 160;
                tempColor.B = 160;
            }
            // Красим элемент в соответствущий цвет, который указан в объекте карточки (OBJ_Card)
            CardMainContainer.BorderBrush = new SolidColorBrush(tempColor);
        }

        private void CardMainContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
        }

        private void CardMainContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
