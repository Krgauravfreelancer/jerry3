using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ManageMedia_UserControl
{
    internal static class TimeLineHelpers
    {
        internal static double GetWidthByTimeSpan(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, TimeSpan time)
        {
            double TotalWidth = MainCanvas.ActualWidth;
            return TotalWidth * (time.TotalSeconds / ViewPortDuration.TotalSeconds);
        }

        internal static TimeSpan GetTimeSpanByLocation(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, double location)
        {
            double TotalWidth = MainCanvas.ActualWidth;
            return ViewPortStart + TimeSpan.FromSeconds(ViewPortDuration.TotalSeconds * (location / TotalWidth));
        }

        internal static double GetLocationByTimeSpan(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, TimeSpan time)
        {
            double TotalWidth = MainCanvas.ActualWidth;
            return TotalWidth * ((time.TotalSeconds - ViewPortStart.TotalSeconds) / ViewPortDuration.TotalSeconds);
        }


    }
}
