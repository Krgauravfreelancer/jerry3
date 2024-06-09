using LocalVoiceGen_UserControl.Helpers;
using ManageMedia_UserControl.Classes;
using ManageMedia_UserControl.Classes.TimeLine;
using ManageMedia_UserControl.Classes.TimeLine.DrawEngine;
using ManageMedia_UserControl.Models;
using NAudio.CoreAudioApi;
using NAudio.Gui;
using Newtonsoft.Json.Linq;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ManageMedia_UserControl.Controls
{
    public partial class TimeLine : UserControl
    {
        TimeSpan _TotalDuration = TimeSpan.FromSeconds(20);
        TimeSpan _ViewportDuration = TimeSpan.FromSeconds(5);
        TimeSpan _ViewportStart = TimeSpan.FromSeconds(0);

        TimeLineMode _TimeLineMode = TimeLineMode.Project;

        List<Media> _Playlist = new List<Media>();
        List<Media> _SelectedMedia = null;

        internal TimeLineDrawEngine TimeLineDrawEngine = new TimeLineDrawEngine();
        TrackItemSelectionEngine TrackItemSelectionEngine = new TrackItemSelectionEngine();

        bool _IsReadOnly = false;
        bool IsManageMedia = true;

        public List<Media> OverlappedEvents = new List<Media>();
        internal TimeSpan MainCursorTime { get; private set; } = TimeSpan.Zero;


        public event EventHandler<int> Delete_Event;
        public event EventHandler<int> Edit_Event;
        public event EventHandler<Media> Clone_Event;
        public event EventHandler<Media> CloneAtEnd_Event;

        public event EventHandler<TrackbarMouseMoveEventModel> TrackbarMouseMoveEvent;
        public event EventHandler<Media> EventSelectionChangedEvent;


        //public event EventHandler<LocationChangedEventModel> LocationChangedEvent;

        public ObservableCollection<Media> CalloutLocationOrSizeChangedMedia = new ObservableCollection<Media>();

        public Point TrackBarPosition = new Point(0, 0);
        public TimeSpan TrackBarTimespan = new TimeSpan(0, 0, 0);
        TrackbarLineControl TrackbarLine2 = new TrackbarLineControl();
        bool _isTrackbarLineDragInProg = false;

        public TimeLine()
        {
            InitializeComponent();
        }

        public void SetReadOnly(bool IsReadOnly)
        {
            _IsReadOnly = IsReadOnly;
        }

        public void SetManageMedia(bool _IsManageMedia)
        {
            IsManageMedia = _IsManageMedia;
        }

        internal void SetTimeLineMode(TimeLineMode Mode)
        {
            _TimeLineMode = Mode;

            if (_TimeLineMode == TimeLineMode.Project)
            {
                TimeLineDrawEngine.SetPlaylist(_Playlist, _TimeLineMode);

                _TotalDuration = TimeSpan.Zero;

                foreach (var item in TimeLineDrawEngine.GetPlaylist())
                {
                    if (item.StartTime + item.Duration > _TotalDuration)
                    {
                        _TotalDuration = item.StartTime + item.Duration;
                    }
                }

                SetTotalTime(_TotalDuration);

            }
            else if (_TimeLineMode == TimeLineMode.Selected)
            {
                if (_SelectedMedia != null)
                {
                    TimeLineDrawEngine.SetPlaylist(_SelectedMedia, _TimeLineMode);
                }
                else
                {
                    _SelectedMedia = new List<Media>();
                    TimeLineDrawEngine.SetPlaylist(_SelectedMedia, _TimeLineMode);
                }

                _TotalDuration = TimeSpan.Zero;

                foreach (var item in TimeLineDrawEngine.GetPlaylist())
                {
                    if (item.StartTime + item.Duration > _TotalDuration)
                    {
                        _TotalDuration = item.StartTime + item.Duration;
                    }
                }

                if (_SelectedMedia == null || _SelectedMedia.Count == 0)
                {
                    _TotalDuration = TimeSpan.FromSeconds(20);
                }
                else
                {
                    _ViewportStart = _SelectedMedia[0].StartTime;
                    _ViewportDuration = _SelectedMedia[0].Duration;
                }

                SetTotalTime(_TotalDuration);

                if (_SelectedMedia?.Count > 0)
                {
                    SetMainCursorByTimeSpan(_SelectedMedia[0].StartTime, true);
                }
            }
        }

        internal TimeLineMode GetTimeLineMode()
        {
            return _TimeLineMode;
        }

        internal List<(TextItem TextItem, int VideoEventID)> GetTextItems()
        {
            List<(TextItem TextItem, int VideoEventID)> NoteItems = TrackItemProcessor.GetTextItems(TimeLineDrawEngine.GetNoteItemControls());
            return NoteItems;
        }

        internal void SetTotalTime(TimeSpan TotalTime)
        {

            TimeLineScrollBar.ValueChanged -= TimeLineScrollBar_ValueChanged;
            _TotalDuration = TotalTime;


            if (_ViewportStart + _ViewportDuration > _TotalDuration)
            {
                _ViewportStart = _TotalDuration - _ViewportDuration;
            }

            if (_ViewportStart.TotalSeconds < 0)
            {
                _ViewportStart = TimeSpan.FromSeconds(0);
            }

            if (_ViewportStart + _ViewportDuration > _TotalDuration)
            {
                _ViewportDuration = _TotalDuration - _ViewportStart;
            }


            TimeLineScrollBar.Maximum = _TotalDuration.TotalSeconds - _ViewportDuration.TotalSeconds;
            TimeLineScrollBar.Minimum = TimeLineDrawEngine.GetOffset().TotalSeconds;


            TimeLineScrollBar.ValueChanged += TimeLineScrollBar_ValueChanged;
            CalculateTimeLineScrollBar();
            DrawTimeLine();
            TrackItemProcessor.RecalculateNoteLimits(TimeLineDrawEngine.GetNoteItemControls());

        }

        public void SetViewport(TimeSpan ViewportStart, TimeSpan ViewportDuration, TimeSpan mainCursorTime)
        {
            if (ViewportStart + ViewportDuration < _TotalDuration)
            {
                _ViewportStart = ViewportStart;
                _ViewportDuration = ViewportDuration;
                if (mainCursorTime < ViewportStart + ViewportDuration)
                {
                    MainCursorTime = mainCursorTime;
                    TimeLineCursorChangedEvent(mainCursorTime);
                }
                else
                {
                    MainCursorTime = TimeSpan.Zero;
                }

            }
            else
            {
                _ViewportStart = TimeSpan.Zero;
                _ViewportDuration = _TotalDuration;
                MainCursorTime = TimeSpan.Zero;
            }

            TimeSpan timeSpan = TrackItemProcessor.GetTotalTime(_Playlist);

            SetTotalTime(timeSpan);
        }

        internal (TimeSpan ViewportStart, TimeSpan ViewportDuration, TimeSpan MainCursorTime) GetViewport()
        {
            return (_ViewportStart, _ViewportDuration, MainCursorTime);
        }

        public void LoadMedia(List<Media> PlayList)
        {
            _Playlist = PlayList;
            TimeLineDrawEngine.SetPlaylist(PlayList, _TimeLineMode);

            _TimeLineMode = TimeLineMode.Project;

            TimeLineDrawEngine.ClearNoteItemControls();

            _TotalDuration = TimeSpan.Zero;

            foreach (var item in PlayList)
            {
                foreach (var element in item.RecordedTextList)
                {
                    NoteItemControl noteItemControl = TrackItemProcessor.CreateNoteControl(item, element, this, IsManageMedia ? _IsReadOnly : true);
                    noteItemControl.SetTimeLine(this);
                    CreateNoteEvents(noteItemControl);
                }

                if (item.StartTime + item.Duration > _TotalDuration)
                {
                    _TotalDuration = item.StartTime + item.Duration;
                }
            }
            _ViewportDuration = _TotalDuration;

            MainCursorTime = TimeSpan.Zero;

            SetTotalTime(_TotalDuration);
        }

        public void ShowTrackbar(double height = 230.0)
        {
            if (!IsManageMedia)
            {
                TrackbarLine2.SetHeight(height);
                Panel.SetZIndex(TrackbarLine2, 5);
                if (!MainCanvas.Children.Contains(TrackbarLine2))
                    MainCanvas.Children.Add(TrackbarLine2);
                Canvas.SetBottom(TrackbarLine2, 0);
                Canvas.SetLeft(TrackbarLine2, 0);


                SetTrackbarMouseEvents();
            }
        }


        private void SetTrackbarMouseEvents()
        {
            TrackbarLine2.MouseLeftButtonDown += (sender, e) =>
            {
                e.Handled = true;
                _isTrackbarLineDragInProg = true;
                TrackbarLine2.CaptureMouse();
            };

            TrackbarLine2.MouseLeftButtonUp += (sender, e) =>
            {
                e.Handled = true;
                _isTrackbarLineDragInProg = false;
                TrackbarLine2.ReleaseMouseCapture();
                OnTrackbarMouseMoved(TrackBarPosition.X);
            };

            TrackbarLine2.MouseMove += (sender, e) =>
            {
                e.Handled = true;
                if (!_isTrackbarLineDragInProg) return;

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(MainCanvas);
                // center the trackbarLine on the mouse
                double mouseX = mousePos.X;

                if (mouseX >= 0)
                {
                    TrackBarPosition = mousePos;
                    TrackBarTimespan = GetTimeSpanByLocation(mousePos.X);
                    Canvas.SetLeft(TrackbarLine2, mouseX);
                   
                }
            };
        }



        private void NoteItemControl_RecalculateClicked(object sender, EventArgs e)
        {
            NoteItemControl noteItemControl = (NoteItemControl)sender;
            TimeSpan MeasuredText = TextMeasurement.Measure(noteItemControl.TextItem.Text);
            noteItemControl.TextItem.Duration = MeasuredText;

            List<Media> NoteWhereDeleted = _Playlist.Where(x => x.VideoEventID == noteItemControl.VideoEventID).ToList();

            if (NoteWhereDeleted.Count != 0)
            {
                RecalculateVideoEventDuration(NoteWhereDeleted[0], null);
                ReProcessNotes();
            }

            NotesEditedEvent();
        }

        private void NoteItemControl_NoteDeleteClicked(object sender, EventArgs e)
        {
            List<(TextItem Item, int VideoEventID)> DeletedNotes = new List<(TextItem Item, int VideoEventID)>();
            if (TimeLineDrawEngine.GetNoteItemControls().Contains(sender))
            {
                NoteItemControl NoteControl = (NoteItemControl)sender;
                DeletedNotes.Add((NoteControl.TextItem, NoteControl.TextItem.NoteID));

                for (int i = 0; i < DeletedNotes.Count; i++)
                {
                    TextItem textItem = DeletedNotes[i].Item;

                    for (int j = 0; j < _Playlist.Count; j++)
                    {
                        Media media = _Playlist[j];
                        media.RecordedTextList.Remove(textItem);
                    }
                }

                TimeLineDrawEngine.RemoveNoteItemControls((NoteItemControl)sender);
            }

            NoteItemControl NoteControl2 = (NoteItemControl)sender;
            List<Media> NoteWhereDeleted = _Playlist.Where(x => x.VideoEventID == NoteControl2.VideoEventID).ToList();

            if (NoteWhereDeleted.Count != 0)
            {
                RecalculateVideoEventDuration(NoteWhereDeleted[0], null);
                ReProcessNotes();
            }

            NotesEditedEvent();
        }

        private void NoteItemControl_NoteChanged(object sender, EventArgs e)
        {
            NoteItemControl SelectedControl = sender as NoteItemControl;
            TimeLineDrawEngine.RedrawNotes(MainCanvas, LegendCanvas, _ViewportStart, _ViewportDuration, this);
            TrackItemProcessor.RecalculateNoteLimits(TimeLineDrawEngine.GetNoteItemControls());
            NotesEditedEvent();
        }

        private void NoteItemControl_NoteSelected(object sender, EventArgs e)
        {
            NoteItemControl SelectedControl = sender as NoteItemControl;
            TrackItemProcessor.NoteSelected(SelectedControl, this);
            Keyboard.Focus(this);
        }

        private void CalculateTimeLineScrollBar()
        {
            TimeLineScrollBar.ValueChanged -= TimeLineScrollBar_ValueChanged;
            double value = _ViewportDuration.TotalSeconds / _TotalDuration.TotalSeconds;

            if (double.IsNaN(value) == true)
            {
                value = 1;
            }

            if (value >= 1)
            {
                value = 1 - 0.000000000001;
            }

            if (value <= 0)
            {
                value = 0;
            }
            TimeLineScrollBar.Value = _ViewportStart.TotalSeconds;

            ZoomLevelUpdatedEvent(value);

            //This should not be touched
            TimeLineScrollBar.ViewportSize = (TimeLineScrollBar.Maximum - TimeLineScrollBar.Minimum) * value / (1 - value);
            TimeLineScrollBar.ValueChanged += TimeLineScrollBar_ValueChanged;
            if(!IsManageMedia) SetTrackbarByTime();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawTimeLine();

            SetTotalTime(_TotalDuration);
            TimeLineScrollBar.Value = 0;

            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                if (IsManageMedia)
                {
                    if (_IsReadOnly == false)
                    {
                        this.KeyDown += Window_KeyDown;
                        this.Focusable = true;
                        Keyboard.Focus(this);
                    }
                    else
                    {
                        MainCanvas.ContextMenu = null;
                    }
                }
                else
                {
                    this.KeyDown += Window_KeyDown;
                    this.Focusable = true;
                    Keyboard.Focus(this);
                    MainCanvas.ContextMenu = null;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeletedSelectedNotes();
            }
        }

        private void DeletedSelectedNotes()
        {
            List<(TextItem Item, int VideoEventID)> DeletedItems = TrackItemProcessor.DeletedSelectedNotes(TimeLineDrawEngine.GetNoteItemControls());

            for (int i = 0; i < DeletedItems.Count; i++)
            {
                TextItem textItem = DeletedItems[i].Item;

                for (int j = 0; j < _Playlist.Count; j++)
                {
                    Media media = _Playlist[j];
                    if (media.RecordedTextList != null)
                    {
                        media.RecordedTextList.Remove(textItem);
                    }
                }
            }

            foreach (var element in DeletedItems)
            {
                List<Media> NoteWhereDeleted = _Playlist.Where(x => x.VideoEventID == element.VideoEventID).ToList();

                if (NoteWhereDeleted.Count != 0)
                {
                    RecalculateVideoEventDuration(NoteWhereDeleted[0], null);
                }
            }

            ReProcessNotes();
            NotesEditedEvent();
        }

        private void DrawTimeLine()
        {
            TimeLineDrawEngine.DrawTimeLine(MainCanvas, LegendCanvas, _ViewportStart, _ViewportDuration, _TotalDuration, MainCursorTime, this, _IsReadOnly, IsManageMedia);
        }

        internal void SetMainCursorTime(TimeSpan time)
        {
            if (time < TimeSpan.Zero)
            {
                MainCursorTime = TimeSpan.Zero;
            }

            if (time > _TotalDuration)
            {
                MainCursorTime = _TotalDuration;
            }

            if (time > TimeSpan.Zero && time < _TotalDuration)
            {
                MainCursorTime = time;
            }

            TimeLineDrawEngine.UpdateMainCursor(MainCanvas, MainCursorTime, _ViewportStart, _ViewportDuration);
        }

        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsLoaded == true)
            {
                DrawTimeLine();
            }
        }

        internal double GetWidthByTimeSpan(TimeSpan time)
        {
            return TimeLineHelpers.GetWidthByTimeSpan(MainCanvas, _ViewportStart, _ViewportDuration, time);
        }

        internal TimeSpan GetTimeSpanByLocation(double location)
        {

            return TimeLineHelpers.GetTimeSpanByLocation(MainCanvas, _ViewportStart, _ViewportDuration, location);
        }

        internal double GetLocationByTimeSpan(TimeSpan time)
        {

            return TimeLineHelpers.GetLocationByTimeSpan(MainCanvas, _ViewportStart, _ViewportDuration, time);
        }

        private void TimeLineScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _ViewportStart = TimeSpan.FromSeconds(TimeLineScrollBar.Value);
            if(!IsManageMedia) SetTrackbarByTime();
            DrawTimeLine();
        }

        internal void AddNewVideoEventByPoint(ImageSource imageItem, Point location, bool PositionAtHalf)
        {
            (Media media, TimeSpan TimeAtPoint) Result = TrackItemProcessor.FindMediaAtPoint(MainCanvas, LegendCanvas, _ViewportStart, _ViewportDuration, _Playlist, location, PositionAtHalf);

            if (Result.media != null)
            {
                TimeSpan TimeRelativeToMedia = Result.TimeAtPoint - Result.media.StartTime;
                if (TimeRelativeToMedia.TotalSeconds < (Result.media.Duration.TotalSeconds / 2))
                {
                    //Should Add Before The Video Event
                    List<Media> MediaToShift = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, Result.media.StartTime);

                    Debug.Assert(MediaToShift[0] != null);

                    int MediaToShiftID = MediaToShift[0].VideoEventID;
                    TimeSpan NewMediaStartTime = MediaToShift[0].StartTime;
                    TimeSpan NewMediaDuration = TimeSpan.FromSeconds(5);

                    TrackItemProcessor.ShiftVideoEvents(MediaToShift, NewMediaDuration, false);

                    TimeSpan timeSpan = TrackItemProcessor.GetTotalTime(_Playlist);

                    byte[] Original = ImageSourceToBytes(new PngBitmapEncoder(), imageItem);
                    //byte[] Copied = new byte[Original.Length];

                    //Array.Copy(Original, Copied, Original.Length);

                    Media NewMedia = new Media()
                    {
                        StartTime = NewMediaStartTime,
                        Duration = NewMediaDuration,
                        mediaType = EnumMedia.IMAGE,
                        ProjectId = _Playlist[0].ProjectId,
                        TrackId = 2,
                        Color = (Color)ColorConverter.ConvertFromString("Tomato"),
                        mediaData = Original
                    };

                    AddNewVideoEvent(NewMedia);

                    SetTotalTime(timeSpan);

                    //Must be After Add And Shift
                    PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));

                }
                else
                {
                    //Should Add After Video Event
                    List<Media> MediaToShift = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, Result.media.StartTime + Result.media.Duration);

                    TimeSpan NewMediaStartTime = TimeSpan.Zero;
                    TimeSpan NewMediaDuration = TimeSpan.Zero;
                    int MediaToShiftID = -1;

                    if (MediaToShift.Count > 0)
                    {
                        MediaToShiftID = MediaToShift[0].VideoEventID;
                        NewMediaStartTime = MediaToShift[0].StartTime;
                        NewMediaDuration = TimeSpan.FromSeconds(5);
                    }
                    else
                    {
                        NewMediaStartTime = Result.media.StartTime + Result.media.Duration;
                        NewMediaDuration = TimeSpan.FromSeconds(5);
                    }

                    List<Media> ItemsToShift = TrackItemProcessor.ShiftVideoEvents(MediaToShift, NewMediaDuration, false).ToList();



                    byte[] Original = ImageSourceToBytes(new PngBitmapEncoder(), imageItem);
                    //byte[] Copied = new byte[Original.Length];

                    //Array.Copy(Original, Copied, Original.Length);

                    Media NewMedia = new Media()
                    {
                        StartTime = NewMediaStartTime,
                        Duration = NewMediaDuration,
                        mediaType = EnumMedia.IMAGE,
                        ProjectId = _Playlist[0].ProjectId,
                        TrackId = 2,
                        Color = (Color)ColorConverter.ConvertFromString("Tomato"),
                        mediaData = Original
                    };

                    AddNewVideoEvent(NewMedia);

                    TimeSpan TotalTime = TrackItemProcessor.GetTotalTime(_Playlist);

                    SetTotalTime(TotalTime);

                    //Must be After Add And Shift
                    PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
                }

                //AddNewVideoEvent(imageItem, Result.TimeAtPoint, Result.media, PositionAtHalf);
            }
            else
            {
                //Add by timespan and shift video events after
                TimeSpan WasDroppedAt = Result.TimeAtPoint;

                byte[] Original = ImageSourceToBytes(new PngBitmapEncoder(), imageItem);
                //byte[] Copied = new byte[Original.Length];

                //Array.Copy(Original, Copied, Original.Length);

                Media NewMedia = new Media()
                {
                    StartTime = WasDroppedAt,
                    Duration = TimeSpan.FromSeconds(5),
                    mediaType = EnumMedia.IMAGE,
                    ProjectId = _Playlist[0].ProjectId,
                    TrackId = 2,
                    Color = (Color)ColorConverter.ConvertFromString("Tomato"),
                    mediaData = Original
                };

                //Find total space
                List<Media> ItemsAfter = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, WasDroppedAt);
                if (ItemsAfter.Count > 0)
                {
                    List<Media> Sorted = ItemsAfter.OrderBy(o => o.StartTime).ToList();
                    TimeSpan TimeOfNextEvent = Sorted[0].StartTime;
                    int ItemToShiftID = Sorted[0].VideoEventID;
                    Sorted.Clear();

                    TimeSpan SpaceAvailable = TimeOfNextEvent - WasDroppedAt;

                    //adjust video event duration to fill space
                    //- if space is less than 3 seconds shift all video events to make space

                    if (SpaceAvailable > TimeSpan.FromSeconds(3))
                    {
                        //There Is Enough Space Available

                        if (SpaceAvailable < TimeSpan.FromSeconds(5))
                        {
                            //But it is not longer than 5 Seconds - so shrink event
                            NewMedia.Duration = SpaceAvailable;
                        }

                        //Add New Video Event With Out Shifting

                        AddNewVideoEvent(NewMedia);

                        TimeSpan TotalTime = TrackItemProcessor.GetTotalTime(_Playlist);

                        SetTotalTime(TotalTime);

                    }
                    else
                    {
                        //There is Not Enough Space Available, Will Need to shift all events after

                        TimeSpan AmountToAdd = NewMedia.Duration - SpaceAvailable;

                        TrackItemProcessor.ShiftVideoEvents(ItemsAfter, AmountToAdd, false);

                        AddNewVideoEvent(NewMedia);

                        TimeSpan TotalTime = TrackItemProcessor.GetTotalTime(_Playlist);

                        SetTotalTime(TotalTime);
                    }
                }
                else
                {
                    //There is no events after this new event

                    //Add New Video Event With Out Shifting

                    AddNewVideoEvent(NewMedia);

                    TimeSpan TotalTime = TrackItemProcessor.GetTotalTime(_Playlist);

                    SetTotalTime(TotalTime);
                }

                //Must be After Add And Shift
                PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
            }
        }

        internal void SetVideoEventDuration(Media media, TimeSpan NewDuration, bool UpdateOriginalTime)
        {
            //Should Add Before The Video Event
            List<Media> MediaToShift = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, media.StartTime);

            MediaToShift.Remove(media);

            TimeSpan OldDuration = media.Duration;

            if (MediaToShift.Count != 0)
            {

                int MediaToShiftID = MediaToShift[0].VideoEventID;

                if (NewDuration < TimeSpan.FromSeconds(1))
                {
                    NewDuration = TimeSpan.FromSeconds(1);
                }





                if (OldDuration > NewDuration)
                {
                    //Old duration is longer so i need to reduce the time
                    TimeSpan AmountToAdd = OldDuration - NewDuration;
                    TrackItemProcessor.ShiftVideoEvents(MediaToShift, AmountToAdd, true);
                }
                else
                {
                    //New duration is longer so i need to Add time
                    TimeSpan AmountToAdd = NewDuration - OldDuration;
                    TrackItemProcessor.ShiftVideoEvents(MediaToShift, AmountToAdd, false);
                }
            }

            media.Duration = NewDuration;

            if (UpdateOriginalTime == true)
            {
                if (media.mediaType != EnumMedia.VIDEO)
                {
                    media.OriginalDuration = NewDuration;
                }
            }


            TimeSpan timeSpan = TrackItemProcessor.GetTotalTime(_Playlist);

            SetTotalTime(timeSpan);

            //Must be After Add And Shift
            PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
        }

        internal byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            encoder.Frames.Clear();

            return bytes;
        }

        internal void AddNewNoteItemByPoint(TextItem textItem, Point location, bool PositionAtHalf)
        {
            (Media media, TimeSpan TimeAtPoint) Result = TrackItemProcessor.FindMediaAtPoint(MainCanvas, LegendCanvas, _ViewportStart, _ViewportDuration, _Playlist, location, PositionAtHalf);

            if (Result.media != null)
            {
                AddNewNoteItem(textItem, Result.TimeAtPoint, Result.media, PositionAtHalf);

                //Must be After Add And Shift
                PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
            }
        }

        internal void AddNewNoteItemByTimeSpan(TextItem textItem, TimeSpan TimeSpanAtPoint, bool PositionAtHalf)
        {
            Media EventFound = TrackItemProcessor.FindMediaAtTimeSpan(_Playlist, textItem, TimeSpanAtPoint, PositionAtHalf);

            if (EventFound != null)
            {
                AddNewNoteItem(textItem, TimeSpanAtPoint, EventFound, PositionAtHalf);
            }
        }

        internal void AddNewNoteItemByTimeSpanIntoSelected(TextItem textItem, TimeSpan TimeSpanAtPoint, bool PositionAtHalf)
        {
            //Media EventFound = TrackItemProcessor.FindMediaAtTimeSpan(_Playlist, textItem, TimeSpanAtPoint, PositionAtHalf);

            if (_SelectedMedia != null && _SelectedMedia.Count == 1)
            {
                AddNewNoteItem(textItem, TimeSpanAtPoint, _SelectedMedia[0], PositionAtHalf);
                SetTimeLineMode(TimeLineMode.Selected);
            }
        }

        private void AddNewNoteItem(TextItem textItem, TimeSpan TimeSpanAtPoint, Media EventFound, bool PositionAtHalf)
        {
            NoteItemControl noteItemControl = TrackItemProcessor.CreateNoteAtTimeSpan(textItem, TimeSpanAtPoint, EventFound, PositionAtHalf, this, IsManageMedia ? _IsReadOnly : true);

            // check if it was dropped on an empty spot
            bool WasDroppedOnNote = false;
            TextItem DroppedOnItem = null;

            if (EventFound.RecordedTextList == null)
            {
                EventFound.RecordedTextList = new List<TextItem>();
            }

            for (int i = 0; i < EventFound.RecordedTextList.Count; i++)
            {
                TextItem recordedText = EventFound.RecordedTextList[i];
                if (textItem.Start >= recordedText.Start && textItem.Start < recordedText.Start + recordedText.Duration)
                {
                    WasDroppedOnNote = true;
                    DroppedOnItem = recordedText;
                    break;
                }
            }


            if (WasDroppedOnNote == true)
            {
                //Will need to add the new note after the one it was dropped on and if another note is in its duration
                //then it will need to be shifted as well as all other notes after it to make space for it.


                //Set the new start time of the new TextItem
                textItem.Start = DroppedOnItem.Start + DroppedOnItem.Duration;
            }

            bool FoundOverLappingNote = false;
            TimeSpan ShiftAmount = TimeSpan.Zero;
            List<(TextItem Item, int VideoEventID)> ItemsToShift = new List<(TextItem Item, int VideoEventID)>();

            for (int i = 0; i < EventFound.RecordedTextList.Count; i++)
            {
                //find overlapping text items with the new text item

                TextItem recordedText = EventFound.RecordedTextList[i];

                if (textItem.Start + textItem.Duration >= recordedText.Start && recordedText.Start + recordedText.Duration > textItem.Start)
                {
                    ShiftAmount = textItem.Start + textItem.Duration - recordedText.Start;
                    FoundOverLappingNote = true;
                    break;
                }
            }

            if (FoundOverLappingNote == true)
            {

                for (int i = 0; i < EventFound.RecordedTextList.Count; i++)
                {
                    //Find all the notes to shift
                    TextItem recordedText = EventFound.RecordedTextList[i];
                    if (recordedText.Start + TimeSpan.FromMilliseconds(100) >= textItem.Start)
                    {
                        ItemsToShift.Add((recordedText, EventFound.VideoEventID));
                    }
                }

                //Update The Notes In The Control
                for (int i = 0; i < ItemsToShift.Count; i++)
                {
                    //Find all the notes to shift
                    TextItem recordedText = ItemsToShift[i].Item;

                    recordedText.Start = recordedText.Start + ShiftAmount;
                }
            }

            bool WasDurationChanged = RecalculateVideoEventDuration(EventFound, textItem);

            EventFound.RecordedTextList.Add(textItem);
            CreateNoteEvents(noteItemControl);
            noteItemControl.SetTimeLine(this);

            //Refresh UI
            ReProcessNotes();
            NotesEditedEvent();

            if (WasDurationChanged == true)
            {
                //Must be After Add And Shift
                PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
            }
        }

        private bool RecalculateVideoEventDuration(Media EventFound, TextItem NewNote)
        {
            TimeSpan CurrentDuration = EventFound.Duration;

            TimeSpan NoteEndTime = TimeSpan.Zero;

            for (int i = 0; i < EventFound.RecordedTextList.Count; i++)
            {
                TextItem textItem = EventFound.RecordedTextList[i];
                if (textItem.Start + textItem.Duration > NoteEndTime)
                {
                    NoteEndTime = textItem.Start + textItem.Duration;
                }
            }

            if (NewNote != null)
            {
                //Check new note aswell
                if (NewNote.Start + NewNote.Duration > NoteEndTime)
                {
                    NoteEndTime = NewNote.Start + NewNote.Duration;
                }
            }

            //Clamp to Original time
            if (NoteEndTime < EventFound.OriginalDuration)
            {
                NoteEndTime = EventFound.OriginalDuration;
            }

            if (NoteEndTime > CurrentDuration)
            {
                //Must Update The Duration And Shift Events
                EventFound.Duration = NoteEndTime;
                TimeSpan ShiftTime = NoteEndTime - CurrentDuration;


                List<Media> ShiftList = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, EventFound.StartTime + TimeSpan.FromMilliseconds(100));
                TimeSpan MediaToShiftTime = ShiftList[0].StartTime;

                TrackItemProcessor.ShiftVideoEvents(ShiftList, ShiftTime, false);


            }
            else
            {
                // Needs to shrink duration and shift video events left

                EventFound.Duration = NoteEndTime;
                TimeSpan ShiftTime = CurrentDuration - NoteEndTime;

                List<Media> ShiftList = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, EventFound.StartTime + TimeSpan.FromMilliseconds(100));

                TrackItemProcessor.ShiftVideoEvents(ShiftList, ShiftTime, true);

            }

            if (CurrentDuration != EventFound.Duration)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void AddNewVideoEvent(Media NewMedia)
        {
            int IndexWhereToInsert = -1;
            for (int i = 0; i < _Playlist.Count; i++)
            {
                Media media = _Playlist[i];

                if (media.StartTime > NewMedia.StartTime)
                {
                    IndexWhereToInsert = i;
                    break;
                }
            }

            if (IndexWhereToInsert == -1)
            {
                _Playlist.Add(NewMedia);
            }
            else
            {
                _Playlist.Insert(IndexWhereToInsert, NewMedia);
            }


            _TimeLineMode = TimeLineMode.Project;

            TimeLineDrawEngine.ClearNoteItemControls();

            _TotalDuration = TimeSpan.Zero;

            foreach (var item in _Playlist)
            {
                if (item.RecordedTextList != null)
                {
                    foreach (var element in item.RecordedTextList)
                    {
                        NoteItemControl noteItemControl = TrackItemProcessor.CreateNoteControl(item, element, this, IsManageMedia ? _IsReadOnly : true);
                        noteItemControl.SetTimeLine(this);
                        CreateNoteEvents(noteItemControl);
                    }
                }

                if (item.StartTime + item.Duration > _TotalDuration)
                {
                    _TotalDuration = item.StartTime + item.Duration;
                }
            }

            SetTotalTime(_TotalDuration);
        }

        private void CreateNoteEvents(NoteItemControl noteItemControl)
        {
            noteItemControl.NoteSelected += NoteItemControl_NoteSelected;
            noteItemControl.NoteChanged += NoteItemControl_NoteChanged;
            noteItemControl.NoteDeleteClicked += NoteItemControl_NoteDeleteClicked;
            noteItemControl.RecalculateClicked += NoteItemControl_RecalculateClicked;

        }

        internal TimeSpan GetNextNoteStartTime(TimeSpan time)
        {
            return TrackItemProcessor.GetNextNoteStartTime(TimeLineDrawEngine.GetNoteItemControls(), _TotalDuration, time);
        }

        internal void SetZoomLevel(double value)
        {
            if (value < 0.1)
            {
                value = 0.1;
            }

            double LocationAtMainCursor = TimeLineHelpers.GetLocationByTimeSpan(MainCanvas, _ViewportStart, _ViewportDuration, MainCursorTime);
            double SubFromDuration = LocationAtMainCursor / MainCanvas.ActualWidth;

            TimeSpan OldDuration = _ViewportDuration;


            TimeSpan NewViewPortDuration = TimeSpan.FromMilliseconds(_TotalDuration.TotalMilliseconds * (1.0 - value));

            if (NewViewPortDuration > TimeSpan.FromSeconds(1))
            {

                double Difference = OldDuration.TotalSeconds - NewViewPortDuration.TotalSeconds;
                TimeSpan StartOffSet = TimeSpan.FromSeconds(Difference * SubFromDuration);


                _ViewportDuration = NewViewPortDuration;
                _ViewportStart = _ViewportStart + StartOffSet;


                if (_TimeLineMode == TimeLineMode.Project)
                {
                    if (_ViewportDuration > _TotalDuration)
                    {
                        _ViewportDuration = _TotalDuration;
                    }
                }
                else if (_TimeLineMode == TimeLineMode.Selected)
                {
                    if (_ViewportDuration > TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset())
                    {
                        _ViewportDuration = TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset();
                    }
                }

                if (_ViewportDuration.TotalSeconds < 1)
                {
                    _ViewportDuration = TimeSpan.FromSeconds(1);
                }

                if (_ViewportStart.TotalSeconds < TimeLineDrawEngine.GetOffset().TotalSeconds)
                {
                    _ViewportStart = TimeLineDrawEngine.GetOffset();
                }

                if (_ViewportStart + _ViewportDuration > _TotalDuration)
                {
                    _ViewportStart = _TotalDuration - _ViewportDuration;
                }


                SetTotalTime(_TotalDuration);
            }
        }

        private void MainCanvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double LocationAtMainCursor = TimeLineHelpers.GetLocationByTimeSpan(MainCanvas, _ViewportStart, _ViewportDuration, MainCursorTime);
            double SubFromDuration = LocationAtMainCursor / MainCanvas.ActualWidth;
            double StartDelta = e.Delta * SubFromDuration;
            double Power = _ViewportDuration.TotalSeconds;

            TimeSpan NewViewPortDuration = TimeSpan.FromMilliseconds(_ViewportDuration.TotalMilliseconds - (e.Delta * Power));

            if (NewViewPortDuration > TimeSpan.FromSeconds(1))
            {
                _ViewportDuration = NewViewPortDuration;
                _ViewportStart = TimeSpan.FromMilliseconds(_ViewportStart.TotalMilliseconds + (StartDelta * Power));


                if (_TimeLineMode == TimeLineMode.Project)
                {
                    if (_ViewportDuration > _TotalDuration)
                    {
                        _ViewportDuration = _TotalDuration;
                    }
                }
                else if (_TimeLineMode == TimeLineMode.Selected)
                {
                    if (_ViewportDuration > TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset())
                    {
                        _ViewportDuration = TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset();
                    }
                }

                if (_ViewportDuration.TotalSeconds < 1)
                {
                    _ViewportDuration = TimeSpan.FromSeconds(1);
                }

                if (_ViewportStart.TotalSeconds < TimeLineDrawEngine.GetOffset().TotalSeconds)
                {
                    _ViewportStart = TimeLineDrawEngine.GetOffset();
                }

                if (_ViewportStart + _ViewportDuration > _TotalDuration)
                {
                    _ViewportStart = _TotalDuration - _ViewportDuration;
                }

                e.Handled = true;

                SetTotalTime(_TotalDuration);
            }
        }

        private void MainCanvas_PreviewMouseWheel___________AtMouse(object sender, MouseWheelEventArgs e)
        {

            Point Location = Mouse.GetPosition(MainCanvas);
            double SubFromDuration = Location.X / MainCanvas.ActualWidth;
            double StartDelta = e.Delta * SubFromDuration;
            double Power = _ViewportDuration.TotalSeconds;

            _ViewportDuration = TimeSpan.FromMilliseconds(_ViewportDuration.TotalMilliseconds - (e.Delta * Power));
            _ViewportStart = TimeSpan.FromMilliseconds(_ViewportStart.TotalMilliseconds + (StartDelta * Power));


            if (_TimeLineMode == TimeLineMode.Project)
            {
                if (_ViewportDuration > _TotalDuration)
                {
                    _ViewportDuration = _TotalDuration;
                }
            }
            else if (_TimeLineMode == TimeLineMode.Selected)
            {
                if (_ViewportDuration > TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset())
                {
                    _ViewportDuration = TimeLineDrawEngine.GetTotalDuration() - TimeLineDrawEngine.GetOffset();
                }
            }

            if (_ViewportDuration.TotalSeconds < 1)
            {
                _ViewportDuration = TimeSpan.FromSeconds(1);
            }

            if (_ViewportStart.TotalSeconds < TimeLineDrawEngine.GetOffset().TotalSeconds)
            {
                _ViewportStart = TimeLineDrawEngine.GetOffset();
            }

            if (_ViewportStart + _ViewportDuration > _TotalDuration)
            {
                _ViewportStart = _TotalDuration - _ViewportDuration;
            }

            e.Handled = true;

            SetTotalTime(_TotalDuration);

        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsManageMedia)
                TrackItemSelectionEngine.MouseDown(MainCanvas);
        }


        public void SelectedEvent(Media videoEvent)
        {
            EventSelectionChangedEvent?.Invoke(new EventArgs(), videoEvent);
        }

        private void OnTrackbarMouseMoved(double positionX)
        {
            OverlappedEvents.Clear();
            TimeSpan TimeSpanAtPoint = GetTimeSpanByLocation(positionX);
            for (int i = 0; i < _Playlist.Count; i++)
            {
                Media media = _Playlist[i];
                if (TimeSpanAtPoint > media.StartTime && (TimeSpanAtPoint < media.StartTime + media.Duration))
                {
                    OverlappedEvents.Add(media);
                }
            }
           
            var trackbarMouseMoveEventPayload = new TrackbarMouseMoveEventModel
            {
                videoeventIds = OverlappedEvents?.OrderBy(x => x.TrackId).Select(x => x.VideoEventID).ToList(),
                timeAtTheMoment = TimeSpanAtPoint.ToString(@"hh\:mm\:ss\.fff"),
                isAnyVideo = OverlappedEvents.Select(x => x.TrackId == 2) != null
            };


            TrackbarMouseMoveEvent.Invoke(new EventArgs(), trackbarMouseMoveEventPayload);
        }

        public TimeSpan GetTrackbarTime()
        {
            return GetTimeSpanByLocation(TrackBarPosition.X);
        }

        public List<Media> GetTrackbarMediaEvents()
        {
            TimeSpan TimeSpanAtPoint = GetTrackbarTime();
            var trackbarEvents = new List<Media>();
            for (int i = 0; i < _Playlist.Count; i++)
            {
                Media media = _Playlist[i];
                if (TimeSpanAtPoint > media.StartTime && (TimeSpanAtPoint < media.StartTime + media.Duration) && media.TrackId == 2)
                {
                    trackbarEvents.Add(media);
                }
            }
            return trackbarEvents;
        }

        public void SetTrackbar()
        {
            Canvas.SetLeft(TrackbarLine2, TrackBarPosition.X);
        }

        public void SetTrackbarByTime()
        {
            TrackBarPosition.X = GetLocationByTimeSpan(TrackBarTimespan);
            Canvas.SetLeft(TrackbarLine2, TrackBarPosition.X);
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            (TimeSpan SelectionStart, TimeSpan SelectionEnd, bool WasMouseCaptured) Result = TrackItemSelectionEngine.MouseUp(MainCanvas, _ViewportStart, _ViewportDuration);

            if (Result.WasMouseCaptured == true)
            {
                SelectNotesInSelectionArea(Result.SelectionStart, Result.SelectionEnd);

                if (Math.Abs(Result.SelectionStart.TotalSeconds - Result.SelectionEnd.TotalSeconds) < 0.1)
                {
                    SetMainCursor();
                }
            }

        }

        private void SetMainCursor()
        {
            MainCursorTime = TimeLineDrawEngine.SetMainCursor(MainCanvas, _ViewportStart, _ViewportDuration);
            TimeLineCursorChangedEvent(MainCursorTime);
        }

        internal void SetMainCursorByTimeSpan(TimeSpan time, bool RaiseEvent)
        {
            MainCursorTime = time;
            TimeLineDrawEngine.UpdateMainCursor(MainCanvas, MainCursorTime, _ViewportStart, _ViewportDuration);
            if (RaiseEvent == true)
            {
                TimeLineCursorChangedEvent(MainCursorTime);
            }
        }

        private void SelectNotesInSelectionArea(TimeSpan selectionStart, TimeSpan selectionEnd)
        {
            TrackItemSelectionEngine.SelectNotesInSelectionArea(TimeLineDrawEngine.GetNoteItemControls(), selectionStart, selectionEnd);
            Keyboard.Focus(this);
        }

        private void MainCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            //TimeLineDrawEngine.SetPointerCursor(MainCanvas, _ViewportStart, _ViewportDuration);

            if (IsManageMedia)
            {
                TimeLineDrawEngine.SetPointerCursor(MainCanvas, _ViewportStart, _ViewportDuration);
                TrackItemSelectionEngine.MouseMoved(MainCanvas);
            }
        }

        private void MainCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            TimeLineDrawEngine.RemoveCursorFromCanvas(MainCanvas);
        }

        internal event EventHandler<CursorTimeUpdatedArgs> TimeLineCursorChanged;

        private void TimeLineCursorChangedEvent(TimeSpan time)
        {
            if (TimeLineCursorChanged != null)
            {
                TimeLineCursorChanged(this, new CursorTimeUpdatedArgs(time));
            }
        }

        Point OpenedContextMenuAt = new Point(0, 0);

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            OpenedContextMenuAt = Mouse.GetPosition(MainCanvas);
        }

        private void AddNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            TextItem textItem = new TextItem()
            {
                Text = "New Note Item",
            };
            textItem.Duration = TextMeasurement.Measure(textItem.Text);

            TimeSpan AtTime = GetTimeSpanByLocation(OpenedContextMenuAt.X);

            AddNewNoteItemByTimeSpan(textItem, AtTime, false);
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            RecordClickedEvent();
        }

        private void ReProcessNotes()
        {
            // I COMMENTED THIS OUT WAS CHANGING MODE TO PROJECT WHEN RECORDING TEXT ITEMS IN SELECTED MODE : TimeLineDrawEngine.SetPlaylist(_Playlist, TimeLineMode.Project);

            TimeLineDrawEngine.ClearNoteItemControls();

            foreach (var item in _Playlist)
            {
                if (item.RecordedTextList != null)
                {
                    foreach (var element in item.RecordedTextList)
                    {
                        NoteItemControl noteItemControl = TrackItemProcessor.CreateNoteControl(item, element, this, IsManageMedia ? _IsReadOnly : true);
                        noteItemControl.SetTimeLine(this);
                        CreateNoteEvents(noteItemControl);
                    }
                }
            }

            TimeLineDrawEngine.RedrawNotes(MainCanvas, LegendCanvas, _ViewportStart, _ViewportDuration, this);

            TimeSpan timeSpan = TrackItemProcessor.GetTotalTime(_Playlist);

            SetTotalTime(timeSpan);
        }

        internal void SetSelectedMedia(int VideoEventID)
        {
            if (VideoEventID >= 0)
            {
                for (int i = 0; i < _Playlist.Count; i++)
                {
                    Media media = _Playlist[i];
                    if (media.VideoEventID == VideoEventID)
                    {
                        _SelectedMedia = new List<Media>() { media };
                    }
                }
                //_SelectedMedia = new List<Media>() { selectedMedia };
            }
            else
            {
                _SelectedMedia = null;
            }
        }

        public event EventHandler RecordClicked;
        public void RecordClickedEvent()
        {
            if (RecordClicked != null)
            {
                RecordClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> NotesEdited;
        public void NotesEditedEvent()
        {
            if (NotesEdited != null)
            {
                NotesEdited(this, EventArgs.Empty);
            }
        }
        internal event EventHandler<ZoomLevelUpdatedArgs> ZoomLevelUpdated;
        internal void ZoomLevelUpdatedEvent(double value)
        {
            if (ZoomLevelUpdated != null)
            {
                ZoomLevelUpdated(this, new ZoomLevelUpdatedArgs(value));
            }
        }

        public event EventHandler<PlayListUpdatedArgs> PlayListUpdated;
        public void PlayListUpdatedEvent(PlayListUpdatedArgs playListArgs)
        {
            if (PlayListUpdated != null)
            {
                PlayListUpdated(this, playListArgs);
            }
        }

        private Media GetMedia(object sender)
        {
            MenuItem MenuItem = sender as MenuItem;

            if (MenuItem != null)
            {
                var menu = ((ContextMenu)MenuItem.Parent).PlacementTarget;
                var videoItem = menu as TrackVideoEventItem;
                if (videoItem != null)
                {
                    return videoItem.Media;
                }
                var calloutItem = menu as TrackCalloutItem;
                if (calloutItem != null)
                {
                    return calloutItem.MediaCallout;
                }
            }
            return null;
        }

        public void EditEventBtnClicked(object sender, RoutedEventArgs e)
        {
            var media = GetMedia(sender);
            Edit_Event(sender, media?.VideoEventID ?? -1);
        }

        public void DeleteEventForTimeline(object sender, RoutedEventArgs e)
        {
            var media = GetMedia(sender);
            Delete_Event(sender, media?.VideoEventID ?? -1);
        }

        public void CloneEventAtTrackbar(object sender, RoutedEventArgs e)
        {
            var media = GetMedia(sender);
            Clone_Event(sender, media);
        }

        public void CloneEventAtTimelineEnd(object sender, RoutedEventArgs e)
        {
            var media = GetMedia(sender);
            CloneAtEnd_Event(sender, media);
        }

        //public void AddImageUsingLibraryAfterSelectedEvent(object sender, RoutedEventArgs e)
        //{
        //    var media = GetMedia(sender);
        //}


        internal void DeleteEventBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = sender as MenuItem;
            if (MenuItem != null)
            {
                TrackVideoEventItem trackVideoEventItem = (TrackVideoEventItem)((ContextMenu)MenuItem.Parent).PlacementTarget;

                _Playlist.Remove(trackVideoEventItem.Media);

                //TimeLineDrawEngine.RemoveNotesInTimeSpan(trackVideoEventItem.Media.StartTime, trackVideoEventItem.Media.StartTime + trackVideoEventItem.Media.Duration);

                List<Media> MediaToShift = TrackItemProcessor.FindMediaAfterTimeSpan(_Playlist, trackVideoEventItem.Media.StartTime);

                TrackItemProcessor.ShiftVideoEvents(MediaToShift, trackVideoEventItem.Media.Duration, true);

                ReProcessNotes();

                //Must be After Add And Shift
                PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
            }
        }


        public void LocationChanged_Event(LocationChangedEventModel LCModel)
        {
            foreach (var item in LCModel.CallOutItems)
            {
                if(CalloutLocationOrSizeChangedMedia != null && CalloutLocationOrSizeChangedMedia.Count > 0)
                {
                    var isAlreadyExist = CalloutLocationOrSizeChangedMedia?.ToList().Find(x => x?.VideoEventID == item?.MediaCallout?.VideoEventID);
                    if (isAlreadyExist != null)
                        CalloutLocationOrSizeChangedMedia.Remove(isAlreadyExist);
                }
                CalloutLocationOrSizeChangedMedia.Add(item.MediaCallout);
            }

            //foreach (var item in LCModel.VideoEventItems)
            //{
            //    Console.WriteLine($"Media Location Changed with ID - {item.Media.VideoEventID} and new starttime {item.Media.StartTime.ToString(@"hh\:mm\:ss\.fff")}");
            //}
        }

        public void CalloutSizeChanged_Event(Media calloutMedia)
        {
            if (calloutMedia != null)
            {
                var isAlreadyExist = CalloutLocationOrSizeChangedMedia?.ToList().Find(x => x?.VideoEventID == calloutMedia?.VideoEventID);
                if (isAlreadyExist != null)
                    CalloutLocationOrSizeChangedMedia.Remove(isAlreadyExist);
                CalloutLocationOrSizeChangedMedia.Add(calloutMedia);
            }
        }


        public event EventHandler<SelectedVideoEventArgs> SelectedVideo;
        internal void FocusEventBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = sender as MenuItem;
            if (MenuItem != null)
            {
                TrackVideoEventItem trackVideoEventItem = (TrackVideoEventItem)((ContextMenu)MenuItem.Parent).PlacementTarget;
                if (SelectedVideo != null)
                {
                    //THIS EVENT WILL TRIGGER IN MANAGE MEDIA CONTROL
                    //SET THE LIST BOX
                    //LIST BOX SELECTION CHANGED WILL TRIGGER
                    //AND IT WILL SET THE SELECTED ITEM IN THIS CONTROL
                    SelectedVideo(this, new SelectedVideoEventArgs(trackVideoEventItem.Media));
                }
            }
        }

        public event EventHandler<SelectedVideoEventArgs> SetDurationEvent;

        internal void SetDurationBtn_Click(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = sender as MenuItem;
            if (MenuItem != null)
            {
                TrackVideoEventItem trackVideoEventItem = (TrackVideoEventItem)((ContextMenu)MenuItem.Parent).PlacementTarget;
                if (SetDurationEvent != null)
                {
                    SetDurationEvent(this, new SelectedVideoEventArgs(trackVideoEventItem.Media));
                }
            }
        }

        internal void FillWithNext(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = sender as MenuItem;
            if (MenuItem != null)
            {
                MissingVideoEventItem missingVideoEventItem = (MissingVideoEventItem)((ContextMenu)MenuItem.Parent).PlacementTarget;
                if (SelectedVideo != null)
                {
                    TimeSpan newStartTime = missingVideoEventItem.Item.Start;

                    missingVideoEventItem.Item.AfterMedia.StartTime = missingVideoEventItem.Item.Start;
                    missingVideoEventItem.Item.AfterMedia.Duration = missingVideoEventItem.Item.AfterMedia.Duration + missingVideoEventItem.Item.Duration;

                    CalculateTimeLineScrollBar();
                    DrawTimeLine();
                    ReProcessNotes();

                    //Must be After Add And Shift
                    PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
                }
            }
        }

        internal void RefreshTimeLine()
        {
            ReProcessNotes();
            SetTotalTime(TrackItemProcessor.GetTotalTime(_Playlist));

            //Must be After Add And Shift
            PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));
        }

        internal void FillWithPrevious(object sender, RoutedEventArgs e)
        {
            MenuItem MenuItem = sender as MenuItem;
            if (MenuItem != null)
            {
                MissingVideoEventItem missingVideoEventItem = (MissingVideoEventItem)((ContextMenu)MenuItem.Parent).PlacementTarget;
                if (SelectedVideo != null)
                {
                    missingVideoEventItem.Item.BeforeMedia.Duration = missingVideoEventItem.Item.BeforeMedia.Duration + missingVideoEventItem.Item.Duration;


                    CalculateTimeLineScrollBar();
                    DrawTimeLine();
                    ReProcessNotes();

                    //Must be After Add And Shift
                    PlayListUpdatedEvent(new PlayListUpdatedArgs(_Playlist));

                }
            }
        }


        internal void ClearResources()
        {
            TimeLineDrawEngine.ClearRecourses();
            if (_Playlist != null)
            {
                _Playlist.Clear();
            }

            if (_SelectedMedia != null)
            {
                _SelectedMedia.Clear();
            }

            MainCanvas.Children.Clear();
            LegendCanvas.Children.Clear();

            TimeLineDrawEngine = null;
            TrackItemSelectionEngine = null;
        }


    }

    public enum TimeLineMode
    {
        Selected, Project
    }

    public class SelectedVideoEventArgs : EventArgs
    {
        public Media MediaItem { get; set; }
        public SelectedVideoEventArgs(Media mediaItem)
        {
            MediaItem = mediaItem;
        }
    }

    internal class CursorTimeUpdatedArgs : EventArgs
    {
        internal TimeSpan Time { get; set; }
        internal CursorTimeUpdatedArgs(TimeSpan time)
        {
            Time = time;
        }
    }

    internal class ZoomLevelUpdatedArgs : EventArgs
    {
        internal double Value { get; set; }
        internal ZoomLevelUpdatedArgs(double value)
        {
            Value = value;
        }
    }

    public class PlayListUpdatedArgs : EventArgs
    {
        public List<Media> PlayList;

        public PlayListUpdatedArgs(List<Media> playList)
        {
            PlayList = playList;
        }
    }


}

