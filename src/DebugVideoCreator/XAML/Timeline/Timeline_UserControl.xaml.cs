using Sqllite_Library.Business;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Timeline.UserControls.Controls;
using Timeline.UserControls.Models;
using Timeline.UserControls.Models.Datatables;
using VideoCreator.Auth;
using VideoCreator.Helpers;
using VideoCreator.Models;

namespace VideoCreator.XAML
{
    public partial class Timeline_UserControl : UserControl
    {
        public bool HasData { get; set; } = false;
        private SelectedProjectEvent selectedProjectEvent;

        private AuthAPIViewModel authApiViewModel;

        public event EventHandler ContextMenu_AddVideoEvent_Clicked;

        public event EventHandler<string> ContextMenu_AddImageEventUsingCBLibrary_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddImageEventUsingCBLibraryInMiddle_Clicked;
        public event EventHandler ContextMenu_ManageMedia_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout1_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout2_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddFormEvent_Clicked;


        public event EventHandler<TrackbarMouseMoveEvent> TrackbarMouse_Moved;

        public event EventHandler ContextMenu_Run_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_CloneEvent_Clicked;

        public event EventHandler<TimelineSelectedEvent> VideoEventSelectionChanged;
        public event EventHandler<List<TimelineVideoEvent>> ContextMenu_SaveAllTimelines_Clicked;
        public event EventHandler<int> ContextMenu_DeleteEventOnTimelines_Clicked;
        public event EventHandler ContextMenu_UndeleteDeletedEvent_Clicked;

        ///  Use the interface ITimelineGridControl to view all available TimelineUserControl methods and description.
        ITimelineGridControl _timelineGridControl;
        private bool ReadOnly;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        //int i;
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


        public void Refresh()
        {
            //InitializeTimeline();
            LoadVideoEventsFromDb(selectedProjectEvent.projdetId);
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
            var screens = DataManagerSqlLite
                .GetScreens()
                .Select(s =>
                            new TimelineScreen
                            {
                                screen_color = s.screen_color,
                                screen_id = s.screen_id,
                                screen_name = s.screen_name,
                                screen_hexcolor = s.screen_hexcolor,
                            }).ToList();

            _timelineGridControl.SetScreenList(screens);
        }

        public void LoadVideoEventsFromDb(int projDetId)
        {
            List<CBVVideoEvent> cbvVideoEvents = DataManagerSqlLite.GetVideoEvents(projDetId, dependentDataFlag: false, designFlag: true);

            // Create list below to store values from database to Timeline
            List<TimelineNote> noteList = new List<TimelineNote>();
            List<TimelineVideoEvent> videoEventList = new List<TimelineVideoEvent>();

            foreach (var videoEvent in cbvVideoEvents)
            {
                videoEventList.Add(new TimelineVideoEvent
                {
                    videoevent_id = videoEvent.videoevent_id,
                    fk_videoevent_projdet = videoEvent.fk_videoevent_projdet,
                    fk_videoevent_media = videoEvent.fk_videoevent_media,
                    videoevent_track = videoEvent.videoevent_track,
                    videoevent_start = videoEvent.videoevent_start,
                    videoevent_duration = videoEvent.videoevent_duration,
                    videoevent_origduration = videoEvent.videoevent_origduration,
                    videoevent_createdate = videoEvent.videoevent_createdate,
                    videoevent_modifydate = videoEvent.videoevent_modifydate,
                    videoevent_isdeleted = videoEvent.videoevent_isdeleted,
                    videoevent_issynced = videoEvent.videoevent_issynced,
                    videoevent_serverid = videoEvent.videoevent_serverid,
                    videoevent_syncerror = videoEvent.videoevent_syncerror,
                    videoevent_end = videoEvent.videoevent_end,
                    fk_design_screen = videoEvent.fk_design_screen,
                    fk_design_background = videoEvent.fk_design_background,
                });


                // Retrieve the corresponding notes from cbv_notes table for the videoevent
                noteList.AddRange(LoadNotesFromDb(videoEvent.videoevent_id, videoEvent.videoevent_start));
            }

            _timelineGridControl.SetVideoEvents(videoEventList);
            _timelineGridControl.SetNotes(noteList);

        }

        public List<TimelineNote> LoadNotesFromDb(int videoEventId, string starttime)
        {
            List<TimelineNote> timelineNotes = new List<TimelineNote>();
            List<CBVNotes> notes = DataManagerSqlLite.GetNotes(videoEventId);

            foreach (var note in notes)
            {
                timelineNotes.Add(new TimelineNote
                {
                    notes_id = note.notes_id,
                    fk_notes_videoevent = note.fk_notes_videoevent,
                    notes_line = note.notes_line,
                    notes_wordcount = note.notes_wordcount,
                    notes_index = note.notes_index,
                    notes_start = (TimeSpan.Parse(note.notes_start) + TimeSpan.Parse(starttime)).ToString(@"hh\:mm\:ss\.fff"),
                    notes_duration = note.notes_duration,
                    notes_createdate = note.notes_createdate,
                    notes_modifydate = note.notes_modifydate,
                    notes_isdeleted = note.notes_isdeleted,
                    notes_issynced = note.notes_issynced,
                    notes_serverid = note.notes_serverid,
                    notes_syncerror = note.notes_syncerror,
                });
            }

            return timelineNotes;
        }

        public void ClearTimeline()
        {
            _timelineGridControl.ClearTimeline();
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


            /// use this event handler to check if the timeline data has been changed and to disable some menu options if no data loaded
            TimelineGridCtrl2.TimelineSelectionChanged -= TimelineSelectionChanged;
            TimelineGridCtrl2.TimelineSelectionChanged += TimelineSelectionChanged;
            //TimelineGridCtrl2.TimelineSelectionChanged += (sender, e) =>
            //{
            //    HasData = _timelineGridControl.HasData();
            //    ResetMenuItems(HasData, _timelineGridControl.GetSelectedEvent());
            //};

            /// use this event handler when the trackbar was moved by mouse action
            TimelineGridCtrl2.TrackbarMouseMoved -= TrackbarMouseMoved;
            TimelineGridCtrl2.TrackbarMouseMoved += TrackbarMouseMoved;
            //TimelineGridCtrl2.TrackbarMouseMoved += (sender, e) =>
            //{
            //    var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            //    TrackbarTimepicker.Set(trackBarPosition.ToString());
            //    var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
            //    listView_trackbarEvents.ItemsSource = trackbarEvents;
            //};

            /// Use this event handler when a video event is deleted
            TimelineGridCtrl2.TimelineVideoEventDeleted -= TimelineVideoEventDeleted;
            TimelineGridCtrl2.TimelineVideoEventDeleted += TimelineVideoEventDeleted;
            //TimelineGridCtrl2.TimelineVideoEventDeleted += (sender, e) =>
            //{
            //    if (e.TimelineEvent.TrackNumber == TrackNumber.Notes)
            //    {
            //        DataManagerSqlLite.DeleteNotesById(e.TimelineEvent.Note.notes_id);
            //    }
            //    else
            //    {
            //        DataManagerSqlLite.DeleteVideoEventsById(e.TimelineEvent.VideoEvent.videoevent_id, cascadeDelete: true);
            //    }

            //};
        }

        private void TimelineSelectionChanged(object sender, EventArgs e)
        {
            HasData = _timelineGridControl.HasData();
            var selectedEvent = _timelineGridControl.GetSelectedEvent();
            ResetMenuItems(HasData, selectedEvent);
            if (selectedEvent != null)
            {
                var payload = new TimelineSelectedEvent
                {
                    Track = selectedEvent.TrackNumber,
                    EventId = selectedEvent.TrackNumber != TrackNumber.Notes ? selectedEvent.VideoEvent.videoevent_id : selectedEvent.Note.notes_id
                };
                VideoEventSelectionChanged.Invoke(sender, payload);
            }
        }

        private void TrackbarMouseMoved(object sender, EventArgs e)
        {
            dispatcherTimer.Tick -= TrackBarMouseMovedAndStopped;
            //i = 0;
            dispatcherTimer.Tick += new EventHandler(TrackBarMouseMovedAndStopped);
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(50);
            dispatcherTimer.Start();
        }

        private void TimelineVideoEventDeleted(object sender, TimelineEventArgs e)
        {
            //TBD
            //if (e.TimelineEvent.TrackNumber == TrackNumber.Notes)
            //{
            //    DataManagerSqlLite.DeleteNotesById(e.TimelineEvent.Note.notes_id);
            //}
            //else
            //{
            //    DataManagerSqlLite.DeleteVideoEventsById(e.TimelineEvent.VideoEvent.videoevent_id, cascadeDelete: true);
            //}
            ContextMenu_DeleteEventOnTimelines_Clicked.Invoke(sender, e.TimelineEvent.VideoEvent.videoevent_id);
            //DataManagerSqlLite.DeleteVideoEventsById(e.TimelineVideoEvent.videoevent_id, cascadeDelete: true);
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
                //var timeSpanInput = TimeSpan.Parse(TrackbarTimepicker.Get());
                //_timelineGridControl.MoveTrackbarToTimeSpan(timeSpanInput, true);
            }
            catch
            {
                // ignore errors
            }
        }

        private void ResetMenuItems(bool hasData, TimelineEvent selectedEvent)
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

                var MenuItem_CloneItemsAtEnd = GetMenuItemByResourceName(contextMenuKey, "MenuItem_CloneItemsAtEnd");
                MenuItem_CloneItemsAtEnd.IsEnabled = (selectedEvent != null);

                var MenuItem_AddImageEventUsingCBLibraryInMiddle = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddImageEventUsingCBLibraryInMiddle");
                MenuItem_AddImageEventUsingCBLibraryInMiddle.IsEnabled = (selectedEvent != null);
            }
            else
            {
                var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
                MenuItem_SaveAllTimelines.IsEnabled = false;

                var MenuItem_DeleteEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_DeleteEvent");
                MenuItem_DeleteEvent.IsEnabled = false;

                var MenuItem_CloneItems = GetMenuItemByResourceName(contextMenuKey, "MenuItem_CloneItems");
                MenuItem_CloneItems.IsEnabled = false;

                var MenuItem_CloneItemsAtEnd = GetMenuItemByResourceName(contextMenuKey, "MenuItem_CloneItemsAtEnd");
                MenuItem_CloneItemsAtEnd.IsEnabled = false;

                var MenuItem_AddImageEventUsingCBLibraryInMiddle = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddImageEventUsingCBLibraryInMiddle");
                MenuItem_AddImageEventUsingCBLibraryInMiddle.IsEnabled = false;
            }
        }

        //private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        //{
        //    if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
        //    {
        //        var projectList = DataManagerSqlLite.GetDownloadedProjectList();

        //        RefreshComboBoxes<CBVProjectForJoin>(ProjectCmbBox, projectList, nameof(CBVProjectForJoin.project_videotitle));

        //        if (ProjectCmbBox.Items.Count > 0)
        //        {
        //            ProjectCmbBox.SelectedIndex = 0;
        //        }
        //    }
        //}

        //private void RefreshComboBoxes<T>(ComboBox combo, List<T> source, string columnNameToShow)
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
        //        int selectedProjectId = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;

        //        if (ProjectCmbBox.SelectedIndex != -1)
        //        {
        //            // Update the project details whenever the selected project changes
        //            var cbvProject = DataManagerSqlLite.GetProjectById(projectId: selectedProjectId, projdetFlag: true);
        //            RefreshComboBoxes<CBVProjdet>(ProjDetCmbBox, cbvProject.projdet_data, nameof(CBVProjdet.projdet_version));

        //            if (ProjDetCmbBox.Items.Count > 0)
        //            {
        //                ProjDetCmbBox.SelectedIndex = 0;

        //                // Trigger the SelectionChanged event
        //                ProjDetComboBox_SelectionChanged(ProjDetCmbBox, null);
        //            }

        //        }


        //    }
        //}

        //private void RefreshProjDetComboBox()
        //{

        //}

        //private void ProjDetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (ProjDetCmbBox.SelectedItem != null)
        //    {
        //        int projDetId = ((CBVProjdet)ProjDetCmbBox.SelectedItem).projdet_id;

        //        if (ProjectCmbBox.SelectedIndex != -1)
        //        {
        //            if (_timelineGridControl != null)
        //                LoadVideoEventsFromDb(projDetId);
        //        }
        //    }
        //}

        #endregion

        #region == TimelineUserControl : Context-menu Options ==

        private void AddAudioEvent_Click(object sender, RoutedEventArgs e)
        {
            // This method is not yet implemented
            //_timelineGridControl.AddNewEventToTimeline(MediaType.audio);
            throw new NotImplementedException();
        }

        private void AddCallout1_Click(object sender, RoutedEventArgs e)
        {
            //int selectedProjectId = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
            //int projDetId = ((CBVProjdet)ProjDetCmbBox.SelectedItem).projdet_id;

            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 3, projDetId);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Callout1";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(projDetId);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            var payload = CalloutPreprocessing();
            ContextMenu_AddCallout1_Clicked.Invoke(sender, payload);
        }

        private void AddCallout2_Click(object sender, RoutedEventArgs e)
        {
            //int projDetId = ((CBVProjdet)ProjDetCmbBox.SelectedItem).projdet_id;

            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 4, projDetId);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Callout2";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(projDetId);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            var payload = CalloutPreprocessing();
            ContextMenu_AddCallout2_Clicked.Invoke(sender, payload);
        }

        private void AddVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            //int projDetId = ((CBVProjdet)ProjDetCmbBox.SelectedItem).projdet_id;

            //ScreenRecorderWindow2 screenRecorderWindow = new ScreenRecorderWindow2(this, trackId: 2, projDetId);
            //screenRecorderWindow.Owner = this;
            //screenRecorderWindow.Title = "Add Video Event";

            //screenRecorderWindow.BtnSaveClickedEvent += () =>
            //{
            //    LoadVideoEventsFromDb(projDetId);
            //};

            //screenRecorderWindow.Show();
            //Hide();
            ContextMenu_AddVideoEvent_Clicked.Invoke(sender, e);
        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            //int selectedProjectId = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
            LoadVideoEventsFromDb(selectedProjectEvent.projdetId);
        }

        private void ClearTimelines(object sender, RoutedEventArgs e)
        {
            _timelineGridControl.ClearTimeline();
            //listView_trackbarEvents.ItemsSource = null;
        }

        private void UndeleteDeletedEvent(object sender, RoutedEventArgs e)
        {
            ContextMenu_UndeleteDeletedEvent_Clicked.Invoke(sender, e);
            DisableUndoDeleteAndReset();
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

            //else if (selectedEvent.TrackNumber == TrackNumber.Notes)
            //    lblSelectedEvent.Content = $"Id: {selectedEvent.Note.notes_id} , Start: {selectedEvent.StartTimeStr} , Dur: {selectedEvent.EventDuration_Double} s";
            //else
            //    lblSelectedEvent.Content = $"Id: {selectedEvent.VideoEvent.videoevent_id} , Track: {selectedEvent.TrackNumber} , Start: {selectedEvent.StartTimeStr} , Dur: {selectedEvent.EventDuration_Double} s";
        }

        private void SaveTimeline(object sender, RoutedEventArgs e)
        {
            // Update changes to the database for existing events
            //var modifiedEvents = _timelineGridControl.GetModifiedTimelineEvents();
            //foreach (var modifiedEvent in modifiedEvents)
            //{
            //    // save modified notes to database
            //    //if (modifiedEvent.TrackNumber == TrackNumber.Notes)
            //    //{
            //    //    var modifiedNotesDt = new NotesDatatable();

            //    //    var timelineNote = new TimelineNote
            //    //    {
            //    //        notes_id = modifiedEvent.Note.notes_id,
            //    //        fk_notes_videoevent = modifiedEvent.Note.fk_notes_videoevent,
            //    //        notes_line = modifiedEvent.Note.notes_line,
            //    //        notes_wordcount = modifiedEvent.Note.notes_wordcount,
            //    //        notes_index = modifiedEvent.Note.notes_index,
            //    //        notes_start = modifiedEvent.StartTimeStr,
            //    //        notes_duration = modifiedEvent.EventDuration,
            //    //        notes_createdate = modifiedEvent.Note.notes_createdate,
            //    //        notes_modifydate = modifiedEvent.Note.notes_modifydate,
            //    //        notes_isdeleted = modifiedEvent.Note.notes_isdeleted,
            //    //        notes_issynced = modifiedEvent.Note.notes_issynced,
            //    //        notes_serverid = modifiedEvent.Note.notes_serverid,
            //    //        notes_syncerror = modifiedEvent.Note.notes_syncerror,
            //    //    };

            //    //    modifiedNotesDt.AddNoteRow(timelineNote);
            //    //    DataManagerSqlLite.UpdateRowsToNotes(modifiedNotesDt);
            //    //}

            //    //// save modified videoevents to database
            //    //if (modifiedEvent.TrackNumber != TrackNumber.Notes)
            //    //{
            //    //    modifiedEvent.VideoEvent.videoevent_end = DataManagerSqlLite.CalcNextEnd(modifiedEvent.EventStart, modifiedEvent.EventDuration);

            //    //    var videoEventDt = new VideoEventDatatable();
            //    //    videoEventDt.AddVideoEventRow(modifiedEvent);

            //    //    DataManagerSqlLite.UpdateRowsToVideoEvent(videoEventDt);
            //    //}
            //}

            var modifiedEvents = _timelineGridControl.GetModifiedTimelineEvents();
            //ContextMenu_SaveAllTimelines_Clicked.Invoke(sender, modifiedEvents);

            _timelineGridControl.ClearTimeline();

            //LoadTimelineDataFromDb_Click(null, null);
            //MessageBox.Show("Save Successful!");
        }

        #endregion


        #region == Planning Events ==
        private async void AddPlanningEvents_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId);
            var payload = new PlanningEvent
            {
                Type = EnumPlanningHead.All,
                TimeAtTheMoment = trackbarTime
            };

            var backgroundImagePath = PlanningHandlerHelper.CheckIfBackgroundPresent();
            if (backgroundImagePath == null)
                MessageBox.Show($"No Background found, plannings cannot be added.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                await PlanningHandlerHelper.Process(payload, selectedProjectEvent, authApiViewModel, this, loader, backgroundImagePath);
                InitializeTimeline();
            }
            LoaderHelper.HideLoader(this, loader);

        }
        #endregion

        #region == delete/undelete and shift ==

        public void EnableUndoDelete(int videoeventLocalId)
        {
            string contextMenuKey = "TimelineMenu";
            var MenuItem_UndoDelete = GetMenuItemByResourceName(contextMenuKey, "MenuItem_UndoDelete");
            MenuItem_UndoDelete.Header = $"Undo Delete (with id - {videoeventLocalId})";
            MenuItem_UndoDelete.IsEnabled = (videoeventLocalId > -1);
        }

        public void DisableUndoDeleteAndReset()
        {
            string contextMenuKey = "TimelineMenu";
            var MenuItem_UndoDelete = GetMenuItemByResourceName(contextMenuKey, "MenuItem_UndoDelete");
            MenuItem_UndoDelete.Header = $"Undo Delete";
            MenuItem_UndoDelete.IsEnabled = false;
        }

        #endregion


        #region == Events in Middle needs shift ==

        private void AddImageEventUsingCBLibraryInMiddle_Click(object sender, RoutedEventArgs e)
        {
            TimelineEvent timelineEvent = _timelineGridControl.GetSelectedEvent();
            TimelineVideoEvent shiftEvent = _timelineGridControl.GetSelectedEvent().VideoEvent;
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = shiftEvent,
                timeAtTheMoment = shiftEvent.videoevent_end
            };

            ContextMenu_AddImageEventUsingCBLibraryInMiddle_Clicked(sender, payload);
        }

        #endregion


        #region == KG Added Events ==
        private void TrackBarMouseMovedAndStopped(object sender, EventArgs ea)
        {
            if (TimelineGridCtrl2._isTrackbarLineDragInProg == false)
            {
                dispatcherTimer.Stop();
                var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;

                var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
                var getImageOrVideo = trackbarEvents.Find(x => (int)x.TrackNumber == (int)EnumTrack.IMAGEORVIDEO);
                if (getImageOrVideo != null)
                {
                    var start = getImageOrVideo.StartTime;
                    trackBarPosition = trackBarPosition - start;
                }
                var payload = new TrackbarMouseMoveEvent
                {
                    timeAtTheMoment = trackBarPosition.ToString(@"hh\:mm\:ss\.fff"),
                    videoeventIds = trackbarEvents?
                                    .Where(x => x.TrackNumber != TrackNumber.Notes)
                                    .GroupBy(x => x.VideoEvent?.videoevent_id)
                                    .Select(x => x.FirstOrDefault().VideoEvent.videoevent_id)
                                    .ToList(),
                    isAnyVideo = trackbarEvents?.Find(x => x.TrackNumber == TrackNumber.Video) != null,
                };
                TrackbarMouse_Moved.Invoke(sender, payload);
            }
        }

        private FormOrCloneEvent CalloutPreprocessing(bool IsCallout = true)
        {
            TimelineVideoEvent selectedEvent;
            var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            var trackbarTime = trackBarPosition.ToString(@"hh\:mm\:ss\.fff");
            var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();
            if (trackbarEvents != null && trackbarEvents?.Count > 0)
                selectedEvent = trackbarEvents[0].VideoEvent;
            else
                selectedEvent = _timelineGridControl.GetSelectedEvent()?.VideoEvent;

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

        private void AddFormEvent_Click(object sender, RoutedEventArgs e)
        {
            var payload = CalloutPreprocessing(false);
            ContextMenu_AddFormEvent_Clicked.Invoke(sender, payload);
        }


        private void ManageMedia_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu_ManageMedia_Clicked.Invoke(sender, e);
        }

        private void AddImageEventUsingCBLibrary_Click(object sender, RoutedEventArgs e)
        {
            var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.FORM, selectedProjectEvent.projdetId);
            ContextMenu_AddImageEventUsingCBLibrary_Clicked.Invoke(sender, trackbarTime);
        }

        private void RunEvent_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu_Run_Clicked.Invoke(sender, e);
        }

        private void CloneEvent_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvent = _timelineGridControl.GetSelectedEvent();

            if (selectedEvent == null)
            {
                MessageBox.Show("No event selected to clone, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();

            var isSameEventPresent = trackbarEvents?.Find(x => x.VideoEvent.videoevent_track == selectedEvent.VideoEvent.videoevent_track);

            if (isSameEventPresent != null)
            {
                MessageBox.Show("The trackbar position indicates there is another event(s) present already, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            var trackbarTime = trackBarPosition.ToString(@"hh\:mm\:ss\.fff");
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent.VideoEvent,
                timeAtTheMoment = trackbarTime
            };
            ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);

        }

        private void CloneEventAtEnd_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvent = _timelineGridControl.GetSelectedEvent();

            if (selectedEvent == null)
            {
                MessageBox.Show("No event selected to clone, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId);
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent.VideoEvent,
                timeAtTheMoment = trackbarTime
            };
            ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);
        }

        #endregion

    }
}