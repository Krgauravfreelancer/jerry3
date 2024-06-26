﻿using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VideoCreatorXAMLLibrary.Auth;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class SyncDbHelper
    {
        #region == Helper Functions ==

        public static SQLiteConnection InitializeDatabaseAndGetConnection()
        {
            var sqlCon = DataManagerSqlLite.GetOpenedConnection(false, true); // Lets keep the flag false for now
            return sqlCon;
        }

        #endregion

        #region == Sync Functions ==

        public static void SyncAppForTransaction(AppModel data, SQLiteConnection sqlCon)
        {
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
            DataManagerSqlLite.UpsertRowsToAppForTransaction(datatable, sqlCon);

        }

        public static void SyncMediaForTransaction(List<MediaModel> data, SQLiteConnection sqlCon)
        {
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
            DataManagerSqlLite.UpsertRowsToMediaForTransaction(datatable, sqlCon);
        }

        public static void SyncScreenForTransaction(List<ScreenModel> data, SQLiteConnection sqlCon)
        {
            var datatable = new DataTable();
            datatable.Columns.Add("screen_id", typeof(int));
            datatable.Columns.Add("screen_name", typeof(string));
            datatable.Columns.Add("screen_color", typeof(string));
            datatable.Columns.Add("screen_hexcolor", typeof(string));
            foreach (var item in data)
            {
                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = item.screen_name.ToLower();
                row["screen_color"] = item.screen_color;
                row["screen_hexcolor"] = item.screen_hexcolor;
                datatable.Rows.Add(row);
            }
            DataManagerSqlLite.UpsertRowsToScreenForTransaction(datatable, sqlCon);
        }

        public static void SyncCompanyForTransaction(List<CompanyModel> data, SQLiteConnection sqlCon)
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
            DataManagerSqlLite.UpsertRowsToCompanyForTransaction(datatable, sqlCon);
        }

        public static async void SyncBackgroundForTransaction(List<BackgroundModel> data, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("background_id", typeof(int));
            dataTable.Columns.Add("fk_background_company", typeof(int));
            dataTable.Columns.Add("background_media", typeof(byte[]));
            dataTable.Columns.Add("background_active", typeof(bool));

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
            DataManagerSqlLite.UpsertRowsToBackgroundForTransaction(dataTable, sqlCon);
        }

        public static int UpsertProjectForTransaction(ProjectWithId projectModel, string version, SQLiteConnection sqlCon)
        {
            DataTable dataTable = new DataTable();
            //project
            dataTable.Columns.Add("project_id", typeof(int));
            dataTable.Columns.Add("project_videotitle", typeof(string));
            dataTable.Columns.Add("project_currwfstep", typeof(string));
            dataTable.Columns.Add("project_uploaded", typeof(bool));
            dataTable.Columns.Add("fk_project_background", typeof(int));

            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_archived", typeof(bool));

            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));

            dataTable.Columns.Add("project_isdeleted", typeof(bool));
            dataTable.Columns.Add("project_issynced", typeof(bool));
            dataTable.Columns.Add("project_serverid", typeof(Int64));
            dataTable.Columns.Add("project_syncerror", typeof(string));

            var row = dataTable.NewRow();
            row["project_id"] = -1;
            row["project_videotitle"] = projectModel.project_videotitle?.Replace("'", "''").Replace("&", "and");
            row["project_currwfstep"] = projectModel.project_currwfstep ?? "";
            row["project_uploaded"] = false;
            row["fk_project_background"] = 1;

            row["project_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_archived"] = false;

            row["project_createdate"] = projectModel.project_createdate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_modifydate"] = projectModel.project_modifydate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            row["project_isdeleted"] = false;
            row["project_issynced"] = true;
            row["project_serverid"] = projectModel.project_id;
            row["project_syncerror"] = "";

            var projdet = projectModel?.project_detail?.Find(x => x.projdet_version == version);
            FillProjectDetails(dataTable, row, projdet);

            dataTable.Rows.Add(row);
            var insertedId = DataManagerSqlLite.UpsertRowsToProjectbyIdForTransaction(dataTable, projectModel.project_id, projdet != null, sqlCon);
            return insertedId;
        }

        public static async Task<List<int>> UpsertPlanningForTransaction(List<PlanningModel> plannings, int localProjectId, ProjectWithId projectModel, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            DataTable dataTable = new DataTable();
            //project
            dataTable.Columns.Add("planning_id", typeof(int));
            dataTable.Columns.Add("fk_planning_project", typeof(int));
            dataTable.Columns.Add("fk_planning_screen", typeof(int));
            dataTable.Columns.Add("planning_customname", typeof(string));
            dataTable.Columns.Add("planning_notesline", typeof(string));
            dataTable.Columns.Add("planning_medialibid", typeof(string));
            dataTable.Columns.Add("planning_sort", typeof(int));
            dataTable.Columns.Add("planning_suggestnotesline", typeof(string));
            dataTable.Columns.Add("planning_createdate", typeof(string));
            dataTable.Columns.Add("planning_modifydate", typeof(string));
            dataTable.Columns.Add("planning_serverid", typeof(Int64));
            dataTable.Columns.Add("planning_issynced", typeof(bool));
            dataTable.Columns.Add("planning_syncerror", typeof(string));
            dataTable.Columns.Add("planning_isedited", typeof(bool));
            dataTable.Columns.Add("planning_desc", typeof(DataTable));
            dataTable.Columns.Add("planning_media", typeof(DataTable));
            dataTable.Columns.Add("planning_audioduration", typeof(string));

            foreach (var planning in plannings)
            {
                string notes = "";
                if (planning.planning_notesline?.Count > 0)
                    notes = string.Join($"$$$NEWNOTES$$$", planning.planning_notesline);

                var row = dataTable.NewRow();
                row["planning_id"] = -1;
                row["fk_planning_project"] = localProjectId;
                row["fk_planning_screen"] = planning.fk_planning_screen;
                row["planning_customname"] = planning.planning_customname?.Replace("'", "''").Replace("&","and");
                row["planning_notesline"] = notes?.Replace("'", "''").Replace("&", "and");
                row["planning_medialibid"] = planning.planning_medialibid?.Replace("'", "''").Replace("&", "and");
                row["planning_sort"] = planning.planning_sort;
                row["planning_suggestnotesline"] = planning.planning_suggestnotesline?.Replace("'", "''").Replace("&", "and");
                row["planning_createdate"] = planning.planning_createdate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["planning_modifydate"] = planning.planning_modifydate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["planning_serverid"] = planning.planning_id;
                row["planning_issynced"] = true;
                row["planning_syncerror"] = "";
                row["planning_isedited"] = false;
                row["planning_audioduration"] = planning.planning_audioduration ?? string.Empty;
                //Dependent Tables
                if (planning.planningdesc != null)
                    row["planning_desc"] = GetPlanningDescriptionDataTable(planning.planningdesc);
                if (!string.IsNullOrEmpty(planning.planning_media_thumb) && !string.IsNullOrEmpty(planning.planning_media_full))
                    row["planning_media"] = await GetPlanningMediaDataTable(planning, authApiViewModel);

                dataTable.Rows.Add(row);
            }

            var insertedIds = DataManagerSqlLite.UpsertRowsToPlanningForTransaction(dataTable, localProjectId, sqlCon);
            return insertedIds;
        }

        private static DataTable GetPlanningDescriptionDataTable(PlanningDesc description)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("planningdesc_id", typeof(int));
            dataTable.Columns.Add("planningdesc_line", typeof(string));
            dataTable.Columns.Add("planningdesc_bullet", typeof(DataTable));
            
            var row = dataTable.NewRow();
            row["planningdesc_id"] = -1;
            row["planningdesc_line"] = description.planningdesc_line?.Replace("'", "''").Replace("&", "and");
            if(description.bullet?.Count > 0)
                row["planningdesc_bullet"] = GetPlanningBulletDataTable(description.bullet);
            dataTable.Rows.Add(row);
            
            return dataTable;
        }

        private static DataTable GetPlanningBulletDataTable(List<PlanningBullet> bullets)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("planningbullet_id", typeof(int));
            dataTable.Columns.Add("fk_planningbullet_desc", typeof(int));
            dataTable.Columns.Add("planningbullet_line", typeof(string));
            foreach (var bullet in bullets)
            {
                var row = dataTable.NewRow();
                row["planningbullet_id"] = -1;
                row["fk_planningbullet_desc"] = -1;
                row["planningbullet_line"] = bullet.planningbullet_line?.Replace("'", "''").Replace("&", "and");
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        private static async Task<DataTable> GetPlanningMediaDataTable(PlanningModel planning, AuthAPIViewModel authApiViewModel)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("planningmedia_id", typeof(int));
            dataTable.Columns.Add("planningmedia_mediathumb", typeof(byte[]));
            dataTable.Columns.Add("planningmedia_mediafull", typeof(byte[]));

            var row = dataTable.NewRow();
            row["planningmedia_id"] = -1;
            row["planningmedia_mediathumb"] = await authApiViewModel.GetSecuredFileByteArray(planning.planning_media_thumb);
            row["planningmedia_mediafull"] = await authApiViewModel.GetSecuredFileByteArray(planning.planning_media_full);
            dataTable.Rows.Add(row);
            return dataTable;
        }

        private static DataTable FillProjectDetails(DataTable dataTable, DataRow row, ProjectDetail projdet)
        {
            //Proj Det
            dataTable.Columns.Add("projdet_serverid", typeof(Int64));
            dataTable.Columns.Add("projdet_version", typeof(string));
            dataTable.Columns.Add("projdet_currver", typeof(bool));
            dataTable.Columns.Add("projdet_comments", typeof(string));
            dataTable.Columns.Add("projdet_createdate", typeof(string));
            dataTable.Columns.Add("projdet_modifydate", typeof(string));

            if (projdet != null)
            {
                row["projdet_serverid"] = projdet.projdet_id;
                row["projdet_version"] = projdet.projdet_version?.Replace("'", "''").Replace("&", "and");
                row["projdet_currver"] = projdet.projdet_currver;
                row["projdet_comments"] = projdet.projdet_comments?.Replace("'", "''").Replace("&", "and");
                row["projdet_createdate"] = projdet.projdet_createdate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["projdet_modifydate"] = projdet.projdet_modifydate ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return dataTable;
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
            dataTable.Columns.Add("project_videotitle", typeof(string));
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
                row["project_videotitle"] = $"Sample Project - {i}";
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

            var currentDirectory = PathHelper.GetTempPath("background");

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
            //DataTable dataTable = new DataTable();
            //dataTable.Columns.Add("locaudio_id", typeof(int));
            //dataTable.Columns.Add("fk_locaudio_notes", typeof(int));
            //dataTable.Columns.Add("locaudio_media", typeof(byte[]));
            //dataTable.Columns.Add("locaudio_createdate", typeof(string));
            //dataTable.Columns.Add("locaudio_modifydate", typeof(string));

            //var currentDirectory = PathHelper.GetTempPath("loc");
            //var row = dataTable.NewRow();
            //row["locaudio_id"] = -1;
            //row["fk_locaudio_notes"] = 1;
            //row["locaudio_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //row["locaudio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //var path = $"{currentDirectory}\\Media\\Audio1.mp3";
            //using (var fileStream = new FileStream(path, FileMode.Open))
            //{
            //    byte[] mp3Byte = StreamToByteArray(fileStream, 0);
            //    row["locaudio_media"] = mp3Byte;
            //}
            //dataTable.Rows.Add(row);


            //var insertedIds = DataManagerSqlLite.InsertRowsToLocAudio(dataTable);
            //if (insertedIds.Count > 0)
            //    MessageBox.Show("Loc Audio Inserted to Database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}
