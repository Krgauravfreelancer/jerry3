﻿using ManageMedia_UserControl.Controls;
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
    internal class DrawVideoEvents
    {
        internal void Draw(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, List<Media> Playlist, List<UIElement> TrackMediaElements, Controls.TimeLine timeline, DrawProperties DrawProperties, bool IsReadOnly, (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result, bool IsManageMedia = true)
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

            for (int i = 0; i < Playlist.Count; i++)
            {
                Media item = Playlist[i];
                if (item.StartTime < ViewPortStart + ViewPortDuration && item.Duration + item.StartTime > ViewPortStart)
                {
                    //It should draw

                    double Start = (item.StartTime.TotalSeconds - ViewPortStart.TotalSeconds) / ViewPortDuration.TotalSeconds;
                    double StartPosition = TotalWidth * Start;
                    double Width = TotalWidth * (item.Duration.TotalSeconds / ViewPortDuration.TotalSeconds);

                    if (item.TrackId == 2)
                    {
                        int red = (int)((double)item.Color.R / 1.5);
                        int green = (int)((double)item.Color.G / 1.5);
                        int blue = (int)((double)item.Color.B / 1.5);

                        var trackVideoEventItem = new TrackVideoEventItem(item, Color.FromArgb(200, (byte)red, (byte)green, (byte)blue), item.mediaType, timeline, Width, TrackHeight, IsReadOnly, IsManageMedia);
                        //VideoEvent
                        trackVideoEventItem.Margin = new Thickness(StartPosition, VideoEvent, 0, 0);

                        trackVideoEventItem.Width = Width;
                        trackVideoEventItem.Height = TrackHeight;
                        trackVideoEventItem.BorderBrush = Brushes.White;
                        trackVideoEventItem.BorderThickness = new Thickness(2);
                        trackVideoEventItem.Background = new SolidColorBrush(trackVideoEventItem.Color);
                        trackVideoEventItem.MediaSelectedEvent += (object s, Media m) =>
                        {
                            trackVideoEventItem.BorderBrush = Brushes.Red;
                            m.IsSelected = true;
                        };
                        timeline.TimeLineDrawEngine.AddTrackMediaElement_ToCanvas(MainCanvas, trackVideoEventItem, true);
                    }
                    else
                    {
                        int red = (int)((double)item.Color.R / 1.5);
                        int green = (int)((double)item.Color.G / 1.5);
                        int blue = (int)((double)item.Color.B / 1.5);

                        int Brightness = red + green + blue;
                        var trackCalloutItem = new TrackCalloutItem(item, Color.FromArgb(200, (byte)red, (byte)green, (byte)blue), item.mediaType, timeline, Width, TrackHeight, IsManageMedia);

                        if (item.TrackId == 3)
                        {
                            trackCalloutItem.Margin = new Thickness(StartPosition, CallOut1, 0, 0);//Callout 1
                        }
                        else if (item.TrackId == 4)
                        {
                            trackCalloutItem.Margin = new Thickness(StartPosition, CallOut2, 0, 0); //Callout 2
                        }
                        else if (item.TrackId == 1)
                        {
                            trackCalloutItem.Margin = new Thickness(StartPosition, Audio, 0, 0);//Audio

                        }
                        
                        trackCalloutItem.Width = Width;
                        trackCalloutItem.Height = TrackHeight;
                        trackCalloutItem.BorderBrush = Brushes.White;
                        trackCalloutItem.BorderThickness = new Thickness(2);
                        Color FillColor = Color.FromArgb(200, (byte)red, (byte)green, (byte)blue);

                        trackCalloutItem.Background = new SolidColorBrush(FillColor);
                        trackCalloutItem.MediaSelectedEvent += (object s, Media m) =>
                        {
                            trackCalloutItem.BorderBrush = Brushes.Red;
                            m.IsSelected = true;
                        };

                        timeline.TimeLineDrawEngine.AddTrackMediaElement_ToCanvas(MainCanvas, trackCalloutItem, true);
                    }
                }


            }
        }

        internal enum EventType
        {

            Image, Video, Form, Audio
        }
    }
}
