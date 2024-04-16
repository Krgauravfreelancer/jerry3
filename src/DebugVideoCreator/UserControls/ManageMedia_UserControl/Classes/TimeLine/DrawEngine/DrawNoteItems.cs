using ManageMedia_UserControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ManageMedia_UserControl.Classes.TimeLine.DrawEngine
{
     internal class DrawNoteItems
    {
         internal void Draw(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, List<NoteItemControl> ItemControlsOnTimeLine, List<NoteItemControl> NoteItemControls, Controls.TimeLine timeline, DrawProperties DrawProperties, (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result)
        {
            double TotalWidth = MainCanvas.ActualWidth;
            double HeaderHeight = DrawProperties.TimeScaleHeight + 20;
            double RemainingHeight = LegendCanvas.ActualHeight - HeaderHeight;
            double NoteTrackHeight = RemainingHeight / 5 * 1.5;

            if (NoteTrackHeight < 0)
            {
                NoteTrackHeight = 1;
            }

            for (int i = 0; i < ItemControlsOnTimeLine.Count; i++)
            {
                NoteItemControl control = ItemControlsOnTimeLine[i];
                MainCanvas.Children.Remove(control);
            }

            for (int i = 0; i < NoteItemControls.Count; i++)
            {
                NoteItemControl control = NoteItemControls[i];
                if (control.GetStartTime() < ViewPortStart + ViewPortDuration && control.GetDuration() + control.GetStartTime() > ViewPortStart)
                {
                    double Start = (control.GetStartTime().TotalSeconds - ViewPortStart.TotalSeconds) / ViewPortDuration.TotalSeconds;
                    double StartPosition = TotalWidth * Start;
                    double Width = TotalWidth * (control.GetDuration().TotalSeconds / ViewPortDuration.TotalSeconds);
                    control.Margin = new Thickness(StartPosition, HeaderHeight, 0, 0);
                    control.Width = Width;
                    control.Height = NoteTrackHeight;
                    control.BorderBrush = Brushes.Transparent;
                    control.BorderThickness = new Thickness(1, 0, 1, 0);

                    timeline.TimeLineDrawEngine.AddTextControlElement_ToCanvas(MainCanvas, control);
                }
            }
        }

    }
}
