using ManageMedia_UserControl;
using ManageMedia_UserControl.Classes;
using ManageMedia_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Loader;
using ManageMedia = ManageMedia_UserControl.Classes.Media;
using SCModels = ScreenRecorder_UserControl.Models;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public class ManageMediaWindowManager
    {
        Window _Window;
        ManageMedia_Control _ManageMedia;
        SelectedProjectEvent selectedProjectEvent;
        public LoadingAnimation loader;
        bool WasDataSaved = false;
        bool ReadOnly = false;
        private AuthAPIViewModel authApiViewModel;

        public Window CreateWindow(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, bool readOnly = false)
        {
            ReadOnly = readOnly;
            authApiViewModel = _authApiViewModel;
            _ManageMedia = new ManageMedia_Control(ReadOnly: ReadOnly);
            selectedProjectEvent = _selectedProjectEvent;

            //ProjectID = Selected_ID;

            //if (Selected_ID != -1)
            if (selectedProjectEvent != null)
            {
                _ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);

                RefreshData();

                //Events Go Here
                _ManageMedia.CloseWindow += _ManageMedia_CloseWindow;
                _ManageMedia.ManageMediaSave += _ManageMedia_ManageMediaSave;
                _ManageMedia.SaveTimeline_Clicked += async (object sender, List<CBVVideoEvent> modifiedEvents) =>
                {
                    await _ManageMedia_SaveTimeline_Clicked(_ManageMedia, modifiedEvents);
                    _ManageMedia.ResetCalloutLocationOrSizeChangedMedia();
                };
            }
            _Window = new Window
            {
                Width = 1200,
                Height = 620,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = AddLoaderAndReturnContent(),
            };
            _Window.Closing += _Window_Closing;
            //window.Show();
            return _Window;
        }

        private async Task _ManageMedia_SaveTimeline_Clicked(ManageMedia_Control _manageMedia, List<CBVVideoEvent> modifiedEvents)
        {
            LoaderHelper.ShowLoader(_manageMedia, loader, "Processing ...");
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
            RefreshData();
            LoaderHelper.HideLoader(_manageMedia, loader);
        }
    

        private Grid AddLoaderAndReturnContent()
        {
            loader = new LoadingAnimation
            {
                Name = "loader",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden,
            };
            var grid = new Grid
            {
                Name = "parentGrid",
            };
            grid.Children.Add(_ManageMedia);
            grid.Children.Add(loader);
            return grid;
        }

        public bool? ShowWindow()
        {
            LoaderHelper.HideLoader(_Window, loader);
            var result = _Window.ShowDialog();
            return result;
        }

        #region == User Triggered Events ==

        private void _ManageMedia_CloseWindow(object sender, EventArgs e)
        {
            WasDataSaved = true;
            _Window.Close();
        }

        private void _Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (WasDataSaved == false)
            {
                bool Saved = _ManageMedia.ShowClosingPrompt();

                if (Saved != true)
                {
                    e.Cancel = true;
                }
            }
        }

        private void _ManageMedia_ManageMediaSave(object sender, ManageMediaSaveEventArgs e)
        {
            LoaderHelper.ShowLoader(_Window, loader);
            List<Media> CreatedVideoEvents = e.CreatedVideoEvents;
            List<Media> DeletedVideoEvents = e.DeletedVideoEvents;
            List<Media> ChangedVideoEvents = e.ChangedVideoEvents;
            List<(SCModels.TextItem TextItem, int VideoEventID)> CreatedNotes = e.CreatedNotes;
            List<SCModels.TextItem> DeletedNotes = e.DeletedNotes;
            List<SCModels.TextItem> ChangedNotes = e.ChangedNotes;

            //Delete Video Events ----------------------
            if (DeletedVideoEvents?.Count > 0)
                foreach (Media media in DeletedVideoEvents)
                {
                    ManageMedia_DeletedVideoEvents.Invoke(media.VideoEventID);
                    //DeleteVideoEvent(media);
                }

            //Delete Notes -----------------------------
            //foreach (TextItem textItem in DeletedNotes)
            //    DeletedNote(textItem);
            var deletedNotesIds = DeletedNotes.Select(x => x.NoteID).ToList();
            if (deletedNotesIds?.Count > 0)
                ManageMedia_NotesDeletedEvent.Invoke(deletedNotesIds);

            //Change Video Events
            //foreach (Media media in ChangedVideoEvents)
            //    ChangeVideoEvent(ChangedVideoEvents);
            if (ChangedVideoEvents?.Count > 0)
                ChangeVideoEvent(ChangedVideoEvents);

            //Change Notes
            if (ChangedNotes?.Count > 0)
                ChangeNotes(ChangedNotes);

            //Create Video Events
            //foreach (Media media in CreatedVideoEvents)
            //    CreateVideoEvent(media);
            if (CreatedVideoEvents?.Count > 0)
                CreateVideoEvent(CreatedVideoEvents);

            //Create Notes - This list only contains notes created on existing VideoEvents
            if (CreatedNotes?.Count > 0)
                CreateNotes(CreatedNotes);


            if (e.CloseOnSave)
            {
                WasDataSaved = true;
                _Window.Close();
            }
            else
            {
                WasDataSaved = true;
                LoaderHelper.HideLoader(_Window, loader);
                //_ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);
                //RefreshData();
            }
        }

        #endregion

        #region == VideoEvent helper Functions
        private void CreateVideoEvent(List<Media> CreatedVideoEvents)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("videoevent_id", typeof(int));
            dataTable.Columns.Add("fk_videoevent_projdet", typeof(int));
            dataTable.Columns.Add("fk_videoevent_media", typeof(int));
            dataTable.Columns.Add("videoevent_track", typeof(int));
            dataTable.Columns.Add("videoevent_start", typeof(string));
            dataTable.Columns.Add("videoevent_duration", typeof(string));
            dataTable.Columns.Add("videoevent_origduration", typeof(string));
            dataTable.Columns.Add("videoevent_createdate", typeof(string));
            dataTable.Columns.Add("videoevent_modifydate", typeof(string));
            dataTable.Columns.Add("media", typeof(byte[])); // Media Column
            dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen
            dataTable.Columns.Add("videoevent_isdeleted", typeof(bool));
            dataTable.Columns.Add("videoevent_issynced", typeof(bool));
            dataTable.Columns.Add("videoevent_serverid", typeof(Int64));
            dataTable.Columns.Add("videoevent_syncerror", typeof(string));

            dataTable.Columns.Add("videoevent_notes", typeof(DataTable));

            // Since this table has Referential Integrity, so lets push one by one

            foreach (var media in CreatedVideoEvents)
            {
                var row = dataTable.NewRow();
                //row["videoevent_id"] = -1;
                row["fk_videoevent_projdet"] = media.ProjectId;
                row["videoevent_track"] = media.TrackId;
                row["videoevent_start"] = media.StartTime.ToString(@"hh\:mm\:ss\.fff");
                row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_isdeleted"] = false;
                row["videoevent_issynced"] = false;
                row["videoevent_serverid"] = 100;
                row["videoevent_syncerror"] = string.Empty;

                var mediaId = 0;

                if (media.mediaType == EnumMedia.IMAGE)
                {
                    mediaId = 1;
                }

                if (media.mediaType == EnumMedia.VIDEO)
                {
                    mediaId = 2;
                }

                row["videoevent_duration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");
                row["videoevent_origduration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");

                row["fk_videoevent_media"] = mediaId;

                row["fk_videoevent_screen"] = -1; // Not needed for this case

                row["media"] = media.mediaData;

                dataTable.Rows.Add(row);

                //List<int> InsertedIDs = CreateMediaEvents(dataTable);

                //if (InsertedIDs.Count > 0)
                //{
                //    SaveNotes(InsertedIDs[0], media);
                //}
            }
            ManageMedia_AddVideoEvents.Invoke(dataTable); //Added by KG
        }

        private void ChangeVideoEvent(List<Media> changedMedia)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("videoevent_id", typeof(int));
            dataTable.Columns.Add("videoevent_serverid", typeof(int));
            dataTable.Columns.Add("videoevent_start", typeof(string));
            dataTable.Columns.Add("videoevent_duration", typeof(string));
            dataTable.Columns.Add("videoevent_origduration", typeof(string));
            dataTable.Columns.Add("videoevent_end", typeof(string));
            dataTable.Columns.Add("videoevent_modifydate", typeof(string));
            dataTable.Rows.Clear();

            foreach (var media in changedMedia)
            {
                var ItemToAdjust = DataManagerSqlLite.GetVideoEventbyId(media.VideoEventID).FirstOrDefault();

                if (ItemToAdjust != null)
                {
                    var row = dataTable.NewRow();
                    row["videoevent_id"] = ItemToAdjust.videoevent_id;
                    row["videoevent_serverid"] = ItemToAdjust.videoevent_serverid;
                    row["videoevent_start"] = media.StartTime.ToString(@"hh\:mm\:ss\.fff");
                    row["videoevent_duration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");
                    if (media.mediaType != EnumMedia.VIDEO)
                    {
                        row["videoevent_origduration"] = media.OriginalDuration.ToString(@"hh\:mm\:ss\.fff");
                    }
                    else
                    {
                        row["videoevent_origduration"] = ItemToAdjust.videoevent_origduration;
                    }
                    row["videoevent_end"] = (media.StartTime + media.Duration).ToString(@"hh\:mm\:ss\.fff");
                    row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }
            }
            ManageMedia_AdjustVideoEvents.Invoke(dataTable);
        }

        #endregion

        #region == Notes helper Functions

        private void CreateNotes(List<(ScreenRecorder_UserControl.Models.TextItem TextItem, int VideoEventID)> CreatedNotes)
        {
            var notesIndex = new Dictionary<int, int>();
            foreach (var videoeventId in CreatedNotes.Select(x => x.VideoEventID).Distinct())
            {
                notesIndex.Add(videoeventId, DataManagerSqlLite.GetMaxIndexForNotes(videoeventId));
            }
            var notedataTable = GetNotesRawTable();
            foreach (var CreatedNote in CreatedNotes)
            {
                var noteRow = notedataTable.NewRow();
                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = CreatedNote.VideoEventID;// DataManagerSqlLite.GetVideoEventbyId(CreatedNote.VideoEventID, false, false)[0].videoevent_id;
                noteRow["notes_line"] = CreatedNote.TextItem.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = CreatedNote.TextItem.WordCount;
                noteRow["notes_index"] = notesIndex[CreatedNote.VideoEventID];
                noteRow["notes_start"] = CreatedNote.TextItem.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = CreatedNote.TextItem.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = true;
                noteRow["notes_serverid"] = -1;
                noteRow["notes_syncerror"] = string.Empty;
                notedataTable.Rows.Add(noteRow);
                notesIndex[CreatedNote.VideoEventID]++;

            }
            ManageMedia_NotesCreatedEvent.Invoke(notedataTable);
        }

        private void ChangeNotes(List<SCModels.TextItem> textItems)
        {
            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("fk_notes_videoevent", typeof(long));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));

            foreach (var ChangedElement in textItems)
            {
                var note = DataManagerSqlLite.GetNotesbyId(ChangedElement.NoteID).FirstOrDefault();

                notedataTable.Rows.Clear();
                var noteRow = notedataTable.NewRow();

                noteRow["notes_id"] = ChangedElement.NoteID;
                noteRow["notes_line"] = ChangedElement.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = ChangedElement.WordCount;
                noteRow["notes_index"] = note.notes_index;
                noteRow["notes_start"] = ChangedElement.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = ChangedElement.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_serverid"] = note.notes_serverid;
                noteRow["notes_issynced"] = true;
                noteRow["notes_syncerror"] = string.Empty;
                noteRow["fk_notes_videoevent"] = note.fk_notes_videoevent;
                notedataTable.Rows.Add(noteRow);
            }
            ManageMedia_NotesChangedEvent.Invoke(notedataTable);
        }

        #endregion

        public void SetProjectID(int projectID)
        {
            //_ProjectID = projectID;
            RefreshData();
        }

        public void RefreshData()
        {
            #region Planned Text

            //List<CBVScreen> screens = DataManagerSqlLite.GetScreens();
            //List<CBVPlanning> PlanningData = DataManagerSqlLite.GetPlanning(selectedProjectEvent.projectId);
            //if (screens?.Count > 0 && PlanningData?.Count > 0)
            //{
            //    List<PlannedTextGroup> PlannedTextGroups = new List<PlannedTextGroup>();
            //    for (int i = 0; i < screens.Count; i++)
            //    {
            //        PlannedTextGroup PlannedGroup = new PlannedTextGroup()
            //        {
            //            GroupID = screens[i].screen_id,
            //            GroupName = screens[i].screen_name,
            //            PlannedTexts = new List<PlannedText>()
            //        };
            //        PlannedTextGroups.Add(PlannedGroup);
            //    }

            //    for (int i = 0; i < PlanningData.Count; i++)
            //    {
            //        CBVPlanning CurrentPlanning = PlanningData[i];
            //        for (int x = 0; x < PlannedTextGroups.Count; x++)
            //        {
            //            PlannedTextGroup CurrentGroup = PlannedTextGroups[x];
            //            if (CurrentPlanning.fk_planning_screen == CurrentGroup.GroupID)
            //            {
            //                CurrentGroup.PlannedTexts.Add(
            //                new PlannedText()
            //                {
            //                    PlannedTextID = CurrentPlanning.planning_id,
            //                    Text = CurrentPlanning.planning_notesline,
            //                    SortKey = CurrentPlanning.planning_sort
            //                });

            //                //if (CurrentPlanning.planning_mediathumb != null && CurrentPlanning.planning_mediafull != null)
            //                //{
            //                //    CurrentGroup.Images.Add(new PlannedImage()
            //                //    {
            //                //        PlannedTextID = CurrentPlanning.planning_id,
            //                //        Image = CurrentPlanning.planning_mediafull,
            //                //        SortKey = CurrentPlanning.planning_sort,
            //                //        ThumbNail = CurrentPlanning.planning_mediathumb,
            //                //    });
            //                //}

            //                if (CurrentPlanning.planning_suggestnotesline != "" && CurrentPlanning.planning_suggestnotesline != null)
            //                {
            //                    CurrentGroup.SuggestedText.Add(CurrentPlanning.planning_suggestnotesline);
            //                }
            //            }
            //        }
            //    }

            //    _ManageMedia.LoadPlannedText(PlannedTextGroups);
            //}


            #endregion

            #region Video Events

            //List<CBVMedia> MediaHead = DataManagerSqlLite.GetMedia();
            var VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true, true);
            var plannings = DataManagerSqlLite.GetPlanning(selectedProjectEvent.projectId, false);


            List<ManageMedia> mediaList = new List<Media>();
            foreach (var item in VideoEventData)
            {


                var Screens = DataManagerSqlLite.GetScreens();
                
                var media = new Media();
                //Color background = Colors.DarkRed;


                string screenColor = "#FFFFFF";
                if (item.videoevent_planning > 0)
                {
                    var screenId = plannings.Where(x => x.planning_serverid == item.videoevent_planning).FirstOrDefault()?.fk_planning_screen;
                    var screen = Screens.Where(x => x.screen_id == screenId).FirstOrDefault();
                    screenColor = screen?.screen_hexcolor;
                    media.screenType = (EnumScreen)screen?.screen_id;
                }
                else if (item.fk_design_screen > 0)
                {
                    var screen = Screens.Where(x => x.screen_id == item.fk_design_screen).FirstOrDefault();
                    screenColor = screen?.screen_hexcolor;
                    media.screenType = (EnumScreen)screen?.screen_id;
                }
                else if (item.fk_videoevent_media == (int)EnumMedia.IMAGE)
                {
                    screenColor = "#FF6347"; //Tomato Color
                    media.screenType = EnumScreen.Image;
                }
                else if (item.fk_videoevent_media == (int)EnumMedia.VIDEO)
                {
                    screenColor = "#D8BFD8"; //Thistle Color 
                    media.screenType = EnumScreen.Video;
                }
                else if (item.fk_videoevent_media == (int)EnumMedia.AUDIO)
                {
                    screenColor = "#FFFF00"; // Yellow
                    media.screenType = EnumScreen.Custom;
                }
                else if (item.fk_videoevent_media == (int)EnumMedia.FORM)
                {
                    screenColor = Screens.Where(x => x.screen_name == "custom")?.FirstOrDefault()?.screen_hexcolor;
                    media.screenType = EnumScreen.Custom;
                }

                if (item.videosegment_data != null && item.videosegment_data.Count > 0)
                {
                    media.mediaData = item.videosegment_data[0].videosegment_media;
                }

                if (item.fk_videoevent_media == 1 || item.fk_videoevent_media == 4)
                {
                    media.mediaType = EnumMedia.IMAGE;
                }
                else if (item.fk_videoevent_media == 2)
                {
                    media.mediaType = EnumMedia.VIDEO;
                }
                else if (item.fk_videoevent_media == 3)
                {
                    media.mediaType = EnumMedia.AUDIO;
                }

                media.Color = (Color)ColorConverter.ConvertFromString(screenColor); ;

                media.VideoEventID = item.videoevent_id;
                media.ProjectId = selectedProjectEvent.projectId;
                media.TrackId = item.videoevent_track;
                if (item.notes_data != null)
                {
                    List<SCModels.TextItem> textItems = new List<SCModels.TextItem>();
                    foreach (var note in item.notes_data)
                    {
                        SCModels.TextItem textItem = new SCModels.TextItem();
                        textItem.NoteID = note.notes_id;
                        textItem.Text = note.notes_line;
                        textItem.Start = TimeSpan.Parse(note.notes_start);
                        textItem.Duration = TimeSpan.Parse(note.notes_duration);

                        textItems.Add(textItem);
                    }
                    media.RecordedTextList = textItems;
                }
                media.Duration = TimeSpan.Parse(item.videoevent_duration);
                media.OriginalDuration = TimeSpan.Parse(item.videoevent_origduration);
                media.StartTime = TimeSpan.Parse(item.videoevent_start);

                mediaList.Add(media);
            }

            _ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);

            _ManageMedia.LoadEvents(mediaList);

            #endregion
        }


        #region == Added By Kumar ==
        public event Action<DataTable> ManageMedia_NotesCreatedEvent;
        public event Action<DataTable> ManageMedia_NotesChangedEvent;
        public event Action<List<int>> ManageMedia_NotesDeletedEvent;
        public event Action<int> ManageMedia_DeletedVideoEvents;
        public event Action<DataTable> ManageMedia_AdjustVideoEvents;
        public event Action<DataTable> ManageMedia_AddVideoEvents;


        private DataTable GetNotesRawTable()
        {
            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_createdate", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));
            return notedataTable;
        }

        #endregion

    }
}
