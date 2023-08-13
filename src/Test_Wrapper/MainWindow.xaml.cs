using System;
using System.Data;
using System.IO;
using System.Windows;
using Sqllite_Library.Business;

namespace Test_Wrapper
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
                dataTable.Columns.Add("videoevent_start", typeof(int));
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
                    row["videoevent_start"] = 1;
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

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message);
        }

        private void BtnQueryApp_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetApp();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "App Table Data");
        }

        private void BtnQueryMedia_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetMedia();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "Media Table Data");
        }

        private void BtnQueryScreen_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetScreens();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "Screen Table Data");
        }

        private void BtnQueryResolution_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetResolution();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "Resolution Table Data");
        }

        private void BtnQueryProject_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetProjects();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "Projects Table Data");
        }

        private void BtnQueryVideoEvent_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetVideoEvents();
            var stringText = "";
            foreach (var item in data)
            {
                stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
            }
            MessageBox.Show(stringText, "VideoEvent Table Data");
        }

        private void BtnQueryAudio_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetAudio();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "Audio Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "Audio Table Data");
            }
        }
        private void BtnQueryVideoSegment_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetVideoSegment();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "VideoSegment Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "VideoSegment Table Data");
            }
        }

        private void BtnQueryDesign_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetDesign();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "Design Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "Design Table Data");
            }
        }

        private void BtnQueryNotes_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetNotes();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "Notes Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "Notes Table Data");
            }
        }

        private void BtnQueryHsts_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetHlsts();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "HLSTS Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "HLSTS Table Data");
            }
        }

        private void BtnQueryStreamts_Click(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetStreamts();
            if (data?.Count > 0)
            {
                var stringText = "";
                foreach (var item in data)
                {
                    stringText += item.ToString() + Environment.NewLine + "------------------------" + Environment.NewLine;
                }
                MessageBox.Show(stringText, "STREAMTS Table Data");
            }
            else
            {
                MessageBox.Show("No Data", "STREAMTS Table Data");
            }
        }





    }
}
