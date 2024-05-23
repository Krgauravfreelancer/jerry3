using ScreenRecorder_UserControl.Models;
using ScreenRecorder_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Data;
using System.Windows.Media;
using VideoCreatorXAMLLibrary.Loader;
using System.Windows.Controls;

namespace VideoCreatorXAMLLibrary.Helpers
{

    public class PlaceholderEvent
    {
        public DataTable newEventsDT { get; set; }
        public TimeSpan newDuration { get; set; }
    }

    internal class ScreenRecorderWindowManager
    {
        Window window;
        ScreenRecorder_Control Recorder;
        SelectedProjectEvent selectedProjectEvent;
        public LoadingAnimation loader;
        bool PromptOveride = false;

        public event Action<DataTable> ScreenRecorder_BtnSaveClickedEvent; //Added By KG
        public event Action<PlaceholderEvent> ScreenRecorder_BtnReplaceEvent; //Added By KG
        

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

        public Window CreateWindow(SelectedProjectEvent _selectedProjectEvent)
        {
            window = new Window();
            window.Width = 1000;
            window.Height = 618;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Recorder = new ScreenRecorder_Control();
            selectedProjectEvent = _selectedProjectEvent;
            PromptOveride = false;

            //if (Selected_ID != -1)
            if (selectedProjectEvent != null)
            {
                LoadProjectData();

                Recorder.Loaded += ScreenRecorder_Control_Loaded;
                Recorder.CloseWindow += ScreenRecorder_Control_CloseWindow;
                Recorder.Save += Recorder_Save;
            }

            window.Content = AddLoaderAndReturnContent(Recorder);
            window.Closing += Window_Closing;
            return window;
        }

        public Window CreateWindowWithPlaceHolder(SelectedProjectEvent _selectedProjectEvent, int placeholderVideoEventId)
        {
            window = new Window();
            window.Width = 1000;
            window.Height = 618;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Recorder = new ScreenRecorder_Control();
            selectedProjectEvent = _selectedProjectEvent;
            PromptOveride = false;

            //if (Selected_ID != -1)
            if (selectedProjectEvent != null && placeholderVideoEventId > 0)
            {
                List<CBVVideoEvent> PlaceHolderItems = DataManagerSqlLite.GetVideoEventbyId(placeholderVideoEventId);

                if (PlaceHolderItems.Count > 0)
                {
                    CBVVideoEvent PlaceHolderItem = PlaceHolderItems.FirstOrDefault();
                    LoadPlaceHolderData(PlaceHolderItem);

                    Recorder.Loaded += ScreenRecorder_Control_Loaded;
                    Recorder.CloseWindow += ScreenRecorder_Control_CloseWindow;
                    Recorder.Save += Recorder_Save;
                }
            }

            window.Content = AddLoaderAndReturnContent(Recorder);
            window.Closing += Window_Closing;
            return window;
        }

        public void LoadProjectData()
        {
            var VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true, true);

            TimeSpan TotalTime = TimeSpan.Zero;

            if (VideoEventData.Count > 0)
            {
                List<CBVVideoEvent> SortedList = VideoEventData.OrderBy(o => TimeSpan.ParseExact(o.videoevent_start, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture) + TimeSpan.ParseExact(o.videoevent_duration, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture)).ToList();
                TimeSpan StartTime = TimeSpan.ParseExact(SortedList.LastOrDefault().videoevent_start, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);
                TotalTime = StartTime + TimeSpan.Parse(SortedList.LastOrDefault().videoevent_duration);
            }

            Recorder.LoadProjectData(TotalTime, selectedProjectEvent.projdetId);
        }

        private void LoadPlaceHolderData(CBVVideoEvent PlaceHolderItems)
        {
            Media media = new Media();
            Color background = Colors.LightGray;

            media.ImageType = "video";
            media.mediaType = MediaType.PlaceHolder;

            media.Color = background;

            media.VideoEventServerID = PlaceHolderItems.videoevent_serverid;
            media.VideoEventID = PlaceHolderItems.videoevent_id;
            media.ProjectId = selectedProjectEvent.projectId;
            media.TrackId = PlaceHolderItems.videoevent_track;
            if (PlaceHolderItems.notes_data != null)
            {
                List<TextItem> textItems = new List<TextItem>();
                foreach (var note in PlaceHolderItems.notes_data)
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
            media.Duration = TimeSpan.Parse(PlaceHolderItems.videoevent_duration);
            media.StartTime = TimeSpan.Parse(PlaceHolderItems.videoevent_start);

            TimeSpan StartTime = TimeSpan.ParseExact(PlaceHolderItems.videoevent_start, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);

            Recorder.LoadPlaceHolderData(media, selectedProjectEvent.projectId, StartTime);
        }


        #region == Events == 

        private void Recorder_Save(object sender, SaveEventArgs e)
        {
            #region == Delete PlaceHolder If Needed ==
            bool isReplace = false;
            if (e.RecordingMode == ScreenRecorder_UserControl.Controls.Mode.RecordPlaceHolder)
            {
                isReplace = true;
                /*
                try
                {
                    var PlaceHolderEvent = DataManagerSqlLite.GetVideoEventbyId(e.PlaceHolderID, true);

                    if (PlaceHolderEvent?.Count > 0)
                    {
                        DataManagerSqlLite.DeleteVideoEventsById(e.PlaceHolderID, true);

                        var VideoEvents = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true);

                        var dataTable = new DataTable();

                        dataTable.Columns.Add("videoevent_start", typeof(string));
                        dataTable.Columns.Add("videoevent_duration", typeof(string));
                        dataTable.Columns.Add("videoevent_origduration", typeof(string));
                        dataTable.Columns.Add("videoevent_end", typeof(string));
                        dataTable.Columns.Add("videoevent_serverid", typeof(Int64));
                        dataTable.Rows.Clear();

                        List<CBVShiftVideoEvent> elements = DataManagerSqlLite.GetShiftVideoEventsbyStartTime(selectedProjectEvent.projdetId, PlaceHolderEvent[0].videoevent_start);

                        TimeSpan AmountToShift = e.RecordingDuration - TimeSpan.ParseExact(PlaceHolderEvent[0].videoevent_duration, @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture);

                        if (AmountToShift > TimeSpan.Zero)
                        {

                            foreach (var element in elements)
                            {
                                var row = dataTable.NewRow();
                                row["videoevent_start"] = DataManagerSqlLite.ShiftRight(element.videoevent_start, AmountToShift.Duration().ToString(@"hh\:mm\:ss\.fff"));
                                row["videoevent_duration"] = element.videoevent_duration;
                                row["videoevent_origduration"] = element.videoevent_origduration;
                                row["videoevent_end"] = DataManagerSqlLite.ShiftRight(element.videoevent_end, AmountToShift.Duration().ToString(@"hh\:mm\:ss\.fff"));
                                row["videoevent_serverid"] = element.videoevent_serverid;
                                dataTable.Rows.Add(row);
                            }

                            DataManagerSqlLite.ShiftVideoEvents(dataTable);
                        }
                        else
                        {
                            foreach (var element in elements)
                            {
                                var row = dataTable.NewRow();
                                row["videoevent_start"] = DataManagerSqlLite.ShiftLeft(element.videoevent_start, AmountToShift.Duration().ToString(@"hh\:mm\:ss\.fff"));
                                row["videoevent_duration"] = element.videoevent_duration;
                                row["videoevent_origduration"] = element.videoevent_origduration;
                                row["videoevent_end"] = DataManagerSqlLite.ShiftLeft(element.videoevent_end, AmountToShift.Duration().ToString(@"hh\:mm\:ss\.fff"));
                                row["videoevent_serverid"] = element.videoevent_serverid;
                                dataTable.Rows.Add(row);
                            }

                            DataManagerSqlLite.ShiftVideoEvents(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                */
            }

            #endregion == Delete PlaceHolder If Needed ==

            #region == Save Recorded Video Events ==

            if (e.RecordedMedia != null)
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

                foreach (Media element in e.RecordedMedia)
                {
                    // Since this table has Referential Integrity, so lets push one by one
                    dataTable.Rows.Clear();


                    var row = dataTable.NewRow();
                    //row["videoevent_id"] = -1;
                    row["fk_videoevent_projdet"] = e.ProjectID;
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
                    row["videoevent_notes"] = GetNotesDataTableWithData(element); //Added By KG
                    row["media"] = element.mediaData;
                    dataTable.Rows.Add(row);

                }
                if (isReplace)
                {
                    var payload = new PlaceholderEvent
                    {
                        newDuration = e.RecordingDuration,
                        newEventsDT = dataTable
                    };
                    ScreenRecorder_BtnReplaceEvent.Invoke(payload);
                }
                else
                {
                    ScreenRecorder_BtnSaveClickedEvent.Invoke(dataTable); //Added by KG
                }
            }

            #endregion == Save Recorded Video Events ==

            if (window != null)
            {
                PromptOveride = true;
                window.Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (PromptOveride == false)
            {
                //bool Saved = Recorder.ShowClosingPrompt();
                Recorder.ShowClosingPrompt();
                bool Saved = false;

                if (Saved != true)
                {
                    e.Cancel = true;
                }
            }
        }

        private void ScreenRecorder_Control_CloseWindow(object sender, EventArgs e)
        {
            if (window != null)
            {
                PromptOveride = true;
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

        #endregion
        public void SetProjectID(int projectID)
        {
            //ProjectID = projectID;
            //LoadProjectData();
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

        #region == Added By KG == 

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
