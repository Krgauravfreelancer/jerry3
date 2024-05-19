using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;

namespace ManageMedia_UserControl.Classes.TimeLine.DrawEngine
{
    internal  class DrawMainCursor
    {
         internal void Draw(Canvas MainCanvas, TimeSpan MainCursorTime, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, Rectangle MainCursor, DrawProperties DrawProperties)
        {
            try
            {
                MainCanvas.Children.Remove(MainCursor);

                if (MainCursorTime > ViewPortStart && MainCursorTime < ViewPortStart + ViewPortDuration)
                {
                    double HeaderHeight = DrawProperties.TimeScaleHeight + 20;
                    MainCursor.Margin = new Thickness(TimeLineHelpers.GetLocationByTimeSpan(MainCanvas, ViewPortStart, ViewPortDuration, MainCursorTime), HeaderHeight, 0, 0);
                    MainCursor.Height = MainCanvas.ActualHeight - HeaderHeight;
                    MainCanvas.Children.Add(MainCursor);
                }
            } 
            catch 
            {
                MainCanvas.Children.Remove(MainCursor);
            }
        }
    }
}
