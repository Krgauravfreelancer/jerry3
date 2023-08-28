using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScreenRecorder_UserControl;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

namespace ScreenRecording_UserControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsSetUp = false;
        int selectionIndex = 0;
        bool showMessageBoxes = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetUp)
            {
                InitializeDatabase();
                IsSetUp = true;
            }

        }

        #region Loading Database
        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                if (showMessageBoxes)
                {
                    MessageBox.Show(message + ", syncing lookup tables !!");
                }
                SyncApp();
                SyncMedia();
                SyncScreen();
                if (showMessageBoxes)
                {
                    MessageBox.Show("lookup tables synced successfully !!");
                }
                RefreshOrLoadComboBoxes(EnumEntity.ALL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(int));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = 1;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = 0;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = 0;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = 0;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = 0;
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SyncMedia()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("media_id", typeof(int));
                datatable.Columns.Add("media_name", typeof(string));
                datatable.Columns.Add("media_color", typeof(string));

                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = "image";
                row["media_color"] = "Tomato";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["media_id"] = -1;
                row2["media_name"] = "video";
                row2["media_color"] = "Thistle";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["media_id"] = -1;
                row3["media_name"] = "audio";
                row3["media_color"] = "Yellow";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["media_id"] = -1;
                row4["media_name"] = "form";
                row4["media_color"] = "LightSalmon";
                datatable.Rows.Add(row4);

                var insertedIds = DataManagerSqlLite.SyncMedia(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SyncScreen()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("screen_id", typeof(int));
                datatable.Columns.Add("screen_name", typeof(string));
                datatable.Columns.Add("screen_color", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "LightSalmon";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["screen_id"] = -1;
                row2["screen_name"] = "prerequisites";
                row2["screen_color"] = "Azure";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["screen_id"] = -1;
                row3["screen_name"] = "screen cast";
                row3["screen_color"] = "Beige";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["screen_id"] = -1;
                row4["screen_name"] = "conclusion";
                row4["screen_color"] = "Aqua";
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["screen_id"] = -1;
                row5["screen_name"] = "next";
                row5["screen_color"] = "LightSteelBlue";
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region ListBox and ComboBox

        private void FillListBoxVideoEvent(List<CBVVideoEvent> source)
        {
            listBoxVideoEvent.SelectedItem = null;
            listBoxVideoEvent.Items.Clear();
            foreach (var item in source)
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
        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
            {
                var data = DataManagerSqlLite.GetProjects(false);
                RefreshComboBoxes<CBVProject>(ProjectCmbBox, data, "project_name");
            }
        }

        private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }

        }

        private void ProjectCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int SelectedIndex = ((ComboBox)sender).SelectedIndex + 1;

            var data = DataManagerSqlLite.GetVideoEvents(SelectedIndex, true);
            FillListBoxVideoEvent(data);
        }
        #endregion

        #region Create Project
        private void CreateProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("project_name", typeof(string));
            dataTable.Columns.Add("project_version", typeof(int));
            dataTable.Columns.Add("project_comments", typeof(string));
            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));
            dataTable.Columns.Add("fk_project_background", typeof(int));
            dataTable.Columns.Add("project_uploaded", typeof(int));
            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_archived", typeof(int));

            var row = dataTable.NewRow();
            row["project_name"] = ProjectNameTxtBox.Text != "" ? ProjectNameTxtBox.Text : "Project";
            row["project_version"] = 0;
            row["fk_project_background"] = 1;
            row["project_uploaded"] = 0;
            row["project_archived"] = 0;
            row["project_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dataTable.Rows.Add(row);

            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    if (showMessageBoxes)
                    {
                        MessageBox.Show("Project was populated to Database");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            InitializeDatabase();
            ProjectCmbBox.SelectedIndex = ProjectCmbBox.Items.Count - 1;
        }
        #endregion

        private void ScreenRecorder_Control_CancelClicked(object sender, EventArgs e)
        {

        }

        private void ScreenRecorder_Control_SaveClicked(object sender, SaveEventArgs e)
        {
            if (ProjectCmbBox.SelectedIndex != -1)
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
                        row["fk_videoevent_project"] = ProjectCmbBox.SelectedIndex + 1;
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

                        if (element.mediaType == MediaType.Video)
                        {
                            row["videoevent_duration"] = 0;
                            mediaId = 2;
                        }

                        row["fk_videoevent_media"] = mediaId;

                        row["fk_videoevent_screen"] = -1; // Not needed for this case

                        row["media"] = element.mediaData;

                        dataTable.Rows.Add(row);
                    }

                    selectionIndex = ProjectCmbBox.SelectedIndex + 1;

                    Create_Event(dataTable);

                    Recorder.Reset();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("Please Select a project");
            }
        }

        private void Recorder_Loaded(object sender, RoutedEventArgs e)
        {
            var displays = Recorder.GetDisplays();

            int i = 1;
            foreach (var display in displays) {
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

            InitializeDatabase();

            ProjectCmbBox.SelectedIndex = selectionIndex - 1;
        }
    }

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
