using ScreenRecording_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using Timeline.UserControls.Config;
using Timeline.UserControls.Models;
using Timeline.UserControls.Variables;
using Timeline.UserControls.Controls;

namespace Timeline_UserControl
{
    public partial class Timeline_UserControl : UserControl
    {
        public bool HasData { get; set; } = false;
        private int selectedProjectId;

        public event EventHandler ContextMenu_AddVideoEvent_Success;
        public event EventHandler ContextMenu_AddForm_Clicked;
        public event EventHandler ContextMenu_Run_Clicked;

        ///  Use the interface ITimelineGridControl to view all available TimelineUserControl methods and description.
        ITimelineGridControl _timelineGridControl;

        public Timeline_UserControl()
        {
            InitializeComponent();
        }

        public void SetSelectedProjectId(int project_id, int videoEvent_id = -1)
        {
            selectedProjectId = project_id;
            InitializeTimeline();
        }

        private void ContextMenuAddVideoEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            var screenRecordingUserControl = new ScreenRecordingUserControl(selectedProjectId);
            var window = new Window
            {
                Title = "Screen Recorder",
                Content = screenRecordingUserControl,
                ResizeMode = ResizeMode.NoResize,
                Height = 450,
                Width = 750,
                RenderSize = screenRecordingUserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                ContextMenu_AddVideoEvent_Success.Invoke(this, new EventArgs());
            }
        }

        private void ContextMenuAddFormEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            ContextMenu_AddForm_Clicked?.Invoke(sender, e);
        }

        private void ContextMenuRunClickEvent(object sender, RoutedEventArgs e)
        {
            ContextMenu_Run_Clicked?.Invoke(sender, e);
        }

        #region == TimelineUserControl : Load from DB functions ==

        public static DataTable AddTimeLineRow(
            DataTable dt,
            int id,
            int timeline,
            string startTime,
            double dur,
            string eventName,
            int? eventMedia,
            int? eventScreen,
            bool modeEvent)
        {
            DataRow dRow = dt.NewRow();

            dRow[TimelineColumn.VideoEventId] = id;
            dRow[TimelineColumn.VideoEventTrack] = timeline;

            //var startTimeStr = TimeSpan.Parse(startTime);
            //dRow[TimelineColumn.VideoEventStart] = new DateTime(1901, 01, 01, startTimeStr.Hours, startTimeStr.Minutes, startTimeStr.Seconds);

            dRow[TimelineColumn.VideoEventStart] = startTime;
            dRow[TimelineColumn.VideoEventDuration] = dur;
            dRow[TimelineColumn.ModeEvent] = modeEvent;

            if (eventMedia != null)
            {
                dRow[TimelineColumn.EventName] = eventName;
                dRow[TimelineColumn.EventMedia] = eventMedia;
            }

            if (eventScreen != null)
            {
                dRow[TimelineColumn.EventScreen] = eventScreen;
            }


            dt.Rows.Add(dRow);

            return dt;
        }



        private void LoadAppFromDb()
        {
            var apps = DataManagerSqlLite.GetApp();
            if(apps != null && apps.Count > 4)
            {
                apps.RemoveAt(4);
            }
            _timelineGridControl.SetAppControl(apps);
        }

        private void LoadDesignFromDb()
        {
            _timelineGridControl.SetDesigns(DataManagerSqlLite.GetDesign());
        }

        private void LoadMediaFromDb()
        {
            _timelineGridControl.SetMediaList(DataManagerSqlLite.GetMedia());
        }

        private void LoadScreenFromDb()
        {
            _timelineGridControl.SetScreenList(DataManagerSqlLite.GetScreens());
        }

        public void LoadVideoEventsFromDb()
        {

            DataTable dt = _timelineGridControl.BuildTimelineDataTable();


            List<CBVVideoEvent> videoEventList = DataManagerSqlLite.GetVideoEvents(selectedProjectId);
            foreach (var videoEvent in videoEventList)
            {

                //image or video or audio
                if (videoEvent.fk_videoevent_media == (int)MediaType.image
                    || videoEvent.fk_videoevent_media == (int)MediaType.video
                    || videoEvent.fk_videoevent_media == (int)MediaType.audio)
                {

                    CBVMedia media = DataManagerSqlLite.GetMedia().Where(s => s.media_id == videoEvent.fk_videoevent_media).FirstOrDefault();

                    AddTimeLineRow(dt,
                        videoEvent.videoevent_id,
                        videoEvent.videoevent_track,
                        videoEvent.videoevent_start,
                        videoEvent.videoevent_duration,
                        media.media_name,
                        media.media_id,
                        null,
                        false
                        );
                }

                //form
                else if (videoEvent.fk_videoevent_media == (int)MediaType.form)
                {
                    List<CBVDesign> designs = DataManagerSqlLite.GetDesign(videoEvent.videoevent_id);

                    /// Need to search design table to retrieve the screens for a "form" type video event.
                    foreach (var design in designs)
                    {
                        CBVMedia media = DataManagerSqlLite.GetMedia().Where(s => s.media_id == videoEvent.fk_videoevent_media).FirstOrDefault();
                        CBVScreen screen = DataManagerSqlLite.GetScreens().Where(s => s.screen_id == design.fk_design_screen).FirstOrDefault();

                        AddTimeLineRow(dt,
                            videoEvent.videoevent_id,
                            videoEvent.videoevent_track,
                            videoEvent.videoevent_start,
                            videoEvent.videoevent_duration,
                            media.media_name,
                            media.media_id,
                            screen.screen_id,
                            false);
                    }
                }

            }

            _timelineGridControl.SetTimelineDatatable(dt);
        }

        #endregion

        #region == TimelineUserControl : Helper functions ==

        private void InitializeTimeline()
        {

            /// 1. use the interface ITimelineGridControl to access the TimelineUserControl exposed methods and definitions
            _timelineGridControl = TimelineGridCtrl2;

            /// 2. this will set the default menu option status
            //ResetMenuItems(false, null);

            /// 3. load the database for app, design, media, screen and videoEvent in proper order
            LoadAppFromDb();
            LoadDesignFromDb();
            LoadMediaFromDb();
            LoadScreenFromDb();
            LoadVideoEventsFromDb();

            /// use this event handler to check if the timeline data has been changed and to disable some menu options if no data loaded
            TimelineGridCtrl2.TimelineSelectionChanged += (sender, e) =>
            {
                HasData = _timelineGridControl.HasData();
                ResetMenuItems(HasData, _timelineGridControl.GetSelectedEvent());
            };

            /// use this event handler when the trackbar was moved by mouse action
            TimelineGridCtrl2.TrackbarMouseMoved += (sender, e) =>
            {
                var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
                TrackbarTimepicker.Value = trackBarPosition;
            };

        }

        private MenuItem GetMenuItemByResourceName(string resourceKey, string menuItemName)
        {
            var contextMenu = this.Resources[resourceKey] as ContextMenu;
            var contextMenuItem = LogicalTreeHelper.FindLogicalNode(contextMenu, menuItemName) as MenuItem;

            return contextMenuItem;
        }


        private void GetCheckedTimelines(object sender, RoutedEventArgs e)
        {
            var selectedTracks = _timelineGridControl.GetCheckedTimelines();
            MessageBox.Show($"GetCheckedTimelines - Total Checked found - {selectedTracks.Count}", "GetCheckedTimelines", MessageBoxButton.OK, MessageBoxImage.Information);
            //do logic here for the checked timelines...
        }

        
        private void GetTrackbarEvents(object sender, RoutedEventArgs e)
        {
            var trackbarEvents = _timelineGridControl.GetTrackbarEvents();
            MessageBox.Show($"GetTrackbarEvents - Total Trackbar Event found - {trackbarEvents.Count}", "GetTrackbarEvents", MessageBoxButton.OK, MessageBoxImage.Information);

            //do logic here for the trackbarEvents ...
        }

        private void MoveTrackbar(object sender, RoutedEventArgs e)
        {
            try
            {
                var timeInputDate = (DateTime)TrackbarTimepicker.Value;
                _timelineGridControl.MoveTrackbar(timeInputDate);
            }
            catch
            {

            }

        }

        private void ResetMenuItems(bool hasData, TimelineEvent selectedEvent)
        {
            string contextMenuKey = "TimelineMenu";
            if (hasData)
            {
                var MenuItem_ClearAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_ClearAllTimelines");
                MenuItem_ClearAllTimelines.IsEnabled = true;

                var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
                MenuItem_SaveAllTimelines.IsEnabled = true;

                var MenuItem_DeleteEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_DeleteEvent");
                MenuItem_DeleteEvent.IsEnabled = (selectedEvent != null);

                var MenuItem_EditEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_EditEvent");
                MenuItem_EditEvent.IsEnabled = (selectedEvent != null);
            }
            else
            {
                var MenuItem_ClearAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_ClearAllTimelines");
                MenuItem_ClearAllTimelines.IsEnabled = false;

                var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
                MenuItem_SaveAllTimelines.IsEnabled = false;

                var MenuItem_DeleteEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_DeleteEvent");
                MenuItem_DeleteEvent.IsEnabled = false;

                var MenuItem_EditEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_EditEvent");
                MenuItem_EditEvent.IsEnabled = false;
            }
        }


        #endregion

        #region == TimelineUserControl : Context-menu Options ==
        
        
        public void LoadTimelineDataFromDb_Click()
        {
            LoadVideoEventsFromDb();
        }
        
        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            LoadTimelineDataFromDb_Click();
        }

        private void ClearTimelines(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.ClearTimeline();
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            var selectedEvent = _timelineGridControl.GetSelectedEvent();
            if (selectedEvent != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete event with Id - {selectedEvent.Id}", "Delete Event", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    DataManagerSqlLite.DeleteVideoEventsById(selectedEvent.Id, true);
                    _timelineGridControl.DeleteSelectedEvent();
                }
            }
        }

        private void EditSelectedEvent(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.EditSelectedEvent();
        }

        private void SaveTimeline(object sender, RoutedEventArgs e)
        {
            var addedEvents = _timelineGridControl.GetAddedTimelineEvents();

            foreach (var newEvent in addedEvents)
            {
                var videoEventDt = new VideoEventDatatable();
                videoEventDt.AddEventRow(newEvent);

                var addedRow = DataManagerSqlLite.InsertRowsToVideoEvent(videoEventDt, false);

                if (newEvent.GetTimelineMediaType() == MediaType.form)
                {
                    var designDt = new DesignDatatable();
                    designDt.AddDesign(addedRow.First(), (int)newEvent.EventScreen);
                    DataManagerSqlLite.InsertRowsToDesign(designDt);
                }

            }

            var modifiedEvents = _timelineGridControl.GetModifiedTimelineEvents();

            foreach (var modifiedEvent in modifiedEvents)
            {
                var videoEventDt = new VideoEventDatatable();
                videoEventDt.AddEventRow(modifiedEvent);

                DataManagerSqlLite.UpdateRowsToVideoEvent(videoEventDt);
            }
        }

        public void SaveTimeline()
        {
            
        }








        #endregion




    }
}