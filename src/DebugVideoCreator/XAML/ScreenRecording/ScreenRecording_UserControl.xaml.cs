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



        private void ScreenRecorder_Control_CancelClicked(object sender, EventArgs e)
        {

        }

        private void ScreenRecorder_Control_SaveClicked(object sender, SaveEventArgs e)
        {
            List<Media> mediaList = e.MediaList;
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
                //optional column
                dataTable.Columns.Add("media", typeof(byte[])); // Media Column
                dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen

                // Since this table has Referential Integrity, so lets push one by one
                dataTable.Rows.Clear();

                foreach (Media element in mediaList)
                {
                    var row = dataTable.NewRow();
                    row["videoevent_id"] = -1;
                    row["fk_videoevent_project"] = selectedProjectId;
                    row["videoevent_track"] = 1;
                    row["videoevent_start"] = element.StartTime.ToString(@"hh\:mm\:ss");
                    row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var mediaId = 0;
                    if (element.mediaType == MediaType.Image)
                    {
                        row["videoevent_duration"] = element.Duration.TotalSeconds;
                        mediaId = 1;
                    }
                    else if (element.mediaType == MediaType.Video)
                    {
                        row["videoevent_duration"] = element.Duration.TotalSeconds;
                        mediaId = 2;
                    }

                    row["fk_videoevent_media"] = mediaId;
                    row["fk_videoevent_screen"] = -1; // Not needed for this case
                    row["media"] = element.mediaData;
                    dataTable.Rows.Add(row);
                }
                Create_Event(dataTable);
                Recorder.Reset();
                FillListBoxVideoEvent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void Recorder_UnLoaded(object sender, RoutedEventArgs e)
        {
            //e.MediaList

        }
    }
}
