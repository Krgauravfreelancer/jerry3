using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ManageMedia_UserControl.Classes.TimeLine.DrawEngine
{
    internal  class DrawProperties
    {
        internal  List<TimeSpan> TimeStampResolutions = new List<TimeSpan>() {
                //Min Amount
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(6),
                    TimeSpan.FromSeconds(7),
                    TimeSpan.FromSeconds(8),
                    TimeSpan.FromSeconds(9),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(20),
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromMinutes(1),
                    TimeSpan.FromMinutes(2),
                    TimeSpan.FromMinutes(3),
                    TimeSpan.FromMinutes(4),
                    TimeSpan.FromMinutes(5),
                    TimeSpan.FromMinutes(10),
                //Max Amount
            };

        internal  SolidColorBrush TimeLineLineStrokColor = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        internal  LinearGradientBrush TimeLineBackgroundColor = new LinearGradientBrush(Color.FromArgb(100, 230, 230, 230), Color.FromArgb(100, 150, 150, 150), new Point(0.5, 0), new Point(0.5, 1));

        internal  SolidColorBrush TimeLineBackgroundGridColor1 = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
        internal  SolidColorBrush TimeLineBackgroundGridColor2 = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));

        internal  double TimeScaleHeight = 10;
    }
}
