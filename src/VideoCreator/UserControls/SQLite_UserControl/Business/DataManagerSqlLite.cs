using Sqllite_Library.Data;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace Sqllite_Library.Business
{
    public static class DataManagerSqlLite
    {

        #region == Database Methods ==
        
        /// <summary>
        /// method to get opened connection for local DB, create DB if nt exist
        /// </summary>
        /// <param name="encryptFlag">Whether Db is enrypted or not</param>
        /// <param name="canCreateRegistryIfNotExists">insert entries in registry for futur use</param>
        /// <returns></returns>
        public static SQLiteConnection GetOpenedConnection(bool encryptFlag, bool canCreateRegistryIfNotExists = false)
        {
            string message;
            if (SqlLiteData.IsDbCreated())
            {
                message = $"{RegisteryHelper.GetFileName()} database already exists!!";
            }
            else
            {
                SqlLiteData.CreateDatabaseIfNotExist(encryptFlag, canCreateRegistryIfNotExists);
                message = $"{RegisteryHelper.GetFileName()} database created successfully!!";
            }
            var sqlCon = SqlLiteData.GetOpenedConnection();
            return sqlCon;
        }


        /// <summary>
        /// Clear registry items
        /// </summary>
        /// <returns></returns>
        public static string ClearRegistryAndDeleteDB()
        {
            string message;
            SqlLiteData.DeleteDB();
            RegisteryHelper.ClearRegistry();
            message = $"Registry Cleaned successfully !!";
            return message;
        }

        #endregion


        #region == Insert Methods ==


        /// <summary>
        /// Insert new records to Project table
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToProject(DataTable data)
        {
            //foreach (DataRow rowMain in data.Rows)
            //{
            //    //var backgroundFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)rowMain["fk_project_background"]);
            //    //if (!backgroundFlag)
            //    //    throw new Exception("background_id foreign key constraint not successful ");
            //}
            return SqlLiteData.InsertRowsToProject(data);
        }

        #region == Video Event and Depenedent Tables ==

        public static List<int> InsertRowsToVideoEvent(DataTable data, bool populateDependentTablesFlag = true)
        {
            var insertedIds = new List<int>();
            foreach (DataRow rowMain in data.Rows)
            {
                var projdetFlag = SqlLiteData.ReferentialKeyPresent("cbv_projdet", "projdet_id", (int)rowMain["fk_videoevent_projdet"]);
                if (!projdetFlag)
                    throw new Exception("projdet_Id foreign key constraint not successful ");

                var mediaId = (int)rowMain["fk_videoevent_media"];
                var mediaFlag = SqlLiteData.ReferentialKeyPresent("cbv_media", "media_id", mediaId);
                if (!mediaFlag)
                    throw new Exception("media_Id foreign key constraint not successful ");

                var insertedId = SqlLiteData.InsertRowsToVideoEvent(rowMain);
                if (insertedId > 0 && populateDependentTablesFlag)
                {
                    var createDate = Convert.ToString(rowMain["videoevent_createdate"]);
                    if (string.IsNullOrEmpty(createDate))
                        createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    var modifyDate = Convert.ToString(rowMain["videoevent_modifydate"]);
                    if (string.IsNullOrEmpty(modifyDate))
                        modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (mediaId == 1 || mediaId == 2) // Image or video
                    {
                        // Insert into cbv_videosegment
                        var dtVideoSegment = new DataTable();

                        dtVideoSegment.Columns.Add("videosegment_id", typeof(int));
                        dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
                        dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
                        dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));

                        dtVideoSegment.Columns.Add("videosegment_isdeleted", typeof(bool));
                        dtVideoSegment.Columns.Add("videosegment_issynced", typeof(bool));
                        dtVideoSegment.Columns.Add("videosegment_serverid", typeof(Int64));
                        dtVideoSegment.Columns.Add("videosegment_syncerror", typeof(string));

                        var rowVideoSegment = dtVideoSegment.NewRow();
                        rowVideoSegment["videosegment_id"] = insertedId;
                        rowVideoSegment["videosegment_media"] = rowMain["media"];
                        rowVideoSegment["videosegment_createdate"] = createDate;
                        rowVideoSegment["videosegment_modifydate"] = modifyDate;
                        rowVideoSegment["videosegment_isdeleted"] = false;
                        rowVideoSegment["videosegment_issynced"] = true;
                        rowVideoSegment["videosegment_serverid"] = 1;
                        rowVideoSegment["videosegment_syncerror"] = "";

                        dtVideoSegment.Rows.Add(rowVideoSegment);

                        var insertedVideoSegmentId = InsertRowsToVideoSegment(dtVideoSegment, insertedId);
                        if (insertedVideoSegmentId <= 0) throw new Exception("Error while inserting videosegment table");
                    }
                    else if (mediaId == 3) //Audio
                    {
                        /*
                        // Insert into cbv_audio
                        var dtAudio = new DataTable();

                        dtAudio.Columns.Add("audio_id", typeof(int));
                        dtAudio.Columns.Add("fk_audio_videoevent", typeof(int));
                        dtAudio.Columns.Add("audio_media", typeof(byte[]));
                        dtAudio.Columns.Add("audio_createdate", typeof(string));
                        dtAudio.Columns.Add("audio_modifydate", typeof(string));

                        var rowAudio = dtAudio.NewRow();
                        rowAudio["audio_id"] = -1;
                        rowAudio["fk_audio_videoevent"] = insertedId;
                        rowAudio["audio_media"] = rowMain["media"];
                        rowAudio["audio_createdate"] = createDate;
                        rowAudio["audio_modifydate"] = modifyDate;
                        dtAudio.Rows.Add(rowAudio);

                        var insertedAudioId = InsertRowsToAudio(dtAudio);
                        if (insertedAudioId <= 0) throw new Exception("Error while inserting audio table");
                        */
                    }
                    else if (mediaId == 4)  //Design
                    {
                        // Insert into cbv_design
                        var dtDesign = new DataTable();
                        dtDesign.Columns.Add("design_id", typeof(int));
                        dtDesign.Columns.Add("fk_design_videoevent", typeof(int));
                        dtDesign.Columns.Add("fk_design_screen", typeof(int));
                        dtDesign.Columns.Add("design_xml", typeof(string));
                        dtDesign.Columns.Add("design_createdate", typeof(string));
                        dtDesign.Columns.Add("design_modifydate", typeof(string));

                        dtDesign.Columns.Add("design_isdeleted", typeof(bool));
                        dtDesign.Columns.Add("design_issynced", typeof(bool));
                        dtDesign.Columns.Add("design_serverid", typeof(Int64));
                        dtDesign.Columns.Add("design_syncerror", typeof(string));

                        var rowDesign = dtDesign.NewRow();
                        rowDesign["design_id"] = -1;
                        rowDesign["fk_design_videoevent"] = insertedId;
                        rowDesign["fk_design_screen"] = (int)rowMain["fk_design_screen"];
                        rowDesign["design_xml"] = rowMain["design_xml"].ToString();
                        rowDesign["design_createdate"] = createDate;
                        rowDesign["design_modifydate"] = modifyDate;

                        rowDesign["design_isdeleted"] = false;
                        rowDesign["design_issynced"] = true;
                        rowDesign["design_serverid"] = 1;
                        rowDesign["design_syncerror"] = "";

                        dtDesign.Rows.Add(rowDesign);
                        var insertedDesignId = InsertRowsToDesign(dtDesign);
                        if (insertedDesignId <= 0) throw new Exception("Error while inserting design table");
                    }
                }
                //else
                //{
                //    throw new Exception("Error while inserting VideoEvent table");
                //}
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        public static List<int> InsertRowsToVideoEventForTransaction(DataTable data, SQLiteConnection sqlCon, bool populateDependentTablesFlag = true)
        {
            var insertedIds = new List<int>();
            foreach (DataRow rowMain in data.Rows)
            {
                var projdetFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_projdet", "projdet_id", (int)rowMain["fk_videoevent_projdet"]);
                if (!projdetFlag)
                    throw new Exception("projdet_Id foreign key constraint not successful ");

                var mediaId = (int)rowMain["fk_videoevent_media"];
                var mediaFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon,"cbv_media", "media_id", mediaId);
                if (!mediaFlag)
                    throw new Exception("media_Id foreign key constraint not successful ");

                var insertedId = SqlLiteData.InsertRowsToVideoEventForTransaction(rowMain, sqlCon);
                if (insertedId > 0 && populateDependentTablesFlag)
                {
                    var createDate = Convert.ToString(rowMain["videoevent_createdate"]);
                    if (string.IsNullOrEmpty(createDate))
                        createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    var modifyDate = Convert.ToString(rowMain["videoevent_modifydate"]);
                    if (string.IsNullOrEmpty(modifyDate))
                        modifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    if (mediaId == 1 || mediaId == 2) // Image or video
                    {
                        // Insert into cbv_videosegment
                        var dtVideoSegment = new DataTable();

                        dtVideoSegment.Columns.Add("videosegment_id", typeof(int));
                        dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
                        dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
                        dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));

                        dtVideoSegment.Columns.Add("videosegment_isdeleted", typeof(bool));
                        dtVideoSegment.Columns.Add("videosegment_issynced", typeof(bool));
                        dtVideoSegment.Columns.Add("videosegment_serverid", typeof(Int64));
                        dtVideoSegment.Columns.Add("videosegment_syncerror", typeof(string));

                        var rowVideoSegment = dtVideoSegment.NewRow();
                        rowVideoSegment["videosegment_id"] = insertedId;
                        rowVideoSegment["videosegment_media"] = rowMain["media"];
                        rowVideoSegment["videosegment_createdate"] = createDate;
                        rowVideoSegment["videosegment_modifydate"] = modifyDate;
                        rowVideoSegment["videosegment_isdeleted"] = false;
                        rowVideoSegment["videosegment_issynced"] = true;
                        rowVideoSegment["videosegment_serverid"] = 1;
                        rowVideoSegment["videosegment_syncerror"] = "";

                        dtVideoSegment.Rows.Add(rowVideoSegment);

                        var insertedVideoSegmentId = InsertRowsToVideoSegmentForTransaction(dtVideoSegment, insertedId, sqlCon);
                        if (insertedVideoSegmentId <= 0) throw new Exception("Error while inserting videosegment table");
                    }
                    else if (mediaId == 3) //Audio
                    {
                        /*
                        // Insert into cbv_audio
                        var dtAudio = new DataTable();

                        dtAudio.Columns.Add("audio_id", typeof(int));
                        dtAudio.Columns.Add("fk_audio_videoevent", typeof(int));
                        dtAudio.Columns.Add("audio_media", typeof(byte[]));
                        dtAudio.Columns.Add("audio_createdate", typeof(string));
                        dtAudio.Columns.Add("audio_modifydate", typeof(string));

                        var rowAudio = dtAudio.NewRow();
                        rowAudio["audio_id"] = -1;
                        rowAudio["fk_audio_videoevent"] = insertedId;
                        rowAudio["audio_media"] = rowMain["media"];
                        rowAudio["audio_createdate"] = createDate;
                        rowAudio["audio_modifydate"] = modifyDate;
                        dtAudio.Rows.Add(rowAudio);

                        var insertedAudioId = InsertRowsToAudio(dtAudio);
                        if (insertedAudioId <= 0) throw new Exception("Error while inserting audio table");
                        */
                    }
                    else if (mediaId == 4)  //Design
                    {
                        // Insert into cbv_design
                        var dtDesign = new DataTable();
                        dtDesign.Columns.Add("design_id", typeof(int));
                        dtDesign.Columns.Add("fk_design_videoevent", typeof(int));
                        dtDesign.Columns.Add("fk_design_screen", typeof(int));
                        dtDesign.Columns.Add("design_xml", typeof(string));
                        dtDesign.Columns.Add("design_createdate", typeof(string));
                        dtDesign.Columns.Add("design_modifydate", typeof(string));

                        dtDesign.Columns.Add("design_isdeleted", typeof(bool));
                        dtDesign.Columns.Add("design_issynced", typeof(bool));
                        dtDesign.Columns.Add("design_serverid", typeof(Int64));
                        dtDesign.Columns.Add("design_syncerror", typeof(string));

                        var rowDesign = dtDesign.NewRow();
                        rowDesign["design_id"] = -1;
                        rowDesign["fk_design_videoevent"] = insertedId;
                        rowDesign["fk_design_screen"] = (int)rowMain["fk_design_screen"];
                        rowDesign["design_xml"] = rowMain["design_xml"].ToString();
                        rowDesign["design_createdate"] = createDate;
                        rowDesign["design_modifydate"] = modifyDate;

                        rowDesign["design_isdeleted"] = false;
                        rowDesign["design_issynced"] = true;
                        rowDesign["design_serverid"] = 1;
                        rowDesign["design_syncerror"] = "";

                        dtDesign.Rows.Add(rowDesign);
                        var insertedDesignId = InsertRowsToDesignForTransaction(dtDesign, sqlCon);
                        if (insertedDesignId <= 0) throw new Exception("Error while inserting design table");
                    }
                }
                //else
                //{
                //    throw new Exception("Error while inserting VideoEvent table");
                //}
                insertedIds.Add(insertedId);
            }
            return insertedIds;
        }

        public static int InsertRowsToVideoSegment(DataTable data, int fk_value)
        {
            // For Johan
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", fk_value);
            //if (!videoeventFlag)
            //    throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToVideoSegment(data);
        }

        public static int InsertRowsToVideoSegmentForTransaction(DataTable data, int fk_value, SQLiteConnection sqlCon)
        {
            // For Johan
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_videoevent", "videoevent_id", fk_value);
            //if (!videoeventFlag)
            //    throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToVideoSegmentForTransaction(data, sqlCon);
        }

        public static int InsertRowsToDesign(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_design_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            var bgFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)data?.Rows[0]["fk_design_background"]);
            if (!bgFlag)
                throw new Exception("background_Id foreign key constraint not successful ");

            var screenFlag = SqlLiteData.ReferentialKeyPresent("cbv_screen", "screen_id", (int)data?.Rows[0]["fk_design_screen"]);
            if (!screenFlag)
                throw new Exception("screen_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToDesign(data);
        }

        public static int InsertRowsToDesignForTransaction(DataTable data, SQLiteConnection sqlCon)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_design_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful for cbv_videoevent");

            var bgFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_background", "background_id", (int)data?.Rows[0]["fk_design_background"]);
            if (!bgFlag)
                throw new Exception("background_Id foreign key constraint not successful for cbv_background");

            var screenFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_screen", "screen_id", (int)data?.Rows[0]["fk_design_screen"]);
            if (!screenFlag)
                throw new Exception("screen_Id foreign key constraint not successful for cbv_screen");


            return SqlLiteData.InsertRowsToDesignForTransaction(data, sqlCon);
        }

        /*
        public static int InsertRowsToAudio(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_audio_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToAudio(data);
        }
        */


        #endregion


        /// <summary>
        /// Insert new records to Notes table
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToNotes(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_notes_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToNotes(data);
        }

        /// <summary>
        /// Insert new records to Notes table (using Transactions)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToNotesForTransaction(DataTable data, SQLiteConnection sqlCon)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresentForTransaction(sqlCon, "cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_notes_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToNotesForTransaction(data, sqlCon);
        }

        /// <summary>
        /// Insert new records to LOC table (wo using Transactions)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<int> InsertRowsToLocAudio(DataTable data)
        {
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_notes", "notes_id", (int)data?.Rows[0]["fk_locaudio_notes"]);
            //if (!videoeventFlag)
            //    throw new Exception("notes_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToLocAudio(data);
        }

        /// <summary>
        /// Insert new records to Final MP4 table (wo using Transactions)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToFinalMp4(DataTable data)
        {
            //var projectFlag = SqlLiteData.ReferentialKeyPresent("cbv_project", "project_id", (int)data?.Rows[0]["fk_finalmp4_project"]);
            //if (!projectFlag)
            //    throw new Exception("project_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToFinalMp4(data);
        }

        /// <summary>
        /// Insert new records to Company table (wo using Transactions)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToCompany(DataTable data)
        {
            return SqlLiteData.InsertRowsToCompany(data);
        }

        /// <summary>
        /// Insert new records to background table (wo using Transactions)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int InsertRowsToBackground(DataTable data)
        {
            //var companyFlag = SqlLiteData.ReferentialKeyPresent("cbv_company", "company_id", (int)data?.Rows[0]["fk_background_company"]);
            //if (!companyFlag)
            //    throw new Exception("company_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToBackground(data);
        }

        /// <summary>
        /// Insert new records to Voice table (wo using Transactions) - NOT IN USE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<int> InsertRowsToVoiceTimer(DataTable data)
        {
            return SqlLiteData.InsertRowsToVoiceTimer(data);
        }

        /// <summary>
        /// Insert new records to Voice Avg table (wo using Transactions) - NOT IN USE
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<int> InsertRowsToVoiceAverage(DataTable data)
        {
            return SqlLiteData.InsertRowsToVoiceAverage(data);
        }

        #endregion


        #region == Sync methods ==

        /// <summary>
        /// Upsert records to APP table (wo using Transactions)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<int> SyncApp(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncApp(dataTable);
            return insertedIds;
        }

        /// <summary>
        /// Upsert records to SCREEN table (wo using Transactions)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<int> SyncScreen(DataTable dataTable)
        {
            var insertedId = SqlLiteData.SyncScreen(dataTable);
            return insertedId;
        }

        /// <summary>
        /// Upsert records to MEDIA table (wo using Transactions)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<int> SyncMedia(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncMedia(dataTable);
            return insertedIds;
        }

        /// <summary>
        /// Upsert records to COMPANY table (wo using Transactions)
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static List<int> SyncCompany(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncCompany(dataTable);
            return insertedIds;
        }

        #endregion


        #region == Get Methods ==

        /// <summary>
        /// Get records From MEDIA table (wo using Transactions)
        /// </summary>
        /// <returns></returns>
        public static List<CBVMedia> GetMedia()
        {
            return SqlLiteData.GetMedia();
        }

        /// <summary>
        /// Get records From SCREENS table (wo using Transactions)
        /// </summary>
        /// <returns></returns>
        public static List<CBVScreen> GetScreens()
        {
            return SqlLiteData.GetScreens();
        }

        /// <summary>
        /// Get records From APP table (wo using Transactions)
        /// </summary>
        /// <returns></returns>
        public static List<CBVApp> GetApp()
        {
            return SqlLiteData.GetApp();
        }

        /// <summary>
        /// Get records From Planning table (wo using Transactions)
        /// </summary>
        public static List<CBVPlanning> GetPlanning(int projectId, bool dependentFlag = true)
        {
            return SqlLiteData.GetPlanning(projectId, dependentFlag);
        }

        /// <summary>
        /// Get records From Planning table (using Transactions)
        /// </summary>
        public static List<CBVPlanning> GetPlanningForTransaction(int projectId, SQLiteConnection sqlCon, bool dependentFlag = true)
        {
            return SqlLiteData.GetPlanningForTransaction(projectId, sqlCon, dependentFlag);
        }

        /// <summary>
        /// Get specific record From Planning table (wo using Transactions)
        /// </summary>
        public static CBVPlanning GetPlanningById(int planningid = 0, Int64 planningServerId = 0)
        {
            return SqlLiteData.GetPlanningById(planningid, planningServerId);
        }

        /// <summary>
        /// Get downloaded projects (wo using Transactions)
        /// </summary>
        public static List<CBVProjectForJoin> GetDownloadedProjectList()
        {
            return SqlLiteData.GetDownloadedProjectList();
        }

        /// <summary>
        /// Get downloaded projects (using Transactions)
        /// </summary>
        public static List<CBVProjectForJoin> GetDownloadedProjectListForTransaction(SQLiteConnection sqlCon)
        {
            return SqlLiteData.GetDownloadedProjectListForTransaction(sqlCon);
        }

        /// <summary>
        /// Get specific record from project (wo using Transactions)
        /// </summary>
        public static CBVProject GetProjectById(int projectId = -1, bool projdetFlag = false)
        {
            return SqlLiteData.GetProjectById(projectId, projdetFlag).FirstOrDefault();
        }

        /// <summary>
        /// Get specific record from project (using Transactions)
        /// </summary>
        public static CBVProject GetProjectByIdForTransaction(SQLiteConnection sqlCon, int projectId = -1, bool projdetFlag = false)
        {
            return SqlLiteData.GetProjectByIdForTransaction(projectId, projdetFlag, sqlCon).FirstOrDefault();
        }

        /// <summary>
        /// Get records count for project table (wo using Transactions)
        /// </summary>
        public static int GetProjectsCount()
        {
            return SqlLiteData.GetCount("cbv_project");
        }

        /// <summary>
        /// Get records count for Background table (wo using Transactions)
        /// </summary>
        public static int GetBackgroundsCount()
        {
            return SqlLiteData.GetCount("cbv_background");
        }

        /// <summary>
        /// Get records count for Voice Timer table (wo using Transactions)
        /// </summary>
        public static int GetVoiceTimerCount()
        {
            return SqlLiteData.GetCount("cbv_voicetimer");
        }

        /// <summary>
        /// Get records count for Voice Average table (wo using Transactions)
        /// </summary>
        public static int GetVoiceAverageCount() // For Testing Purpose
        {
            return SqlLiteData.GetCount("cbv_voiceaverage");
        }

        /// <summary>
        /// Get records count for videoevents based upong 2 params (wo using Transactions)
        /// </summary>
        public static int GetVideoEventCountProjectAndDetailId(int ProjectId, int ProjectdetailId)
        {
            return SqlLiteData.GetVideoEventCountProjectAndDetailId(ProjectId, ProjectdetailId);
        }

        /// <summary>
        /// Get records count for videoevents based upong 3 params (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetVideoEvents(int projdetId = 0, bool dependentDataFlag = false, bool designFlag = false)
        {
            return SqlLiteData.GetVideoEvents(projdetId, dependentDataFlag, designFlag);
        }

        /// <summary>
        /// Get placeholder videoevents - For Johan (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetPlaceholderVideoEvents(int projdetId)
        {
            return SqlLiteData.GetPlaceholderVideoEvents(projdetId);
        }

        /// <summary>
        /// Check if provided id is placeholder or not - For Johan (wo using Transactions)
        /// </summary>
        public static bool IsPlaceHolderEvent(int videoeventId)
        {
            return SqlLiteData.IsPlaceHolderEvent(videoeventId);
        }


        /// <summary>
        /// Find all the events which are not present on server but locally (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetNotSyncedVideoEvents(int projdetId = 0, bool dependentDataFlag = true)
        {
            return SqlLiteData.GetNotSyncedVideoEvents(projdetId, dependentDataFlag);
        }

        /// <summary>
        /// Find specific videoevent by local Id (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetVideoEventbyId(int videoeventId = 0, bool dependentDataFlag = false, bool designFlag = false)
        {
            return SqlLiteData.GetVideoEventbyId(videoeventId, dependentDataFlag, designFlag);
        }

        /// <summary>
        /// Find specific videoevent by server Id (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetVideoEventbyServerId(Int64 videoeventServerId = 0)
        {
            return SqlLiteData.GetVideoEventbyServerId(videoeventServerId);
        }

        /// <summary>
        /// Find videoevents which needs to be shifted based upon End Time (wo using Transactions)
        /// </summary>
        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyEndTime(int fk_videoevent_projdet, string endTime, EnumTrack track = EnumTrack.IMAGEORVIDEO)
        {
            return SqlLiteData.GetShiftVideoEventsbyTime(fk_videoevent_projdet, endTime, track);
        }

        /// <summary>
        /// Find videoevents which needs to be shifted based upon Start Time (wo using Transactions)
        /// </summary>
        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyStartTime(int fk_videoevent_projdet, string startTime, EnumTrack track = EnumTrack.IMAGEORVIDEO)
        {
            return SqlLiteData.GetShiftVideoEventsbyTime(fk_videoevent_projdet, startTime, track);
        }

        /// <summary>
        /// Find videoevents which are overlapping - based upon start/end time (wo using Transactions)
        /// </summary>
        public static List<CBVVideoEvent> GetOverlappingCalloutsByTime(int fk_videoevent_projdet, string startTime, string endtime)
        {
            return SqlLiteData.GetOverlappingCalloutsByTime(fk_videoevent_projdet, startTime, endtime);
        }

        /// <summary>
        /// Get records count for Videoevent table (wo using Transactions)
        /// </summary>
        public static int GetVideoEventsCount()
        {
            return SqlLiteData.GetCount("cbv_videoevent");
        }

        /*
        public static List<CBVAudio> GetAudio(int VideoEventId = -1)
        {
            return SqlLiteData.GetAudio(VideoEventId);
        }
        */

        /// <summary>
        /// Find all videosegment for provided videoevent Id (wo using Transactions)
        /// </summary>
        public static List<CBVVideoSegment> GetVideoSegment(int VideoEventId = -1)
        {
            return SqlLiteData.GetVideoSegment(VideoEventId);
        }

        /// <summary>
        /// Find all Design for provided videoevent Id (wo using Transactions)
        /// </summary>
        public static List<CBVDesign> GetDesign(int VideoEventId = -1)
        {
            return SqlLiteData.GetDesign(VideoEventId);
        }

        /// <summary>
        /// Find all Notes for provided videoevent Id (wo using Transactions)
        /// </summary>
        public static List<CBVNotes> GetNotes(int VideoEventId = -1)
        {
            return SqlLiteData.GetNotes(VideoEventId);
        }

        /// <summary>
        /// Find specific notes record by local id (wo using Transactions)
        /// </summary>
        public static List<CBVNotes> GetNotesbyId(int notesId)
        {
            return SqlLiteData.GetNotesbyId(notesId);
        }

        /// <summary>
        /// Find max index for notes records by videoevent Id (wo using Transactions)
        /// </summary>
        public static int GetMaxIndexForNotes(int fkVideoEventId)
        {
            return SqlLiteData.GetMaxIndexForNotes(fkVideoEventId);
        }

        /// <summary>
        /// Not in use
        /// </summary>
        public static List<CBVLocAudio> GetLocAudio(int notesId = -1)
        {
            return SqlLiteData.GetLocAudio(notesId);
        }

        /// <summary>
        /// not in use
        /// </summary>
        public static List<CBVFinalMp4> GetFinalMp4(int ProjectId = -1, bool dependentDataFlag = false)
        {
            return SqlLiteData.GetFinalMp4(ProjectId, dependentDataFlag);
        }

        /// <summary>
        /// Find All company records (wo using Transactions)
        /// </summary>
        public static List<CBVCompany> GetCompany()
        {
            return SqlLiteData.GetCompany();
        }
        
        /// <summary>
        /// Find background by company Id (wo using Transactions)
        /// </summary>
        public static List<CBVBackground> GetBackground(int company_id = -1)
        {
            return SqlLiteData.GetBackground(company_id);
        }

        /// <summary>
        /// Find background by company Id (using Transactions)
        /// </summary>
        public static List<CBVBackground> GetBackgroundForTransaction(SQLiteConnection sqlCon, int company_id = -1)
        {
            return SqlLiteData.GetBackgroundForTransaction(company_id, sqlCon);
        }

        /// <summary>
        /// Find VoiceTimers - Not In Use (WO using Transactions)
        /// </summary>
        public static List<CBVVoiceTimer> GetVoiceTimers()
        {
            return SqlLiteData.GetVoiceTimers();
        }

        /// <summary>
        /// Find VoiceAverage - Not In Use (WO using Transactions)
        /// </summary>
        public static List<CBVVoiceAvergae> GetVoiceAverage()
        {
            return SqlLiteData.GetVoiceAverage();
        }

        #endregion


        #region == Update Methods ==

        /// <summary>
        /// Update Rows to Project table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToProject(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToProject(dataTable);
        }

        /// <summary>
        /// Update Rows to Project table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToVideoEvent(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVideoEvent(dataTable);
        }

        /// <summary>
        /// Update VideoEvent table > Server Id (WO using Transactions)
        /// </summary>
        public static void UpdateVideoEventDataTableServerId(int localId, Int64 serverId, string ErrorMessage = "")
        {
            SqlLiteData.UpdateServerId("videoevent", localId, serverId, ErrorMessage);
        }

        /// <summary>
        /// Update Rows to VideoSegment table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToVideoSegment(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVideoSegment(dataTable);
        }

        /// <summary>
        /// Update Rows to VideoSegment table based upon server Id (WO using Transactions)
        /// </summary>
        public static void UpdateVideoSegmentDataTableServerId(int localId, Int64 serverId, string ErrorMessage = "")
        {
            SqlLiteData.UpdateServerId("videosegment", localId, serverId, ErrorMessage);
        }

        /// <summary>
        /// Update Design table > Server Id (WO using Transactions)
        /// </summary>
        public static void UpdateDesignDataTableServerId(int localId, Int64 serverId, string ErrorMessage = "")
        {
            SqlLiteData.UpdateServerId("design", localId, serverId, ErrorMessage);
        }


        /*
        public static void UpdateRowsToAudio(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToAudio(dataTable);
        }
        */

        /// <summary>
        /// Update Rows to Design table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToDesign(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToDesign(dataTable);
        }

        /// <summary>
        /// Update Rows to Notes table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToNotes(DataTable dataTable, bool isNotesServerId = false)
        {
            SqlLiteData.UpdateRowsToNotes(dataTable, isNotesServerId);
        }

        /// <summary>
        /// Update Rows to Notes table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToNotes(DataRow dr)
        {
            SqlLiteData.UpdateRowsToNotes(dr);
        }

        /// <summary>
        /// Update Rows to LocAudio table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToLocAudio(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToLocAudio(dataTable);
        }

        /// <summary>
        /// Update Rows to FinalMp4 table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToFinalMp4(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToFinalMp4(dataTable);
        }

        /// <summary>
        /// Update Rows to VoiceTimer table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToVoiceTimer(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVoiceTimer(dataTable);
        }

        /// <summary>
        /// Update Rows to VoiceAverage table based upon datatable (WO using Transactions)
        /// </summary>
        public static void UpdateRowsToVoiceAverage(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVoiceAverage(dataTable);
        }

        /// <summary>
        /// Update Rows to shift VideoEvents based upon datatable (WO using Transactions)
        /// </summary>
        public static void ShiftVideoEvents(DataTable dataTable)
        {
            SqlLiteData.ShiftVideoEvents(dataTable);
        }

        /// <summary>
        /// Update Rows to ProjectDetail table based upon datatable (WO using Transactions)
        /// </summary>
        public static void SetProjectDetailSubmitted(int projdet_id)
        {
            SqlLiteData.SetProjectDetailSubmitted(projdet_id);
        }

        #endregion


        #region == Soft Delete Methods ==

        public static void DeleteNotesById(int notesId = -1)
        {
            SqlLiteData.DeleteNotesById(notesId);
        }

        public static void DeleteVideoEventsById(int videoeventId, bool cascadeDelete)
        {
            SqlLiteData.DeleteVideoEventsById(videoeventId, cascadeDelete, true);
        }

        public static void UndeleteVideoEventsById(int videoeventId, bool cascadeDelete)
        {
            SqlLiteData.DeleteVideoEventsById(videoeventId, cascadeDelete, false);
        }

        public static void DeleteAllVideoEventsByProjdetId(int projdetId, bool cascadeDelete)
        {
            SqlLiteData.DeleteAllVideoEventsByProjdetId(projdetId, cascadeDelete);
        }

        #endregion


        #region == Upsert Methods ==

        /// <summary>
        /// Upsert row to APP means insert if not exist else update 
        /// </summary>
        public static void UpsertRowsToAppForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            SqlLiteData.UpsertRowsToAppForTransaction(dataTable, sqlCon);
        }

        /// <summary>
        /// Upsert row to Media means insert if not exist else update 
        /// </summary>
        public static void UpsertRowsToMediaForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            SqlLiteData.UpsertRowsToMediaForTransaction(dataTable, sqlCon);
        }

        /// <summary>
        /// Upsert row to Screen means insert if not exist else update 
        /// </summary>
        public static void UpsertRowsToScreenForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            SqlLiteData.UpsertRowsToScreenForTransaction(dataTable, sqlCon);
        }

        /// <summary>
        /// Upsert row to Company means insert if not exist else update 
        /// </summary>
        public static void UpsertRowsToCompanyForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            SqlLiteData.UpsertRowsToCompanyForTransaction(dataTable, sqlCon);
        }

        /// <summary>
        /// Upsert row to Background means insert if not exist else update 
        /// </summary>
        public static void UpsertRowsToBackgroundForTransaction(DataTable dataTable, SQLiteConnection sqlCon)
        {
            SqlLiteData.UpsertRowsToBackgroundForTransaction(dataTable, sqlCon);
        }

        public static int IsProjectAvailableForTransaction(int projectServerId, SQLiteConnection sqlCon)
        {
            return SqlLiteData.IsProjectAvailableForTransaction(projectServerId, sqlCon);
        }

        /// <summary>
        /// Upsert row to project means insert if not exist else update 
        /// </summary>
        public static int UpsertRowsToProjectbyIdForTransaction(DataTable data, int projectServerId, bool projdetAvailable, SQLiteConnection sqlCon)
        {
            foreach (DataRow rowMain in data.Rows)
            {
                var backgroundFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)rowMain["fk_project_background"]);
                if (!backgroundFlag)
                    throw new Exception("background_id foreign key constraint not successful ");
            }

            var projectId = IsProjectAvailableForTransaction(projectServerId, sqlCon);
            if (projectId == -1)
                projectId = SqlLiteData.UpsertRowsToProjectForTransaction(data, sqlCon);

            if (projdetAvailable)
                SqlLiteData.InsertRowsToProjectDetail(data, projectId, sqlCon);
            return projectId;
        }

        /// <summary>
        /// Upsert row to Planning means insert if not exist else update 
        /// </summary>
        public static List<int> UpsertRowsToPlanningForTransaction(DataTable data, int projectId, SQLiteConnection sqlCon)
        {
            var planningId = SqlLiteData.IsProjectPlanningAvailableForTransaction(projectId, sqlCon);
            if (planningId == -1)
            {
                var planningIds = SqlLiteData.InsertRowsToPlanningForTransaction(data, sqlCon);
                return planningIds;
            }
            return null;
        }

        #endregion


        #region == Hard Delete ==

        /// <summary>
        /// hard delete all planning - not recoverable 
        /// </summary>
        public static void HardDeletePlanningsByProjectIdForTransaction(int projectId, SQLiteConnection sqlCon)
        {
            SqlLiteData.HardDeletePlanningsByProjectIdForTransaction(projectId, sqlCon);
        }

        /// <summary>
        /// hard delete VideoEvent by Id - not recoverable 
        /// </summary>
        public static void HardDeleteVideoEventsByIdForTransaction(int videoeventId, bool cascadeDelete, SQLiteConnection sqlCon)
        {
            SqlLiteData.HardDeleteVideoEventsByIdForTransaction(videoeventId, cascadeDelete, sqlCon);
        }

        /// <summary>
        /// hard delete VideoEvent by serverId - not recoverable 
        /// </summary>
        public static void HardDeleteVideoEventsByServerIdForTransaction(int serverVideoeventId, bool cascadeDelete, SQLiteConnection sqlCon)
        {
            SqlLiteData.HardDeleteVideoEventsByServerIdForTransaction(serverVideoeventId, cascadeDelete, sqlCon);
        }

        #endregion


        #region == Helper Function == 

        /// <summary>
        /// Return next available start time based for a project detail
        /// </summary>
        public static string GetNextStart(int fk_videoevent_media, int projdetId)
        {
            return SqlLiteData.GetNextStart(fk_videoevent_media, projdetId);
        }

        /// <summary>
        /// Return next available start time based for a project detail (using transaction)
        /// </summary>
        public static string GetNextStartForTransaction(int fk_videoevent_media, int projdetId, SQLiteConnection sqlCon)
        {
            return SqlLiteData.GetNextStartForTransaction(fk_videoevent_media, projdetId, sqlCon);
        }

        /// <summary>
        /// Return next available end time by adding start and duration
        /// </summary>
        public static string CalcNextEnd(string start, string duration)
        {
            return SqlLiteData.ShiftRight(start, duration);
        }

        /// <summary>
        /// Return next available end time by adding start and duration
        /// </summary>
        public static string ShiftRight(string start, string duration)
        {
            return SqlLiteData.ShiftRight(start, duration);
        }

        /// <summary>
        /// Return next available end time by subtracting start and duration
        /// </summary>
        public static string ShiftLeft(string time, string durationToShift)
        {
            return SqlLiteData.ShiftLeft(time, durationToShift);
        }

        /// <summary>
        /// Convert Timespan to millisceond
        /// </summary>
        public static int GetMillisecondsFromTimespan(string timespan)
        {
            return SqlLiteData.GetMillisecondsFromTimespan(timespan);
        }

        /// <summary>
        /// Convert millisecond to Timespan
        /// </summary>
        public static string GetTimespanFromSeconds(int seconds)
        {
            return SqlLiteData.GetTimespanFromSeconds(seconds);
        }

        #endregion
    }
}
