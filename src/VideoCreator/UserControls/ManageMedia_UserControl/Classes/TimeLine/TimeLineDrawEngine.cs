using ManageMedia_UserControl.Classes.TimeLine.DrawEngine;
using ManageMedia_UserControl.Controls;
using ManageMedia_UserControl.Models;
using ScreenRecorder_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ManageMedia_UserControl.Classes.TimeLine
{
    internal class TimeLineDrawEngine
    {
        List<Media> _Playlist = new List<Media>();

        List<UIElement> _TimeScaleElements = new List<UIElement>();
        List<UIElement> _TrackMediaElements = new List<UIElement>();
        List<UIElement> _TrackMissingMediaElements = new List<UIElement>();

        List<NoteItemControl> _NoteItemControls = new List<NoteItemControl>();
        List<NoteItemControl> _ItemControlsOnTimeLine = new List<NoteItemControl>();

        Line TimeLineCursor = new Line() { Stroke = Brushes.Red, StrokeThickness = 1, IsHitTestVisible = false, Opacity = 0.7 };
        Rectangle MainCursor = new Rectangle() { Fill = Brushes.Black, Width = 2, IsHitTestVisible = false };

        TextBlock TimeLineCursorTime = new TextBlock() { Text = "00:00:00", Background = Brushes.White, IsHitTestVisible = false };

        TimeSpan _PlayListStartOffset = TimeSpan.Zero;
        TimeSpan _PlayListDuration = TimeSpan.Zero;

        DrawBackground DrawBackground = new DrawBackground();
        DrawLegend DrawLegend = new DrawLegend();
        DrawMainCursor DrawMainCursor = new DrawMainCursor();
        DrawNoteItems DrawNoteItems = new DrawNoteItems();
        DrawProperties DrawProperties = new DrawProperties();
        DrawTimeTrack DrawTimeTrack = new DrawTimeTrack();
        DrawVideoEvents DrawVideoEvents = new DrawVideoEvents();
        DrawMissingVideoEvents DrawMissingVideoEvents = new DrawMissingVideoEvents();
        List<TrackVideoEventItem> TrackVideoEventItems = new List<TrackVideoEventItem>();
        List<TrackCalloutItem> TrackCalloutItems = new List<TrackCalloutItem>();
        List<TrackVideoEventItem> ModifiedVideoEventItems = new List<TrackVideoEventItem>();
        List<TrackCalloutItem> ModifiedCalloutItems = new List<TrackCalloutItem>();
        TrackCalloutItem NextElement = null;
        TrackCalloutItem PrevElement = null;

        bool alreadyAdded = false;
        internal void SetPlaylist(List<Media> Playlist, TimeLineMode Mode)
        {
            _Playlist = Playlist;

            if (Mode == TimeLineMode.Project)
            {
                _PlayListStartOffset = TimeSpan.Zero;
            }
            else
            {
                if (_Playlist.Count > 0)
                {
                    TimeSpan offset = _Playlist[0].StartTime;

                    for (int i = 0; i < _Playlist.Count; i++)
                    {
                        if (_Playlist[0].StartTime < offset)
                        {
                            offset = _Playlist[0].StartTime;
                        }
                    }

                    if (offset < TimeSpan.Zero)
                    {
                        offset = TimeSpan.Zero;
                    }

                    _PlayListStartOffset = offset;
                }
                else
                {
                    _PlayListStartOffset = TimeSpan.Zero;
                }

            }

            if (_Playlist.Count > 0)
            {
                TimeSpan duration = _Playlist[0].Duration;

                for (int i = 0; i < _Playlist.Count; i++)
                {
                    if (_Playlist[0].StartTime + _Playlist[0].Duration > duration)
                    {
                        duration = _Playlist[0].StartTime + _Playlist[0].Duration;
                    }
                }

                _PlayListDuration = duration;
            }
            else
            {
                if (Mode == TimeLineMode.Selected && (Playlist == null || Playlist.Count == 0))
                {
                    _PlayListDuration = TimeSpan.FromSeconds(20);
                }
                else
                {
                    _PlayListDuration = TimeSpan.Zero;
                }
            }
        }

        internal List<Media> GetPlaylist()
        {
            return _Playlist;
        }

        internal TimeSpan GetOffset()
        {
            return _PlayListStartOffset;
        }

        internal TimeSpan GetTotalDuration()
        {
            return _PlayListDuration;
        }

        internal void RemoveNotesInTimeSpan(TimeSpan StartTime, TimeSpan EndTime)
        {
            List<NoteItemControl> notes = _NoteItemControls;

            List<NoteItemControl> NotesToRemove = new List<NoteItemControl>();

            for (int i = 0; i < notes.Count; i++)
            {
                NoteItemControl note = notes[i];
                if (note.GetStartTime() >= StartTime && note.GetStartTime() < EndTime)
                {
                    NotesToRemove.Add(note);
                }
            }

            for (int i = 0; i < NotesToRemove.Count; i++)
            {
                NoteItemControl control = NotesToRemove[i];
                _NoteItemControls.Remove(control);
            }
        }

        internal void DrawTimeLine(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, TimeSpan TotalDuration, TimeSpan MainCursorTime, Controls.TimeLine timeline, bool IsReadOnly, bool IsManageMedia)
        {
            TrackVideoEventItems.Clear();
            TrackCalloutItems.Clear();
            if (MainCanvas.ActualHeight > 10 && MainCanvas.ActualWidth > 10)
            {
                DrawLegend.Draw(LegendCanvas, DrawProperties);

                (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result = CalculateSectionWidth(MainCanvas, ViewPortStart, ViewPortDuration);

                if (Result.SectionWidth > 0)
                {

                    //Clear TimeScale
                    for (int i = 0; i < _TimeScaleElements.Count; i++)
                    {
                        MainCanvas.Children.Remove(_TimeScaleElements[i]);
                    }
                    _TimeScaleElements.Clear();

                    //Draw TimeScale
                    DrawBackground.Draw(MainCanvas, LegendCanvas, timeline, DrawProperties, Result);
                    DrawTimeTrack.Draw(MainCanvas, ViewPortStart, timeline, DrawProperties, Result);

                    //Draw Track Items
                    DrawVideoEvents.Draw(MainCanvas, LegendCanvas, ViewPortStart, ViewPortDuration, _Playlist, _TrackMediaElements, timeline, DrawProperties, IsReadOnly, Result, TrackVideoEventItems, TrackCalloutItems, IsManageMedia);
                    if (!alreadyAdded && !IsManageMedia)
                        AddEventHandlers(MainCanvas, timeline);
                    DrawNoteItems.Draw(MainCanvas, LegendCanvas, ViewPortStart, ViewPortDuration, _ItemControlsOnTimeLine, _NoteItemControls, timeline, DrawProperties, Result);

                    //Draw Missing Spaces
                    List<(Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration)> MissingTimeSpans = TrackItemProcessor.FindMissingVideoEventTimeSpans(_Playlist);
                    DrawMissingVideoEvents.Draw(MainCanvas, LegendCanvas, ViewPortStart, ViewPortDuration, MissingTimeSpans, _TrackMissingMediaElements, timeline, DrawProperties, IsReadOnly, Result, IsManageMedia);

                    //Draw Cursor
                    DrawMainCursor.Draw(MainCanvas, MainCursorTime, ViewPortStart, ViewPortDuration, MainCursor, DrawProperties);
                }
            }
        }

        #region == Added by KG for move element ==

        bool drag = false, isFirstClick = true;
        double diff = 0;
        private void AddEventHandlers(Canvas MainCanvas, Controls.TimeLine timeline)
        {
            MainCanvas.MouseLeftButtonDown += (object s, MouseButtonEventArgs e) =>
            {
                MainCanvas.CaptureMouse();
                isFirstClick = true;
                drag = true;
                diff = 0;
                PrevElement = null;
                NextElement = null;
            };

            MainCanvas.MouseMove += (object s, MouseEventArgs e) =>
            {
                if (!drag) return;


                var videoEvent = TrackVideoEventItems.Find(x => x.IsSelected);
                var calloutEvent = TrackCalloutItems.Find(x => x.IsSelected);

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(MainCanvas);

                // center the trackbarLine on the mouse
                double mouseX = mousePos.X;
                if (mouseX >= 0 && (videoEvent != null || calloutEvent != null))
                {

                    var leftPos = videoEvent != null ? Canvas.GetLeft(videoEvent) : Canvas.GetLeft(calloutEvent);
                    if (isFirstClick == false)
                    {
                        if (videoEvent != null)
                        {
                            Canvas.SetLeft(videoEvent, (mouseX - diff));
                            videoEvent.Media.StartTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                            var isExistsAlready = ModifiedVideoEventItems.Find(x => x.Media?.VideoEventID == videoEvent.Media?.VideoEventID);
                            if (isExistsAlready != null)
                                ModifiedVideoEventItems.Remove(isExistsAlready);
                            ModifiedVideoEventItems.Add(videoEvent);
                        }
                        if (calloutEvent != null && calloutEvent.MediaCallout != null)
                        {
                            var canMoveAhead = CanMoveAhead(calloutEvent, timeline, mouseX);
                            var canMoveBack = CanMoveBack(calloutEvent, timeline, mouseX);
                            Console.WriteLine($"canMoveAhead - {canMoveAhead} | canMoveBack - {canMoveBack} | mouseX - {mouseX} | diff - {diff}");
                            var canMove = canMoveAhead || canMoveBack;

                            if (canMove && (mouseX - diff) > 0)
                            {
                                if(NextElement == null && PrevElement == null)
                                {
                                    // Min 0, MAX = End of timeline
                                    var currentTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                                    {
                                        Move(calloutEvent, mouseX, currentTime);
                                    }
                                }
                                else if (NextElement == null && PrevElement != null)
                                {
                                    // MAX = End of timeline, MIN is previous end
                                    var currentTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                                    if(currentTime >= PrevElement.MediaCallout.StartTime + PrevElement.MediaCallout.Duration)
                                    {
                                        Move(calloutEvent, mouseX, currentTime);
                                    }

                                }
                                else if (NextElement != null && PrevElement == null)
                                {
                                    // MIN = 0, // MAX = Start of Next
                                    var currentTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                                    if (currentTime + calloutEvent.MediaCallout.Duration <= NextElement.MediaCallout.StartTime)
                                    {
                                        Move(calloutEvent, mouseX, currentTime);
                                    }
                                }
                                else
                                {
                                    // MIN is previous end, Start of Next
                                    var currentTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                                    if ((currentTime >= PrevElement.MediaCallout.StartTime + PrevElement.MediaCallout.Duration) &&
                                    (currentTime + calloutEvent.MediaCallout.Duration <= NextElement.MediaCallout.StartTime))
                                    {
                                        Move(calloutEvent, mouseX, currentTime);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        isFirstClick = false;
                        diff = mouseX - leftPos;
                    }
                }
            };

            MainCanvas.MouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                drag = false;
                MainCanvas.ReleaseMouseCapture();
                NextElement = null;
                PrevElement = null;
                var payload = new LocationChangedEventModel
                {
                    CallOutItems = ModifiedCalloutItems,
                    VideoEventItems = ModifiedVideoEventItems,
                };
                timeline.LocationChanged_Event(payload);
            };
            alreadyAdded = true;
        }

        private bool CanMoveAhead(TrackCalloutItem calloutEvent, Controls.TimeLine timeline, double mouseX)
        {
            var canMove = false;
            if (NextElement == null)
                NextElement = TrackCalloutItems
                                        .FindAll(x => x.MediaCallout.TrackId == calloutEvent.MediaCallout.TrackId)?
                                        .OrderBy(x => x.MediaCallout.StartTime)?
                                        .Where(x => x.MediaCallout.StartTime > calloutEvent.MediaCallout.StartTime)?
                                        .FirstOrDefault();
            if (NextElement != null)
            {
                var newEndTime = timeline.GetTimeSpanByLocation(mouseX - diff) + calloutEvent.MediaCallout.Duration;
                if (newEndTime <= NextElement.MediaCallout.StartTime)
                    canMove = true;
                else if (newEndTime > NextElement.MediaCallout.StartTime)
                    canMove = false;
            }
            else
                canMove = true;
            return canMove;
        }

        private bool CanMoveBack(TrackCalloutItem calloutEvent, Controls.TimeLine timeline, double mouseX)
        {
            var canMove = false;
            if (PrevElement == null)
            {
                PrevElement = TrackCalloutItems
                                        .FindAll(x => x.MediaCallout.TrackId == calloutEvent.MediaCallout.TrackId)?
                                        .OrderBy(x => x.MediaCallout.StartTime)?
                                        .Where(x => x.MediaCallout.StartTime < calloutEvent.MediaCallout.StartTime)?
                                        .LastOrDefault();
            }
            TimeSpan previousEndTime = new TimeSpan(0,0,0);
            var newStartTime = new TimeSpan(0, 0, 0);
            if (PrevElement != null && (mouseX - diff) >= 0)
            {
                newStartTime = timeline.GetTimeSpanByLocation(mouseX - diff);
                previousEndTime = PrevElement.MediaCallout.StartTime + PrevElement.MediaCallout.Duration;
            }

            if (newStartTime < previousEndTime)
                canMove = false;
            else if (newStartTime >= previousEndTime)
                canMove = false;

            return canMove;
        }


        private void Move(TrackCalloutItem calloutEvent, double mouseX, TimeSpan currentTime)
        {
            Canvas.SetLeft(calloutEvent, (mouseX - diff));
            calloutEvent.MediaCallout.StartTime = currentTime;
            var isExistsAlready = ModifiedCalloutItems.Find(x => x.MediaCallout?.VideoEventID == calloutEvent.MediaCallout?.VideoEventID);
            if (isExistsAlready != null)
                ModifiedCalloutItems.Remove(isExistsAlready);

            ModifiedCalloutItems.Add(calloutEvent);
        }

        #endregion

        private (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) CalculateSectionWidth(Canvas MainCanvas, TimeSpan ViewportStart, TimeSpan ViewportDuration)
        {
            double TotalWidth = MainCanvas.ActualWidth;

            TimeSpan _TimeStampResolution = TimeSpan.Zero;

            for (int i = 0; i < DrawProperties.TimeStampResolutions.Count; i++)
            {
                double TimeStampCount = ViewportDuration.TotalMilliseconds / DrawProperties.TimeStampResolutions[i].TotalMilliseconds;
                double SectionWidth = TotalWidth / TimeStampCount;

                if (SectionWidth > 75)
                {
                    _TimeStampResolution = DrawProperties.TimeStampResolutions[i];

                    double Offset = ViewportStart.TotalSeconds / DrawProperties.TimeStampResolutions[i].TotalSeconds;
                    Offset = Math.Ceiling(Offset);
                    Offset = (DrawProperties.TimeStampResolutions[i].TotalSeconds * Offset) - ViewportStart.TotalSeconds;
                    TimeSpan OffsetTime = TimeSpan.FromSeconds(Offset);
                    Offset = SectionWidth * (Offset / DrawProperties.TimeStampResolutions[i].TotalSeconds);

                    (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result = (_TimeStampResolution, (int)Math.Floor(TimeStampCount), SectionWidth, OffsetTime, Offset);

                    if (double.IsNaN(Result.SectionWidth) == true || double.IsInfinity(Result.SectionWidth) || double.IsNaN(Result.OffsetPixels) == true || double.IsInfinity(Result.OffsetPixels))
                    {
                        return (TimeSpan.Zero, 0, 0, TimeSpan.Zero, 0);
                    }

                    return (_TimeStampResolution, (int)Math.Floor(TimeStampCount), SectionWidth, OffsetTime, Offset);
                }
            }

            {
                int TimeStampCount = Convert.ToInt32(ViewportDuration.Ticks / DrawProperties.TimeStampResolutions.LastOrDefault().Ticks);
                double SectionWidth = TotalWidth / TimeStampCount;

                double Offset = ViewportStart.TotalSeconds / DrawProperties.TimeStampResolutions.LastOrDefault().TotalSeconds;
                Offset = Math.Ceiling(Offset);
                Offset = (DrawProperties.TimeStampResolutions.LastOrDefault().TotalSeconds * Offset) - ViewportStart.TotalSeconds;
                TimeSpan OffsetTime = TimeSpan.FromSeconds(Offset);
                Offset = SectionWidth * (Offset / DrawProperties.TimeStampResolutions.LastOrDefault().TotalSeconds);

                return (DrawProperties.TimeStampResolutions.LastOrDefault(), TimeStampCount, SectionWidth, OffsetTime, Offset);
            }
        }

        internal void AddTextControlElement_ToCanvas(Canvas MainCanvas, NoteItemControl element)
        {
            _ItemControlsOnTimeLine.Add(element);
            MainCanvas.Children.Add(element);
        }

        internal void AddTrackMediaElement_ToCanvas(Canvas MainCanvas, UIElement element, bool IsHitTestVisible)
        {
            element.IsHitTestVisible = IsHitTestVisible;
            if (_TrackMediaElements.Contains(element))
                _TrackMediaElements.Remove(element);
            _TrackMediaElements.Add(element);
            if (MainCanvas.Children.Contains(element))
                MainCanvas.Children?.Remove(element);
            MainCanvas.Children.Add(element);
        }

        internal void AddTimeScaleElement_ToCanvas(Canvas MainCanvas, UIElement element)
        {
            element.IsHitTestVisible = false;
            _TimeScaleElements.Add(element);
            MainCanvas.Children.Add(element);
        }

        internal void RedrawNotes(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, Controls.TimeLine timeline)
        {
            (TimeSpan SectionTime, int TimeStampCount, double SectionWidth, TimeSpan OffsetTime, double OffsetPixels) Result = CalculateSectionWidth(MainCanvas, ViewPortStart, ViewPortDuration);
            if (Result.SectionWidth > 0)
            {
                DrawNoteItems.Draw(MainCanvas, LegendCanvas, ViewPortStart, ViewPortDuration, _ItemControlsOnTimeLine, _NoteItemControls, timeline, DrawProperties, Result);
            }
        }

        internal TimeSpan SetMainCursor(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration)
        {
            Point Location = Mouse.GetPosition(MainCanvas);
            TimeSpan MainCursorTime = TimeLineHelpers.GetTimeSpanByLocation(MainCanvas, ViewPortStart, ViewPortDuration, Location.X);
            DrawMainCursor.Draw(MainCanvas, MainCursorTime, ViewPortStart, ViewPortDuration, MainCursor, DrawProperties);
            return MainCursorTime;
        }

        internal void SetPointerCursor(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration)
        {
            MainCanvas.Children.Remove(TimeLineCursor);
            MainCanvas.Children.Remove(TimeLineCursorTime);

            if (MainCanvas.IsMouseDirectlyOver)
            {
                Point Location = Mouse.GetPosition(MainCanvas);
                TimeLineCursor.X1 = Location.X;
                TimeLineCursor.X2 = Location.X;

                TimeLineCursor.Y1 = 0;
                TimeLineCursorTime.Text = TimeLineHelpers.GetTimeSpanByLocation(MainCanvas, ViewPortStart, ViewPortDuration, Location.X).ToString("mm\\:ss\\:fff");
                TimeLineCursorTime.Margin = new Thickness(Location.X, 0, 0, 0);

                TimeLineCursor.Y1 = MainCanvas.ActualHeight;

                MainCanvas.Children.Add(TimeLineCursorTime);
                MainCanvas.Children.Add(TimeLineCursor);
            }
        }



        internal void UpdateMainCursor(Canvas MainCanvas, TimeSpan MainCursorTime, TimeSpan ViewPortStart, TimeSpan ViewPortDuration)
        {
            DrawMainCursor.Draw(MainCanvas, MainCursorTime, ViewPortStart, ViewPortDuration, MainCursor, DrawProperties);
        }

        internal void RemoveCursorFromCanvas(Canvas MainCanvas)
        {
            MainCanvas.Children.Remove(TimeLineCursor);
            MainCanvas.Children.Remove(TimeLineCursorTime);
        }

        internal List<NoteItemControl> GetNoteItemControls()
        {
            return _NoteItemControls;
        }

        internal void AddNoteItemControls(NoteItemControl noteItemControl)
        {
            _NoteItemControls.Add(noteItemControl);
        }

        internal void RemoveNoteItemControls(NoteItemControl noteItemControl)
        {
            _NoteItemControls.Remove(noteItemControl);
        }

        internal void ClearNoteItemControls()
        {
            for (int i = 0; i < _NoteItemControls.Count; i++)
            {
                NoteItemControl noteItemControl = _NoteItemControls[i];
                noteItemControl.ClearResources();
            }

            _NoteItemControls.Clear();
        }

        internal void ClearRecourses()
        {
            _Playlist = null;

            _TimeScaleElements.Clear();
            _TrackMediaElements.Clear();

            _NoteItemControls.Clear();
            _ItemControlsOnTimeLine.Clear();

            TimeLineCursor = null;
            MainCursor = null;

            TimeLineCursorTime = null;

            _PlayListStartOffset = TimeSpan.Zero;
            _PlayListDuration = TimeSpan.Zero;

            DrawBackground = null;
            DrawLegend = null;
            DrawMainCursor = null;
            DrawNoteItems = null;
            DrawProperties = null;
            DrawTimeTrack = null;
            DrawVideoEvents = null;
        }
    }
}
