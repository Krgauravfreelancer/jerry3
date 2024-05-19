using ManageMedia_UserControl;
using ManageMedia_UserControl.Classes;
using ManageMedia_UserControl.Controls;
using ManageMedia_UserControl.Models;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using LocalVoiceGen_UserControl.Helpers;

namespace ManageMedia_Wrapper.Helpers
{
    internal class ManageMediaWindowManager
    {
        Window _Window;
        ManageMedia_Control _ManageMedia;
        int _ProjectID = -1;

        bool WasDataSaved = false;
        public Window CreateWindow(int Selected_ID)
        {
            _Window = new Window();
            _Window.Width = 1200;
            _Window.Height = 655;

            WasDataSaved = false;

            _ManageMedia = new ManageMedia_Control();

            TimeSpan MeasuredText = TextMeasurement.Measure("Hello");

            _ProjectID = Selected_ID;

            if (Selected_ID != -1)
            {
                _ManageMedia.SetProjectInfo(Selected_ID);

                RefreshData();

                //Events Go Here
                _ManageMedia.CloseWindow += _ManageMedia_CloseWindow;
                _ManageMedia.ManageMediaSave += _ManageMedia_ManageMediaSave;


            }

            _Window.Closing += _Window_Closing;

            _Window.Content = _ManageMedia;
            _Window.Show();
            return _Window;
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
            List<Media> CreatedVideoEvents = e.CreatedVideoEvents;
            List<Media> DeletedVideoEvents = e.DeletedVideoEvents;
            List<Media> ChangedVideoEvents = e.ChangedVideoEvents;
            List<(TextItem TextItem, int VideoEventID)> CreatedNotes = e.CreatedNotes;
            List<TextItem> DeletedNotes = e.DeletedNotes;
            List<TextItem> ChangedNotes = e.ChangedNotes;

            //Delete Video Events ----------------------
            foreach (Media media in DeletedVideoEvents)
            {
                DeleteVideoEvent(media);
            }

            //Delete Notes -----------------------------
            foreach (TextItem textItem in DeletedNotes)
            {
                DeletedNote(textItem);
            }

            //Change Video Events
            foreach (Media media in ChangedVideoEvents)
            {
                ChangeVideoEvent(media);
            }

            //Change Notes
            ChangeNotes(ChangedNotes);

            //Create Video Events
            foreach (Media media in CreatedVideoEvents)
            {
                CreateVideoEvent(media);
            }

            //Create Notes
            // This list only contains notes created on existing VideoEvents
            CreateNotes(CreatedNotes);



            if (e.CloseOnSave)
            {
                WasDataSaved = true;
                _Window.Close();
            }
            else
            {
                //Refresh Data In Control
                if (_ProjectID != -1)
                {
                    _ManageMedia.SetProjectInfo(_ProjectID);

                    RefreshData();
                }
            }

        }

        private void CreateNotes(List<(TextItem TextItem, int VideoEventID)> CreatedNotes)
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

            foreach (var CreatedNote in CreatedNotes)
            {
                notedataTable.Rows.Clear();
                var noteRow = notedataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = CreatedNote.VideoEventID;
                noteRow["notes_line"] = CreatedNote.TextItem.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = CreatedNote.TextItem.WordCount;
                noteRow["notes_start"] = CreatedNote.TextItem.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = CreatedNote.TextItem.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = false;
                noteRow["notes_serverid"] = 1;
                noteRow["notes_syncerror"] = string.Empty;

                notedataTable.Rows.Add(noteRow);

                CreateNote(notedataTable);
            }
        }

        private void CreateVideoEvent(Media media)
        {
            #region Add New Video Event
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

                // Since this table has Referential Integrity, so lets push one by one
                dataTable.Rows.Clear();


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

                if (media.mediaType == MediaType.Image)
                {
                    mediaId = 1;
                }

                if (media.mediaType == MediaType.Video)
                {
                    mediaId = 2;
                }

                row["videoevent_duration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");
                row["videoevent_origduration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");

                row["fk_videoevent_media"] = mediaId;

                row["fk_videoevent_screen"] = -1; // Not needed for this case

                row["media"] = media.mediaData;

                dataTable.Rows.Add(row);

                List<int> InsertedIDs = CreateMediaEvents(dataTable);

                if (InsertedIDs.Count > 0)
                {
                    SaveNotes(InsertedIDs[0], media);
                }

            }
            #endregion
        }

        private void SaveNotes(int InsertedID, Media media)
        {

            bool InsertNotes = false;

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

            notedataTable.Rows.Clear();


            int NotesIndex = 0;
            if (media.RecordedTextList != null)
            {
                foreach (var element in media.RecordedTextList)
                {
                    InsertNotes = true;
                    var noteRow = notedataTable.NewRow();

                    noteRow["notes_id"] = -1;
                    noteRow["fk_notes_videoevent"] = InsertedID;
                    noteRow["notes_line"] = element.Text.Replace("'", "''"); ;
                    noteRow["notes_wordcount"] = element.WordCount;
                    noteRow["notes_index"] = NotesIndex;
                    noteRow["notes_start"] = element.Start.ToString(@"hh\:mm\:ss\.fff");
                    noteRow["notes_duration"] = element.Duration.ToString(@"hh\:mm\:ss\.fff");
                    noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    noteRow["notes_isdeleted"] = false;
                    noteRow["notes_issynced"] = false;
                    noteRow["notes_serverid"] = 1;
                    noteRow["notes_syncerror"] = string.Empty;

                    notedataTable.Rows.Add(noteRow);
                    NotesIndex++;

                }

                if (InsertNotes == true)
                {
                    CreateNote(notedataTable);
                }
            }
        }

        private void ChangeNotes(List<TextItem> textItems)
        {
            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));

            foreach (var ChangedElement in textItems)
            {
                var note = DataManagerSqlLite.GetNotesbyId(ChangedElement.NoteID);

                notedataTable.Rows.Clear();
                var noteRow = notedataTable.NewRow();

                noteRow["notes_id"] = ChangedElement.NoteID;
                noteRow["notes_line"] = ChangedElement.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = ChangedElement.WordCount;
                noteRow["notes_index"] = note[0].notes_index;
                noteRow["notes_start"] = ChangedElement.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = ChangedElement.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_issynced"] = note[0].notes_issynced;
                noteRow["notes_serverid"] = note[0].notes_serverid;
                noteRow["notes_syncerror"] = note[0].notes_syncerror;

                notedataTable.Rows.Add(noteRow);


            }
            UpdateNotes(notedataTable);
        }

        private void ChangeVideoEvent(Media media)
        {
            #region Change Video Event
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("videoevent_id", typeof(int));
                dataTable.Columns.Add("fk_videoevent_media", typeof(int));
                dataTable.Columns.Add("videoevent_track", typeof(int));
                dataTable.Columns.Add("videoevent_start", typeof(string));
                dataTable.Columns.Add("videoevent_duration", typeof(string));
                dataTable.Columns.Add("videoevent_origduration", typeof(string));
                dataTable.Columns.Add("videoevent_end", typeof(string));
                dataTable.Columns.Add("videoevent_modifydate", typeof(string));
                dataTable.Columns.Add("videoevent_isdeleted", typeof(bool));
                dataTable.Columns.Add("videoevent_issynced", typeof(bool));
                dataTable.Columns.Add("videoevent_syncerror", typeof(string));

                dataTable.Rows.Clear();

                List<CBVVideoEvent> ItemsToAdjust = DataManagerSqlLite.GetVideoEventbyId(media.VideoEventID);

                if (ItemsToAdjust.Count > 0)
                {
                    CBVVideoEvent ItemToAdjust = ItemsToAdjust[0];

                    var row = dataTable.NewRow();

                    row["videoevent_id"] = ItemToAdjust.videoevent_id;
                    row["fk_videoevent_media"] = ItemToAdjust.fk_videoevent_media;
                    row["videoevent_track"] = ItemToAdjust.videoevent_track;
                    row["videoevent_start"] = media.StartTime.ToString(@"hh\:mm\:ss\.fff");
                    row["videoevent_duration"] = media.Duration.ToString(@"hh\:mm\:ss\.fff");
                    if (media.mediaType != MediaType.Video)
                    {
                        row["videoevent_origduration"] = media.OriginalDuration.ToString(@"hh\:mm\:ss\.fff");
                    }
                    else
                    {
                        row["videoevent_origduration"] = ItemToAdjust.videoevent_origduration;
                    }
                    row["videoevent_end"] = (media.StartTime + media.Duration).ToString(@"hh\:mm\:ss\.fff");
                    row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    row["videoevent_isdeleted"] = ItemToAdjust.videoevent_isdeleted;
                    row["videoevent_issynced"] = ItemToAdjust.videoevent_issynced;
                    row["videoevent_syncerror"] = ItemToAdjust.videoevent_syncerror;

                    dataTable.Rows.Add(row);

                    AdjustVideoEvent(dataTable);
                }
            }
            #endregion
        }

        private int CreateNote(DataTable dataTable)
        {
            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToNotes(dataTable);
                return insertedId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return -1;
        }

        private void DeleteVideoEvent(Media media)
        {
            try
            {
                var VideoEvent = DataManagerSqlLite.GetVideoEventbyId(media.VideoEventID, true);

                DataManagerSqlLite.DeleteVideoEventsById(media.VideoEventID, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeletedNote(TextItem textItem)
        {
            try
            {
                DataManagerSqlLite.DeleteNotesById(textItem.NoteID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateNotes(DataTable dataTable)
        {
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    string text = Convert.ToString(row["notes_modifydate"]);
                    if (string.IsNullOrEmpty(text))
                    {
                        text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    }

                    int num = Convert.ToInt32(row["notes_id"]);
                    string updateQuery = string.Format(" UPDATE cbv_notes\r\n                                        SET \r\n                                            notes_index = {0},\r\n                                            notes_line = '{1}',\r\n                                            notes_wordcount = {2},\r\n                                            notes_start = '{3}',\r\n                                            notes_duration = {4},\r\n                                            notes_issynced = {5},\r\n                                            notes_serverid = {6},\r\n                                            notes_syncerror = '{7}',\r\n                                            notes_modifydate = '{8}'\r\n                                        WHERE \r\n                                            notes_id = {9}", Convert.ToInt32(row["notes_index"]), Convert.ToString(row["notes_line"]).Trim('\''), Convert.ToInt32(row["notes_wordcount"]), Convert.ToString(row["notes_start"]), Convert.ToString(row["notes_duration"]), Convert.ToBoolean(row["notes_issynced"]), Convert.ToInt64(row["notes_serverid"]), Convert.ToString(row["notes_syncerror"]), text, num);
                }


                DataManagerSqlLite.UpdateRowsToNotes(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _ManageMedia_CloseWindow(object sender, EventArgs e)
        {
            WasDataSaved = true;
            _Window.Close();
        }

        private void AdjustVideoEvent(DataTable dataTable)
        {
            try
            {
                DataManagerSqlLite.UpdateRowsToVideoEvent(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void ReleaseReferences()
        {
            _ManageMedia.CloseWindow -= _ManageMedia_CloseWindow;
            _ManageMedia.ManageMediaSave -= _ManageMedia_ManageMediaSave;
            _ManageMedia = null;
            _Window.Content = null;
            _Window = null;
        }

        private List<int> CreateMediaEvents(DataTable dataTable)
        {
            List<int> insertedId = null;

            try
            {
                insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return insertedId;
        }

        public void SetProjectID(int projectID)
        {
            _ProjectID = projectID;
            RefreshData();
        }

        private void RefreshData()
        {
            #region Planned Text

            List<CBVPlanningHead> PlanningHead = DataManagerSqlLite.GetPlanningHead();
            List<CBVPlanning> PlanningData = DataManagerSqlLite.GetPlanning(_ProjectID);
            if (PlanningHead?.Count > 0 && PlanningData?.Count > 0)
            {
                List<PlannedTextGroup> PlannedTextGroups = new List<PlannedTextGroup>();
                for (int i = 0; i < PlanningHead.Count; i++)
                {
                    PlannedTextGroup PlannedGroup = new PlannedTextGroup()
                    {
                        GroupID = PlanningHead[i].planninghead_id,
                        GroupName = PlanningHead[i].planninghead_name,
                        PlannedTexts = new List<PlannedText>()
                    };
                    PlannedTextGroups.Add(PlannedGroup);
                }

                for (int i = 0; i < PlanningData.Count; i++)
                {
                    CBVPlanning CurrentPlanning = PlanningData[i];
                    for (int x = 0; x < PlannedTextGroups.Count; x++)
                    {
                        PlannedTextGroup CurrentGroup = PlannedTextGroups[x];
                        if (CurrentPlanning.fk_planning_head == CurrentGroup.GroupID)
                        {
                            CurrentGroup.PlannedTexts.Add(
                            new PlannedText()
                            {
                                PlannedTextID = CurrentPlanning.planning_id,
                                Text = CurrentPlanning.planning_notesline,
                                SortKey = CurrentPlanning.planning_sort
                            });

                            if (CurrentPlanning.planning_mediathumb != null && CurrentPlanning.planning_mediafull != null)
                            {
                                CurrentGroup.Images.Add(new PlannedImage()
                                {
                                    PlannedTextID = CurrentPlanning.planning_id,
                                    Image = CurrentPlanning.planning_mediafull,
                                    SortKey = CurrentPlanning.planning_sort,
                                    ThumbNail = CurrentPlanning.planning_mediathumb,
                                });
                            }

                            if (CurrentPlanning.planning_suggestnotesline != "" && CurrentPlanning.planning_suggestnotesline != null)
                            {
                                CurrentGroup.SuggestedText.Add(CurrentPlanning.planning_suggestnotesline);
                            }
                        }
                    }
                }

                _ManageMedia.LoadPlannedText(PlannedTextGroups);
            }


            #endregion

            #region Video Events

            //List<CBVMedia> MediaHead = DataManagerSqlLite.GetMedia();
            List<CBVVideoEvent> VideoEventData = DataManagerSqlLite.GetVideoEvents(_ProjectID, true, true);


            List<Media> mediaList = new List<Media>();
            foreach (var item in VideoEventData)
            {
                List<CBVDesign> Designs = item.design_data;
                //obj.fk_design_background, obj.fk_design_screen

                List<CBVScreen> Screens = DataManagerSqlLite.GetScreens();

                Media media = new Media();

                Color background = Colors.DarkRed;
                if (Designs.Count > 0 && Screens.Count >= 7)
                {
                    background = (Color)ColorConverter.ConvertFromString(Screens[Designs[0].fk_design_screen - 1].screen_hexcolor);
                    media.ImageType = Screens[Designs[0].fk_design_screen - 1].screen_name;
                }
                else if (Screens.Count >= 7) 
                {
                    background = (Color)ColorConverter.ConvertFromString(Screens[4].screen_hexcolor);
                }

               


                if (item.videosegment_data != null && item.videosegment_data.Count > 0)
                {
                    media.mediaData = item.videosegment_data[0].videosegment_media;
                }

                if (item.fk_videoevent_media == 1 || item.fk_videoevent_media == 4)
                {
                    media.mediaType = MediaType.Image;
                }
                else if (item.fk_videoevent_media == 2)
                {
                    media.mediaType = MediaType.Video;

                    if (Screens.Count >= 7)
                    {
                        background = (Color)ColorConverter.ConvertFromString(Screens[4].screen_hexcolor);
                    }
                }
                else if (item.fk_videoevent_media == 3)
                {
                    media.mediaType = MediaType.Audio;
                }
                else if (item.fk_videoevent_media == 4)
                {
                    media.mediaType = MediaType.Form;
                }

                media.Color = background;

                media.VideoEventID = item.videoevent_id;
                media.ProjectId = _ProjectID;
                media.TrackId = item.videoevent_track;
                if (item.notes_data != null)
                {
                    List<TextItem> textItems = new List<TextItem>();
                    foreach (var note in item.notes_data)
                    {
                        TextItem textItem = new TextItem();
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

            _ManageMedia.SetProjectInfo(_ProjectID);

            _ManageMedia.LoadEvents(mediaList);

            #endregion
        }
    }
}
