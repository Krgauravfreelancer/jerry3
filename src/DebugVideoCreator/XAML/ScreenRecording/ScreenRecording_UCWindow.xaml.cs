using ScreenRecorder_UserControl.Models;
using ScreenRecorder_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Timeline.UserControls.Models;
using Timeline.UserControls;
using System.Windows.Controls.Primitives;
using VideoCreator.XAML;
using VoiceScripter_UserControl.Classes;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for ScreenRecording_UCWindow.xaml
    /// </summary>
    public partial class ScreenRecording_UCWindow : UserControl
    {
        //int selectionIndex = 0;
        bool showMessageBoxes = false;

        private readonly Window _mainWindow;
        private readonly int _trackID;
        private readonly int _projectID;


        public event Action<DataTable> BtnSaveClickedEvent;

        public ScreenRecording_UCWindow(Window mainWindow, int trackId, int projectID)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _trackID = trackId;
            _projectID = projectID;
            SetProjectID(_projectID);


            //RefreshOrLoadComboBoxes(EnumEntity.ALL);


        }

        public ScreenRecording_UCWindow(int trackId, int projectID)
        {
            InitializeComponent();
            _trackID = trackId;
            _projectID = projectID;
            SetProjectID(_projectID);
            //RefreshOrLoadComboBoxes(EnumEntity.ALL);
        }

        public void Show()
        {
            _mainWindow.ShowDialog();
        }


        private List<int> CreateMediaEvents(DataTable dataTable)
        {
            List<int> insertedId = null;

            try
            {
                insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);

                if (showMessageBoxes)
                {
                    MessageBox.Show("VideoEvent Table populated to Database");
                }
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

                if (showMessageBoxes)
                {
                    MessageBox.Show("Notes Table populated to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void Create_Event(DataTable dataTable)
        {
            this.IsEnabled = true;
            this.Focus();

            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);

                if (showMessageBoxes)
                {
                    MessageBox.Show("VideoEvent Table populated to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }




        private void Recorder_CloseWindow(object sender, EventArgs e)
        {
            ForwardDataTable(null);
            //this.Close();
        }

        private void ScreenRecorderWindow_Closed(object sender, EventArgs e)
        {
            _mainWindow.Show(); // Show the main window when the NewWindow is closed
        }


        //private void Recorder_TextReceived(object sender, TextReceivedEventArgs e)
        //{
        //    TextTestBox.Text += $"{Environment.NewLine}Start: {e.TextItem.Start.TotalSeconds} - Duration: {e.TextItem.Duration.TotalSeconds} - Text: {e.TextItem.Text}";
        //}

        private void ScreenRecorder_Control_CancelClicked(object sender, EventArgs e)
        {
            /// Here we will close the ScreenRecorder and ignore any video or changes made.
            // Close();
        }

        private void ScreenRecorder_Control_SaveClicked2(object sender, MediaRecordingCompletedEventArgs e)
        {


            if (e.MediaItem != null)
            {
                try
                {
                    var dataTable = new DataTable();
                    dataTable.Columns.Add("videoevent_id", typeof(int));
                    dataTable.Columns.Add("fk_videoevent_project", typeof(int));
                    dataTable.Columns.Add("fk_videoevent_media", typeof(int));
                    dataTable.Columns.Add("videoevent_track", typeof(int));
                    dataTable.Columns.Add("videoevent_start", typeof(string));
                    dataTable.Columns.Add("videoevent_duration", typeof(int));
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

                    Media element = e.MediaItem;

                    var row = dataTable.NewRow();
                    //row["videoevent_id"] = -1;
                    row["fk_videoevent_project"] = e.MediaItem.ProjectId;
                    row["videoevent_track"] = e.MediaItem.TrackId;
                    row["videoevent_start"] = element.StartTime.ToString(@"hh\:mm\:ss");
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

                    row["videoevent_duration"] = element.Duration.TotalSeconds;
                    row["fk_videoevent_media"] = mediaId;
                    row["fk_videoevent_screen"] = -1; // Not needed for this case
                    row["media"] = element.mediaData;

                    row["videoevent_notes"] = GetNotesDataTable(element);
                    dataTable.Rows.Add(row);

                    List<int> InsertedIDs = CreateMediaEvents(dataTable);

                    if (InsertedIDs.Count > 0)
                    {
                        SaveNotes(InsertedIDs[0], element);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                RefreshData();

            }



        }





        public void SetProjectID(int projectID)
        {

            var VideoEventData = DataManagerSqlLite.GetVideoEvents(projectID, true)?.Where(e => e.videoevent_track == _trackID)?.ToList();

            TimeSpan TotalTime = TimeSpan.Zero;

            if (VideoEventData.Count > 0)
            {
                TimeSpan StartTime = TimeSpan.ParseExact(VideoEventData.LastOrDefault().videoevent_start, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
                TotalTime = StartTime + TimeSpan.FromSeconds(VideoEventData.LastOrDefault().videoevent_duration);
            }

            Recorder.SetProjectInfo(projectID, _trackID, TotalTime);

            RefreshData();
        }


        private void Recorder_DeleteMedia(object sender, MediaDeleteEventArgs e)
        {
            DataManagerSqlLite.DeleteVideoEventsById(e.MediaItem.VideoEventID, true);
            RefreshData();
        }

        private DataTable GetNotesDataTable(Media media)
        {
            var notesDataTable = new DataTable();
            notesDataTable.Columns.Add("notes_id", typeof(int));
            notesDataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notesDataTable.Columns.Add("notes_line", typeof(string));
            notesDataTable.Columns.Add("notes_wordcount", typeof(int));
            notesDataTable.Columns.Add("notes_index", typeof(int));
            notesDataTable.Columns.Add("notes_start", typeof(string));
            notesDataTable.Columns.Add("notes_duration", typeof(int));
            notesDataTable.Columns.Add("notes_createdate", typeof(string));
            notesDataTable.Columns.Add("notes_modifydate", typeof(string));
            notesDataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notesDataTable.Columns.Add("notes_issynced", typeof(bool));
            notesDataTable.Columns.Add("notes_serverid", typeof(long));
            notesDataTable.Columns.Add("notes_syncerror", typeof(string));

            notesDataTable.Rows.Clear();


            int NotesIndex = 0;
            foreach (var element in media.RecordedTextList)
            {
                var noteRow = notesDataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = -1;
                noteRow["notes_line"] = element.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = element.WordCount;
                noteRow["notes_index"] = NotesIndex;
                noteRow["notes_start"] = element.Start.ToString(@"hh\:mm\:ss");
                noteRow["notes_duration"] = (int)element.Duration.TotalSeconds;
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
        private void Recorder_Loaded(object sender, RoutedEventArgs e)
        {
            var displays = Recorder.GetDisplays();

            int i = 1;
            foreach (var display in displays)
            {
                Recorder.AddRecordRegion(new System.Drawing.Rectangle(display.screen.PhysicalBounds.Left, display.screen.PhysicalBounds.Top, display.screen.PhysicalBounds.Width / 2, display.screen.PhysicalBounds.Height), $"Display {display.DisplayNumber} - Region {i}");
                i++;
            }
        }

        private void RefreshData()
        {
            var VideoEventData = DataManagerSqlLite.GetVideoEvents(_projectID, true);


            List<Media> mediaList = new List<Media>();
            foreach (var item in VideoEventData)
            {
                if (item.videoevent_track == _trackID)
                {
                    Media media = new Media();

                    if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == (int)MediaType.Image)
                    {
                        media.mediaType = MediaType.Image;
                        media.mediaData = item.videosegment_data[0].videosegment_media;
                    }
                    else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == (int)MediaType.Video)
                    {
                        media.mediaType = MediaType.Video;
                        media.mediaData = item.videosegment_data[0].videosegment_media;
                    }

                    media.VideoEventID = item.videoevent_id;
                    media.ProjectId = _projectID;
                    media.TrackId = item.videoevent_track;
                    if (item.notes_data != null)
                    {
                        List<TextItem> textItems = new List<TextItem>();
                        foreach (var note in item.notes_data)
                        {
                            TextItem textItem = new TextItem();
                            textItem.Text = note.notes_line;
                            textItem.Start = TimeSpan.Parse(note.notes_start);
                            textItem.Duration = TimeSpan.FromSeconds(note.notes_duration);

                            textItems.Add(textItem);
                        }
                        media.RecordedTextList = textItems;
                    }
                    media.Duration = TimeSpan.FromSeconds(item.videoevent_duration);
                    media.StartTime = TimeSpan.Parse(item.videoevent_start);



                    mediaList.Add(media);
                }

            }

            Recorder.LoadEvents(mediaList);
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
            notedataTable.Columns.Add("notes_duration", typeof(int));
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
                noteRow["notes_start"] = element.Start.ToString(@"hh\:mm\:ss");
                noteRow["notes_duration"] = (int)element.Duration.TotalSeconds;
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



        #region Wrapper methods

        private void ForwardDataTable(DataTable dataTable)
        {
            if (BtnSaveClickedEvent != null)
                BtnSaveClickedEvent(dataTable);
        }

        #endregion


        public class VideoEventExtended : CBVVideoEvent
        {
            public string MediaName { get; set; }
            public string Start { get; set; }
            public string ClipDuration { get; set; }
            public VideoEventExtended(CBVVideoEvent ch)
            {
                foreach (var prop in ch.GetType().GetProperties())
                {
                    this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(ch, null), null);
                }
            }
        }
    }
}
