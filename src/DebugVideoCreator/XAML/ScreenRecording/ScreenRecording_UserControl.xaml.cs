using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScreenRecorder_UserControl;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for MainWindow.VideoCreator
    /// </summary>
    public partial class ScreenRecordingUserControl : UserControl
    {
        bool showMessageBoxes = false;
        int selectedProjectId = -1;

        public ScreenRecordingUserControl(int projectId)
        {
            InitializeComponent();
            selectedProjectId = projectId;
            FillListBoxVideoEvent();
        }

        #region ListBox and ComboBox

        private void FillListBoxVideoEvent()
        {
            var source = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);

            listBoxVideoEvent.SelectedItem = null;
            listBoxVideoEvent.Items.Clear();
            if (source != null)
            {
                foreach (var item in source.FindAll(x => x.fk_videoevent_media <= 2))
                {
                    var itemExtended = new VideoEventExtended(item)
                    {
                        Start = item.videoevent_start,
                        ClipDuration = item.videoevent_duration.ToString() + " sec",
                    };
                    if (item.audio_data != null && item.audio_data.Count > 0 && item.fk_videoevent_media == 3)
                    {
                        itemExtended.MediaName = "Audio";
                    }
                    else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 1)
                    {
                        itemExtended.MediaName = "Image";
                    }
                    else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 2)
                    {
                        itemExtended.MediaName = "Video";
                    }
                    else if (item.design_data != null && item.design_data.Count > 0 && item.fk_videoevent_media == 4)
                    {
                        itemExtended.MediaName = "Design";
                    }
                    else
                    {
                        itemExtended.MediaName = "None";
                    }
                    listBoxVideoEvent.Items.Add(itemExtended);
                }
            }
        }

        #endregion



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

        private void Recorder_TextReceived(object sender, TextReceivedEventArgs e)
        {
            TextTestBox.Text += $"{Environment.NewLine}Start: {e.TextItem.Start.TotalSeconds} - Duration: {e.TextItem.Duration.TotalSeconds} - Text: {e.TextItem.Text}";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            
                List<Media> mediaList = Recorder.GetMedia();

                if (mediaList != null)
                {
                    try
                    {
                        Recorder.IsEnabled = false;

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

                        var SelectedProject = selectedProjectId;


                        foreach (Media element in mediaList)
                        {
                            var row = dataTable.NewRow();
                            row["fk_videoevent_project"] = selectedProjectId;
                            row["videoevent_track"] = 1;
                            row["videoevent_start"] = element.StartTime.ToString(@"hh\:mm\:ss");
                            row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            row["videoevent_isdeleted"] = false;
                            row["videoevent_issynced"] = false;
                            row["videoevent_serverid"] = 1;
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

                            dataTable.Rows.Add(row);


                        }

                        List<int> InsertedIDs = CreateMediaEvents(dataTable);

                        SaveNotes(InsertedIDs, mediaList);

                       
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    Recorder.Reset();
                    Recorder.IsEnabled = true;
                }

            
        }

        private void SaveNotes(List<int> InsertedIDs, List<Media> mediaList)
        {

            if (InsertedIDs != null)
            {
                if (InsertedIDs.Count > 0)
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

                    for (int i = 0; i < InsertedIDs.Count; i++)
                    {
                        int NotesIndex = 0;
                        foreach (var element in mediaList[i].RecordedTextList)
                        {
                            InsertNotes = true;
                            var noteRow = notedataTable.NewRow();

                            noteRow["notes_id"] = -1;
                            noteRow["fk_notes_videoevent"] = InsertedIDs[0];
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
                    }

                    if (InsertNotes == true)
                    {
                        CreateNotes(notedataTable, mediaList);
                    }
                }
            }

        }

        private void CreateNotes(DataTable dataTable, List<Media> MediaItems)
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
    }

    
}

