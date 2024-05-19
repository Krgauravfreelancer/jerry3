using ManageMedia_UserControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using NAudio.Mixer;
using System.Windows.Media.Imaging;

namespace ManageMedia_UserControl.Classes.TimeLine.DrawEngine
{
    internal class DrawMissingVideoEvents
    {
        internal void Draw(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, List<(Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration)> MissingTimeSpans, List<UIElement> TrackMediaElements, Controls.TimeLine timeline, DrawProperties DrawProperties, bool IsReadOnly, (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result, bool IsManageMedia = true)
        {
            for (int i = 0; i < TrackMediaElements.Count; i++)
            {
                MainCanvas.Children.Remove(TrackMediaElements[i]);
            }

            //NoteTrack 
            //Callout - 2
            //Callout - 1
            //VideoEvent
            //Audio
            double TotalWidth = MainCanvas.ActualWidth;
            double HeaderHeight = DrawProperties.TimeScaleHeight + 20;
            double RemainingHeight = LegendCanvas.ActualHeight - HeaderHeight;

            double NoteTrackHeight = RemainingHeight / 5 * 1.5;
            double TrackHeight = (RemainingHeight - NoteTrackHeight) / 4;
            //---------------------------------------------------------------------
            double CallOut2 = HeaderHeight + NoteTrackHeight + (TrackHeight * 0);
            double CallOut1 = HeaderHeight + NoteTrackHeight + (TrackHeight * 1);
            double VideoEvent = HeaderHeight + NoteTrackHeight + (TrackHeight * 2);
            double Audio = HeaderHeight + NoteTrackHeight + (TrackHeight * 3);


            if (TrackHeight < 1 || double.IsNaN(TrackHeight) || double.IsInfinity(TrackHeight))
            {
                TrackHeight = 1;
            }



            for (int i = 0; i < MissingTimeSpans.Count; i++)
            {
                (Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration) item = MissingTimeSpans[i];
                if (item.Start < ViewPortStart + ViewPortDuration && item.Duration + item.Start > ViewPortStart)
                {
                    //It should draw

                    double Start = (item.Start.TotalSeconds - ViewPortStart.TotalSeconds) / ViewPortDuration.TotalSeconds;
                    double StartPosition = TotalWidth * Start;
                    double Width = TotalWidth * (item.Duration.TotalSeconds / ViewPortDuration.TotalSeconds);

                    MissingVideoEventItem trackVideoEventItem = new MissingVideoEventItem(item, timeline, IsReadOnly);
                    //VideoEvent
                    trackVideoEventItem.Margin = new Thickness(StartPosition, VideoEvent, 0, 0);

                    trackVideoEventItem.Width = Width;
                    trackVideoEventItem.Height = TrackHeight;
                    trackVideoEventItem.BorderBrush = Brushes.White;
                    trackVideoEventItem.BorderThickness = new Thickness(1);
                    trackVideoEventItem.Background = new SolidColorBrush(trackVideoEventItem.Color);
                    timeline.TimeLineDrawEngine.AddTrackMediaElement_ToCanvas(MainCanvas, trackVideoEventItem, true);

                }
            }
        }

        internal enum EventType
        {

            Image, Video, Form, Audio
        }
    }
}
