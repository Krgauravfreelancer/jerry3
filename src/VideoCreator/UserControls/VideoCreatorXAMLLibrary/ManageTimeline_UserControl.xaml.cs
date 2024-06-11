using ManageMedia_UserControl.Classes;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Helpers;
using VideoCreatorXAMLLibrary.MediaLibraryData;
using VideoCreatorXAMLLibrary.Models;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using Windows = System.Windows.Controls;

namespace VideoCreatorXAMLLibrary
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimeline_UserControl : UserControl, IDisposable
    {
        private PopupWindow popup;
        private readonly AuthAPIViewModel authApiViewModel;
        private bool ReadOnlyFlag;
        private EnumRole Role;
        private int RetryIntervalInSeconds = 300;
        SelectedProjectEvent selectedProjectEvent;


        private ManageMedia_UserControl.Models.TrackbarMouseMoveEventModel mouseEventToProcess;
        private CBVVideoEvent selectedVideoEvent;
        private int selectedVideoEventId = -1;
        private int undoVideoEventId = -1;



        /// <summary>
        /// Constructor to call the manage timeline
        /// </summary>
        /// <param name="_selectedProjectEvent"> selected project event from main grid</param>
        /// <param name="_authApiViewModel">Authentication API model</param>
        /// <param name="_readonlyFlag">Whether to open window in readonly mode or not</param>
        public ManageTimeline_UserControl(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, bool _readonlyFlag)
        {
            InitializeComponent();
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            ReadOnlyFlag = _readonlyFlag;
            Role = _selectedProjectEvent.role;
            var subjectText = "Project Id - " + selectedProjectEvent.projectId;
            lblSelectedProjectId.Content = ReadOnlyFlag ? $"[READONLY] {subjectText}" : subjectText;
            InitializeChildren();
            new Action(async () =>
            {
                await SyncServerEventsHelper.ConfirmAndSyncServerDataToLocalDB(this, btnDownloadServerData, selectedProjectEvent, loader, authApiViewModel);
                //Refresh();
            })();

            BackgroundProcessHelper.SetBackgroundProcess(selectedProjectEvent, authApiViewModel, btnUploadNotSyncedData);
            loader.Visibility = Visibility.Hidden;
        }



        /// <summary>
        /// Initialize all child usercontrol and bind the events which are needed to perform the functions
        /// </summary>
        private void InitializeChildren()
        {
            popup = new PopupWindow();
            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectEvent, authApiViewModel, ReadOnlyFlag);
            TimelineUserConrol.TrackbarMouseMoveEvent += TimelineUserConrol_TrackbarMouseMoveEvent;
            TimelineUserConrol.VideoEventSelectionChangedEvent += TimelineUserConrol_VideoEventSelectionChangedEvent;
            TimelineUserConrol.ContextMenu_AddCallout1_Clicked += TimelineUserConrol_ContextMenu_AddCallout1_Clicked;
            TimelineUserConrol.ContextMenu_AddCallout2_Clicked += TimelineUserConrol_ContextMenu_AddCallout2_Clicked;
            TimelineUserConrol.ContextMenu_AddVideoEvent_Clicked += TimelineUserConrol_ContextMenu_AddVideoEvent_Clicked;
            TimelineUserConrol.ContextMenu_AddFormEvent_Clicked += TimelineUserConrol_ContextMenu_AddFormEvent_Clicked;
            TimelineUserConrol.ContextMenu_AddImageEventUsingCBLibrary_Clicked += TimelineUserConrol_AddImageEventFromCBLibrary_Clicked;
            TimelineUserConrol.ContextMenu_ManageMedia_Clicked += TimelineUserConrol_ContextMenu_ManageMedia_Clicked;
            TimelineUserConrol.ContextMenu_Run_Clicked += TimelineUserConrol_ContextMenu_Run_Clicked;

            TimelineUserConrol.ContextMenu_EditFormEvent_Clicked += TimelineUserConrol_EditFormEvent;
            TimelineUserConrol.ContextMenu_DeleteEventOnTimelines_Clicked += TimelineUserConrol_DeleteEventOnTimelines;
            TimelineUserConrol.ContextMenu_UndeleteDeletedEvent_Clicked += TimelineUserConrol_UndeleteDeletedEvent_Clicked;
            TimelineUserConrol.ContextMenu_CloneEvent_Clicked += TimelineUserConrol_ContextMenu_CloneEvent_Clicked;
            //TimelineUserConrol.ContextMenu_AddImageEventUsingCBLibraryInMiddle_Clicked += TimelineUserConrol_AddImageEventUsingCBLibraryInMiddle_Clicked;
            TimelineUserConrol.ContextMenu_SaveAllTimelines_Clicked += TimelineUserConrol_SaveAllTimelines_Clicked;
        }

        /// <summary>
        /// refresh the timeline once any CRUD operations are completed
        /// </summary>
        private void Refresh()
        {
            TimelineUserConrol.Refresh();
            if (mouseEventToProcess != null)
                TimelineUserConrol_TrackbarMouseMoveEvent(null, mouseEventToProcess);
        }


        #region == Video Event Context Menu ==

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Run context menu option is selected
        /// </summary>
        private void TimelineUserConrol_ContextMenu_Run_Clicked(object sender, EventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            var uc = new ManageMediaWindowManager();
            var GeneratedRecorderWindow = uc.CreateWindow(selectedProjectEvent, readOnly: true);

            uc.ManageMedia_NotesCreatedEvent += (DataTable dt) => { };
            uc.ManageMedia_NotesChangedEvent += (DataTable dt) => { };
            uc.ManageMedia_NotesDeletedEvent += (List<int> DeletedIds) => { };
            uc.ManageMedia_AddVideoEvents += (DataTable dt) => { };
            uc.ManageMedia_DeletedVideoEvents += (int videoeventLocalId) => { };
            uc.ManageMedia_AdjustVideoEvents += (DataTable table) => { };

            // Logic to Display window
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader);
            var result = uc.ShowWindow();
            if (result.HasValue)
            {
                Refresh();
            }
            LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Add Video Event context menu option is selected
        /// </summary>
        private void TimelineUserConrol_ContextMenu_AddVideoEvent_Clicked(object sender, EventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            var uc = new ScreenRecorderWindowManager();
            var GeneratedRecorderWindow = uc.CreateWindow(selectedProjectEvent);
            GeneratedRecorderWindow.Closed += (se, ev) =>
            {
                //this.Visibility = Visibility.Visible;
                GeneratedRecorderWindow = null;
                //LoadPlaceHolderBox(Selected_ID);
                //LoadVideoEventBox(Selected_ID);
            };
            uc.ScreenRecorder_BtnSaveClickedEvent += async (DataTable dt) =>
            {
                LoaderHelper.ShowLoader(this, loader, $"saving {dt?.Rows.Count} events ..");
                await AddVideoEvent_Clicked(dt);
                uc.LoadProjectData();
                LoaderHelper.HideLoader(this, loader);
            };

            //uc.ScreenRecorder_BtnDeleteMediaClicked += async (int videoeventLocalId) =>
            //{
            //    LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Deleting event with id - {videoeventLocalId} and shifting other events");
            //    // Logic Here
            //    var videoevents = DataManagerSqlLite.GetVideoEventbyId(videoeventLocalId, false, false);
            //    var videoevent = videoevents.FirstOrDefault();
            //    await ShiftEventsHelper.DeleteAndShiftEvent(videoeventLocalId, videoevent.videoevent_serverid, isShift: true, EnumTrack.IMAGEORVIDEO, videoevent.videoevent_duration, videoevent.videoevent_end, selectedProjectEvent, authApiViewModel);

            //    LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            //    uc.LoadProjectData();
            //};

            //uc.ScreenRecorder_NotesCreatedEvent += async (DataTable dtNotes) =>
            //{
            //    LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Adding {dtNotes?.Rows?.Count} Notes");
            //    var notesAll = NotesEventHandlerHelper.GetNotesModelList(dtNotes);
            //    foreach(KeyValuePair<Int64, List<NotesModelPost>> item in notesAll)
            //    {
            //        var savedNotes = await authApiViewModel.POSTNotes(item.Key, item.Value);
            //        if (savedNotes != null)
            //        {
            //            var localVideoEventId = DataManagerSqlLite.GetVideoEventbyServerId(item.Key).FirstOrDefault().videoevent_id;
            //            var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(savedNotes.Notes, localVideoEventId);
            //            DataManagerSqlLite.InsertRowsToNotes(notesDatatable);
            //        }
            //    }
            //    LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            //    uc.LoadProjectData();
            //};

            //uc.ScreenRecorder_NotesChangedEvent += async (DataTable dtNotes) =>
            //{
            //    LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Updating {dtNotes?.Rows?.Count} Notes");
            //    var notesAll = NotesEventHandlerHelper.GetNotesModelListPut(dtNotes);
            //    foreach (KeyValuePair<Int64, List<NotesModelPut>> item in notesAll)
            //    {
            //        var serverVideoEventId = item.Key;
            //        var updatedNotes = await authApiViewModel.PUTNotes(serverVideoEventId, item.Value);
            //        if (updatedNotes != null)
            //        {
            //            var localVideoEventId = DataManagerSqlLite.GetVideoEventbyServerId(item.Key).FirstOrDefault().videoevent_id;
            //            var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(updatedNotes, localVideoEventId);
            //            DataManagerSqlLite.UpdateRowsToNotes(notesDatatable, true);
            //        }
            //    }
            //    LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            //    uc.LoadProjectData();
            //};

            //uc.ScreenRecorder_NotesDeletedEvent += async (List<int> Ids) =>
            //{
            //    LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Deleting {Ids.Count} Notes");

            //    // Logic Here
            //    foreach (var noteId in Ids)
            //    {
            //        var noteItem = DataManagerSqlLite.GetNotesbyId(noteId).FirstOrDefault();
            //        var videoEventServerId = DataManagerSqlLite.GetVideoEventbyId(noteItem.fk_notes_videoevent).FirstOrDefault().videoevent_serverid;
            //        var deletedNotes = await authApiViewModel.DeleteNotesById(videoEventServerId, noteItem.notes_serverid);
            //        if (deletedNotes != null)
            //        {
            //            DataManagerSqlLite.DeleteNotesById(noteItem.notes_id);
            //        }
            //    }

            //    LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            //    uc.LoadProjectData();
            //};

            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader);
            var result = uc.ShowWindow(GeneratedRecorderWindow);
            if (result.HasValue)
            {
                Refresh();
            }
            LoaderHelper.HideLoader(this, loader);
        }


        #region == Manage Media Section ==

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media context menu option is selected
        /// </summary>
        private void TimelineUserConrol_ContextMenu_ManageMedia_Clicked(object sender, EventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            var uc = new ManageMediaWindowManager();
            var GeneratedRecorderWindow = uc.CreateWindow(selectedProjectEvent);
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader);

            uc.ManageMedia_NotesCreatedEvent += async (DataTable dt) =>
            {
                await ManageMedia_NotesCreatedEvent(GeneratedRecorderWindow, dt, uc);
            };
            uc.ManageMedia_NotesChangedEvent += async (DataTable dt) =>
            {
                await ManageMedia_NotesChangedEvent(GeneratedRecorderWindow, dt, uc);
            };
            uc.ManageMedia_NotesDeletedEvent += async (List<int> DeletedIds) =>
            {
                await ManageMedia_NotesDeletedEvent(GeneratedRecorderWindow, DeletedIds, uc);
            };
            uc.ManageMedia_AddVideoEvents += async (DataTable dt) =>
            {
                await ManageMedia_AddVideoEvents(GeneratedRecorderWindow, dt, uc);
            };
            uc.ManageMedia_DeletedVideoEvents += async (int videoeventLocalId) =>
            {
                await ManageMedia_DeletedVideoEvents(GeneratedRecorderWindow, videoeventLocalId, uc);
            };
            uc.ManageMedia_AdjustVideoEvents += async (DataTable table) =>
            {
                await ManageMedia_AdjustVideoEvents(GeneratedRecorderWindow, table, uc);
            };

            // Logic to Display window

            var result = uc.ShowWindow();
            if (result.HasValue)
            {
                Refresh();
            }
            LoaderHelper.HideLoader(this, loader);
        }



        #region == Manage Media VideoEvent Section==

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> New Video Event is added
        /// </summary>
        private async Task ManageMedia_AddVideoEvents(Window GeneratedRecorderWindow, DataTable datatable, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"saving {datatable?.Rows.Count} events ..");
            await AddVideoEvent_Clicked(datatable);
            uc.RefreshData();
            LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> Delete Video Event is called
        /// </summary>
        private async Task ManageMedia_DeletedVideoEvents(Window GeneratedRecorderWindow, int videoeventLocalId, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(this, loader, $"Deleting Event & shifting other events");
            await HandleDeleteLogicForVideoEvent(videoeventLocalId);
            uc.RefreshData();
            LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> Adjust Video Event is called
        /// </summary>
        private async Task ManageMedia_AdjustVideoEvents(Window GeneratedRecorderWindow, DataTable table, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(this, loader, $"Adjusting Video Events");
            await ShiftEventsHelper.ManageMediaAdjustVideoEvents(table, selectedProjectEvent, authApiViewModel);
            LoaderHelper.HideLoader(this, loader);
        }

        #endregion


        #region == Manage Media Notes Section ==

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> New Note is created
        /// </summary>
        private async Task ManageMedia_NotesCreatedEvent(Window GeneratedRecorderWindow, DataTable dtNotes, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Adding {dtNotes?.Rows.Count} notes ..");
            var notesAll = NotesEventHandlerHelper.GetNotesModelList(dtNotes);
            foreach (KeyValuePair<Int64, List<NotesModelPost>> item in notesAll)
            {
                var localVideoEventId = item.Key;
                var selectedServerVideoEventId = DataManagerSqlLite.GetVideoEventbyId(Convert.ToInt32(localVideoEventId), false, false)[0].videoevent_serverid;
                var savedNotes = await authApiViewModel.POSTNotes(selectedServerVideoEventId, item.Value);
                if (savedNotes != null)
                {
                    var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(savedNotes.Notes, Convert.ToInt32(localVideoEventId));
                    DataManagerSqlLite.InsertRowsToNotes(notesDatatable);
                }
            }
            uc.RefreshData();
            LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> Note is changed
        /// </summary>
        private async Task ManageMedia_NotesChangedEvent(Window GeneratedRecorderWindow, DataTable dtNotes, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Updating {dtNotes?.Rows?.Count} Notes");
            var notesAll = NotesEventHandlerHelper.GetNotesModelListPut(dtNotes);
            foreach (KeyValuePair<Int64, List<NotesModelPut>> item in notesAll)
            {
                var localVideoEventId = Convert.ToInt32(item.Key);
                var selectedServerVideoEventId = DataManagerSqlLite.GetVideoEventbyId(localVideoEventId, false, false)[0].videoevent_serverid;

                var updatedNotes = await authApiViewModel.PUTNotes(selectedServerVideoEventId, item.Value);
                if (updatedNotes != null)
                {
                    var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(updatedNotes, localVideoEventId);
                    DataManagerSqlLite.UpdateRowsToNotes(notesDatatable, true);
                }
            }
            LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            uc.RefreshData();
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Manage Media ==> Note is Deleted
        /// </summary>
        private async Task ManageMedia_NotesDeletedEvent(Window GeneratedRecorderWindow, List<int> Ids, ManageMediaWindowManager uc)
        {
            LoaderHelper.ShowLoader(GeneratedRecorderWindow, uc.loader, $"Deleting {Ids.Count} Notes");

            // Logic Here
            foreach (var noteId in Ids)
            {
                var noteItem = DataManagerSqlLite.GetNotesbyId(noteId).FirstOrDefault();
                var videoEventServerId = DataManagerSqlLite.GetVideoEventbyId(noteItem.fk_notes_videoevent).FirstOrDefault().videoevent_serverid;
                var deletedNotes = await authApiViewModel.DeleteNotesById(videoEventServerId, noteItem.notes_serverid);
                if (deletedNotes != null)
                {
                    DataManagerSqlLite.DeleteNotesById(noteItem.notes_id);
                }
            }

            LoaderHelper.HideLoader(GeneratedRecorderWindow, uc.loader);
            uc.RefreshData();
        }

        #endregion == Manage Media Notes Section ==

        #endregion

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Logic when Video Event is Deleted
        /// </summary>
        private async Task HandleDeleteLogicForVideoEvent(int videoeventLocalId)
        {
            //Logic Here
            var videoevent = DataManagerSqlLite.GetVideoEventbyId(videoeventLocalId, false, false)?.FirstOrDefault();

            if (videoevent?.videoevent_track == 2)
            {
                await ShiftEventsHelper.DeleteAndShiftEvent(videoeventLocalId, videoevent.videoevent_serverid, isShift: true, EnumTrack.IMAGEORVIDEO, videoevent.videoevent_duration, videoevent.videoevent_end, selectedProjectEvent, authApiViewModel);

                //TBD - we need to shift the callout events as well
                var overlappedCallouts = DataManagerSqlLite.GetOverlappingCalloutsByTime(selectedProjectEvent.projdetId, videoevent.videoevent_start, videoevent.videoevent_end);
                if (overlappedCallouts?.Count > 0)
                {
                    var confirmation = MessageBox.Show($"You have {overlappedCallouts?.Count} overlapping callouts. Do you want to delete and shift callouts ? ", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmation == MessageBoxResult.Yes)
                    {
                        //Step 1: Delete all overlapping items
                        foreach (var item in overlappedCallouts)
                        {
                            await ShiftEventsHelper.DeleteAndShiftEvent(item.videoevent_id, item.videoevent_serverid, isShift: false, EnumTrack.CALLOUT1, item.videoevent_duration, videoevent.videoevent_end, selectedProjectEvent, authApiViewModel);
                        }
                        //Step 2: Find all videoevents, which needs to be shifted
                        var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyEndTime(selectedProjectEvent.projdetId, videoevent.videoevent_end, EnumTrack.CALLOUT1);
                        // Step-3 Call server API to shift video event and then save locally the shifted events
                        if (tobeShiftedVideoEvents?.Count > 0)
                        {
                            var tobeServerShiftedVideoEvents = new List<ShiftVideoEventModel>();
                            foreach (var item in tobeShiftedVideoEvents)
                            {
                                var model = new ShiftVideoEventModel
                                {
                                    videoevent_id = (int)item.videoevent_serverid,
                                    videoevent_duration = item.videoevent_duration,
                                    videoevent_origduration = item.videoevent_origduration,
                                    videoevent_end = DataManagerSqlLite.ShiftLeft(item.videoevent_end, videoevent.videoevent_duration),
                                    videoevent_start = DataManagerSqlLite.ShiftLeft(item.videoevent_start, videoevent.videoevent_duration)
                                };
                                tobeServerShiftedVideoEvents.Add(model);
                            }
                            var serverShiftedVideoEvents = await MediaEventHandlerHelper.ShiftVideoEventsToServer(selectedProjectEvent, tobeServerShiftedVideoEvents, authApiViewModel);

                            var dtShiftedVideoEvents = new DataTable();
                            dtShiftedVideoEvents.Columns.Add("videoevent_serverid", typeof(Int64));
                            dtShiftedVideoEvents.Columns.Add("videoevent_start", typeof(string));
                            dtShiftedVideoEvents.Columns.Add("videoevent_end", typeof(string));
                            dtShiftedVideoEvents.Columns.Add("videoevent_duration", typeof(string));
                            dtShiftedVideoEvents.Columns.Add("videoevent_origduration", typeof(string));
                            foreach (var item in serverShiftedVideoEvents)
                            {
                                var row = dtShiftedVideoEvents.NewRow();
                                row["videoevent_serverid"] = item.videoevent_id;
                                row["videoevent_start"] = item.videoevent_start;
                                row["videoevent_end"] = item.videoevent_end;
                                row["videoevent_duration"] = item.videoevent_duration;
                                row["videoevent_origduration"] = item.videoevent_origduration;
                                dtShiftedVideoEvents.Rows.Add(row);
                            }
                            DataManagerSqlLite.ShiftVideoEvents(dtShiftedVideoEvents);
                        }
                    }
                    else
                    {
                        // Do Nothing
                    }
                }
                else
                {
                    // Shift the callouts as well by equal duration
                    //Step 2: Find all videoevents, which needs to be shifted
                    var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyEndTime(selectedProjectEvent.projdetId, videoevent.videoevent_end, EnumTrack.CALLOUT1);
                    // Step-3 Call server API to shift video event and then save locally the shifted events
                    if (tobeShiftedVideoEvents?.Count > 0)
                    {
                        var tobeServerShiftedVideoEvents = new List<ShiftVideoEventModel>();
                        foreach (var item in tobeShiftedVideoEvents)
                        {
                            var model = new ShiftVideoEventModel
                            {
                                videoevent_id = (int)item.videoevent_serverid,
                                videoevent_duration = item.videoevent_duration,
                                videoevent_origduration = item.videoevent_origduration,
                                videoevent_end = DataManagerSqlLite.ShiftLeft(item.videoevent_end, videoevent.videoevent_duration),
                                videoevent_start = DataManagerSqlLite.ShiftLeft(item.videoevent_start, videoevent.videoevent_duration)
                            };
                            tobeServerShiftedVideoEvents.Add(model);
                        }
                        var serverShiftedVideoEvents = await MediaEventHandlerHelper.ShiftVideoEventsToServer(selectedProjectEvent, tobeServerShiftedVideoEvents, authApiViewModel);

                        var dtShiftedVideoEvents = new DataTable();
                        dtShiftedVideoEvents.Columns.Add("videoevent_serverid", typeof(Int64));
                        dtShiftedVideoEvents.Columns.Add("videoevent_start", typeof(string));
                        dtShiftedVideoEvents.Columns.Add("videoevent_end", typeof(string));
                        dtShiftedVideoEvents.Columns.Add("videoevent_duration", typeof(string));
                        dtShiftedVideoEvents.Columns.Add("videoevent_origduration", typeof(string));
                        foreach (var item in serverShiftedVideoEvents)
                        {
                            var row = dtShiftedVideoEvents.NewRow();
                            row["videoevent_serverid"] = item.videoevent_id;
                            row["videoevent_start"] = item.videoevent_start;
                            row["videoevent_end"] = item.videoevent_end;
                            row["videoevent_duration"] = item.videoevent_duration;
                            row["videoevent_origduration"] = item.videoevent_origduration;
                            dtShiftedVideoEvents.Rows.Add(row);
                        }
                        DataManagerSqlLite.ShiftVideoEvents(dtShiftedVideoEvents);
                    }
                }
            }
            else if (videoevent?.videoevent_track == 3 || videoevent?.videoevent_track == 4)
                await ShiftEventsHelper.DeleteAndShiftEvent(videoeventLocalId, videoevent.videoevent_serverid, isShift: false, EnumTrack.CALLOUT1, videoevent.videoevent_duration, videoevent.videoevent_end, selectedProjectEvent, authApiViewModel);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Edit Design form is called
        /// </summary>
        private async void TimelineUserConrol_EditFormEvent(object sender, int editVideoeventLocalId)
        {
            var editVideoEvent = DataManagerSqlLite.GetVideoEventbyId(editVideoeventLocalId, true, false).FirstOrDefault();
            if (editVideoEvent != null)
            {
                if (editVideoEvent.videoevent_track == (int)EnumTrack.CALLOUT1 || editVideoEvent.videoevent_track == (int)EnumTrack.CALLOUT2)
                {
                    LoaderHelper.ShowLoader(this, loader, "Edit Callout Window is opened ..");
                    await FormHandlerHelper.EditCallOut(editVideoeventLocalId, selectedProjectEvent, authApiViewModel, this, loader);
                    Refresh();
                    LoaderHelper.HideLoader(this, loader);
                }
                else if (editVideoEvent.videoevent_track == (int)EnumTrack.IMAGEORVIDEO)
                {
                    var isPlaceholder = DataManagerSqlLite.IsPlaceHolderEvent(editVideoeventLocalId);
                    if (editVideoEvent.fk_videoevent_media == (int)EnumMedia.VIDEO || editVideoEvent.fk_videoevent_media == (int)EnumMedia.IMAGE || (editVideoEvent.fk_videoevent_media == (int)EnumMedia.FORM && isPlaceholder))
                    {

                        LoaderHelper.ShowLoader(this, loader, "Edit event Window is opened ..");
                        var uc = new ScreenRecorderWindowManager();
                        var placeholderWindow = uc.CreateWindowWithPlaceHolder(selectedProjectEvent, editVideoeventLocalId);

                        uc.ScreenRecorder_BtnReplaceEvent += async (PlaceholderEvent placeholderEvent) =>
                        {
                            LoaderHelper.ShowLoader(this, loader, $"Deleting event with id - {editVideoeventLocalId} and shifting other events");

                            // Logic Here
                            var videoevent = DataManagerSqlLite.GetVideoEventbyId(editVideoeventLocalId, true, false).FirstOrDefault();

                            // Step-1 Delete the existing event and shift afterwards event
                            await ShiftEventsHelper.DeleteAndShiftEvent(editVideoeventLocalId, videoevent.videoevent_serverid, isShift: true, EnumTrack.IMAGEORVIDEO, videoevent.videoevent_duration, videoevent.videoevent_end, selectedProjectEvent, authApiViewModel);

                            LoaderHelper.ShowLoader(this, loader, $"saving {placeholderEvent?.newEventsDT?.Rows.Count} events ..");

                            // Step-2 Fetch next events of Added event
                            var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyStartTime(selectedProjectEvent.projdetId, videoevent.videoevent_start);

                            // Step-3 Add new element(s)
                            await AddVideoEvent_Clicked(placeholderEvent?.newEventsDT);


                            // Step-4 Call server API to shift video event and then save locally the shifted events
                            await ShiftEventsHelper.ShiftRight(tobeShiftedVideoEvents, placeholderEvent.newDuration.ToString(@"hh\:mm\:ss\.fff"), selectedProjectEvent, authApiViewModel);


                            LoaderHelper.HideLoader(this, loader);

                            //Step-5: Finally refresh the timeline
                            Refresh();
                        };

                        placeholderWindow.Closed += (se, ev) => 
                        {
                            placeholderWindow = null;
                            Refresh();
                        };
                        placeholderWindow.Show();
                        LoaderHelper.HideLoader(this, loader);

                    }
                    else if (editVideoEvent.fk_videoevent_media == (int)EnumMedia.FORM)
                    {
                        LoaderHelper.ShowLoader(this, loader, "Edit event Window is opened ..");
                        await FormHandlerHelper.EditFormEvent(editVideoeventLocalId, selectedProjectEvent, authApiViewModel, this, loader);
                        Refresh();
                        LoaderHelper.HideLoader(this, loader);
                    }

                }
            }
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Event is deleted
        /// </summary>
        private async void TimelineUserConrol_DeleteEventOnTimelines(object sender, int videoeventLocalId)
        {
            LoaderHelper.ShowLoader(this, loader, $"Deleting Event & shifting other events");
            undoVideoEventId = videoeventLocalId; // Very Important to set this for undo delete
            await HandleDeleteLogicForVideoEvent(videoeventLocalId);
            TimelineUserConrol.EnableUndoDelete(undoVideoEventId);
            Refresh();
            LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// Event ==> TimelineUserConrol ==> Event is undeleted (undo)
        /// </summary>
        private async void TimelineUserConrol_UndeleteDeletedEvent_Clicked(object sender, EventArgs e)
        {
            var canUndelete = ShiftEventsHelper.CheckIfUndeleteCanbeDone(undoVideoEventId, TimelineUserConrol);
            if (canUndelete)
            {
                LoaderHelper.ShowLoader(this, loader, $"Undeleting Event & shifting other events");

                //Logic Here
                var videoevents = DataManagerSqlLite.GetVideoEventbyId(undoVideoEventId, false, false);
                var videoevent = videoevents.FirstOrDefault();


                if (videoevent?.videoevent_track == 2)
                    await ShiftEventsHelper.UndeleteAndShiftEvent(undoVideoEventId, videoevent: videoevent, isShift: true, selectedProjectEvent, authApiViewModel);
                else if (videoevent?.videoevent_track == 3 || videoevent?.videoevent_track == 4)
                    await ShiftEventsHelper.UndeleteAndShiftEvent(undoVideoEventId, videoevent: videoevent, isShift: false, selectedProjectEvent, authApiViewModel);


                undoVideoEventId = -1; // Very Important to set this for undo delete
                TimelineUserConrol.DisableUndoDeleteAndReset();
                LoaderHelper.HideLoader(this, loader);

                Refresh();

            }

        }


        #region == Add Video/Image Event ==

        /// <summary>
        /// Logic when new videoevent is deleted
        /// </summary>
        private async Task AddVideoEvent_Clicked(DataTable dataTable)
        {
            // We need to insert the Data to server here and once it is success, then to local DB
            foreach (DataRow row in dataTable.Rows)
                await ProcessVideoSegmentDataRowByRow(row);
        }

        /// <summary>
        /// Logic to process video segment data one by one as there can be N events
        /// </summary>
        private async Task<int> ProcessVideoSegmentDataRowByRow(DataRow row)
        {
            DataTable dtNotes = null;
            if (row != null && row["videoevent_notes"] != DBNull.Value)
                dtNotes = (DataTable)row["videoevent_notes"];
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerForVideoOrImage(row, dtNotes, selectedProjectEvent, authApiViewModel);
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await ProcessVideoSegmentDataRowByRow(row);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForSaveImageorVideo(row);
                else
                    return -1;
            }
            else
                return SuccessFlowForSaveImageorVideo(row, addedData);
        }

        /// <summary>
        /// Logic when new videoevent is added and success result from server is returned
        /// </summary>
        private int SuccessFlowForSaveImageorVideo(DataRow row, VideoEventResponseModel addedData)
        {
            var insertedVideoSegmentId = -1;
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImage(addedData, selectedProjectEvent.projdetId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var videoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImage(blob, videoEventId, addedData.videosegment);
                insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, addedData.videoevent.videoevent_id);
                if (insertedVideoSegmentId > 0)
                {
                    Refresh();
                }
                if (addedData?.notes?.Count > 0)
                {
                    DataManagerSqlLite.InsertRowsToNotes(NotesDataTableToSave(addedData?.notes, videoEventId));
                }
            }
            return insertedVideoSegmentId;
        }

        /// <summary>
        /// Logic when new videoevent is added and failure result from server is returned
        /// </summary>
        private int FailureFlowForSaveImageorVideo(DataRow row)
        {
            // Save the record locally with server Id = temp and issynced = false
            var localServerVideoEventId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImageLocally(row, localServerVideoEventId, selectedProjectEvent.projdetId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var videoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var localServerVideoSegmentId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImageLocally(blob, videoEventId, localServerVideoSegmentId);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, videoEventId);
                if (insertedVideoSegmentId > 0)
                {
                    Refresh();
                }
            }
            return -1;
        }

        private DataTable NotesDataTableToSave(List<NotesModel> notes, int videoeventId)
        {
            var notesDataTable = new DataTable();
            notesDataTable.Columns.Add("notes_id", typeof(int));
            notesDataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notesDataTable.Columns.Add("notes_line", typeof(string));
            notesDataTable.Columns.Add("notes_wordcount", typeof(int));
            notesDataTable.Columns.Add("notes_index", typeof(int));
            notesDataTable.Columns.Add("notes_start", typeof(string));
            notesDataTable.Columns.Add("notes_duration", typeof(string));
            notesDataTable.Columns.Add("notes_createdate", typeof(string));
            notesDataTable.Columns.Add("notes_modifydate", typeof(string));
            notesDataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notesDataTable.Columns.Add("notes_issynced", typeof(bool));
            notesDataTable.Columns.Add("notes_serverid", typeof(long));
            notesDataTable.Columns.Add("notes_syncerror", typeof(string));

            notesDataTable.Rows.Clear();


            int NotesIndex = 0;
            foreach (var element in notes)
            {
                var noteRow = notesDataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = videoeventId;
                noteRow["notes_line"] = element.notes_line;
                noteRow["notes_wordcount"] = element.notes_wordcount;
                noteRow["notes_index"] = NotesIndex;
                noteRow["notes_start"] = element.notes_start;
                noteRow["notes_duration"] = element.notes_duration;
                noteRow["notes_createdate"] = element.notes_createdate;
                noteRow["notes_modifydate"] = element.notes_modifydate;
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = true;
                noteRow["notes_serverid"] = element.notes_id;
                noteRow["notes_syncerror"] = string.Empty;

                notesDataTable.Rows.Add(noteRow);
                NotesIndex++;

            }
            return notesDataTable;
        }

        #endregion == Add Video/Image Event ==

        #endregion


        #region == ContextMenu > AddImageEvent > Using CB Library ==

        /// <summary>
        /// TimelineUserConrol ==> Add Image Event From CB Library is Clicked
        /// </summary>
        private void TimelineUserConrol_AddImageEventFromCBLibrary_Clicked(object sender, string startTime)
        {
            int trackId = (int)EnumTrack.IMAGEORVIDEO;
            var mediaLibraryUserControl = new MediaLibrary_UserControl(trackId, selectedProjectEvent, authApiViewModel, startTime);
            mediaLibraryUserControl.BtnUseAndSaveClickedEventInMiddle += (MediaEventInMiddle me) => { };
            mediaLibraryUserControl.BtnUseAndSaveClickedEvent += MediaLibraryUserControl_BtnUseAndSaveClickedEvent;
            var window = new Window
            {
                Title = "Media Library",
                Content = mediaLibraryUserControl,
                WindowState = WindowState.Normal,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            LoaderHelper.ShowLoader(window, loader, "Processing...");
            var result = window.ShowDialog();
            if (result.HasValue && mediaLibraryUserControl.isEventAdded)
            {
                Refresh();
                mediaLibraryUserControl.Dispose();
            }
            else
                LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// Use and save image logic for TimelineUserConrol ==> Add Image Event From CB Library is Clicked 
        /// </summary>
        private async void MediaLibraryUserControl_BtnUseAndSaveClickedEvent(DataTable dataTable)
        {
            // We need to insert the Data to server here and once it is success, then to local DB
            foreach (DataRow row in dataTable.Rows)
                await ProcessVideoSegmentDataRowByRow(row);
            LoaderHelper.HideLoader(this, loader);
        }

        #endregion


        #region == ContextMenu > OtherEvents ==

        /// <summary>
        /// Logic for TimelineUserConrol ==> Context menu ==> Save all timeline is Clicked 
        /// </summary>
        private async void TimelineUserConrol_SaveAllTimelines_Clicked(object sender, List<CBVVideoEvent> modifiedEvents)
        {
            LoaderHelper.ShowLoader(this, loader, "Processing ...");
            foreach (var modifiedEvent in modifiedEvents)
            {
                var response = await MediaEventHandlerHelper.UpdateVideoEventOnlyToServer(modifiedEvent, selectedProjectEvent, authApiViewModel);
                if (response != null)
                {

                    var videoEventDt = new DataTable();
                    videoEventDt.Columns.Add("videoevent_id", typeof(int));
                    videoEventDt.Columns.Add("fk_videoevent_media", typeof(int));
                    videoEventDt.Columns.Add("videoevent_track", typeof(int));
                    videoEventDt.Columns.Add("videoevent_start", typeof(string));

                    videoEventDt.Columns.Add("videoevent_duration", typeof(string));
                    videoEventDt.Columns.Add("videoevent_origduration", typeof(string));
                    videoEventDt.Columns.Add("videoevent_end", typeof(string));
                    videoEventDt.Columns.Add("videoevent_isdeleted", typeof(bool));
                    videoEventDt.Columns.Add("videoevent_issynced", typeof(bool));
                    videoEventDt.Columns.Add("videoevent_syncerror", typeof(string));

                    videoEventDt.Columns.Add("videoevent_modifyDate", typeof(string));

                    DataRow dataRow = videoEventDt.NewRow();
                    dataRow["videoevent_id"] = modifiedEvent.videoevent_id;
                    dataRow["fk_videoevent_media"] = response.fk_videoevent_media;
                    dataRow["videoevent_track"] = response.videoevent_track;
                    dataRow["videoevent_start"] = response.videoevent_start;
                    dataRow["videoevent_duration"] = response.videoevent_duration;
                    dataRow["videoevent_origduration"] = response.videoevent_origduration;
                    dataRow["videoevent_end"] = response.videoevent_end;
                    dataRow["videoevent_isdeleted"] = response.videoevent_isdeleted;
                    dataRow["videoevent_issynced"] = response.videoevent_issynced;
                    dataRow["videoevent_syncerror"] = response.videoevent_syncerror ?? string.Empty;
                    videoEventDt.Rows.Add(dataRow);
                    DataManagerSqlLite.UpdateRowsToVideoEvent(videoEventDt);
                }
            }
            Refresh();
            LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// Logic for TimelineUserConrol ==> Timeline ==> When selected video event is changed 
        /// </summary>
        private void TimelineUserConrol_VideoEventSelectionChangedEvent(object sender, TimelineSelectedEvent selectedEvent)
        {
            if (selectedEvent.Track == EnumTrack.NOTES)
            {
                selectedVideoEvent = null;
            }
            else
            {
                selectedVideoEvent = DataManagerSqlLite.GetVideoEventbyId(selectedEvent.EventId).FirstOrDefault();
            }

            if (selectedVideoEvent != null)
            {
                selectedVideoEventId = selectedEvent.EventId;
                lblSelectedVideoeventId.Content = $"[Selected VideoEvent Id - {selectedVideoEventId}]";
                CommentsHelper.ShowComments(selectedVideoEventId, commentsBlock);
                CommentsHelper.ShowNotes(selectedVideoEventId, notesBlock);
            }
            else
            {
                selectedVideoEventId = -1;
                lblSelectedVideoeventId.Content = $"";
                CommentsHelper.ShowComments(selectedVideoEventId, commentsBlock);
                CommentsHelper.ShowNotesById(selectedEvent.EventId, notesBlock);
            }

            //NotesUserConrol.HandleVideoEventSelectionChanged(selectedVideoEventId);
        }

        /*private bool IfNeedToReProcess(TrackbarMouseMoveEvent e)
        //{
        //    if (e != null && e.isAnyVideo) return true;
        //    if (e?.videoeventIds?.Count != mouseEventToProcess?.videoeventIds?.Count) return true;
        //    foreach (var id in e?.videoeventIds)
        //    {
        //        if (mouseEventToProcess?.videoeventIds?.Contains(id) == false) return true;
        //    }
        //    return false;
        //}
        */

        /// <summary>
        /// Logic for TimelineUserConrol ==> Timeline ==> When tracbar mouse is moved
        /// </summary>
        private void TimelineUserConrol_TrackbarMouseMoveEvent(object sender, ManageMedia_UserControl.Models.TrackbarMouseMoveEventModel _mouseEventToProcess)
        {
            mouseEventToProcess = _mouseEventToProcess;
            if (_mouseEventToProcess != null)
            {
                PreviewUserControl.Process(mouseEventToProcess);
            }
        }

        #endregion


        #region == ContextMenu > CloneEvent ==

        /// <summary>
        /// Logic for TimelineUserConrol ==> Context menu ==> Clone Event is Clicked 
        /// </summary>
        private async void TimelineUserConrol_ContextMenu_CloneEvent_Clicked(object sender, FormOrCloneEvent payload)
        {
            LoaderHelper.ShowLoader(this, loader);
            int i = 1;
            var showErrorMessage = false;

            var message = string.Empty;

            var videoEventList = DataManagerSqlLite.GetVideoEventbyId(payload.timelineVideoEvent.VideoEventID, true, false);
            var dtVideoSegment = CloneEventHandlerHelper.GetVideoSegmentDataTableClient(videoEventList[0].videosegment_data, selectedProjectEvent.serverProjectId, -1);
            var blob = CloneEventHandlerHelper.GetBlobBytes(dtVideoSegment);
            var videoEventResponse = await CloneEventHandlerHelper.PostVideoEventToServerForClone(videoEventList, blob, selectedProjectEvent, authApiViewModel, payload.timeAtTheMoment);

            if (videoEventResponse != null)
            {
                message += $"{i++}. Successfully cloned event to server !!" + Environment.NewLine;

                var dtVideoEvent = CloneEventHandlerHelper.GetVideoEventDataTableServer(videoEventResponse, selectedProjectEvent.projdetId);
                var videoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dtVideoEvent, false);
                var insertedVideoEventId = videoEventIds?.Count > 0 ? videoEventIds[0] : -1;
                if (insertedVideoEventId > 0)
                {
                    message += $"{i++}. Successfully saved cloned videoevent to local DB !!" + Environment.NewLine;


                    // Insert VideoSegment
                    if (videoEventResponse.videosegment != null)
                    {
                        var dtVS = CloneEventHandlerHelper.GetVideoSegmentDataTableServer(blob, videoEventResponse.videosegment, insertedVideoEventId);
                        var insertedVSId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVS, insertedVideoEventId);
                        if (insertedVSId > 0)
                            message += $"{i++}. Successfully saved cloned videosegment to local DB !!" + Environment.NewLine;
                    }

                    // Insert Design
                    if (videoEventResponse.design?.Count > 0)
                    {
                        var dtDesigns = CloneEventHandlerHelper.GetDesignDataTableServer(videoEventResponse.design, insertedVideoEventId);
                        var insertedDesignsIdLast = DataManagerSqlLite.InsertRowsToDesign(dtDesigns);
                        if (insertedDesignsIdLast > 0)
                            message += $"{i++}. Successfully saved cloned design [{dtDesigns?.Rows?.Count}] to local DB !!" + Environment.NewLine;
                    }

                    // Insert Notes
                    if (videoEventResponse.notes?.Count > 0)
                    {
                        var dtNotes = CloneEventHandlerHelper.GetNotesDataTableServer(videoEventResponse.notes, insertedVideoEventId);
                        var insertedNotesIdLast = DataManagerSqlLite.InsertRowsToNotes(dtNotes);
                        if (insertedNotesIdLast > 0)
                            message += $"{i++}. Successfully saved cloned notes [{dtNotes?.Rows?.Count}] to local DB !!" + Environment.NewLine;
                    }
                }
                else
                {
                    showErrorMessage = true || showErrorMessage;
                    message += $"{i++}. Failed in saving cloned videoevent to local DB !!" + Environment.NewLine;
                }
            }
            else
            {
                showErrorMessage = true || showErrorMessage;
                message += $"{i++}. Failed in cloning event to server !!" + Environment.NewLine;
            }
            if (showErrorMessage)
            {
                LoaderHelper.HideLoader(this, loader);
                MessageBox.Show($"Something went wrong, please see below for details {Environment.NewLine} {message}", "Failure !!!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                LoaderHelper.HideLoader(this, loader);
                Refresh();
                MessageBox.Show($"Successfully Cloned Event, please see below for details {Environment.NewLine}{Environment.NewLine}{message}", "Success !!!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            LoaderHelper.HideLoader(this, loader);
        }

        #endregion


        #region == Design/Form Context Menu Option ==

        /// <summary>
        /// Logic for TimelineUserConrol ==> ContextMenu > Add Callout 1/ callout 2 or Form is Clicked
        /// </summary>
        private async Task HandleFormLogic(FormOrCloneEvent calloutEvent, EnumTrack track, string imagePath, bool isFormEvent)
        {
            await FormHandlerHelper.CallOut(calloutEvent, "Designer", selectedProjectEvent, authApiViewModel, track, this, loader, imagePath, isFormEvent);
            Refresh();
            LoaderHelper.HideLoader(this, loader);
        }

        /// <summary>
        /// TimelineUserConrol ==> ContextMenu > Add Callout 1 is Clicked
        /// </summary>
        private async void TimelineUserConrol_ContextMenu_AddCallout1_Clicked(object sender, FormOrCloneEvent calloutEvent)
        {
            LoaderHelper.ShowLoader(this, loader, "Callout Window is opened ..");
            if (calloutEvent.timelineVideoEvent != null && calloutEvent.timeAtTheMoment != "00:00:00.000")
            {
                var convertedImage = await FormHandlerHelper.PreprocessAndGetBackgroundImage(calloutEvent);
                await HandleFormLogic(calloutEvent, EnumTrack.CALLOUT1, convertedImage, false);
            }
            else
                await HandleFormLogic(calloutEvent, EnumTrack.CALLOUT1, null, false);
        }

        /// <summary>
        /// TimelineUserConrol ==> ContextMenu > Add Callout 2 is Clicked
        /// </summary>
        private async void TimelineUserConrol_ContextMenu_AddCallout2_Clicked(object sender, FormOrCloneEvent calloutEvent)
        {
            LoaderHelper.ShowLoader(this, loader, "Callout Window is opened ..");
            if (calloutEvent.timelineVideoEvent != null && calloutEvent.timeAtTheMoment != "00:00:00.000")
            {
                var convertedImage = await FormHandlerHelper.PreprocessAndGetBackgroundImage(calloutEvent);
                await HandleFormLogic(calloutEvent, EnumTrack.CALLOUT2, convertedImage, false);
            }
            else
                await HandleFormLogic(calloutEvent, EnumTrack.CALLOUT2, null, false);
        }

        /// <summary>
        /// TimelineUserConrol ==> ContextMenu > Add Form event is Clicked
        /// </summary>
        private async void TimelineUserConrol_ContextMenu_AddFormEvent_Clicked(object sender, FormOrCloneEvent calloutEvent)
        {
            LoaderHelper.ShowLoader(this, loader, "Form Window is opened ..");
            await HandleFormLogic(calloutEvent, EnumTrack.IMAGEORVIDEO, null, true);
        }

        /* Add Form Event in Middle
        private void TimelineUserConrol_AddFormEventInMiddle_Clicked(object sender, FormOrCloneEvent e)
        {
            //LoaderHelper.ShowLoader(this, loader, "Form Window is opened ..");
            throw new NotImplementedException();
        }

        private void TimelineUserConrol_AddImageEventUsingCBLibraryInMiddle_Clicked(object sender, FormOrCloneEvent payload)
        {
            //var mediaLibraryUserControl = new MediaLibrary_UserControl((int)EnumTrack.IMAGEORVIDEO, selectedProjectEvent, authApiViewModel, payload.timeAtTheMoment, payload.timelineVideoEvent);

            //mediaLibraryUserControl.BtnUseAndSaveClickedEvent += (DataTable dt) => { };
            //mediaLibraryUserControl.BtnUseAndSaveClickedEventInMiddle += MediaLibraryUserControl_BtnUseAndSaveClickedEventInMiddle;
            //var window = new Window
            //{
            //    Title = "Media Library",
            //    Content = mediaLibraryUserControl,
            //    WindowState = WindowState.Normal,
            //    ResizeMode = ResizeMode.CanResize,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen
            //};
            //LoaderHelper.ShowLoader(window, loader, "Processing...");
            //var result = window.ShowDialog();
            //if (result.HasValue && mediaLibraryUserControl.isEventAdded)
            //{
            //    Refresh();
            //    mediaLibraryUserControl.Dispose();
            //}
            //else
            //    LoaderHelper.HideLoader(this, loader);
        }
        
        
        private async void MediaLibraryUserControl_BtnUseAndSaveClickedEventInMiddle(MediaEventInMiddle mediaEventInMiddle)
        {
            var shiftEventLocation = mediaEventInMiddle.shiftVideoEventLocation;
            var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyStartTime(selectedProjectEvent.projdetId, shiftEventLocation.videoevent_end);
            foreach (DataRow row in mediaEventInMiddle?.datatable.Rows)
            {
                var insertedVideoeventId = await ProcessVideoSegmentDataRowByRow(row);
                if (insertedVideoeventId > 0 && shiftEventLocation.videoevent_track == (int)EnumTrack.IMAGEORVIDEO)
                {
                    // ShiftAll pending events
                    await ShiftEventsHelper.ShiftRight(tobeShiftedVideoEvents, Convert.ToString(row["videoevent_duration"]), selectedProjectEvent, authApiViewModel);
                }
                Refresh();
            }
            LoaderHelper.HideLoader(this, loader);
        }
        */

        #endregion

        private async void btnUploadNotSyncedData_Click(object sender, RoutedEventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader, "Processing in Background");
            var result = await BackgroundProcessHelper.PeriodicallyCheckOfflineDataAndSync(true);
            if (result != null && result == true)
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data";
                btnUploadNotSyncedData.Width = 160;
                btnUploadNotSyncedData.IsEnabled = false;
                btnDownloadServerData.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Something went wrong, please try later", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoaderHelper.HideLoader(this, loader);
        }


        /// <summary>
        /// Logic for Downloading server video events when local DB does not have the event(s)
        /// </summary>
        private async void btnDownloadServer_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show($"This will overwrite all local changes and server data will be synchronised to local DB", "Please confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                await SyncServerEventsHelper.SyncServerDataToLocalDB(null, this, btnDownloadServerData, selectedProjectEvent, loader, authApiViewModel);
                Refresh();
            }
        }

        /// <summary>
        /// Called when Dispose is called
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
