using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;

/// <summary>
/// ***************************************************************************************************************************************
/// **************************************************** WRAPPER REMARKS ******************************************************************
/// ***************************************************************************************************************************************
/// ---------------------------------------------------------------------------------------------------------------------------------------
/// ------------------------------------------------ Wrapper Helper Methods ---------------------------------------------------------------
/// ---------------------------------------------------------------------------------------------------------------------------------------
///    
///   Introduction
///       The Wrapper contains several temporary methods only used to populate the wrapper application's database. In production,
///       all data is loaded from the server. You should not need to change any of these helper methods. If any changes are needed
///       please let us know.
///       
/// ---------------------------------------------------------------------------------------------------------------------------------------
/// --------------------------------------------- Database User Control Public Methods ----------------------------------------------------
/// ---------------------------------------------------------------------------------------------------------------------------------------
///     These are the methods that you will need for your user control:    
///     
///     1.Need to sync App table data database
///     DataManagerSqlLite.SyncApp(datatable)
///         - Comments: Synchronise the APP Table with the latest data
///         - Parameters: datatable
///                       columns - app_id and app_name
///         - Returns: None
///     
///     2.Need to sync MEDIA table data database
///     DataManagerSqlLite.SyncMedia(datatable)
///         - Comments: Synchronise the MEDIA Table with the latest data
///         - Parameters: datatable
///                       Columns - media_id, media_name and media_color
///         - Returns: None
///         
///     3.Need to sync SCREEN table data database         
///     DataManagerSqlLite.SyncScreen(datatable)
///         - Comments: Synchronise the SCREEN Table with the latest data
///         - Parameters: datatable
///                       Columns - screen_id, screen_name and screen_color
///         - Returns: None
///       
///     4.Need to sync RESOLUTION table data database         
///     DataManagerSqlLite.SyncResolution(datatable)
///         - Comments: Synchronise the RESOLUTION Table with the latest data
///         - Parameters: datatable
///                       Columns - resolution_id, resolution_name
///         - Returns: None
///       
///     5. Query VideoEvents rows
///     DataManagerSqlLite.GetVideoEvents(selectedprojectId, dependentTableFlag)
///         - Comments: Fetch video events for the project Id provided. 
///         - Parameters: selectedprojectId - project Id for which videoevent needs to be fetched
///                       dependentTableFlag - If true, also fetched dependent/associated tables. See ** tables
///         - Returns - List<CBVVideoEvent>. Fields are as below -
///                     int videoevent_id
///                     int fk_videoevent_project
///                     int fk_videoevent_media
///                     int videoevent_track
///                     string videoevent_start - HH:mm:ss
///                     int videoevent_duration
///                     DateTime videoevent_createdate
///                     DateTime videoevent_modifydate
///                     **List<CBVAudio> audio_data - associated table data
///                     **List<CBVVideoSegment> videosegment_data - associated table data
///                     **List<CBVDesign> design_data - associated table data
///      
///     6. Query Media rows
///     DataManagerSqlLite.GetMedia()
///         - Comments: Fetch media table master data. 
///         - Parameters: NA
///         - Returns - List<CBVMedia>. Fields are as below -
///                     int media_id
///                     string media_name
///                     string media_color
///      
///     7. Query VideoSegment Table for image/Video files
///     DataManagerSqlLite.GetVideoSegment(int VideoEventId = -1)
///         - Comments: Fetch all videosegment present in the database 
///         - VideoEventId: VideoEventId is -1 then fetch all rows else dependent videosegment rows for the videoevent id passed
///         - Returns - List<CBVVideoSegment>. Fields are as below -
///                     int - videosegment_id
///                     int fk_videosegment_videoevent
///                     byte[] videosegment_media
///                     DateTime videosegment_createdate
///                     DateTime videosegment_modifydate
///                     
///     8. Insert VideoSegment Table for image/Video files
///         ** converting files to blob (byte[]) and passing
///         DataManagerSqlLite.InsertRowsToVideoSegment(dataTable)
///         -Comments - To add data to video Segment table
///         -Parameters - datatable
///             Columns -videosegment_id, fk_videosegment_videoevent, videosegment_media, videosegment_createdate, videosegment_modifydate
///          - Returns - Inserted VideoSegment Id
///          
///     9. Update VideoSegment Table for image/Video files
///         ** converting files to blob (byte[]) and passing
///         DataManagerSqlLite.UpdateRowsToVideoSegment(dataTable)
///         -Comments - To add data to video Segment table
///         -Parameters - datatable
///             Columns -videosegment_id, fk_videosegment_videoevent, videosegment_media, videosegment_modifydate
///          - Returns - None
///          
///     
/// **************************************************** DEVELOPER REMARKS END ************************************************************
/// </summary>


namespace DesignerImageDB_Wrapper
{
    public partial class MainWindow : Window
    {
        private bool IsSetUp = false;
        private bool PopulateVideoEventFlag = true; // Should be  handled as per need, keeping true for now
        private bool PopulateProjectFlag = true; // Should be  handled as per need, keeping true for now

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetUp)
            {
                InitializeDatabase();
                IsSetUp = true;
            }
        }

        #region == Helper Functions ==

        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                MessageBox.Show(message + ", syncing lookup tables !!");
                SyncApp();
                SyncMedia();
                SyncScreen();
                SyncResolution();
                MessageBox.Show("lookup tables synced successfully !!");
                if (PopulateProjectFlag)
                {
                    if (DataManagerSqlLite.GetProjectsCount() == 0)
                        PopulateProjectTable();
                }

                if (PopulateVideoEventFlag)
                {
                    if (DataManagerSqlLite.GetVideoEventsCount() == 0)
                        PopulateVideoEventTable();
                }
                RefreshOrLoadComboBoxes(EnumEntity.ALL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        #endregion

        #region == Sync Functions ==

        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                datatable.Rows.Add(row4);
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

        private void SyncResolution()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("resolution_id", typeof(int));
                datatable.Columns.Add("resolution_name", typeof(string));

                var row = datatable.NewRow();
                row["resolution_id"] = -1;
                row["resolution_name"] = "480px";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["resolution_id"] = -1;
                row2["resolution_name"] = "720px";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["resolution_id"] = -1;
                row3["resolution_name"] = "1080px";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["resolution_id"] = -1;
                row4["resolution_name"] = "1280px";
                datatable.Rows.Add(row4);
                var insertedIds = DataManagerSqlLite.SyncResolution(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region == Populate functions ==

        private void PopulateProjectTable()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("id", typeof(int));
                dataTable.Columns.Add("project_name", typeof(string));
                dataTable.Columns.Add("project_version", typeof(int));
                dataTable.Columns.Add("project_comments", typeof(string));
                dataTable.Columns.Add("project_createdate", typeof(string));
                dataTable.Columns.Add("project_modifydate", typeof(string));

                for (var i = 1; i <= 5; i++)
                {
                    var row = dataTable.NewRow();
                    row["id"] = 1;
                    row["project_name"] = $"Sample Project - {i}";
                    row["project_version"] = i;
                    row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show("Projects populated to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void PopulateVideoEventTable()
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
                //optional column
                dataTable.Columns.Add("media", typeof(byte[])); // Media Column
                dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen
                for (var i = 0; i < 16; i++)
                {
                    // Since this table has Referential Integrity, so lets push one by one
                    dataTable.Rows.Clear();
                    var row = dataTable.NewRow();
                    row["videoevent_id"] = -1;
                    row["fk_videoevent_project"] = (i % 5) + 1;
                    row["videoevent_track"] = 1;
                    row["videoevent_start"] = "00:15:00";
                    row["videoevent_duration"] = 10;
                    row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    var mediaId = (i % 4) + 1;
                    row["fk_videoevent_media"] = mediaId;

                    if (mediaId == 4) //select screen if media is form
                    {
                        row["fk_videoevent_screen"] = 1; // Hardcoded 1 for now
                        row["media"] = null;
                    }
                    else
                    {
                        //Fill Media in case image, video or audio is selected
                        row["fk_videoevent_screen"] = -1; // Not needed for this case
                        var currentDirectory = Directory.GetCurrentDirectory();
                        if (mediaId == 1)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Image1.jpg";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if (mediaId == 2)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Screencast1.mp4";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if (mediaId == 3)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Audio1.mp3";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                    }
                    dataTable.Rows.Add(row);
                    var insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);
                }
                MessageBox.Show("VideoEvent Table populated to Database");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private static byte[] StreamToByteArray(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        #endregion

        #region == Events ==

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message);
        }

        private void cmbMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedMedia = (CBVMedia)cmbMedia.SelectedItem;
            if (selectedMedia?.media_name == "image")
            {
                btnAddImage.IsEnabled = true;
                int i = 0;
                var imageData = new Dictionary<int, string>
                {
                    { i++, "Image1.jpg" },
                    { i++, "Image2.jpg" },
                    { i++, "Image3.jpg" },
                    { i++, "Image4.jpg" },
                    { i++, "Image5.jpg" }
                   
                };
                RefreshTempComboBoxes(cmbBlob, imageData);
            }
            else if (selectedMedia?.media_name == "video")
            {
                btnAddImage.IsEnabled = false;
                var videoData = new Dictionary<int, string>();
                RefreshTempComboBoxes(cmbBlob, videoData);
            }
            else
            {
                btnAddImage.IsEnabled = false;
                var noData = new Dictionary<int, string> ();
                RefreshTempComboBoxes(cmbBlob, noData);
            }
        }

        
        private void BtnInsertImage_Click(object sender, RoutedEventArgs e)
        {
            InsertImageOrVideoToDB();
        }

        private void InsertImageOrVideoToDB()
        {
            try
            {
                var dtVideoSegment = new DataTable();
                dtVideoSegment.Columns.Add("videosegment_id", typeof(int));
                dtVideoSegment.Columns.Add("fk_videosegment_videoevent", typeof(int));
                dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
                dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
                dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));

                var rowVideoSegment = dtVideoSegment.NewRow();
                rowVideoSegment["videosegment_id"] = -1;
                rowVideoSegment["fk_videosegment_videoevent"] = ((CBVVideoEvent)cmbVideoEvent.SelectedItem)?.videoevent_id ?? 0;
                rowVideoSegment["videosegment_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                rowVideoSegment["videosegment_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (cmbBlob.SelectedIndex > -1)
                {
                    var currentDirectory = Directory.GetCurrentDirectory();
                    var path = $"{currentDirectory}\\MediaFiles\\{cmbBlob.SelectedItem}";
                    using (var fileStream = new FileStream(path, FileMode.Open))
                    {
                        byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                        rowVideoSegment["videosegment_media"] = mp3Byte;
                    }
                }
                else
                {
                    throw new Exception("Please select blob file");
                }

                dtVideoSegment.Rows.Add(rowVideoSegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment);
                if (insertedVideoSegmentId > 0)
                {
                    MessageBox.Show("Row Added to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        #endregion == Events ==

        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.MEDIA)
            {
                var data = DataManagerSqlLite.GetMedia();
                RefreshComboBoxes<CBVMedia>(cmbMedia, data, "media_name");
            }

            if (entity == EnumEntity.ALL || entity == EnumEntity.VIDEOEVENT)
            {
                var data = DataManagerSqlLite.GetVideoEvents(-1, false);
                RefreshComboBoxes<CBVVideoEvent>(cmbVideoEvent, data, "videoevent_id");
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

        private void RefreshTempComboBoxes(System.Windows.Controls.ComboBox combo, Dictionary<int, string> source)
        {
            combo.Items.Clear();
            combo.SelectedItem = null;
            foreach (var item in source)
            {
                combo.Items.Add(item.Value);
            }
        }

        
    }
}