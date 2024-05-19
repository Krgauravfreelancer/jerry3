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
using Timeline.UserControls.Models;

namespace TimelineWrapper.Windows
{
    /// <summary>
    /// Interaction logic for ScreenRecorderWindow.xaml
    /// </summary>
    public partial class ScreenRecorderWindow : Window
    {

        int selectionIndex = 0;
        bool showMessageBoxes = false;

        private readonly MainWindow _mainWindow;
        public event Action<DataTable> BtnSaveClickedEvent;

        public ScreenRecorderWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;


            RefreshOrLoadComboBoxes(EnumEntity.ALL);
            
        }

        private void ScreenRecorderWindow_Closed(object sender, EventArgs e)
        {
            _mainWindow.Show(); // Show the main window when the NewWindow is closed
        }

        #region ListBox and ComboBox

        //private void FillListBoxVideoEvent(List<CBVVideoEvent> source)
        //{
        //    listBoxVideoEvent.SelectedItem = null;
        //    listBoxVideoEvent.Items.Clear();
        //    foreach (var item in source)
        //    {
        //        var itemExtended = new VideoEventExtended(item)
        //        {
        //            Start = item.videoevent_start,
        //            ClipDuration = item.videoevent_duration.ToString() + " sec",
        //        };
        //        if (item.audio_data != null && item.audio_data.Count > 0 && item.fk_videoevent_media == 3)
        //        {
        //            itemExtended.MediaName = "Audio";
        //        }
        //        else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 1)
        //        {
        //            itemExtended.MediaName = "Image";
        //        }
        //        else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 2)
        //        {
        //            itemExtended.MediaName = "Video";
        //        }
        //        else if (item.design_data != null && item.design_data.Count > 0 && item.fk_videoevent_media == 4)
        //        {
        //            itemExtended.MediaName = "Design";
        //        }
        //        else
        //        {
        //            itemExtended.MediaName = "None";
        //        }
        //        listBoxVideoEvent.Items.Add(itemExtended);
        //    }
        //}

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

            //int SelectedIndex = ((ComboBox)sender).SelectedIndex + 1;

            //var data = DataManagerSqlLite.GetVideoEvents(SelectedIndex, true);
            //FillListBoxVideoEvent(data);
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

            //InitializeDatabase();
            ProjectCmbBox.SelectedIndex = ProjectCmbBox.Items.Count - 1;
        }
        #endregion

        private void ScreenRecorder_Control_CancelClicked(object sender, EventArgs e)
        {
            /// Here we will close the ScreenRecorder and ignore any video or changes made.
            Close();
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
                    dataTable.Columns.Add("videoevent_end", typeof(string));//added by me
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

                    //Create_Event(dataTable);


                    Recorder.Reset();

                    ForwardDataTable(dataTable);
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

            //InitializeDatabase();

            ProjectCmbBox.SelectedIndex = selectionIndex - 1;
        }




        #region Wrapper methods
        private void ForwardDataTable(DataTable dataTable) { 
            if(BtnSaveClickedEvent != null)
            {
                BtnSaveClickedEvent(dataTable);
                this.Close();
            }
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
