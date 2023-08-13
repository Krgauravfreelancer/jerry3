using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

/// <summary>
/// ***************************************************************************************************************************************
/// **************************************************** DEVELOPER REMARKS ****************************************************************
/// ***************************************************************************************************************************************
/// ---------------------------------------------------- METHODS --------------------------------------------------------------------------
///     InitializeDatabase()
///         This method is called when control is loaded And It does the following work.  
///         Check if DB exists. If Not Create the Database using two parametes
///         Params encryptFlag - create DB is encryption mode, canCreateRegistryIfNotExists - Flag to create registry in case does not exist
///         Synchronise the APP, MEDIA AND SCREEN Tables
///         Populate some rows to Project table, controlled by PopulateProjectFlag in case not needed
///         Populate some rows to VideoEvent table, controlled by PopulateVideoEventFlag in case not needed
///     
///     DataManagerSqlLite.SyncApp(datatable)
///         Synchronise the APP Table with the latest data
///         datatable columns - app_id and app_name
///     
///     DataManagerSqlLite.GetApp()
///         To get APP master data
///         
///     DataManagerSqlLite.SyncMedia(datatable)
///         Synchronise the MEDIA Table with the latest data
///         datatable columns - media_id, media_name and media_color
///         
///     DataManagerSqlLite.GetMedia()
///         To get MEDIA master data
///         
///     DataManagerSqlLite.SyncScreen(datatable)
///         Synchronise the SCREEN Table with the latest data
///         datatable columns - screen_id, screen_name and screen_color
///         
///     DataManagerSqlLite.GetScreens()
///         To get Screen master data
///         
///     DataManagerSqlLite.GetProjects(archivedFlag)
///         Fetch all projects present in the database where archived flag is false / true
///         
///     DataManagerSqlLite.InsertRowsToProject(dataTable)
///         To insert the data to project table
///         datatable columns - id, project_name, project_version and project_comments, createdate and modifydate
///         returns project Id of the inserted record
///         
///     DataManagerSqlLite.GetVideoEvents(selectedprojectId, dependentTableFlag)
///         Fetch video events for the project Id provided. If dependentTableFlag is true, also fetched dependent/associated tables
///         
///     DataManagerSqlLite.InsertRowsToVideoEvent(dataTable)
///         To insert the data to videoevent table
///         datatable columns - videoevent_id, fk_videoevent_project, fk_videoevent_media, videoevent_track, videoevent_start and videoevent_duration, createdate and modifydate
///         ** conditional columns - media and fk_videoevent_screen
///         ** Also populate below table based upon media chosen
///             For image/video media - populate videosegment, for audio - populate audio and for notes - populate design table
///         Return video event id for inserted row
///         
///     DataManagerSqlLite.InsertRowsToHistory(datatable);
///         To insert the data to history table
///         datatable columns - history_id, fk_history_app, fk_history_project, history_version, history_note, createdate and modifydate
///         
/// **************************************************** DEVELOPER REMARKS END ************************************************************
/// </summary>
namespace SQLite_Wrapper
{
    public partial class MainWindow : Window
    {
        private bool IsSetUp = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetUp)
            {
                InitializeDatabase(sender, e);
                IsSetUp = true;
            }
            RefreshOrLoadComboBoxes(EnumEntity.ALL);
        }

        #region == Helper Functions ==

        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
            {
                UpdateStatus("Fetching Projects !!");
                var data = DataManagerSqlLite.GetProjects(false);
                RefreshComboBoxes<CBVProject>(cmbProjectForVideoEvent, data, "project_name");
                RefreshComboBoxes<CBVProject>(cmbProjectForHistory, data, "project_name");
                RefreshComboBoxes<CBVProject>(cmbProjectForHlsts, data, "project_name");
            }
            
            if (entity == EnumEntity.ALL || entity == EnumEntity.SCREEN)
            {
                UpdateStatus("Fetching Screens !!");
                var data = DataManagerSqlLite.GetScreens();
                RefreshComboBoxes<CBVScreen>(cmbScreenForVideoEvent, data, "screen_name");
            }

            if (entity == EnumEntity.ALL || entity == EnumEntity.MEDIA)
            {
                UpdateStatus("Fetching Medias !!");
                var data = DataManagerSqlLite.GetMedia();
                RefreshComboBoxes<CBVMedia>(cmbMediaForVideoEvent, data, "media_name");
            }

           if (entity == EnumEntity.ALL || entity == EnumEntity.VIDEOEVENT)
            {
                UpdateStatus("Fetching Video Event !!");
                var data = DataManagerSqlLite.GetVideoEvents(-1, false);
                RefreshComboBoxes<CBVVideoEvent>(cmbVideoEventForNotes, data, "videoevent_id");
            }

            if (entity == EnumEntity.ALL || entity == EnumEntity.APP)
            {
                UpdateStatus("Fetching APP !!");
                var data = DataManagerSqlLite.GetApp();
                RefreshComboBoxes<CBVApp>(cmbAppForHistory, data, "app_name");
            }

            UpdateStatus();
        }

        private void UpdateStatus(string status = "") // Not working for now, Parked
        {
            lblStatus.Content = status;
            lblStatus.Visibility = Visibility.Hidden;
        }

        private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            lblStatus.Content = "Adding Items to combo !!";
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }

        }

        private void InitializeDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false);
                MessageBox.Show(message + ", syncing lookup tables now !!");
                SyncApp();
                SyncMedia();
                SyncScreen();
                MessageBox.Show("lookup tables synced successfully !!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }


        #endregion

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message);
        }



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

        #endregion

        #region == Sync Clicks ==

        private void BtnSyncApp_Click(object sender, RoutedEventArgs e)
        {
            SyncApp();
        }

        private void BtnSyncMedia_Click(object sender, RoutedEventArgs e)
        {
            SyncMedia();
        }

        private void BtnSyncScreen_Click(object sender, RoutedEventArgs e)
        {
            SyncScreen();
        }

        #endregion

        private void BtnLoadProjectManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var uc = new ManageProjectsContainer();

                var showdialog = uc.ShowDialog();
                if (showdialog.HasValue)
                {
                    if (uc.buildProjectControl.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        // read the public method that returns the datatable
                        var dt = uc.buildProjectControl.dataTable;
                        if (dt != null)
                        {
                            var insertedId = DataManagerSqlLite.InsertRowsToProject(dt);
                            if (insertedId > -1)
                            {
                                MessageBox.Show("Project Added to Database");
                                RefreshOrLoadComboBoxes(EnumEntity.PROJECT);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BtnInsertHistoryData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("history_id", typeof(int));
                datatable.Columns.Add("fk_history_app", typeof(int));
                datatable.Columns.Add("fk_history_project", typeof(int));
                datatable.Columns.Add("history_version", typeof(int));
                datatable.Columns.Add("history_note", typeof(string));
                datatable.Columns.Add("history_createdate", typeof(string));
                datatable.Columns.Add("history_modifydate", typeof(string));

                var row = datatable.NewRow();
                row["history_id"] = -1;
                row["fk_history_app"] = ((CBVApp)cmbAppForHistory.SelectedItem)?.app_id ?? 0; ;
                row["fk_history_project"] = ((CBVProject)cmbProjectForHistory.SelectedItem)?.project_id ?? 0; ;
                row["history_version"] = 1;
                row["history_note"] = "Some Random history Notes";
                row["history_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["history_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                datatable.Rows.Add(row);

                var insertedId = DataManagerSqlLite.InsertRowsToHistory(datatable);
                if (insertedId > -1) MessageBox.Show("History Row Added to Database");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        #region == Video Event Insert ==

        private void BtnInsertVideoEventData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var datatable = new DataTable();
                datatable.Columns.Add("videoevent_id", typeof(int));
                datatable.Columns.Add("fk_videoevent_project", typeof(int));
                datatable.Columns.Add("fk_videoevent_media", typeof(int));
                datatable.Columns.Add("videoevent_track", typeof(int));
                datatable.Columns.Add("videoevent_start", typeof(int));
                datatable.Columns.Add("videoevent_duration", typeof(int));
                datatable.Columns.Add("videoevent_createdate", typeof(string));
                datatable.Columns.Add("videoevent_modifydate", typeof(string));
                //optional column
                datatable.Columns.Add("media", typeof(byte[])); // Media Column
                datatable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen

                var row = datatable.NewRow();

                row["videoevent_id"] = -1;
                row["fk_videoevent_project"] = ((CBVProject)cmbProjectForVideoEvent.SelectedItem)?.project_id ?? 0;

                row["videoevent_track"] = 1;
                row["videoevent_start"] = 1;
                row["videoevent_duration"] = 10;
                row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var selectedMedia = (CBVMedia)cmbMediaForVideoEvent.SelectedItem;
                row["fk_videoevent_media"] = selectedMedia != null ? selectedMedia.media_id : 0;
                if (selectedMedia != null)
                {
                    if (selectedMedia.media_name == "form") //select screen if media is form
                    {
                        row["fk_videoevent_screen"] = ((CBVScreen)cmbScreenForVideoEvent.SelectedItem)?.screen_id ?? 0;
                        row["media"] = null;
                    }
                    else
                    {
                        //Fill Media in case image, video or audio is selected
                        row["fk_videoevent_screen"] = -1; // Not needed for this case
                        var currentDirectory = Directory.GetCurrentDirectory();
                        if (selectedMedia.media_name == "audio")
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Audio1.mp3";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if(selectedMedia.media_name == "image")
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Image1.jpg";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if(selectedMedia.media_name == "video")
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Screencast1.mp4";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                    }
                }
                datatable.Rows.Add(row);

                var insertedIds = DataManagerSqlLite.InsertRowsToVideoEvent(datatable);
                if (insertedIds.Count > 0)
                {
                    MessageBox.Show("VideoEvent Row Added to Database");
                    RefreshOrLoadComboBoxes(EnumEntity.VIDEOEVENT);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void cmbMediaForVideoEvent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var name = ((CBVMedia)cmbMediaForVideoEvent.SelectedItem)?.media_name;
            if (name == "form")
            {
                lblMediaInfo.Visibility = Visibility.Visible;
                cmbScreenForVideoEvent.Visibility = Visibility.Visible;
                lblChooseScreen.Visibility = Visibility.Visible;
            }
            else
            {
                cmbScreenForVideoEvent.SelectedIndex = -1;
                lblMediaInfo.Visibility = Visibility.Hidden;
                cmbScreenForVideoEvent.Visibility = Visibility.Hidden;
                lblChooseScreen.Visibility = Visibility.Hidden;
            }

        }

        #endregion


        #region == Group Box For Design, Audio, Hlsts and Notes ==

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

        /*

        private void BtnInsertDesign_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var datatable = new DataTable();

                    datatable.Columns.Add("design_id", typeof(int));
                    datatable.Columns.Add("fk_design_videoevent", typeof(int));
                    datatable.Columns.Add("design_xnk", typeof(string));
                    datatable.Columns.Add("design_createdate", typeof(string));
                    datatable.Columns.Add("design_modifydate", typeof(string));

                    var row = datatable.NewRow();
                    row["design_id"] = -1;
                    row["fk_design_videoevent"] = ((CBVVideoEvent)cmbVideoEventForChildren.SelectedItem)?.videoevent_id ?? 0;
                    row["design_xnk"] = "Sample design XNK";
                    row["design_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["design_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    datatable.Rows.Add(row);

                var insertedId = DataManagerSqlLite.InsertRowsToDesign(datatable);
                if (insertedId > -1) MessageBox.Show("Design Row Added to Database");
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BtnInsertAudio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Read audio Bytes into a byte array
                var path = @"C:\\commercialBase\\sample2.mp3";
                using(var fileStream = new FileStream(path, FileMode.Open))
                {
                    byte[] mp3Byte = streamToByteArray(fileStream, 0);


                    var datatable = new DataTable();

                    datatable.Columns.Add("audio_id", typeof(int));
                    datatable.Columns.Add("fk_audio_videoevent", typeof(int));
                    datatable.Columns.Add("audio_media", typeof(byte[]));
                    datatable.Columns.Add("audio_createdate", typeof(string));
                    datatable.Columns.Add("audio_modifydate", typeof(string));

                    var row = datatable.NewRow();
                    row["audio_id"] = -1;
                    row["fk_audio_videoevent"] = ((CBVVideoEvent)cmbVideoEventForChildren.SelectedItem)?.videoevent_id ?? 0;
                    row["audio_media"] = mp3Byte;
                    row["audio_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["audio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    datatable.Rows.Add(row);

                    var insertedId = DataManagerSqlLite.InsertRowsToAudio(datatable);
                    if (insertedId > -1) MessageBox.Show("Audio Row Added to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        */
        #endregion

        #region == Hlsts Insert ==

        private void BtnInsertHlsts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Read hlsts Bytes into a byte array
                var currentDirectory = Directory.GetCurrentDirectory();
                var path = $"{currentDirectory}\\MediaFiles\\Image1.jpg";
                using (var fileStream = new FileStream(path, FileMode.Open))
                { 
                    byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                    var datatable = new DataTable();

                    datatable.Columns.Add("hlsts_id", typeof(int));
                    datatable.Columns.Add("fk_hlsts_project", typeof(int));
                    datatable.Columns.Add("hlsts_media480", typeof(byte[]));
                    datatable.Columns.Add("hlsts_media720", typeof(byte[]));
                    datatable.Columns.Add("hlsts_media1080", typeof(byte[]));
                    datatable.Columns.Add("hlsts_media1280", typeof(byte[]));
                    datatable.Columns.Add("hlsts_createdate", typeof(string));
                    datatable.Columns.Add("hlsts_modifydate", typeof(string));

                    var row = datatable.NewRow();
                    row["hlsts_id"] = -1;
                    row["fk_hlsts_project"] = ((CBVProject)cmbProjectForHlsts.SelectedItem)?.project_id ?? 0;
                    row["hlsts_media480"] = mp3Byte;
                    row["hlsts_media720"] = mp3Byte;
                    row["hlsts_media1080"] = mp3Byte;
                    row["hlsts_media1280"] = mp3Byte;
                    row["hlsts_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["hlsts_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    datatable.Rows.Add(row);

                    var insertedId = DataManagerSqlLite.InsertRowsToHLSTS(datatable);
                    if (insertedId > -1) MessageBox.Show("HLSTS Row Added to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        #endregion

        #region == Notes Insert ==
        private void BtnInsertNotes_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                var notes = "Sample notes line";
                var datatable = new DataTable();

                datatable.Columns.Add("notes_id", typeof(int));
                datatable.Columns.Add("fk_notes_videoevent", typeof(int));
                datatable.Columns.Add("notes_line", typeof(string));
                datatable.Columns.Add("notes_wordcount", typeof(int));
                datatable.Columns.Add("notes_createdate", typeof(string));
                datatable.Columns.Add("notes_modifydate", typeof(string));

                var row = datatable.NewRow();
                row["notes_id"] = -1;
                row["fk_notes_videoevent"] = ((CBVVideoEvent)cmbVideoEventForNotes.SelectedItem)?.videoevent_id ?? 0;
                row["notes_line"] = notes;
                row["notes_wordcount"] = notes.Length;
                row["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                datatable.Rows.Add(row);

                var insertedId = DataManagerSqlLite.InsertRowsToNotes(datatable);
                if (insertedId > -1) MessageBox.Show("Notes Row Added to Database");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


    }
}
