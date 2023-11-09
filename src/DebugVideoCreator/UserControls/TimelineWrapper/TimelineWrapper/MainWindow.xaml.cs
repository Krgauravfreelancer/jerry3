using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Timeline.UserControls.Config;
using Timeline.UserControls.Controls;
using Timeline.UserControls.Models;
using TimelineWrapper.Windows;

namespace TimelineWrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool IsSetUp = false;

        public bool HasData { get; set; } = false;
        ITimelineGridControl _timelineGridControl;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetUp)
            {
                InitializeDatabase();
                IsSetUp = true;

                InitializeTimeline();
            }
        }

        #region == Helper Functions ==

        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                MessageBox.Show(message + ", syncing lookup tables !!");
                SyncApp();
                SyncMedia();
                SyncScreen();
                SyncResolution();
                MessageBox.Show("lookup tables synced successfully !!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        #endregion
        #region == Sync Functions ==

        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(int));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = 1;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = 0;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = 0;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = 0;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = 0;
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void SyncMedia()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("media_id", typeof(int));
                datatable.Columns.Add("media_name", typeof(string));
                datatable.Columns.Add("media_color", typeof(string));

                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = "image";
                row["media_color"] = "Tomato";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["media_id"] = -1;
                row2["media_name"] = "video";
                row2["media_color"] = "Thistle";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["media_id"] = -1;
                row3["media_name"] = "audio";
                row3["media_color"] = "Yellow";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["media_id"] = -1;
                row4["media_name"] = "form";
                row4["media_color"] = "LightSalmon";
                datatable.Rows.Add(row4);

                var insertedIds = DataManagerSqlLite.SyncMedia(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SyncScreen()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("screen_id", typeof(int));
                datatable.Columns.Add("screen_name", typeof(string));
                datatable.Columns.Add("screen_color", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "LightSalmon";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["screen_id"] = -1;
                row2["screen_name"] = "prerequisites";
                row2["screen_color"] = "Azure";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["screen_id"] = -1;
                row3["screen_name"] = "screen cast";
                row3["screen_color"] = "Beige";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["screen_id"] = -1;
                row4["screen_name"] = "conclusion";
                row4["screen_color"] = "Aqua";
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["screen_id"] = -1;
                row5["screen_name"] = "next";
                row5["screen_color"] = "LightSteelBlue";
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SyncResolution()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("resolution_id", typeof(int));
                datatable.Columns.Add("resolution_name", typeof(string));

                var row = datatable.NewRow();
                row["resolution_id"] = -1;
                row["resolution_name"] = "480px";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["resolution_id"] = -1;
                row2["resolution_name"] = "720px";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["resolution_id"] = -1;
                row3["resolution_name"] = "1080px";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["resolution_id"] = -1;
                row4["resolution_name"] = "1280px";
                datatable.Rows.Add(row4);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region == Populate functions ==
        private void PopulateProjectTable()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("id", typeof(int));
                dataTable.Columns.Add("project_name", typeof(string));
                dataTable.Columns.Add("project_version", typeof(int));
                dataTable.Columns.Add("project_comments", typeof(string));
                dataTable.Columns.Add("project_createdate", typeof(string));
                dataTable.Columns.Add("project_modifydate", typeof(string));

                for (var i = 1; i <= 5; i++)
                {
                    var row = dataTable.NewRow();
                    row["id"] = 1;
                    row["project_name"] = $"Sample Project - {i}";
                    row["project_version"] = i;
                    row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show("Projects populated to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        #endregion


        #region == TimelineUserControl : Load from DB functions ==

        public DataTable AddTimelineRow(
            DataTable dt,
            int videoevent_id,
            int fk_videoevent_project,
            int fk_videoevent_media,
            int videoevent_track,
            string videoevent_start,
            int videoevent_duration,
            string videoevent_end,
            DateTime videoevent_createdate,
            DateTime videoevent_modifydate
            )
        {
            DataRow dRow = dt.NewRow();

            dRow[nameof(TimelineVideoEvent.videoevent_id)] = videoevent_id;
            dRow[nameof(TimelineVideoEvent.fk_videoevent_project)] = fk_videoevent_project;
            dRow[nameof(TimelineVideoEvent.fk_videoevent_media)] = fk_videoevent_media;
            dRow[nameof(TimelineVideoEvent.videoevent_track)] = videoevent_track;
            dRow[nameof(TimelineVideoEvent.videoevent_start)] = videoevent_start;
            dRow[nameof(TimelineVideoEvent.videoevent_duration)] = videoevent_duration;
            dRow[nameof(TimelineVideoEvent.videoevent_end)] = videoevent_end;
            dRow[nameof(TimelineVideoEvent.videoevent_createdate)] = videoevent_createdate;
            dRow[nameof(TimelineVideoEvent.videoevent_modifydate)] = videoevent_modifydate;

            dt.Rows.Add(dRow);

            return dt;
        }

        private void LoadAppFromDb()
        {
            var apps = DataManagerSqlLite.GetApp().Select(a => new TimelineApp
            {
                app_id = a.app_id,
                app_name = a.app_name,
                app_active = a.app_active,
            }).ToList();

            _timelineGridControl.SetAppControl(apps);
        }

        private void LoadDesignFromDb()
        {
            var designs = DataManagerSqlLite.GetDesign().Select(d => new TimelineDesign
            {
                design_createdate = d.design_createdate,
                design_id = d.design_id,
                design_modifydate = d.design_modifydate,
                //fk_design_background = d.fk_design_background,
                fk_design_screen = d.fk_design_screen,
                fk_design_videoevent = d.fk_design_videoevent
            }).ToList();

            _timelineGridControl.SetDesigns(designs);
        }

        private void LoadMediaFromDb()
        {
            var mediaList = DataManagerSqlLite.GetMedia().Select(m => new TimelineMedia
            {
                media_id = m.media_id,
                media_name = m.media_name,
                media_color = m.media_color,
            }).ToList();

            _timelineGridControl.SetMediaList(mediaList);
        }

        private void LoadScreenFromDb()
        {
            var screens = DataManagerSqlLite.GetScreens().Select(s =>
                                                                    new TimelineScreen
                                                                    {
                                                                        screen_color = s.screen_color,
                                                                        screen_id = s.screen_id,
                                                                        screen_name = s.screen_name
                                                                    }).ToList();

            _timelineGridControl.SetScreenList(screens);
        }
       
        public void LoadVideoEventsFromDb()
        {

            DataTable dt = _timelineGridControl.BuildTimelineDataTable();


            List<CBVVideoEvent> videoEventList = DataManagerSqlLite.GetVideoEvents();
            foreach (var videoEvent in videoEventList)
            {

                AddTimelineRow(
                    dt,
                    videoEvent.videoevent_id,
                    videoEvent.fk_videoevent_project,
                    videoEvent.fk_videoevent_media,
                    videoEvent.videoevent_track,
                    videoEvent.videoevent_start,
                    videoEvent.videoevent_duration,
                    videoEvent.videoevent_end,
                    videoEvent.videoevent_createdate,
                    videoEvent.videoevent_modifydate);

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
            ResetMenuItems(false, null);

            /// 3. load the database for app, design, media, screen and videoEvent in proper order
            LoadAppFromDb();
            LoadDesignFromDb();
            LoadMediaFromDb();
            LoadScreenFromDb();

            //LoadVideoEventsFromDb();

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

                
                var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
                listView_trackbarEvents.ItemsSource = trackbarEvents;
            };

            

        }

        private void Button_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // This event handler will be called when the context menu is opening
            HasData = _timelineGridControl.HasData();
            ResetMenuItems(HasData, _timelineGridControl.GetSelectedEvent());
        }


      

        private MenuItem GetMenuItemByResourceName(string resourceKey, string menuItemName)
        {
            var contextMenu = this.Resources[resourceKey] as ContextMenu;
            var contextMenuItem = LogicalTreeHelper.FindLogicalNode(contextMenu, menuItemName) as MenuItem;

            return contextMenuItem;
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

        private void ResetMenuItems(bool hasData, TimelineVideoEvent selectedEvent)
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


        private void AddAudioEvent_Click(object sender, RoutedEventArgs e)
        {
            //_timelineGridControl.AddNewEventToTimeline(MediaType.audio);
        }

        private void AddVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            ScreenRecorderWindow screenRecorderWindow = new ScreenRecorderWindow(this);
            screenRecorderWindow.Owner = this;

            /// We need to set the eventhandler for the BtnSaveClickedEvent from the ScreenRecorderWindow to handle the datatable 
            screenRecorderWindow.BtnSaveClickedEvent += dataTable =>
            {

                /// Here we will load the dataTable to Timeline when it's returned
                _timelineGridControl.SetTimelineDatatable(dataTable);
            };

            screenRecorderWindow.Show();
            Hide(); //hide the main window so it's not showing when ScreenRecorder is opened.

        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            //show a warning here that user will lose all changes if the changes are not saved
            LoadVideoEventsFromDb();
        }

        private void ClearTimelines(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.ClearTimeline();
            listView_trackbarEvents.ItemsSource = null;
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.DeleteSelectedEvent();
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
                videoEventDt.AddVideoEventRow(newEvent);

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
                videoEventDt.AddVideoEventRow(modifiedEvent);

                DataManagerSqlLite.UpdateRowsToVideoEvent(videoEventDt);
            }

            _timelineGridControl.ClearTimeline();
            MessageBox.Show("Save Successful!");
        }

      

        #endregion
    }
}
