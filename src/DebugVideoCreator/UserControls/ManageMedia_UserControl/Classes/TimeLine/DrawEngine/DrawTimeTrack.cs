using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;

namespace ManageMedia_UserControl.Classes.TimeLine.DrawEngine
{
     internal class DrawTimeTrack
    {
       
         internal void Draw(Canvas MainCanvas, TimeSpan ViewportStart, Controls.TimeLine timeline, DrawProperties DrawProperties, (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result)
        {
            double TotalWidth = MainCanvas.ActualWidth;

            Line MainLine = new Line();
            MainLine.X1 = 0;
            MainLine.Y1 = 0;
            MainLine.X2 = TotalWidth;
            MainLine.Y2 = 0;
            MainLine.Stroke = DrawProperties.TimeLineLineStrokColor;

            timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, MainLine);

            //double SectionWidth = TotalWidth / Result.TimeStampCount;

            for (int i = 0; i < Result.TimeStampCount + 1; i++)
            {
                double LineX = Result.SectionWidth * i + Result.OffsetPixels;
                Line TimeLine = new Line();
                TimeLine.X1 = LineX;
                TimeLine.Y1 = 0;
                TimeLine.X2 = LineX;
                TimeLine.Y2 = DrawProperties.TimeScaleHeight;
                TimeLine.Stroke = DrawProperties.TimeLineLineStrokColor;
                timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, TimeLine);

                Label label = new Label();
                label.Foreground = DrawProperties.TimeLineLineStrokColor;
                label.Padding = new Thickness(0);
                label.Content = (TimeSpan.FromSeconds(Result.SectionTime.TotalSeconds * i + ViewportStart.TotalSeconds) + Result.OffsetTime).ToString("mm\\:ss\\:fff");
                label.Margin = new Thickness(LineX - 25, DrawProperties.TimeScaleHeight, 0, 0);
                timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, label);
            }
        }
    }
}
