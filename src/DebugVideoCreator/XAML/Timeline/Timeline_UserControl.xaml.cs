using DebugVideoCreator.Models;
using Newtonsoft.Json;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Timeline.UserControls.Config;
using Timeline.UserControls.Controls;
using Timeline.UserControls.Models;
using VideoCreator.Auth;
using VideoCreator.Helpers;
namespace VideoCreator.XAML
{
    public partial class Timeline_UserControl : UserControl
    {
        public bool HasData { get; set; } = false;
        private SelectedProjectEvent selectedProjectEvent;

        private AuthAPIViewModel authApiViewModel;

        public event EventHandler ContextMenu_AddVideoEvent_Clicked;
        //public event EventHandler ContextMenu_AddVideoEvent_Success;

        public event EventHandler<string> ContextMenu_AddImageEventUsingCBLibrary_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout1_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout2_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddFormEvent_Clicked;

        public event EventHandler<TrackbarMouseMoveEvent> TrackbarMouse_Moved;

        public event EventHandler ContextMenu_Run_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_CloneEvent_Clicked;

        public event EventHandler<TimelineVideoEvent> VideoEventSelectionChanged;
        public event EventHandler<List<TimelineVideoEvent>> ContextMenu_SaveAllTimelines_Clicked;
        public event EventHandler<List<int>> ContextMenu_DeleteTimelines_Clicked;

        ///  Use the interface ITimelineGridControl to view all available TimelineUserControl methods and description.
        ITimelineGridControl _timelineGridControl;
        private bool ReadOnly;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        int i = 0;
        public Timeline_UserControl()
        {
            InitializeComponent();
        }

        public void SetSelectedProjectId(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, bool readonlyMode = false)
        {
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            InitializeTimeline();
            ReadOnly = readonlyMode;
            ResetContextMenu();
        }

        private void ResetContextMenu()
        {
            if (ReadOnly)
            {
                var contextMenu = this.Resources["TimelineMenu"] as ContextMenu;
                for (int i = 0; i < contextMenu.Items.Count; i++)
                {
                    MenuItem item = contextMenu.Items[i] as MenuItem;
                    if (item != null)
                    {
                        if (item?.Name != null && item?.Name == "MenuItem_Run")
                        {
                            item.IsEnabled = true; //.Visibility = Visibility.Hidden
                        }
                        else
                            item.IsEnabled = false;
                    }
                }
            }
        }


        #region == TimelineUserControl : Load from DB functions ==



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
                                                                        screen_name = s.screen_name,
                                                                        screen_hexcolor = s.screen_hexcolor,
                                                                    }).ToList();

            _timelineGridControl.SetScreenList(screens);
        }

        public void LoadVideoEventsFromDb(int projdetId)
        {
            DataTable dt = _timelineGridControl.BuildTimelineDataTable();

            List<CBVVideoEvent> videoEventList = DataManagerSqlLite.GetVideoEvents(projdetId, false, true);
            foreach (var videoEvent in videoEventList)
            {
                DataRow dRow = dt.NewRow();

                dRow[nameof(TimelineVideoEvent.videoevent_id)] = videoEvent.videoevent_id;
                dRow[nameof(TimelineVideoEvent.fk_videoevent_projdet)] = videoEvent.fk_videoevent_projdet;
                dRow[nameof(TimelineVideoEvent.fk_videoevent_media)] = videoEvent.fk_videoevent_media;
                dRow[nameof(TimelineVideoEvent.videoevent_track)] = videoEvent.videoevent_track;
                dRow[nameof(TimelineVideoEvent.videoevent_start)] = videoEvent.videoevent_start;
                dRow[nameof(TimelineVideoEvent.videoevent_duration)] = DataManagerSqlLite.GetMillisecondsFromTimespan(videoEvent.videoevent_duration) / 1000; ;

                dRow[nameof(TimelineVideoEvent.videoevent_createdate)] = videoEvent.videoevent_createdate;
                dRow[nameof(TimelineVideoEvent.videoevent_modifydate)] = videoEvent.videoevent_modifydate;

                dRow[nameof(TimelineVideoEvent.videoevent_isdeleted)] = videoEvent.videoevent_isdeleted;
                dRow[nameof(TimelineVideoEvent.videoevent_issynced)] = videoEvent.videoevent_issynced;
                dRow[nameof(TimelineVideoEvent.videoevent_serverid)] = videoEvent.videoevent_serverid;
                dRow[nameof(TimelineVideoEvent.videoevent_syncerror)] = videoEvent.videoevent_syncerror ?? String.Empty;

                dRow[nameof(TimelineVideoEvent.videoevent_end)] = videoEvent.videoevent_end;

                //added for design screen
                dRow[nameof(TimelineVideoEvent.fk_design_screen)] = videoEvent.fk_design_screen;
                dRow[nameof(TimelineVideoEvent.fk_design_background)] = videoEvent.fk_design_background;

                dt.Rows.Add(dRow);

            }

            _timelineGridControl.SetTimelineDatatable(dt);
        }

        public void ClearTimeline()
        {
            _timelineGridControl.ClearTimeline();
        }

        #endregion

        #region == TimelineUserControl : Helper functions ==


        public void InitializeTimeline()
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

            //int Selected_ID = ((CBVProject)ProjectCmbBox.SelectedItem).project_id;
            //int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
            LoadVideoEventsFromDb(selectedProjectEvent.projdetId);

            /// use this event handler to check if the timeline data has been changed and to disable some menu options if no data loaded
            TimelineGridCtrl2.TimelineSelectionChanged += (sender, e) =>
            {
                HasData = _timelineGridControl.HasData();
                var selectedEvent = _timelineGridControl.GetSelectedEvent();
                ResetMenuItems(HasData, selectedEvent);
                VideoEventSelectionChanged.Invoke(sender, selectedEvent);
            };

            /// use this event handler when the trackbar was moved by mouse action
            TimelineGridCtrl2.TrackbarMouseMoved += (sender, e) =>
            {
                dispatcherTimer.Tick -= TrackBarMouseMovedAndStopped;
                i = 0;
                dispatcherTimer.Tick += new EventHandler(TrackBarMouseMovedAndStopped);
                dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
                dispatcherTimer.Start();

            };

        }

        private void TrackBarMouseMovedAndStopped(object sender, EventArgs ea)
        {
            if (TimelineGridCtrl2._isTrackbarLineDragInProg == false)
            {

                dispatcherTimer.Stop();
                var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
                //TrackbarTimepicker.Value = trackBarPosition;
                //TrackbarTimepicker.Set(trackBarPosition.ToString("HH:mm:ss.fff"));


                var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
                //listView_trackbarEvents.ItemsSource = trackbarEvents;
                //Console.WriteLine($"Mouse.Captured - {Mouse.LeftButton == MouseButtonState.Pressed}, {Mouse.LeftButton == MouseButtonState.Released}");
                var payload = new TrackbarMouseMoveEvent
                {
                    timeAtTheMoment = trackBarPosition.ToString("HH:mm:ss.fff"),
                    videoeventIds = trackbarEvents?.GroupBy(x => x.videoevent_id).Select(x => x.First().videoevent_id).ToList(),
                    isAnyVideo = trackbarEvents?.Find(x => x.fk_videoevent_media == 2) != null,
                };
                TrackbarMouse_Moved.Invoke(sender, payload);
            }
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
                //var timeInputDate = DateTime.ParseExact(TrackbarTimepicker.Get(), _timePickerFormat, null);
                //_timelineGridControl.MoveTrackbar(timeInputDate);
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
                var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
                MenuItem_SaveAllTimelines.IsEnabled = true;

                var MenuItem_DeleteEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_DeleteEvent");
                MenuItem_DeleteEvent.IsEnabled = (selectedEvent != null);

                var MenuItem_CloneItems = GetMenuItemByResourceName(contextMenuKey, "MenuItem_CloneItems");
                MenuItem_CloneItems.IsEnabled = (selectedEvent != null);
            }
            else
            {
                var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
                MenuItem_SaveAllTimelines.IsEnabled = false;

                var MenuItem_DeleteEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_DeleteEvent");
                MenuItem_DeleteEvent.IsEnabled = false;
                
                var MenuItem_CloneItems = GetMenuItemByResourceName(contextMenuKey, "MenuItem_CloneItems");
                MenuItem_CloneItems.IsEnabled = false;
            }
        }


        //private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        //{
        //    if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
        //    {
        //        var data = DataManagerSqlLite.GetDownloadedProjectList();

        //        // Remove the duplicate on projects based on project_id property
        //        List<CBVProjectForJoin> uniqueProjects = data
        //                                         .GroupBy(p => p.project_id)
        //                                         .Select(group => group.First())
        //                                         .ToList();

        //        RefreshComboBoxes<CBVProjectForJoin>(ProjectCmbBox, uniqueProjects, nameof(CBVProjectForJoin.project_videotitle));
        //        if (ProjectCmbBox.Items.Count > 0)
        //        {
        //            ProjectCmbBox.SelectedIndex = 0;
        //        }
        //    }
        //}

        //private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        //{
        //    combo.SelectedItem = null;
        //    combo.DisplayMemberPath = columnNameToShow;
        //    combo.Items.Clear();
        //    foreach (var item in source)
        //    {
        //        combo.Items.Add(item);
        //    }
        //}

        //private void ProjectCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{

        //    if (ProjectCmbBox.SelectedItem != null)
        //    {
        //        int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;

        //        if (ProjectCmbBox.SelectedIndex != -1)
        //        {

        //            if (_timelineGridControl != null)
        //                LoadVideoEventsFromDb(Selected_ID);
        //        }
        //    }
        //}

        #endregion


        #region == TimelineUserControl : Context-menu Options ==



        private void AddAudioEvent_Click(object sender, RoutedEventArgs e)
        {
            //_timelineGridControl.AddNewEventToTimeline(MediaType.audio);
        }

        private FormOrCloneEvent CalloutPreprocessing(bool IsCallout = true)
        {
            TimelineVideoEvent selectedEvent;
            var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            var trackbarTime = trackBarPosition.ToString("HH:mm:ss.fff");
            var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
            if (trackbarEvents != null && trackbarEvents?.Count > 0)
                selectedEvent = trackbarEvents[0];
            else
                selectedEvent = _timelineGridControl.GetSelectedEvent();

            if ((trackbarTime == "00:00:00" || trackbarTime == "00:00:00.000") && selectedEvent == null)
            {
                var emptyPayload = new FormOrCloneEvent
                {
                    timelineVideoEvent = selectedEvent,
                    timeAtTheMoment = trackbarTime
                };
                return emptyPayload;
            }
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent,
                timeAtTheMoment = IsCallout ? trackbarTime : DataManagerSqlLite.GetNextStart((int)EnumMedia.FORM, selectedProjectEvent.projdetId)
            };
            return payload;
        }

        private void AddCallout1_Click(object sender, RoutedEventArgs e)
        {
            //int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;

            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 3, Selected_ID);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Callout1";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(Selected_ID);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            var payload = CalloutPreprocessing();
            ContextMenu_AddCallout1_Clicked.Invoke(sender, payload);
        }

        private void AddCallout2_Click(object sender, RoutedEventArgs e)
        {
            //int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;

            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 4, Selected_ID);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Callout2";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(Selected_ID);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            var payload = CalloutPreprocessing();
            ContextMenu_AddCallout2_Clicked.Invoke(sender, payload);
        }

        private void AddFormEvent_Click(object sender, RoutedEventArgs e)
        {
            var payload = CalloutPreprocessing(false);
            ContextMenu_AddFormEvent_Clicked.Invoke(sender, payload);
        }




        private void AddImageEventUsingCBLibrary_Click(object sender, RoutedEventArgs e)
        {
            var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            var trackbarTime = trackBarPosition.ToString("HH:mm:ss.fff");
            ContextMenu_AddImageEventUsingCBLibrary_Clicked.Invoke(sender, trackbarTime);
        }

        private void AddVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            //int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 2, Selected_ID);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Video Event";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(Selected_ID);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            ContextMenu_AddVideoEvent_Clicked.Invoke(sender, e);
        }

        private void RunEvent_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu_Run_Clicked.Invoke(sender, e);
        }

        private void CloneEvent_Click(object sender, RoutedEventArgs e)
        {
            TimelineVideoEvent selectedEvent = _timelineGridControl.GetSelectedEvent();

            if (selectedEvent == null)
            {
                MessageBox.Show("No event selected to clone, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
            if (trackbarEvents != null && trackbarEvents?.Count > 0)
            {
                MessageBox.Show("The trackbar position indicates there is another event(s) present already, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            var trackbarTime = trackBarPosition.ToString("HH:mm:ss.fff");
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent,
                timeAtTheMoment = trackbarTime
            };
            ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);

        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            //show a warning here that user will lose all changes if the changes are not saved
            //int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
            LoadVideoEventsFromDb(selectedProjectEvent.projdetId);
        }

        private void ClearTimelines(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.ClearTimeline();
            //listView_trackbarEvents.ItemsSource = null;
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.DeleteSelectedEvent();
        }

        private void GetSelectedEvent(object sender, RoutedEventArgs e)
        {
            //var selectedEvent = _timelineGridControl.GetSelectedEvent();

            //if (selectedEvent == null)
            //    lblSelectedEvent.Content = "No selected event";

            //else
            //    lblSelectedEvent.Content = $"Id: {selectedEvent.videoevent_id} , Track: {selectedEvent.videoevent_track} , Start: {selectedEvent.StartTimeStr} , Dur: {selectedEvent.videoevent_duration}";
        }

        private void SaveTimeline(object sender, RoutedEventArgs e)
        {
            // Add new events to the database
            var addedEvents = _timelineGridControl.GetAddedTimelineEvents();

            //foreach (var newEvent in addedEvents)
            //{
            //    var videoEventDt = new VideoEventDatatable();
            //    videoEventDt.AddVideoEventRow(newEvent);

            //    var addedRow = DataManagerSqlLite.InsertRowsToVideoEvent(videoEventDt, false);

            //    if (newEvent.GetTimelineMediaType() == MediaType.form)
            //    {
            //        var designDt = new DesignDatatable();
            //        designDt.AddDesign(addedRow.First(), (int)newEvent.EventScreen);
            //        DataManagerSqlLite.InsertRowsToDesign(designDt);
            //    }

            //}

            // Update changes to the database for existing events
            var modifiedEvents = _timelineGridControl.GetModifiedTimelineEvents();
            ContextMenu_SaveAllTimelines_Clicked.Invoke(sender, modifiedEvents);
            //foreach (var modifiedEvent in modifiedEvents)
            //{
            //    var videoEventDt = new VideoEventDatatable();
            //    videoEventDt.AddVideoEventRow(modifiedEvent);

            //    DataManagerSqlLite.UpdateRowsToVideoEvent(videoEventDt);
            //}

            // Removes the deleted events from the database
            var deletedEventIds = _timelineGridControl.GetDeletedTimelineEventsId();
            ContextMenu_DeleteTimelines_Clicked?.Invoke(sender, deletedEventIds);

            //foreach (var id in deletedEventIds)
            //{
            //    // Please adjust cascadeDelete as required
            //    DataManagerSqlLite.DeleteVideoEventsById(id, cascadeDelete: true);
            //}

            ////_timelineGridControl.ClearTimeline();
            //LoadTimelineDataFromDb_Click(null, null);
            //MessageBox.Show("Save Successful!");
        }

        #endregion


        #region == AutoFill Events ==

        public event EventHandler<AutofillEvent> Autofill_Clicked;
        /*
        private void AddTitleAutofill_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = TimelineGridCtrl2.TrackbarPosition.ToString("HH:mm:ss.fff");
            var payload = new AutofillEvent
            {
                AutofillType = AutofillEnumType.Title,
                timeAtTheMoment = trackbarTime,
                Duration = 10
            };
            Autofill_Clicked.Invoke(sender, payload);
        }

        private void AddRequirementAutofill_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = TimelineGridCtrl2.TrackbarPosition.ToString("HH:mm:ss.fff");
            var payload = new AutofillEvent
            {
                AutofillType = AutofillEnumType.Requirement,
                timeAtTheMoment = trackbarTime,
                Duration = 10
            };
            Autofill_Clicked.Invoke(sender, payload);
        }

        private void AddObjectiveAutofill_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = TimelineGridCtrl2.TrackbarPosition.ToString("HH:mm:ss.fff");
            var payload = new AutofillEvent
            {
                AutofillType = AutofillEnumType.Objective,
                timeAtTheMoment = trackbarTime,
                Duration = 10
            };
            Autofill_Clicked.Invoke(sender, payload);
        }

        private void AddNextAutofill_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = TimelineGridCtrl2.TrackbarPosition.ToString("HH:mm:ss.fff");
            var payload = new AutofillEvent
            {
                AutofillType = AutofillEnumType.Next,
                timeAtTheMoment = trackbarTime,
                Duration = 10
            };
            Autofill_Clicked.Invoke(sender, payload);
        }

        */
        private void AddAllAutofill_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = TimelineGridCtrl2.TrackbarPosition.ToString("HH:mm:ss.fff");
            var payload = new AutofillEvent
            {
                AutofillType = AutofillEnumType.All,
                timeAtTheMoment = trackbarTime,
                DurationInSec = 10,
                Duration = "00:00:10.000"
            };
            Autofill_Clicked.Invoke(sender, payload);
        }
        #endregion

    }
}