using Sqllite_Library.Business;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace VideoCreator.Helpers
{
    public static class InitialiseDbHelper
    {
        #region == Helper Functions ==

        public static void InitializeDatabase()
        {
            DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
            SyncApp();
            SyncMedia();
            SyncScreen();
            SyncResolution();
            SyncCompany(); // TBD 
            if (DataManagerSqlLite.GetProjectsCount() == 0)
                PopulateProjectTable();

            if (DataManagerSqlLite.GetBackgroundsCount() == 0)
                PopulateBackgroundTable();

        }

        #endregion

        #region == Sync Functions ==

        private static void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(bool));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = true;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = false;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = false;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = false;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = false;
                datatable.Rows.Add(row5);
                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SyncMedia()
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

        private static void SyncScreen()
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

        private static void SyncResolution()
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

        private static void SyncCompany()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("company_id", typeof(int));
                datatable.Columns.Add("company_name", typeof(string));

                var row = datatable.NewRow();
                row["company_id"] = -1;
                row["company_name"] = "CommercialBase";
                datatable.Rows.Add(row);
                var insertedIds = DataManagerSqlLite.SyncCompany(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region == Populate functions ==

        private static void PopulateProjectTable()
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
                dataTable.Columns.Add("project_uploaded", typeof(bool));
                dataTable.Columns.Add("project_archived", typeof(bool));
                dataTable.Columns.Add("fk_project_company", typeof(int));
                dataTable.Columns.Add("project_date", typeof(string));

                for (var i = 1; i <= 1; i++) // Made 1 rows to check DataGrid fuctionality
                {
                    var row = dataTable.NewRow();
                    row["id"] = 1;
                    row["project_name"] = $"Sample Project - {i}";
                    row["project_version"] = i;
                    row["project_uploaded"] = false;
                    row["project_archived"] = false;
                    row["fk_project_company"] = 1;
                    row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["project_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show("Projects populated to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private static void PopulateBackgroundTable()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("background_id", typeof(int));
                dataTable.Columns.Add("fk_background_company", typeof(int));
                dataTable.Columns.Add("background_media", typeof(byte[]));
                dataTable.Columns.Add("background_active", typeof(bool));

                var row = dataTable.NewRow();
                row["background_id"] = -1;
                row["fk_background_company"] = 1;
                row["background_active"] = 1;

                var currentDirectory = Directory.GetCurrentDirectory();

                var path = $"{currentDirectory}\\Images\\white_background.png";
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                    row["background_media"] = mp3Byte;
                }
                dataTable.Rows.Add(row);

                var row2 = dataTable.NewRow();
                row2["background_id"] = -1;
                row2["fk_background_company"] = 1;
                row2["background_active"] = 0;

                path = $"{currentDirectory}\\Images\\black_background.png";
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                    row2["background_media"] = mp3Byte;
                }
                dataTable.Rows.Add(row2);

                var insertedId = DataManagerSqlLite.InsertRowsToBackground(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show("Background images populated to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
    }
}
