﻿using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using Sqllite_Library.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Shapes;
using VideoCreator.Auth;

namespace VideoCreator.Helpers
{
    public static class SyncDbHelper
    {
        #region == Helper Functions ==

        public static void InitializeDatabase()
        {
            DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
            //SyncScreen();
            //SyncCompany(); // TBD 



            //if (DataManagerSqlLite.GetProjectsCount() == 0)
            //    PopulateProjectTable();

            //if (DataManagerSqlLite.GetVoiceTimerCount() == 0)
            //{
            //    SyncVoiceTimer();
            //    var data = DataManagerSqlLite.GetVoiceTimers(); // For testing purpose to see if all functions are working
            //    Console.WriteLine(data.Count);
            //}
        }

        #endregion

        #region == Sync Functions ==

        public static void SyncApp(AppModel data)
        {
            InitializeDatabase();
            var datatable = new DataTable();
            datatable.Columns.Add("app_id", typeof(int));
            datatable.Columns.Add("app_name", typeof(string));
            datatable.Columns.Add("app_active", typeof(bool));
            foreach (PropertyInfo prop in data.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var value = Convert.ToInt32(prop.GetValue(data));
                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = prop.Name.ToLower();
                row["app_active"] = value == 1;
                datatable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToApp(datatable);

        }

        public static void SyncMedia(List<MediaModel> data)
        {
            InitializeDatabase();
            var datatable = new DataTable();
            datatable.Columns.Add("media_id", typeof(int));
            datatable.Columns.Add("media_name", typeof(string));
            datatable.Columns.Add("media_color", typeof(string));
            foreach (var item in data)
            {
                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = item.media_name.ToLower();
                row["media_color"] = item.media_color;
                datatable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToMedia(datatable);
        }

        public static void SyncScreen(List<ScreenModel> data)
        {
            InitializeDatabase();
            var datatable = new DataTable();
            datatable.Columns.Add("screen_id", typeof(int));
            datatable.Columns.Add("screen_name", typeof(string));
            datatable.Columns.Add("screen_color", typeof(string));
            foreach (var item in data)
            {
                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = item.screen_name.ToLower();
                row["screen_color"] = item.screen_color;
                datatable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToScreen(datatable);
        }

        public static void SyncCompany(List<CompanyModel> data)
        {
            var datatable = new DataTable();
            datatable.Columns.Add("company_id", typeof(int));
            datatable.Columns.Add("company_name", typeof(string));
            foreach (var item in data)
            {
                var row = datatable.NewRow();
                row["company_id"] = item.company_id;
                row["company_name"] = item.company_name;
                datatable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToCompany(datatable);
        }

        public static async void SyncBackground(List<BackgroundModel> data, AuthAPIViewModel authApiViewModel)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("background_id", typeof(int));
            dataTable.Columns.Add("fk_background_company", typeof(int));
            dataTable.Columns.Add("background_media", typeof(byte[]));
            dataTable.Columns.Add("background_active", typeof(bool));

            var currentDirectory = Directory.GetCurrentDirectory();

            foreach (var item in data)
            {
                var row = dataTable.NewRow();
                row["background_id"] = item.backgrounds_id;
                row["fk_background_company"] = item.fk_backgrounds_company;
                row["background_active"] = item.backgrounds_active;
                if (!string.IsNullOrEmpty(item.backgrounds_url) && item.backgrounds_url.IndexOf('.') > -1)
                    row["background_media"] = await authApiViewModel.GetSecuredFileByteArray(item.backgrounds_url);//GetSecuredFileByteArray
                dataTable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToBackground(dataTable);
        }

        public static void SyncProject_Insert(ProjectModel data)
        {
            InitializeDatabase();
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("project_id", typeof(int));
            dataTable.Columns.Add("project_name", typeof(string));
            dataTable.Columns.Add("project_version", typeof(int));
            dataTable.Columns.Add("project_comments", typeof(string));
            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));
            dataTable.Columns.Add("project_uploaded", typeof(bool));
            dataTable.Columns.Add("project_archived", typeof(bool));
            dataTable.Columns.Add("fk_project_background", typeof(int));
            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_issynced", typeof(bool));
            dataTable.Columns.Add("project_serverid", typeof(Int64));
            dataTable.Columns.Add("project_syncerror", typeof(string));

            var row = dataTable.NewRow();
            row["project_id"] = -1;
            row["project_name"] = data.project_name;
            row["project_version"] = data.project_version;
            row["project_uploaded"] = false;
            row["project_archived"] = data.project_archived;
            row["fk_project_background"] = 1;
            row["project_createdate"] = data.project_createdate;
            row["project_modifydate"] = data.project_modifydate;
            row["project_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            row["project_issynced"] = true;
            row["project_serverid"] = data.project_id;
            row["project_syncerror"] = "";

            dataTable.Rows.Add(row);
            var insertedId = DataManagerSqlLite.UpsertRowsToProject(dataTable);
            if (insertedId > -1)
            {
                //MessageBox.Show("Projects populated to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }






        private static void SyncVoiceTimer()
        {
            var textToInsert = string.Empty;
            var datatable = new DataTable();

            datatable.Columns.Add("voicetimer_id", typeof(int));
            datatable.Columns.Add("voicetimer_line", typeof(string));
            datatable.Columns.Add("voicetimer_wordcount", typeof(int));
            datatable.Columns.Add("voicetimer_index", typeof(int));

            var row = datatable.NewRow();
            textToInsert = "Accounting is how you get a clear picture of your financial position. It tells you whether or not you’re making a profit, what your cash flow is, what the current value of your company’s assets and liabilities is, and which parts of your business are actually making money.";
            row["voicetimer_id"] = -1;
            row["voicetimer_line"] = textToInsert;
            row["voicetimer_wordcount"] = textToInsert.Trim().Split(' ').Length;
            row["voicetimer_index"] = 1;
            datatable.Rows.Add(row);

            var row2 = datatable.NewRow();
            textToInsert = "Accounting and bookkeeping are both part of the same process: keeping your financial records in order. However, bookkeeping is more concerned with recording everyday financial transactions and operations, while accounting puts that financial data to good use through analysis, strategy, and tax planning.";
            row2["voicetimer_id"] = -1;
            row2["voicetimer_line"] = textToInsert;
            row2["voicetimer_wordcount"] = textToInsert.Trim().Split(' ').Length;
            row2["voicetimer_index"] = 2;
            datatable.Rows.Add(row2);

            var row3 = datatable.NewRow();
            textToInsert = "Accounting begins with recording transactions. Business transactions need to be put into your company’s general ledger. Recording business transactions this way is part of bookkeeping.";
            row3["voicetimer_id"] = -1;
            row3["voicetimer_line"] = textToInsert;
            row3["voicetimer_wordcount"] = textToInsert.Trim().Split(' ').Length;
            row3["voicetimer_index"] = 3;
            datatable.Rows.Add(row3);

            var row4 = datatable.NewRow();
            textToInsert = "Bookkeeping is the first step of what accountants call the “accounting cycle”: a process designed to take in transaction data and spit out accurate and consistent financial reports.";
            row4["voicetimer_id"] = -1;
            row4["voicetimer_line"] = textToInsert;
            row4["voicetimer_wordcount"] = textToInsert.Trim().Split(' ').Length;
            row4["voicetimer_index"] = 4;
            datatable.Rows.Add(row4);


            var insertedIds = DataManagerSqlLite.InsertRowsToVoiceTimer(datatable);
        }

        #endregion

        #region == Populate functions ==

        private static void PopulateProjectTable()
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
            dataTable.Columns.Add("fk_project_background", typeof(int));
            dataTable.Columns.Add("project_date", typeof(string));

            for (var i = 1; i <= 2; i++) // Made 1 rows to check DataGrid fuctionality
            {
                var row = dataTable.NewRow();
                row["id"] = 1;
                row["project_name"] = $"Sample Project - {i}";
                row["project_version"] = i;
                row["project_uploaded"] = false;
                row["project_archived"] = i % 2 == 0;
                row["fk_project_background"] = 1;
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

        private static void PopulateBackgroundTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("background_id", typeof(int));
            dataTable.Columns.Add("fk_background_company", typeof(int));
            dataTable.Columns.Add("background_media", typeof(byte[]));
            dataTable.Columns.Add("background_active", typeof(bool));

            var currentDirectory = Directory.GetCurrentDirectory();

            var row = dataTable.NewRow();
            row["background_id"] = -1;
            row["fk_background_company"] = 1;
            row["background_active"] = 0;


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

            var row3 = dataTable.NewRow();
            row3["background_id"] = -1;
            row3["fk_background_company"] = 1;
            row3["background_active"] = 1;

            path = $"{currentDirectory}\\Images\\manufacturing_background.png";
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                row3["background_media"] = mp3Byte;
            }
            dataTable.Rows.Add(row3);

            var insertedId = DataManagerSqlLite.InsertRowsToBackground(dataTable);
            if (insertedId > -1)
            {
                //MessageBox.Show("Background images populated to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static byte[] StreamToByteArray(Stream stream, int initialLength)
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


        public static void PopulateLOCAudioTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("locaudio_id", typeof(int));
            dataTable.Columns.Add("fk_locaudio_notes", typeof(int));
            dataTable.Columns.Add("locaudio_media", typeof(byte[]));
            dataTable.Columns.Add("locaudio_createdate", typeof(string));
            dataTable.Columns.Add("locaudio_modifydate", typeof(string));

            var currentDirectory = Directory.GetCurrentDirectory();

            var row = dataTable.NewRow();
            row["locaudio_id"] = -1;
            row["fk_locaudio_notes"] = 1;
            row["locaudio_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["locaudio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var path = $"{currentDirectory}\\Media\\Audio1.mp3";
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                row["locaudio_media"] = mp3Byte;
            }
            dataTable.Rows.Add(row);


            var insertedIds = DataManagerSqlLite.InsertRowsToLocAudio(dataTable);
            if (insertedIds.Count > 0)
                MessageBox.Show("Loc Audio Inserted to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
