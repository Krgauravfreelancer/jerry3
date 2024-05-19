using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using ScreenRecorder_UserControl;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

namespace ScreenRecorder_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool IsSetUp = false;
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
            if (ProjectCmbBox.SelectedItem != null)
            {
                int Selected_ID = ((CBVProject)ProjectCmbBox.SelectedItem).project_id;

                var data = DataManagerSqlLite.GetVideoEvents(Selected_ID, true);
                FillListBoxVideoEvent(data);
            }
        }
        #endregion

        #region Create Project
        private void CreateProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("project_id", typeof(int));
            dataTable.Columns.Add("project_name", typeof(string));
            dataTable.Columns.Add("project_version", typeof(int));
            dataTable.Columns.Add("project_comments", typeof(string));
            dataTable.Columns.Add("project_uploaded", typeof(int));
            dataTable.Columns.Add("fk_project_background", typeof(int));
            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_archived", typeof(int));
            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));
            dataTable.Columns.Add("project_isdeleted", typeof(int));
            dataTable.Columns.Add("project_issynced", typeof(int));
            dataTable.Columns.Add("project_serverid", typeof(int));
            dataTable.Columns.Add("project_syncerror", typeof(string));

            dataTable.Rows.Clear();

            var row = dataTable.NewRow();


            row["project_name"] = ProjectNameTxtBox.Text != "" ? ProjectNameTxtBox.Text : "Project";
            row["project_version"] = 1;
            row["project_uploaded"] = 0;
            row["fk_project_background"] = 1;
            row["project_archived"] = 0;
            row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_isdeleted"] = 0;
            row["project_issynced"] = 0;
            row["project_serverid"] = 1;


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
            if (ProjectCmbBox.SelectedIndex != -1)
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

                        var SelectedProject = ProjectCmbBox.SelectedIndex;


                        foreach (Media element in mediaList)
                        {
                            var row = dataTable.NewRow();
                            row["fk_videoevent_project"] = ((CBVProject)ProjectCmbBox.SelectedItem).project_id;
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

                        ProjectCmbBox.SelectedIndex = SelectedProject;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    Recorder.Reset();
                    Recorder.IsEnabled = true;

                    InitializeDatabase();

                    ProjectCmbBox.SelectedIndex = ProjectCmbBox.Items.Count;
                }

            }
            else
            {
                MessageBox.Show("Please Select a project");
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
