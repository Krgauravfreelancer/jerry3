using FullScreenPlayer_UserControl.Controls;
using FullScreenPlayer_UserControl.Models;
using LocalVoiceGen_UserControl.Helpers;
using ManageMedia_UserControl.Classes;
using ManageMedia_UserControl.Controls;
using ManageMedia_UserControl.Models;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using VoiceScripter_UserControl;
using VoiceScripter_UserControl.Models;

namespace ManageMedia_UserControl
{
    public partial class ManageMedia_Control : System.Windows.Controls.UserControl
    {
        int _ProjectID = -1;
        List<Media> _PlayList = new List<Media>();
        List<Media> _OriginalPlayList = new List<Media>();
        VoiceScripter_Library voiceScripter_Library;
        TimeSpan _RecordingStartOffsetTime = TimeSpan.Zero;
        TimeSpan _RecordingEndTime = TimeSpan.Zero;
        DateTime _RecordingStartTime = DateTime.Now;
        bool _IsRecording = false;
        DispatcherTimer _RecordingTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(50) };
        bool _ReadOnly = false;

        bool ItemsAreSaved = false;

        public event EventHandler<List<CBVVideoEvent>> SaveTimeline_Clicked;
        (TimeSpan ViewportStart, TimeSpan ViewportDuration, TimeSpan MainCursorTime) Viewport;

        public ManageMedia_Control()
        {
            InitializeComponent();
            _ReadOnly = false;
            Initialize();
        }

        public ManageMedia_Control(bool ReadOnly = false)
        {
            InitializeComponent();

            _ReadOnly = ReadOnly;

            Initialize();
        }

        private void Initialize()
        {
            TimeLineControl.SetReadOnly(_ReadOnly);

            List<TimeLineMode> timeLineModes = new List<TimeLineMode>()
            {
                TimeLineMode.Project,
                TimeLineMode.Selected,
            };
            ModeBox.ItemsSource = timeLineModes;
            ModeBox.SelectedIndex = 0;
            if (_ReadOnly == false)
            {
                _RecordingTimer.Tick += _RecordingTimer_Tick;
            }
            else
            {
                RecordPanel.Visibility = Visibility.Collapsed;
                ControlBar.Visibility = Visibility.Visible;
                RecordBtn.IsEnabled = false;
            }
            ResetCalloutLocationOrSizeChangedMedia();
        }

        private void CalloutLocationOrSizeChangedMedia_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                SaveBtn.IsEnabled = true;
                SaveBtnIcon.Opacity = 1;
                SaveBtnTxt.Text = $"- {TimeLineControl?.CalloutLocationOrSizeChangedMedia?.Count} Unsaved Changes ...";
            }
            else
            {
                SaveBtn.IsEnabled = false;
                SaveBtnIcon.Opacity = 0.5;
                SaveBtnTxt.Text = "- Media Items Saved   ✓";
            }
        }

        public void ResetCalloutLocationOrSizeChangedMedia()
        {
            TimeLineControl.CalloutLocationOrSizeChangedMedia = new System.Collections.ObjectModel.ObservableCollection<Media>();
            TimeLineControl.CalloutLocationOrSizeChangedMedia.CollectionChanged += CalloutLocationOrSizeChangedMedia_CollectionChanged;
        }

        public void SetProjectInfo(int projectID)
        {
            _ProjectID = projectID;
        }

        public void SetNoteID(int index, TextItem textItem)
        {
            textItem.NoteID = index;
        }

        public void LoadEvents(List<Media> MediaList)
        {
            List<Media> SortedList = MediaList.OrderBy(o => o.StartTime).ToList();
            MediaList.Clear();
            _PlayList = SortedList;
            _OriginalPlayList.Clear();

            ItemsAreSaved = true;
            UpdateSaveBtnStatus();

            //Duplicate PlayList with out media to compare when saving
            for (int i = 0; i < SortedList.Count; i++)
            {
                Media media = SortedList[i];

                List<TextItem> DuplicatedTextList = new List<TextItem>();

                for (int j = 0; j < media.RecordedTextList.Count; j++)
                {
                    TextItem OriginalItem = media.RecordedTextList[j];
                    TextItem NewItem = new TextItem()
                    {
                        NoteID = OriginalItem.NoteID,
                        Start = OriginalItem.Start,
                        Duration = OriginalItem.Duration,
                        WordCount = OriginalItem.WordCount,
                        Text = OriginalItem.Text,
                        AddedToMedia = OriginalItem.AddedToMedia
                    };

                    DuplicatedTextList.Add(NewItem);
                }

                _OriginalPlayList.Add(new Media()
                {
                    ProjectId = media.ProjectId,
                    TrackId = media.TrackId,
                    VideoEventID = media.VideoEventID,
                    StartTime = media.StartTime,
                    Duration = media.Duration,
                    OriginalDuration = media.OriginalDuration,
                    mediaType = media.mediaType,
                    RecordedTextList = DuplicatedTextList,
                    Color = media.Color
                });
            }

            LoadPlayer();

            TimeLineControl.LoadMedia(SortedList);

            if (Viewport.ViewportStart != TimeSpan.Zero || Viewport.ViewportDuration != TimeSpan.Zero)
            {
                TimeLineControl.SetViewport(Viewport.ViewportStart, Viewport.ViewportDuration, Viewport.MainCursorTime);
            }
        }

        private void UpdateSaveBtnStatus()
        {
            if (ItemsAreSaved == false)
            {
                SaveBtn.IsEnabled = true;
                SaveBtnIcon.Opacity = 1;
                SaveBtnTxt.Text = "- Unsaved Changes ...";
            }
            else
            {
                SaveBtn.IsEnabled = false;
                SaveBtnIcon.Opacity = 0.5;
                SaveBtnTxt.Text = "- Media Items Saved   ✓";
            }
        }

        private void LoadPlayer()
        {
            List<PlaylistItem> playlist = new List<PlaylistItem>();
            foreach (var item in _PlayList)
            {
                if (item.TrackId == 2)
                {
                    var itemExtended = new MediaExtended(item);

                    if (item.ImageType != null && item.ImageType != "")
                    {
                        itemExtended.MediaName = FirstCharToUpper(item.ImageType);
                    }
                    else
                    {
                        itemExtended.MediaName = item.mediaType.ToString();
                    }

                    if (item.TrackId == 2)
                    {
                        if (item.mediaType == EnumMedia.IMAGE)
                        {
                            itemExtended.MediaIcon = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Image-Small-Dark.png"));
                        }
                        else if (item.mediaType == EnumMedia.VIDEO)
                        {
                            itemExtended.MediaIcon = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Video-Small-Dark.png"));
                        }
                    }
                    else if (item.TrackId == 3)
                    {
                        itemExtended.MediaIcon = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));
                    }
                    else if (item.TrackId == 4)
                    {
                        itemExtended.MediaIcon = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));
                    }
                    else if (item.TrackId == 1)
                    {
                        itemExtended.MediaIcon = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small-Dark.png"));
                    }

                    itemExtended.Start = item.StartTime.ToString(@"mm\:ss\:fff");
                    itemExtended.ClipDuration = item.Duration.ToString(@"mm\:ss\:fff");
                }

                var playlistItem = CreatePlaylistItem(item);
                if (playlistItem != null)
                {
                    playlist.Add(CreatePlaylistItem(item));
                }
            }



            TotalTimeTxt.Text = (_PlayList.LastOrDefault()?.StartTime + _PlayList.LastOrDefault()?.Duration)?.ToString(@"mm\:ss\:fff");

            Player.Init(playlist);
        }

        private string FirstCharToUpper(string input)
        {
            if (input.Length > 0)
            {
                return input[0].ToString().ToUpper() + input.Substring(1);
            }
            else
            {
                return input;
            }
        }

        private PlaylistItem CreatePlaylistItem(Media media)
        {
            FullScreenPlayer_UserControl.Models.MediaType mediaType = FullScreenPlayer_UserControl.Models.MediaType.Image;
            bool IsVideoMedia = true;

            switch (media.mediaType)
            {
                case EnumMedia.IMAGE:
                    mediaType = FullScreenPlayer_UserControl.Models.MediaType.Image;
                    break;
                case EnumMedia.VIDEO:
                    mediaType = FullScreenPlayer_UserControl.Models.MediaType.Video;
                    break;
                default:
                    IsVideoMedia = false;
                    break;
            }

            Track VideoTrack = Track.Track1;


            switch (media.TrackId)
            {
                case 1:
                    VideoTrack = Track.Track1;
                    break;
                case 2:
                    VideoTrack = Track.Track2;
                    break;
                case 3:
                    VideoTrack = Track.Track3;
                    break;
                case 4:
                    VideoTrack = Track.Track4;
                    break;
            }

            switch (media.mediaType)
            {
                case EnumMedia.IMAGE:
                    mediaType = FullScreenPlayer_UserControl.Models.MediaType.Image;
                    break;
                case EnumMedia.VIDEO:
                    mediaType = FullScreenPlayer_UserControl.Models.MediaType.Video;
                    break;
                default:
                    IsVideoMedia = false;
                    break;
            }

            if (IsVideoMedia == true)
            {
                if (media.mediaData != null)
                {
                    byte[] MediaData = media.mediaData;

                    List<PlayListTextItem> PlayListTextItemList = new List<PlayListTextItem>();

                    if (media.RecordedTextList != null)
                    {
                        foreach (TextItem element in media.RecordedTextList)
                        {
                            PlayListTextItemList.Add(new PlayListTextItem() { StartTime = element.Start, Text = element.Text });
                        }
                    }

                    PlaylistItem playlistItem = new PlaylistItem(media.VideoEventID, mediaType, media.StartTime, media.Duration, PlayListTextItemList, VideoTrack, MediaData);

                    return playlistItem;
                }
            }

            return null;
        }

        private void ModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeBox.SelectedItem != null)
            {
                MediaExtended selectedMedia = null;

                if (selectedMedia != null)
                {
                    TimeLineControl.SetSelectedMedia(((Media)selectedMedia).VideoEventID);
                }
                else
                {
                    TimeLineControl.SetSelectedMedia(-1);
                }

                TimeLineControl.SetTimeLineMode((TimeLineMode)ModeBox.SelectedItem);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
            {
                voiceScripter_Library = new VoiceScripter_Library();
                voiceScripter_Library.TextReceived += VoiceScripter_Library_TextReceived;
                voiceScripter_Library.StartedRecording += VoiceScripter_Library_StartedRecording;
                voiceScripter_Library.StoppedRecording += VoiceScripter_Library_StoppedRecording;
                AudioSrcCmb.ItemsSource = voiceScripter_Library.GetSources();

                if (AudioSrcCmb.Items.Count > 0)
                {
                    AudioSrcCmb.SelectedIndex = 0;
                }
            }
        }

        private void VoiceScripter_Library_StoppedRecording(object sender, EventArgs e)
        {
            VoiceStatusTxt.Text = "Idle";
        }

        private void VoiceScripter_Library_StartedRecording(object sender, EventArgs e)
        {
            VoiceStatusTxt.Text = "Active";
        }

        private void VoiceScripter_Library_TextReceived(object sender, NewLineReceivedEventArgs e)
        {

            TimeSpan TextTime = _RecordingStartOffsetTime + e.TextItem.Start;
            if (TimeLineControl.GetTimeLineMode() == TimeLineMode.Project)
            {
                TimeLineControl.AddNewNoteItemByTimeSpan(new TextItem()
                {
                    Text = e.TextItem.Text,
                    Duration = e.TextItem.Duration,
                }, TextTime, false);
            }
            else if (TimeLineControl.GetTimeLineMode() == TimeLineMode.Selected) 
            {
                TimeLineControl.AddNewNoteItemByTimeSpanIntoSelected(new TextItem()
                {
                    Text = e.TextItem.Text,
                    Duration = e.TextItem.Duration,
                }, TextTime, false);
            }
        }

        private void TimeLineControl_TimeLineCursorChanged(object sender, CursorTimeUpdatedArgs e)
        {
            CurrentTimeTxt.Text = e.Time.ToString(@"mm\:ss\:fff");
            Player.Position_Changed -= Player_Position_Changed;
            Player.SeekAndPause(e.Time);
            Player.Position_Changed += Player_Position_Changed;
        }


        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_IsRecording == false)
            {
                Player.Position_Changed -= Player_Position_Changed;
                Player.Play();
                voiceScripter_Library.StartRecording();
                RecordBtnLight.Background = new SolidColorBrush(Colors.DarkRed);
                _IsRecording = true;
                _RecordingTimer.Start();
                _RecordingStartTime = DateTime.Now;
                _RecordingStartOffsetTime = TimeLineControl.MainCursorTime;
                _RecordingEndTime = TimeLineControl.GetNextNoteStartTime(_RecordingStartOffsetTime);
            }
            else
            {
                StopRecording();
            }
        }

        private void AudioSrcCmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (voiceScripter_Library != null)
            {
                voiceScripter_Library.SetRecordingDevice(AudioSrcCmb.SelectedIndex);
            }
        }

        private void _RecordingTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan ElapsedTime = DateTime.Now - _RecordingStartTime;
            TimeSpan ElapsedTimeWithOffset = ElapsedTime + _RecordingStartOffsetTime;

            TimeLineControl.SetMainCursorTime(ElapsedTimeWithOffset);

            if (TimeLineControl.GetTimeLineMode() == TimeLineMode.Project)
            {
                // When reaching the end of all the events stop recording
                if (ElapsedTimeWithOffset + _RecordingTimer.Interval > _RecordingEndTime)
                {
                    StopRecording();
                }
            }
            else if (TimeLineControl.GetTimeLineMode() == TimeLineMode.Selected)
            {
                // When reaching the end of selected event pause player and keep recording
                if (ElapsedTimeWithOffset + _RecordingTimer.Interval > _RecordingEndTime)
                {
                    Player.Pause();
                }
            }
        }

        private void StopRecording()
        {

            voiceScripter_Library.StopRecording();
            RecordBtnLight.Background = new SolidColorBrush(Colors.Gray);
            _IsRecording = false;
            _RecordingTimer.Stop();
            Player.Position_Changed -= Player_Position_Changed;
            Player.Position_Changed += Player_Position_Changed;

        }

        private void TimeLineControl_RecordClicked(object sender, EventArgs e)
        {
            RecordBtn_Click(null, null);
        }

        private void ReloadFullScreenPlayerTextItems()
        {
            //Reload Changed Text Items Into FullScreenPlayer
            List<(TextItem TextItem, int VideoEventID)> TextItems = TimeLineControl.GetTextItems();
            List<(PlayListTextItem TextItem, int VideoEventID)> PlayListTextItemList = new List<(PlayListTextItem TextItem, int VideoEventID)>();
            foreach ((TextItem TextItem, int VideoEventID) element in TextItems)
            {
                PlayListTextItemList.Add((new PlayListTextItem() { StartTime = element.TextItem.Start, Text = element.TextItem.Text }, element.VideoEventID));
            }

            Player.ReloadNotes(PlayListTextItemList);
        }

        private void Player_Position_Changed(object sender, FullScreenPlayer_UserControl.PositionChangedArgs e)
        {
            TimeLineControl.SetMainCursorByTimeSpan(e.Position, false);
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Player.State == FullScreenPlayer_UserControl.FullScreenPlayerState.Playing)
            {
                Player.Pause();
            }
            else
            {
                Player.Play();
            }
        }

        private void PrevBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Prev();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Next();
        }

        private void Player_Playing(object sender, EventArgs e)
        {
            ((Rectangle)PlayBtn.Content).OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/Pause.png")));
        }

        private void Player_Paused(object sender, EventArgs e)
        {
            ((Rectangle)PlayBtn.Content).OpacityMask = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/Play.png")));
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ZoomSlider.SelectionEnd = ZoomSlider.Value;
            if (TimeLineControl != null)
            {
                TimeLineControl.ZoomLevelUpdated -= TimeLineControl_ZoomLevelUpdated;
                TimeLineControl.SetZoomLevel((double)ZoomSlider.Value / (double)ZoomSlider.Maximum);
                TimeLineControl.ZoomLevelUpdated += TimeLineControl_ZoomLevelUpdated;
            }
        }

        public bool ShowClosingPrompt()
        {
            (List<Media> DeletedVideoEvents,
                List<Media> ChangedVideoEvents,
                List<Media> CreatedVideoEvents,
                List<TextItem> DeletedTextItem,
                List<TextItem> ChangedTextItem,
                List<(TextItem TextItem, int VideoEventID)> CreatedTextItem) Result = AnalyseChanges();

            if (
                Result.DeletedVideoEvents.Count != 0 ||
                Result.ChangedVideoEvents.Count != 0 ||
                Result.CreatedVideoEvents.Count != 0 ||
                Result.DeletedTextItem.Count != 0 ||
                Result.ChangedTextItem.Count != 0 ||
                Result.CreatedTextItem.Count != 0
                )
            {
                ClosingPrompt closingPrompt = new ClosingPrompt();

                closingPrompt.CloseClicked += (se, ee) => {
                    if (CloseWindow != null)
                    {
                        CloseWindow(this, EventArgs.Empty);
                    }
                };

                closingPrompt.SaveClicked += (se, ee) => {
                    ManageMediaSaveEvent(
                        new ManageMediaSaveEventArgs(
                        Result.CreatedVideoEvents,
                        Result.DeletedVideoEvents,
                        Result.ChangedVideoEvents,
                        Result.CreatedTextItem,
                        Result.DeletedTextItem,
                        Result.ChangedTextItem, 
                        true));
                };

                closingPrompt.CancelClicked += (se, ee) => {
                    ControlContainer.Children.Remove(closingPrompt);
                    MainContainer.Effect = null;
                };

                ControlContainer.Children.Add(closingPrompt);

                BlurEffect blurEffect = new BlurEffect() { Radius = 10 };
                MainContainer.Effect = blurEffect;

                return false; //return saved as false
            }
            else
            {
                return true; //return saved as true
            }
        }

        private void ZoomInBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ZoomSlider.Value = ZoomSlider.Value + 1;
        }

        private void ZoomOutBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ZoomSlider.Value = ZoomSlider.Value - 1;
        }

        private void TimeLineControl_ZoomLevelUpdated(object sender, ZoomLevelUpdatedArgs e)
        {
            int value = (int)Math.Round((8.0 * (1.0 - e.Value)));
            ZoomSlider.ValueChanged -= ZoomSlider_ValueChanged;
            ZoomSlider.Value = value;
            ZoomSlider.SelectionEnd = ZoomSlider.Value;
            ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
        }

        private void TimeLineControl_SelectedVideo(object sender, SelectedVideoEventArgs e)
        {

        }

        private void TimeLineControl_PlayListUpdated(object sender, PlayListUpdatedArgs e)
        {
            _PlayList = e.PlayList;
            LoadPlayer();

            ItemsAreSaved = false;
            UpdateSaveBtnStatus();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Player.Init(new List<PlaylistItem>());
            TimeLineControl.ClearResources();
            _PlayList.Clear();
            voiceScripter_Library = null;
            _RecordingTimer.Tick -= _RecordingTimer_Tick;
            _RecordingTimer = null;

            Player.Position_Changed -= Player_Position_Changed;
            Player.Playing -= Player_Playing;
            Player.Paused -= Player_Paused;
            PlayerContainer.Child = null;

            this.Content = null;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (TimeLineControl?.CalloutLocationOrSizeChangedMedia?.Count > 0)
            {
                var modifiedEvents = new List<CBVVideoEvent>();
                foreach (var item in TimeLineControl?.CalloutLocationOrSizeChangedMedia)
                {
                    if (item != null)
                    {
                        var videoevent = DataManagerSqlLite.GetVideoEventbyId(item.VideoEventID, false, false).FirstOrDefault();
                        videoevent.videoevent_start = item.StartTime.ToString(@"hh\:mm\:ss\.fff");
                        videoevent.videoevent_duration = item.Duration.ToString(@"hh\:mm\:ss\.fff");
                        videoevent.videoevent_end = DataManagerSqlLite.CalcNextEnd(videoevent.videoevent_start, videoevent.videoevent_duration);
                        modifiedEvents.Add(videoevent);
                    }
                }
                SaveTimeline_Clicked.Invoke(sender, modifiedEvents);
                ItemsAreSaved = true;
                UpdateSaveBtnStatus();
            }
            else
            {
                // Will be used in READONLY = FALSE
                (List<Media> DeletedVideoEvents,
                    List<Media> ChangedVideoEvents,
                    List<Media> CreatedVideoEvents,
                    List<TextItem> DeletedTextItem,
                    List<TextItem> ChangedTextItem,
                    List<(TextItem TextItem, int VideoEventID)> CreatedTextItem) Result = AnalyseChanges();

                Viewport = TimeLineControl.GetViewport();

                ManageMediaSaveEvent(
                    new ManageMediaSaveEventArgs(
                    Result.CreatedVideoEvents,
                    Result.DeletedVideoEvents,
                    Result.ChangedVideoEvents,
                    Result.CreatedTextItem,
                    Result.DeletedTextItem,
                    Result.ChangedTextItem,
                    false));

                ItemsAreSaved = true;
                UpdateSaveBtnStatus();
            }
        }

        private (List<Media> DeletedVideoEvents, List<Media> ChangedVideoEvents, List<Media> CreatedVideoEvents,
            List<TextItem> DeletedTextItem, List<TextItem> ChangedTextItem, List<(TextItem TextItem, int VideoEventID)> CreatedTextItem) AnalyseChanges()
        {
            List<Media> DeletedVideoEvents = new List<Media>();
            List<Media> ChangedVideoEvents = new List<Media>();
            List<Media> CreatedVideoEvents = new List<Media>();

            List<TextItem> DeletedTextItem = new List<TextItem>();
            List<TextItem> ChangedTextItem = new List<TextItem>();
            List<(TextItem TextItem, int VideoEventID)> CreatedTextItem = new List<(TextItem TextItem, int index)>();

            DataValidator dataValidator = new DataValidator();

            #region Find Deleted VideoEvents
            //Cycle through the Original PlayList and see if the new PlayList contains it
            for (int i = 0; i < _OriginalPlayList.Count; i++)
            {
                Media OldMedia = _OriginalPlayList[i];

                Media NewMedia = dataValidator.DoesItContain(OldMedia, _PlayList);

                if (NewMedia == null)
                {
                    //It Was Deleted
                    DeletedVideoEvents.Add(OldMedia);
                }
            }
            #endregion

            #region Find Created VideoEvents
            //Cycle through the new PlayList and see if the old PlayList contains it
            for (int i = 0; i < _PlayList.Count; i++)
            {
                Media NewMedia = _PlayList[i];

                Media OldMedia = dataValidator.DoesItContain(NewMedia, _OriginalPlayList);

                if (OldMedia == null)
                {
                    //It Was Created
                    Debug.Assert(NewMedia.VideoEventID == 0);
                    CreatedVideoEvents.Add(NewMedia);
                }
            }
            #endregion

            #region Find Changed VideoEvents
            //Make List of Created And Deleted items removed
            //Deleted items aren't in the list already
            List<Media> ListToAnalyse = dataValidator.CreateListWithItemsRemoved(_PlayList, CreatedVideoEvents);
            for (int i = 0; i < ListToAnalyse.Count; i++)
            {
                Media NewMedia = ListToAnalyse[i];

                Media OldMedia = dataValidator.FindMediaByVideoEventID(_OriginalPlayList, NewMedia.VideoEventID);

                //If it is null it is a newly created video event
                Debug.Assert(OldMedia != null);

                bool IsDifferent = dataValidator.IsMediaDifferent(OldMedia, NewMedia);

                if (IsDifferent == true)
                {
                    ChangedVideoEvents.Add(NewMedia);
                }
            }
            #endregion

            #region Find Created Notes
            //Loop through the video events
            //- loop through the textItems
            //-Analise if the old list contains it
            for (int i = 0; i < _PlayList.Count; i++)
            {
                Media NewMedia = _PlayList[i];
                Media OldMedia = dataValidator.FindMediaByVideoEventID(_OriginalPlayList, NewMedia.VideoEventID);
                if (OldMedia == null)
                {
                    // It Is A New Video Event, All Notes Are Newly Created

                    //because of this i need the videoID so new videoEvents need to update new textItems separately
                    //for (int j = 0; j < NewMedia.RecordedTextList.Count; j++)
                    //{
                    //    TextItem textItem = NewMedia.RecordedTextList[j];
                    //    CreatedTextItem.Add(textItem);
                    //}
                }
                else
                {
                    // It is a already created video event
                    // Notes might still be created
                    // Cycle through Text List And see if it exists
                    for (int j = 0; j < NewMedia.RecordedTextList.Count; j++)
                    {
                        TextItem textItem = NewMedia.RecordedTextList[j];

                        if (textItem.NoteID != -1)
                        {
                            TextItem OldTextItem = dataValidator.FindNoteByNoteID(OldMedia.RecordedTextList, textItem.NoteID);

                            if (OldTextItem == null)
                            {
                                //Does not exist in the old VideoEvent so its a new note
                                CreatedTextItem.Add((textItem, OldMedia.VideoEventID));
                            }
                        }
                        else
                        {
                            // It is a new note because new notes are initialized with -1
                            CreatedTextItem.Add((textItem, OldMedia.VideoEventID));
                        }
                    }
                }
            }
            #endregion

            #region Find Deleted Notes
            //Loop through the Old video events
            //- loop through the textItems
            //-Analise if the New list contains it
            for (int i = 0; i < _OriginalPlayList.Count; i++)
            {
                Media oldMedia = _OriginalPlayList[i];
                Media newMedia = dataValidator.FindMediaByVideoEventID(_PlayList, oldMedia.VideoEventID);
                if (newMedia == null)
                {
                    // It Is A Deleted Video Event, All Notes Are Deleted automatically
                }
                else
                {
                    // It is a already created video event
                    // Notes might still be deleted
                    // Cycle through Text List And see if it exists
                    for (int j = 0; j < oldMedia.RecordedTextList.Count; j++)
                    {
                        TextItem textItem = oldMedia.RecordedTextList[j];

                        TextItem NewTextItem = dataValidator.FindNoteByNoteID(newMedia.RecordedTextList, textItem.NoteID);

                        if (NewTextItem == null)
                        {
                            //Does not exist in the new VideoEvent so its a deleted note
                            DeletedTextItem.Add(textItem);
                        }
                    }
                }
            }
            #endregion

            #region Find Changed Notes
            //loop through notes 
            //- if there is both an old note and a new note then compare them
            //- if they are different then it has changed

            //only old notes can be changed so loop through old playList
            for (int i = 0; i < _OriginalPlayList.Count; i++)
            {
                Media OldMedia = _OriginalPlayList[i];
                Media NewMedia = dataValidator.FindMediaByVideoEventID(_PlayList, OldMedia.VideoEventID);

                if (NewMedia != null)
                {
                    //There is both new media and old media

                    // loop trough notes and look for changes
                    for (int j = 0; j < OldMedia.RecordedTextList.Count; j++)
                    {
                        TextItem OldTextItem = OldMedia.RecordedTextList[j];
                        TextItem NewTextItem = dataValidator.FindNoteByNoteID(NewMedia.RecordedTextList, OldTextItem.NoteID);

                        if (NewTextItem != null)
                        {
                            //It Exists in Both Lists compare them

                            bool IsDifferent = dataValidator.IsNotesDifferent(OldTextItem, NewTextItem);

                            if (IsDifferent == true)
                            {
                                //The new note is different from the old note so it has changed

                                //If it is -1 then its a new note
                                Debug.Assert(NewTextItem.NoteID != -1);

                                ChangedTextItem.Add(NewTextItem);
                            }
                        }
                    }
                }

            }

            #endregion

            return (DeletedVideoEvents, ChangedVideoEvents, CreatedVideoEvents, DeletedTextItem, ChangedTextItem, CreatedTextItem);
        }

        public event EventHandler<ManageMediaSaveEventArgs> ManageMediaSave;
        public void ManageMediaSaveEvent(ManageMediaSaveEventArgs saveArgs)
        {
            if (ManageMediaSave != null)
            {
                ManageMediaSave(this, saveArgs);
            }
        }

        public event EventHandler<EventArgs> CloseWindow;

        private void TimeLineControl_NotesEdited(object sender, EventArgs e)
        {
            ReloadFullScreenPlayerTextItems();

            ItemsAreSaved = false;
            UpdateSaveBtnStatus();
        }

        private void TimeLineControl_SetDurationEvent(object sender, SelectedVideoEventArgs e)
        {
            SetDurationPrompt SetDurationPrompt = new SetDurationPrompt(e.MediaItem);

            SetDurationPrompt.SaveClicked += (se, ee) => {
                ControlContainer.Children.Remove(SetDurationPrompt);
                MainContainer.Effect = null;
                TimeLineControl.SetVideoEventDuration(ee.Media, ee.NewDuration, ee.UpdateOriginalTime);
            };

            SetDurationPrompt.CancelClicked += (se, ee) => {
                ControlContainer.Children.Remove(SetDurationPrompt);
                MainContainer.Effect = null;
            };

            ControlContainer.Children.Add(SetDurationPrompt);

            BlurEffect blurEffect = new BlurEffect() { Radius = 10 };
            MainContainer.Effect = blurEffect;
        }

        private void FullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.IsControlBarVisible = true;
            Player.ToggleFullScreenMode();
        }

        private void Player_ExitFullScreen_Clicked(object sender, EventArgs e)
        {
            Player.IsControlBarVisible = false;
        }
    }

    public class MediaExtended : Media
    {
        public string MediaName { get; set; }
        public string Start { get; set; }
        public string ClipDuration { get; set; }
        public ImageSource MediaIcon { get; set; }

        public MediaExtended(Media media)
        {
            foreach (var prop in media.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(media, null), null);
            }
        }
    }

    public class ManageMediaSaveEventArgs : EventArgs
    {
        public List<Media> CreatedVideoEvents { get; set; }
        public List<Media> DeletedVideoEvents { get; set; }
        public List<Media> ChangedVideoEvents { get; set; }
        public List<(TextItem TextItem, int VideoEventID)> CreatedNotes { get; set; }
        public List<TextItem> DeletedNotes { get; set; }
        public List<TextItem> ChangedNotes { get; set; }

        public bool CloseOnSave { get; set; }

        public ManageMediaSaveEventArgs(List<Media> createdVideoEvents, List<Media> deletedVideoEvents, List<Media> changedVideoEvents, List<(TextItem TextItem, int VideoEventID)> createdNotes, List<TextItem> deletedNotes, List<TextItem> changedNotes, bool closeOnSave)
        {

            CreatedVideoEvents = createdVideoEvents;
            DeletedVideoEvents = deletedVideoEvents;
            ChangedVideoEvents = changedVideoEvents;
            CreatedNotes = createdNotes;
            DeletedNotes = deletedNotes;
            ChangedNotes = changedNotes;
            CloseOnSave = closeOnSave;
        }
    }
}
