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
     internal class DrawLegend
    {
         internal void Draw(Canvas LegendCanvas, DrawProperties DrawProperties)
        {
            LegendCanvas.Children.Clear();

            double LegendHeight = LegendCanvas.ActualHeight;
            double HeaderHeight = DrawProperties.TimeScaleHeight + 20;
            double LegendWidth = LegendCanvas.ActualWidth;

            Rectangle HeaderBar = new Rectangle()
            {
                Width = LegendWidth,
                Height = HeaderHeight,
                Fill = DrawProperties.TimeLineBackgroundColor
            };

            LegendCanvas.Children.Add(HeaderBar);

            double RemainingHeight = LegendCanvas.ActualHeight - HeaderHeight;

            //NoteTrack 
            //Callout - 2
            //Callout - 1
            //VideoEvent
            //Audio

            double NoteTrackHeight = RemainingHeight / 5 * 1.5;
            double TrackHeight = (RemainingHeight - NoteTrackHeight) / 4;

            if (NoteTrackHeight < 1 || double.IsNaN(NoteTrackHeight) || double.IsInfinity(NoteTrackHeight))
            {
                NoteTrackHeight = 1;
            }

            if (TrackHeight < 1 || double.IsNaN(TrackHeight) || double.IsInfinity(TrackHeight))
            {
                TrackHeight = 1;
            }

            Label NoteBlock = new Label()
            {
                Height = NoteTrackHeight,
                Width = LegendWidth,
                Background = DrawProperties.TimeLineBackgroundGridColor2,
                Content = "Note's Track",
                Margin = new Thickness(0, HeaderHeight, 0, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            LegendCanvas.Children.Add(NoteBlock);

            string[] Labels = new string[] {
                    "Callout Track 2",
                    "Callout Track 1",
                    "Video Track",
                    "Audio Track",
                };

            bool Stagger = false;
            for (int i = 0; i < Labels.Length; i++)
            {
                string label = Labels[i];
                Label TrackLabel = new Label()
                {
                    Height = TrackHeight,
                    Width = LegendWidth,
                    Background = Brushes.Blue,
                    Content = label,
                    Padding = new Thickness(5, 0, 0, 0),
                    Margin = new Thickness(0, HeaderHeight + NoteTrackHeight + (TrackHeight * i), 0, 0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                };

                if (Stagger == false)
                {
                    TrackLabel.Background = DrawProperties.TimeLineBackgroundGridColor1;
                }
                else
                {
                    TrackLabel.Background = DrawProperties.TimeLineBackgroundGridColor2;
                }

                Stagger = !Stagger;

                LegendCanvas.Children.Add(TrackLabel);
            }

            Line NoteLine = new Line();
            NoteLine.X1 = 0;
            NoteLine.Y1 = HeaderHeight + NoteTrackHeight;
            NoteLine.X2 = LegendWidth;
            NoteLine.Y2 = HeaderHeight + NoteTrackHeight;
            NoteLine.Stroke = Brushes.LightGray;
            LegendCanvas.Children.Add(NoteLine);

            for (int i = 0; i < Labels.Length; i++)
            {
                Line TrackLine = new Line();
                TrackLine.X1 = 0;
                TrackLine.Y1 = HeaderHeight + NoteTrackHeight + (TrackHeight * i);
                TrackLine.X2 = LegendWidth;
                TrackLine.Y2 = HeaderHeight + NoteTrackHeight + (TrackHeight * i);
                TrackLine.Stroke = Brushes.LightGray;

                LegendCanvas.Children.Add(TrackLine);
            }

            Line LegendEndLine = new Line();
            LegendEndLine.X1 = LegendWidth - 1;
            LegendEndLine.Y1 = 0;
            LegendEndLine.X2 = LegendWidth - 1;
            LegendEndLine.Y2 = LegendHeight;
            LegendEndLine.Stroke = Brushes.LightGray;
            LegendCanvas.Children.Add(LegendEndLine);
        }
    }
}
