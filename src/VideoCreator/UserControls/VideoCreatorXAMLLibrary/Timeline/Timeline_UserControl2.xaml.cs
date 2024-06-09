using FullScreenPlayer_UserControl.Controls;
using ManageMedia_UserControl.Models;
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
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Helpers;
using VideoCreatorXAMLLibrary.Models;
using System.Collections.ObjectModel;
using MMClass = ManageMedia_UserControl.Classes;
using MMModel = ManageMedia_UserControl.Models;

namespace VideoCreatorXAMLLibrary
{
    public partial class Timeline_UserControl2 : UserControl
    {
        public bool HasData { get; set; } = false;
        private SelectedProjectEvent selectedProjectEvent;

        private AuthAPIViewModel authApiViewModel;

        public event EventHandler ContextMenu_AddVideoEvent_Clicked;

        public event EventHandler<string> ContextMenu_AddImageEventUsingCBLibrary_Clicked;
        //public event EventHandler<FormOrCloneEvent> ContextMenu_AddImageEventUsingCBLibraryInMiddle_Clicked;
        public event EventHandler ContextMenu_ManageMedia_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout1_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddCallout2_Clicked;
        public event EventHandler<FormOrCloneEvent> ContextMenu_AddFormEvent_Clicked;
        public event EventHandler<MMModel.TrackbarMouseMoveEventModel> TrackbarMouseMoveEvent;
        public event EventHandler<int> ContextMenu_DeleteEventOnTimelines_Clicked;
        public event EventHandler<int> ContextMenu_EditFormEvent_Clicked;

        public event EventHandler ContextMenu_Run_Clicked;

        public event EventHandler<FormOrCloneEvent> ContextMenu_CloneEvent_Clicked;

        public event EventHandler<TimelineSelectedEvent> VideoEventSelectionChangedEvent;
        public event EventHandler<List<CBVVideoEvent>> ContextMenu_SaveAllTimelines_Clicked;
        public event EventHandler ContextMenu_UndeleteDeletedEvent_Clicked;

        private bool ReadOnlyFlag;
        private EnumRole Role;
        //int i;
        public Timeline_UserControl2()
        {
            InitializeComponent();
            lblMesssage.Content = "";
            TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia.CollectionChanged += CalloutLocationOrSizeChangedMedia_CollectionChanged;
        }

        void CalloutLocationOrSizeChangedMedia_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                btnSave.IsEnabled = true;
                lblMesssage.Content = $"{TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia.Count} events changed.";
            }
            else
            {
                btnSave.IsEnabled = false;
                lblMesssage.Content = "";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveTimeline(sender, e);
        }

        public void SetSelectedProjectId(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, bool readonlyMode = false)
        {
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            ReadOnlyFlag = readonlyMode;
            Role = _selectedProjectEvent.role;
            MMTimelineHelper.Init(selectedProjectEvent, TimelineGridCtrl2);
        }


        public void Refresh()
        {
            MMTimelineHelper.Init(selectedProjectEvent, TimelineGridCtrl2);
            TimelineGridCtrl2.SetTrackbar();
            TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia = new ObservableCollection<MMClass.Media>();
            btnSave.IsEnabled = TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia.Count > 0;
            TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia.CollectionChanged += CalloutLocationOrSizeChangedMedia_CollectionChanged;
            lblMesssage.Content = "";
        }

        private MessageBoxResult ConfirmOverwriteChanges()
        {
            var result = MessageBoxResult.Yes;
            if (TimelineGridCtrl2.CalloutLocationOrSizeChangedMedia?.Count > 0)
            {
                result = MessageBox.Show($"You have unsaved changes, do you want to save those changes first?{Environment.NewLine}Press Cancel to return.", "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    SaveTimeline(null, null);
                }
            }
            return result;
        }

        private void TimelineMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            string contextMenuKey = "TimelineMenu";
            var menuItem_PopulateFromDatabase = GetMenuItemByResourceName(contextMenuKey, "MenuItem_PopulateFromDatabase");
            menuItem_PopulateFromDatabase.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddPlanningEvents = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddPlanningEvents");
            MenuItem_AddPlanningEvents.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            //var MenuItem_SaveAllTimelines = GetMenuItemByResourceName(contextMenuKey, "MenuItem_SaveAllTimelines");
            //MenuItem_SaveAllTimelines.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddAudioEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddAudioEvent");
            MenuItem_AddAudioEvent.IsEnabled = false;

            var MenuItem_AddCallout1 = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddCallout1");
            MenuItem_AddCallout1.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddCallout2 = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddCallout2");
            MenuItem_AddCallout2.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddVideoEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddVideoEvent");
            MenuItem_AddVideoEvent.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddFormEvent = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddFormEvent");
            MenuItem_AddFormEvent.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_AddImageEventUsingCBLibrary = GetMenuItemByResourceName(contextMenuKey, "MenuItem_AddImageEventUsingCBLibrary");
            MenuItem_AddImageEventUsingCBLibrary.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            //var MenuItem_ManageMedia = GetMenuItemByResourceName(contextMenuKey, "MenuItem_ManageMedia");
            //MenuItem_ManageMedia.IsEnabled = Role == EnumRole.PROJECT_WRITE;

            var MenuItem_Run = GetMenuItemByResourceName(contextMenuKey, "MenuItem_Run");
            MenuItem_Run.IsEnabled = true;

            var MenuItem_UndoDelete = GetMenuItemByResourceName(contextMenuKey, "MenuItem_UndoDelete");
            MenuItem_UndoDelete.IsEnabled = Role == EnumRole.PROJECT_WRITE;
        }


        #region == TimelineUserControl : Helper functions ==

        private void TimelineGridCtrl2_EditFormEvent(object sender, int EditVideoEventId)
        {
            ContextMenu_EditFormEvent_Clicked.Invoke(sender, EditVideoEventId);
        }

        private void TimelineGridCtrl2_Delete_Event(object sender, int DeletedId)
        {
            ContextMenu_DeleteEventOnTimelines_Clicked.Invoke(sender, DeletedId);
        }

        private void TimelineGridCtrl2_Clone_Event(object sender, MMClass.Media selectedEvent)
        {
            var trackbarTime = TimelineGridCtrl2.GetTrackbarTime();
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent,
                timeAtTheMoment = trackbarTime.ToString(@"hh\:mm\:ss\.fff")
            };
            ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);
        }

        private void TimelineGridCtrl2_CloneAtEnd_Event(object sender, MMClass.Media selectedEvent)
        {
            var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId);
            var payload = new FormOrCloneEvent
            {
                timelineVideoEvent = selectedEvent,
                timeAtTheMoment = trackbarTime
            };
            ContextMenu_CloneEvent_Clicked.Invoke(sender, payload);
        }


        private void TimelineGridCtrl2_TrackbarMouseMoveEvent(object sender, MMModel.TrackbarMouseMoveEventModel payload)
        {
            TrackbarMouseMoveEvent.Invoke(sender, payload);
        }


        private void EventSelectionChangedEvent(object sender, MMClass.Media selectedEvent)
        {
            if (selectedEvent != null)
            {
                var payload = new TimelineSelectedEvent
                {
                    Track = (EnumTrack)selectedEvent.TrackId,
                    EventId = selectedEvent.TrackId != -1 ? selectedEvent.VideoEventID : -1
                };
                VideoEventSelectionChangedEvent.Invoke(sender, payload);
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
            var result = ConfirmOverwriteChanges();
            if(result != MessageBoxResult.Cancel)
                ContextMenu_AddVideoEvent_Clicked.Invoke(sender, e);
        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
                Refresh();
        }

        private void UndeleteDeletedEvent(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
                ContextMenu_UndeleteDeletedEvent_Clicked.Invoke(sender, e);
            DisableUndoDeleteAndReset();
        }


        private void SaveTimeline(object sender, RoutedEventArgs e)
        {
            if (TimelineGridCtrl2?.CalloutLocationOrSizeChangedMedia?.Count > 0)
            {
                var modifiedEvents = new List<CBVVideoEvent>();
                foreach (var item in TimelineGridCtrl2?.CalloutLocationOrSizeChangedMedia)
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
                ContextMenu_SaveAllTimelines_Clicked.Invoke(sender, modifiedEvents);
            }
            else
            {
                MessageBox.Show("Nothing to save here !!!", "Information");
            }
        }

        #endregion


        #region == Planning Events ==
        private async void AddPlanningEvents_Click(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                var sqlCon = SyncDbHelper.InitializeDatabaseAndGetConnection();
                using (var transaction = sqlCon.BeginTransaction())
                {
                    try
                    {

                        var trackbarTime = DataManagerSqlLite.GetNextStartForTransaction((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId, sqlCon);
                        var payload = new PlanningEvent
                        {
                            Type = EnumScreen.All,
                            TimeAtTheMoment = trackbarTime
                        };

                        var backgroundImagePath = PlanningHandlerHelper.CheckIfBackgroundPresent(sqlCon);
                        if (backgroundImagePath == null)
                            MessageBox.Show($"No Background found, plannings cannot be added.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            await PlanningHandlerHelper.ProcessForTransaction(payload, selectedProjectEvent, authApiViewModel, this, loader, sqlCon, backgroundImagePath);
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    transaction.Commit();
                    sqlCon?.Close();
                }
                Refresh();
                LoaderHelper.HideLoader(this, loader);
            }

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
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                var payload = CalloutPreprocessing();
                ContextMenu_AddCallout1_Clicked.Invoke(sender, payload);
            }
        }

        private void AddCallout2_Click(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                var payload = CalloutPreprocessing();
                ContextMenu_AddCallout2_Clicked.Invoke(sender, payload);
            }
        }

        private FormOrCloneEvent CalloutPreprocessing(bool IsCallout = true)
        {
            var trackbarTime = TimelineGridCtrl2.GetTrackbarTime().ToString(@"hh\:mm\:ss\.fff");
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
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                var payload = CalloutPreprocessing(false);
                ContextMenu_AddFormEvent_Clicked.Invoke(sender, payload);
            }
        }


        private void ManageMedia_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu_ManageMedia_Clicked.Invoke(sender, e);
        }

        private void AddImageEventUsingCBLibrary_Click(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                var trackbarTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.FORM, selectedProjectEvent.projdetId);
                ContextMenu_AddImageEventUsingCBLibrary_Clicked.Invoke(sender, trackbarTime);
            }
        }

        private void RunEvent_Click(object sender, RoutedEventArgs e)
        {
            var result = ConfirmOverwriteChanges();
            if (result != MessageBoxResult.Cancel)
            {
                ContextMenu_Run_Clicked.Invoke(sender, e);
            }
        }
        #endregion


    }
}