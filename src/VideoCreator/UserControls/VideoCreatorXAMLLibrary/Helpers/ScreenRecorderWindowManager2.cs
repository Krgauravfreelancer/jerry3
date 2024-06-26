﻿using ScreenRecorder_UserControl;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VideoCreatorXAMLLibrary.Loader;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public class ScreenRecorderWindowManager2
    {
        Window window;
        ScreenRecorder_Control Recorder;
        SelectedProjectEvent selectedProjectEvent;
        public LoadingAnimation loader;
        public Window CreateWindow(SelectedProjectEvent _selectedProjectEvent)
        {
            window = new Window();
            window.Width = 1000;
            window.Height = 500;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Recorder = new ScreenRecorder_Control();
            selectedProjectEvent = _selectedProjectEvent;
            //ProjectID = Selected_ID;

            //if (Selected_ID != -1)
            if (selectedProjectEvent != null)
            {
                var VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true);

                TimeSpan TotalTime = TimeSpan.Zero;

                if (VideoEventData.Count > 0)
                {
                    List<CBVVideoEvent> SortedList = VideoEventData.OrderBy(o => o.videoevent_start).ToList();
                    TimeSpan StartTime = TimeSpan.ParseExact(SortedList.LastOrDefault().videoevent_start, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
                    TotalTime = StartTime + TimeSpan.Parse(VideoEventData.LastOrDefault().videoevent_duration);
                }

                Recorder.SetProjectInfo(selectedProjectEvent.projdetId, TotalTime);

                RefreshData();

                Recorder.Loaded += ScreenRecorder_Control_Loaded;
                Recorder.CloseWindow += ScreenRecorder_Control_CloseWindow;
                Recorder.DeleteMedia += ScreenRecorder_Control_DeleteMedia;
                Recorder.MediaRecordingCompleted += ScreenRecorder_Control_MediaRecordingCompleted;

                Recorder.NotesChanged += Recorder_NotesChanged;
                Recorder.NotesDeleted += Recorder_NotesDeleted;
                Recorder.NotesCreated += Recorder_NotesCreated;
                Recorder.NotesChangeCompleted += Recorder_NotesChangeCompleted;
            }

            window.Content = AddLoaderAndReturnContent(Recorder);
            //window.Show();
            return window;
        }


        private Grid AddLoaderAndReturnContent(ScreenRecorder_Control Recorder)
        {
            var grid = new Grid
            {
                Name = "parentGrid"
            };
            grid.Children.Add(Recorder);

            loader = new LoadingAnimation
            {
                Name = "loader",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden,
            };
            grid.Children.Add(loader);
            return grid;


        }

        private void Recorder_NotesChanged(object sender, NotesChangedArgs e)
        {
            var notedataTable = GetNotesRawTable();
            foreach (var ChangedElement in e.ChangedNotes)
            {
                var noteRow = notedataTable.NewRow();
                var localNote = DataManagerSqlLite.GetNotesbyId(ChangedElement.Item.NoteID)[0];
                noteRow["notes_id"] = ChangedElement.Item.NoteID;
                noteRow["fk_notes_videoevent"] = DataManagerSqlLite.GetVideoEventbyId(ChangedElement.VideoEventID, false, false)[0].videoevent_serverid;
                noteRow["notes_line"] = ChangedElement.Item.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = ChangedElement.Item.WordCount;
                noteRow["notes_index"] = localNote.notes_index;
                noteRow["notes_start"] = ChangedElement.Item.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = ChangedElement.Item.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = localNote.notes_createdate;
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = localNote.notes_isdeleted;
                noteRow["notes_issynced"] = localNote.notes_issynced;
                noteRow["notes_serverid"] = localNote.notes_serverid;
                noteRow["notes_syncerror"] = string.Empty;
                notedataTable.Rows.Add(noteRow);
            }
            ScreenRecorder_NotesChangedEvent.Invoke(notedataTable);
        }

        private void Recorder_NotesCreated(object sender, NotesChangedArgs e)
        {
            // For calculating the index
            var notesIndex = new Dictionary<int, int>();
            foreach (var videoeventId in e.ChangedNotes.Select(x => x.VideoEventID).Distinct())
            {
                notesIndex.Add(videoeventId, DataManagerSqlLite.GetMaxIndexForNotes(videoeventId));
            }

            var notedataTable = GetNotesRawTable();
            foreach (var ChangedElement in e.ChangedNotes)
            {
                var noteRow = notedataTable.NewRow();
                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = DataManagerSqlLite.GetVideoEventbyId(ChangedElement.VideoEventID, false, false)[0].videoevent_serverid;
                noteRow["notes_line"] = ChangedElement.Item.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = ChangedElement.Item.WordCount;
                noteRow["notes_index"] = notesIndex[ChangedElement.VideoEventID];
                noteRow["notes_start"] = ChangedElement.Item.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = ChangedElement.Item.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = true;
                noteRow["notes_serverid"] = -1;
                noteRow["notes_syncerror"] = string.Empty;
                notedataTable.Rows.Add(noteRow);
                notesIndex[ChangedElement.VideoEventID]++;
            }
            ScreenRecorder_NotesCreatedEvent.Invoke(notedataTable);
        }

        private void Recorder_NotesDeleted(object sender, NotesChangedArgs e)
        {
            var ids = new List<int>();
            foreach (var deletedItem in e.ChangedNotes)
            {
                ids.Add(deletedItem.Item.NoteID);
            }
            ScreenRecorder_NotesDeletedEvent.Invoke(ids);
        }

        private void Recorder_NotesChangeCompleted(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void ScreenRecorder_Control_MediaRecordingCompleted(object sender, MediaRecordingCompletedEventArgs e)
        {
            //if (ProjectID != -1)
            //{
            if (e.MediaItems != null)
            {
                try
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
                    dataTable.Columns.Add("videoevent_notes", typeof(DataTable)); //Added By KG
                    foreach (Media element in e.MediaItems)
                    {
                        // Since this table has Referential Integrity, so lets push one by one
                        //dataTable.Rows.Clear();


                        var row = dataTable.NewRow();
                        //row["videoevent_id"] = -1;
                        row["fk_videoevent_projdet"] = element.ProjectId;
                        row["videoevent_track"] = element.TrackId;
                        row["videoevent_start"] = element.StartTime.ToString(@"hh\:mm\:ss\.fff");
                        row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        row["videoevent_isdeleted"] = false;
                        row["videoevent_issynced"] = false;
                        row["videoevent_serverid"] = 100;
                        row["videoevent_syncerror"] = string.Empty;

                        var mediaId = 0;

                        if (element.mediaType == MediaType.Image)
                        {
                            mediaId = 1;
                        }

                        if (element.mediaType == MediaType.Video)
                        {
                            mediaId = 2;
                        }

                        row["videoevent_duration"] = element.Duration.ToString(@"hh\:mm\:ss\.fff");
                        row["videoevent_origduration"] = element.Duration.ToString(@"hh\:mm\:ss\.fff");
                        row["fk_videoevent_media"] = mediaId;

                        row["fk_videoevent_screen"] = -1; // Not needed for this case

                        row["media"] = element.mediaData;
                        row["videoevent_notes"] = GetNotesDataTableWithData(element); //Added By KG
                        dataTable.Rows.Add(row);

                        //List<int> InsertedIDs = CreateMediaEvents(dataTable);

                        //if (InsertedIDs.Count > 0)
                        //{
                        //    SaveNotes(InsertedIDs[0], element);
                        //}
                    }
                    ScreenRecorder_BtnSaveClickedEvent.Invoke(dataTable); //Added by KG
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                RefreshData();
            }

            //}
            //else
            //{
            //    MessageBox.Show("Please Select a project");
            //}
        }

        public void SetProjectID(int projectID)
        {
            //ProjectID = projectID;
            //RefreshData();
        }

        private void ScreenRecorder_Control_DeleteMedia(object sender, MediaDeleteEventArgs e)
        {
            ScreenRecorder_BtnDeleteMediaClicked.Invoke(e.MediaItem.VideoEventID);
            RefreshData();
        }

        private void ScreenRecorder_Control_CloseWindow(object sender, EventArgs e)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private void ScreenRecorder_Control_Loaded(object sender, RoutedEventArgs e)
        {
            var displays = Recorder.GetDisplays();

            int i = 1;
            foreach (var display in displays)
            {
                Recorder.AddRecordRegion(new System.Drawing.Rectangle(display.screen.PhysicalBounds.Left, display.screen.PhysicalBounds.Top, display.screen.PhysicalBounds.Width / 2, display.screen.PhysicalBounds.Height), $"Display {display.DisplayNumber} - Region {i}");
                i++;
            }
        }

        public void RefreshData()
        {
            var VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projectId, true);


            List<Media> mediaList = new List<Media>();
            foreach (var item in VideoEventData)
            {
                Media media = new Media();

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
                }

                media.VideoEventServerID = item.videoevent_serverid;
                media.VideoEventID = item.videoevent_id;
                media.ProjectId = selectedProjectEvent.projectId;
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
                media.StartTime = TimeSpan.Parse(item.videoevent_start);

                mediaList.Add(media);
            }

            TimeSpan TotalTime = TimeSpan.Zero;

            if (VideoEventData.Count > 0)
            {
                List<CBVVideoEvent> SortedList = VideoEventData.OrderBy(o => o.videoevent_start).ToList();
                TimeSpan StartTime = TimeSpan.ParseExact(SortedList.LastOrDefault().videoevent_start, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
                TotalTime = StartTime + TimeSpan.Parse(VideoEventData.LastOrDefault().videoevent_duration);
            }

            Recorder.SetProjectInfo(selectedProjectEvent.projectId, TotalTime);

            Recorder.LoadEvents(mediaList);
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

        private void CreateNotes(DataTable dataTable)
        {
            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToNotes(dataTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                CreateNotes(notedataTable);
            }
        }


        #region == Added By KG == 
        public event Action<DataTable> ScreenRecorder_BtnSaveClickedEvent; //Added By KG
        public event Action<int> ScreenRecorder_BtnDeleteMediaClicked; //Added By KG
        public event Action<DataTable> ScreenRecorder_NotesCreatedEvent;
        public event Action<DataTable> ScreenRecorder_NotesChangedEvent;
        public event Action<List<int>> ScreenRecorder_NotesDeletedEvent;

        private DataTable GetNotesDataTableWithData(Media media)
        {
            var notesDataTable = GetNotesRawTable();

            int NotesIndex = 0;
            foreach (var element in media.RecordedTextList)
            {
                var noteRow = notesDataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = -1;
                noteRow["notes_line"] = element.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = element.WordCount;
                noteRow["notes_index"] = NotesIndex;
                noteRow["notes_start"] = element.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = 0;// element.Duration.ToString(@"hh\:mm\:ss\.fff"); ;
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = false;
                noteRow["notes_serverid"] = 1;
                noteRow["notes_syncerror"] = string.Empty;

                notesDataTable.Rows.Add(noteRow);
                NotesIndex++;

            }
            return notesDataTable;
        }

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

        public bool? ShowWindow(Window window)
        {
            LoaderHelper.HideLoader(window, loader);
            var result = window.ShowDialog();
            return result;
        }

        #endregion
    }
}
