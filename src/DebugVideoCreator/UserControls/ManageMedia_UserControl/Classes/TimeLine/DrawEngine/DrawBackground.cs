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
     internal class DrawBackground
    {
         internal void Draw(Canvas MainCanvas, Canvas LegendCanvas, Controls.TimeLine timeline, DrawProperties DrawProperties, (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result)
        {

            double TotalWidth = MainCanvas.ActualWidth;
            double TotalHeight = MainCanvas.ActualHeight;

            double SectionWidth = Result.SectionWidth / 4;
            bool Stagger = false;
            for (int i = 0; i < (Result.TimeStampCount + 2) * 4; i++)
            {
                double LineX = SectionWidth * i + Result.OffsetPixels - Result.SectionWidth;
                Rectangle TimeLineGridRect = new Rectangle();
                TimeLineGridRect.Margin = new Thickness(LineX, 0, 0, 0);
                TimeLineGridRect.Width = SectionWidth + 1;
                TimeLineGridRect.Height = TotalHeight;
                if (Stagger == false)
                {
                    TimeLineGridRect.Fill = DrawProperties.TimeLineBackgroundGridColor1;
                }
                else
                {
                    TimeLineGridRect.Fill = DrawProperties.TimeLineBackgroundGridColor2;
                }
                Stagger = !Stagger;
                timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, TimeLineGridRect);
            }

            Rectangle TimeSpanRect = new Rectangle();
            TimeSpanRect.SnapsToDevicePixels = true;
            TimeSpanRect.Height = DrawProperties.TimeScaleHeight + 20;
            TimeSpanRect.Width = TotalWidth;
            TimeSpanRect.Fill = DrawProperties.TimeLineBackgroundColor;
            timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, TimeSpanRect);
            //---------------------------------------------------------------------


            //NoteTrack 
            //Callout - 2
            //Callout - 1
            //VideoEvent
            //Audio

            double HeaderHeight = DrawProperties.TimeScaleHeight + 20;
            double RemainingHeight = LegendCanvas.ActualHeight - HeaderHeight;
            double NoteTrackHeight = RemainingHeight / 5 * 1.5;
            double TrackHeight = (RemainingHeight - NoteTrackHeight) / 4;

            for (int i = 0; i < 4; i++)
            {
                Line TrackLine = new Line();
                TrackLine.X1 = 0;
                TrackLine.Y1 = HeaderHeight + NoteTrackHeight + (TrackHeight * i);
                TrackLine.X2 = TotalWidth;
                TrackLine.Y2 = HeaderHeight + NoteTrackHeight + (TrackHeight * i);
                TrackLine.Stroke = Brushes.LightGray;

                timeline.TimeLineDrawEngine.AddTimeScaleElement_ToCanvas(MainCanvas, TrackLine);
            }
        }
    }
}
