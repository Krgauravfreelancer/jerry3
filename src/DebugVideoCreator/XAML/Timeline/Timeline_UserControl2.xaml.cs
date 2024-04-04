using FullScreenPlayer_UserControl.Controls;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Timeline.UserControls.Controls;
using Timeline.UserControls.Models;
using Timeline.UserControls.Models.Datatables;
using VideoCreator.Auth;
using VideoCreator.Helpers;
using VideoCreator.Models;
using MMClass = ManageMedia_UserControl.Classes;
using MMModel = ManageMedia_UserControl.Models;

namespace VideoCreator.XAML
{
    public partial class Timeline_UserControl2 : UserControl
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
        public event EventHandler<MMModel.TrackbarMouseMoveEvent> TrackbarMouse_Moved;
        public event EventHandler<int> ContextMenu_DeleteEventOnTimelines_Clicked;

        public event EventHandler ContextMenu_Run_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_CloneEvent_Clicked;

        public event EventHandler<TimelineSelectedEvent> VideoEventSelectionChanged;
        public event EventHandler<List<TimelineVideoEvent>> ContextMenu_SaveAllTimelines_Clicked;
        public event EventHandler ContextMenu_UndeleteDeletedEvent_Clicked;

        private bool ReadOnly;
        //int i;
        public Timeline_UserControl2()
        {
            InitializeComponent();
        }

        public void SetSelectedProjectId(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, bool readonlyMode = false)
        {
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            ReadOnly = readonlyMode;
            MMTimelineHelper.Init(selectedProjectEvent, TimelineGridCtrl2);
        }


        public void Refresh()
        {
            MMTimelineHelper.Init(selectedProjectEvent, TimelineGridCtrl2);
            TimelineGridCtrl2.SetTrackbar();
        }


        #region == TimelineUserControl : Helper functions ==

        private void TimelineGridCtrl2_Delete_Event(object sender, int DeletedId)
        {
            ContextMenu_DeleteEventOnTimelines_Clicked.Invoke(sender, DeletedId);
        }


        private void TimelineGridCtrl2_MouseDown_Event(object sender, MMModel.MouseDownEvent payload)
        {
            EventSelectionChanged(sender, payload.selectedEvent);
            TrackbarMouse_Moved.Invoke(sender, payload.trackbarMouseMoveEvent);
        }


        private void EventSelectionChanged(object sender, MMModel.SelectedEvent selectedEvent)
        {
            if (selectedEvent != null)
            {
                var payload = new TimelineSelectedEvent
                {
                    Track = (TrackNumber)selectedEvent.TrackId,
                    EventId = selectedEvent.TrackId != -1 ? selectedEvent.EventId : -1
                };
                VideoEventSelectionChanged.Invoke(sender, payload);
            }
        }

        private MenuItem GetMenuItemByResourceName(string resourceKey, string menuItemName)
        {
            var contextMenu = this.Resources[resourceKey] as ContextMenu;
            var contextMenuItem = LogicalTreeHelper.FindLogicalNode(contextMenu, menuItemName) as MenuItem;

            return contextMenuItem;
        }

        #endregion

        #region == TimelineUserControl : Context-menu Options ==

        private void AddAudioEvent_Click(object sender, RoutedEventArgs e)
        {
            //_timelineGridControl.AddNewEventToTimeline(MediaType.audio);
            throw new NotImplementedException();
        }

        private void AddVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu_AddVideoEvent_Clicked.Invoke(sender, e);
        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void UndeleteDeletedEvent(object sender, RoutedEventArgs e)
        {
            ContextMenu_UndeleteDeletedEvent_Clicked.Invoke(sender, e);
            DisableUndoDeleteAndReset();
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

            //var modifiedEvents = _timelineGridControl.GetModifiedTimelineEvents();
            //ContextMenu_SaveAllTimelines_Clicked.Invoke(sender, modifiedEvents);

            //_timelineGridControl.ClearTimeline();

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
                Refresh();
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
            //TimelineEvent timelineEvent = _timelineGridControl.GetSelectedEvent();
            //TimelineVideoEvent shiftEvent = _timelineGridControl.GetSelectedEvent().VideoEvent;
            //var payload = new FormOrCloneEvent
            //{
            //    timelineVideoEvent = shiftEvent,
            //    timeAtTheMoment = shiftEvent.videoevent_end
            //};

            //ContextMenu_AddImageEventUsingCBLibraryInMiddle_Clicked(sender, payload);
        }

        #endregion


        #region == KG Added Events ==

        private void AddCallout1_Click(object sender, RoutedEventArgs e)
        {
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

        private FormOrCloneEvent CalloutPreprocessing(bool IsCallout = true)
        {
            var trackbarTime = TimelineGridCtrl2.GetTrackbarTime();
            MMClass.Media selectedEvent = TimelineGridCtrl2.GetTrackbarMediaEvents().FirstOrDefault();

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
            //var selectedEvent = _timelineGridControl.GetSelectedEvent();

            //if (selectedEvent == null)
            //{
            //    MessageBox.Show("No event selected to clone, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //var trackbarEvents = _timelineGridControl.GetTrackbarVideoEvents();

            //var isSameEventPresent = trackbarEvents?.Find(x => x.VideoEvent.videoevent_track == selectedEvent.VideoEvent.videoevent_track);

            //if (isSameEventPresent != null)
            //{
            //    MessageBox.Show("The trackbar position indicates there is another event(s) present already, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //var trackBarPosition = TimelineGridCtrl2.TrackbarPosition;
            //var trackbarTime = trackBarPosition.ToString(@"hh\:mm\:ss\.fff");
            //var payload = new FormOrCloneEvent
            //{
            //    timelineVideoEvent = selectedEvent.VideoEvent,
            //    timeAtTheMoment = trackbarTime
            //};
            //ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);

        }

        private void CloneEventAtEnd_Click(object sender, RoutedEventArgs e)
        {
            //var selectedEvent = _timelineGridControl.GetSelectedEvent();

            //if (selectedEvent == null)
            //{
            //    MessageBox.Show("No event selected to clone, so cant continue", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId);
            //var payload = new FormOrCloneEvent
            //{
            //    timelineVideoEvent = selectedEvent.VideoEvent,
            //    timeAtTheMoment = trackbarTime
            //};
            //ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);
        }




        #endregion

        
    }
}