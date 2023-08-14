using System;
using Sqllite_Library.Data;
using Sqllite_Library.Models;
using System.Data;
using System.Collections.Generic;
using Sqllite_Library.Helpers;

namespace Sqllite_Library.Business
{
    public static class DataManagerSqlLite
    {

        #region == Database Methods ==
        public static string CreateDatabaseIfNotExist(bool encryptFlag, bool canCreateRegistryIfNotExists = false)
        {
            string message;
            try
            {
                if (SqlLiteData.IsDbCreated())
                {
                    message = $"{RegisteryHelper.GetFileName()} database already exists!!";
                }
                else
                {
                    SqlLiteData.CreateDatabaseIfNotExist(encryptFlag, canCreateRegistryIfNotExists);
                    message = $"{RegisteryHelper.GetFileName()} database created successfully!!";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return message;
        }

        public static string ClearRegistryAndDeleteDB()
        {
            string message;
            try
            {
                SqlLiteData.DeleteDB();
                RegisteryHelper.ClearRegistry();
                message = $"Registry Cleaned successfully !!";
            }
            catch (Exception err)
            {
                message = $"Error - {err.Message} !!";
            }
            return message;
        }

        #endregion


        #region == Insert Methods ==

        public static int InsertRowsToProject(DataTable data)
        {
            foreach (DataRow rowMain in data.Rows)
            {
                var backgroundFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)rowMain["fk_project_background"]);
                if (!backgroundFlag)
                    throw new Exception("background_id foreign key constraint not successful ");
            }
            return SqlLiteData.InsertRowsToProject(data);
        }

        #region == Video Event and Depenedent Tables ==

        public static List<int> InsertRowsToVideoEvent(DataTable data, bool populateDependentTablesFlag = true)
        {
            var insertedIds = new List<int>();
            foreach (DataRow rowMain in data.Rows)
            {
                var projectFlag = SqlLiteData.ReferentialKeyPresent("cbv_project", "project_id", (int)rowMain["fk_videoevent_project"]);
                if (!projectFlag)
                    throw new Exception("project_Id foreign key constraint not successful ");

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
                        dtVideoSegment.Columns.Add("fk_videosegment_videoevent", typeof(int));
                        dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
                        dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
                        dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));

                        var rowVideoSegment = dtVideoSegment.NewRow();
                        rowVideoSegment["videosegment_id"] = -1;
                        rowVideoSegment["fk_videosegment_videoevent"] = insertedId;
                        rowVideoSegment["videosegment_media"] = rowMain["media"];
                        rowVideoSegment["videosegment_createdate"] = createDate;
                        rowVideoSegment["videosegment_modifydate"] = modifyDate;
                        dtVideoSegment.Rows.Add(rowVideoSegment);

                        var insertedVideoSegmentId = InsertRowsToVideoSegment(dtVideoSegment);
                        if (insertedVideoSegmentId <= 0) throw new Exception("Error while inserting videosegment table");
                    }
                    else if (mediaId == 3) //Audio
                    {
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

                        var rowDesign = dtDesign.NewRow();
                        rowDesign["design_id"] = -1;
                        rowDesign["fk_design_videoevent"] = insertedId;
                        rowDesign["fk_design_screen"] = (int)rowMain["fk_design_screen"];
                        rowDesign["design_xml"] = rowMain["design_xml"].ToString();
                        rowDesign["design_createdate"] = createDate;
                        rowDesign["design_modifydate"] = modifyDate;
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

        public static int InsertRowsToVideoSegment(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_videosegment_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToVideoSegment(data);
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

        public static int InsertRowsToAudio(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_audio_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToAudio(data);
        }

        #endregion

        public static int InsertRowsToNotes(DataTable data)
        {
            var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_notes_videoevent"]);
            if (!videoeventFlag)
                throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToNotes(data);
        }

        public static int InsertRowsToFinalMp4(DataTable data)
        {
            var projectFlag = SqlLiteData.ReferentialKeyPresent("cbv_project", "project_id", (int)data?.Rows[0]["fk_finalmp4_project"]);
            if (!projectFlag)
                throw new Exception("project_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToFinalMp4(data);
        }

        public static int InsertRowsToCompany(DataTable data)
        {
            return SqlLiteData.InsertRowsToCompany(data);
        }

        public static int InsertRowsToBackground(DataTable data)
        {
            var companyFlag = SqlLiteData.ReferentialKeyPresent("cbv_company", "company_id", (int)data?.Rows[0]["fk_background_company"]);
            if (!companyFlag)
                throw new Exception("company_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToBackground(data);
        }

        #endregion


        #region == Sync methods ==

        public static List<int> SyncApp(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncApp(dataTable);
            return insertedIds;
        }

        public static List<int> SyncScreen(DataTable dataTable)
        {
            var insertedId = SqlLiteData.SyncScreen(dataTable);
            return insertedId;
        }

        public static List<int> SyncMedia(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncMedia(dataTable);
            return insertedIds;
        }

        public static List<int> SyncCompany(DataTable dataTable)
        {
            var insertedIds = SqlLiteData.SyncCompany(dataTable);
            return insertedIds;
        }

        #endregion


        #region == Get Methods ==

        public static List<CBVMedia> GetMedia()
        {
            return SqlLiteData.GetMedia();
        }

        public static List<CBVScreen> GetScreens()
        {
            return SqlLiteData.GetScreens();
        }

        public static List<CBVApp> GetApp()
        {
            return SqlLiteData.GetApp();
        }

        public static List<CBVProject> GetProjects(bool includeArchived = false, bool startedFlag = false)
        {
            return SqlLiteData.GetProjects(includeArchived, startedFlag);
        }

        public static List<CBVWIPOrArchivedProjectList> GetWIPOrArchivedProjectList(bool includeArchived = false, bool startedFlag = false)
        {
            return SqlLiteData.GetWIPOrArchivedProjectList(includeArchived, startedFlag);
        }

        public static List<CBVPendingProjectList> GetPendingProjectList()
        {
            return SqlLiteData.GetPendingProjectList();
        }

        public static int GetProjectsCount()
        {
            return SqlLiteData.GetCount("cbv_project");
        }

        public static int GetBackgroundsCount()
        {
            return SqlLiteData.GetCount("cbv_background");
        }

        // Lets not fetch dependent tables be default and thats why flag is false
        public static List<CBVVideoEvent> GetVideoEvents(int projectId = 0, bool dependentDataFlag = false)
        {
            return SqlLiteData.GetVideoEvents(projectId, dependentDataFlag);
        }

        public static int GetVideoEventsCount()
        {
            return SqlLiteData.GetCount("cbv_videoevent");
        }

        public static List<CBVAudio> GetAudio(int VideoEventId = -1)
        {
            return SqlLiteData.GetAudio(VideoEventId);
        }

        public static List<CBVVideoSegment> GetVideoSegment(int VideoEventId = -1)
        {
            return SqlLiteData.GetVideoSegment(VideoEventId);
        }

        public static List<CBVDesign> GetDesign(int VideoEventId = -1)
        {
            return SqlLiteData.GetDesign(VideoEventId);
        }

        public static List<CBVNotes> GetNotes(int VideoEventId = -1)
        {
            return SqlLiteData.GetNotes(VideoEventId);
        }

        public static List<CBVFinalMp4> GetFinalMp4(int ProjectId = -1, bool dependentDataFlag = false)
        {
            return SqlLiteData.GetFinalMp4(ProjectId, dependentDataFlag);
        }

        public static List<CBVCompany> GetCompany()
        {
            return SqlLiteData.GetCompany();
        }

        public static List<CBVBackground> GetBackground(int company_id = -1)
        {
            return SqlLiteData.GetBackground(company_id);
        }

        #endregion


        #region == Update Methods ==

        public static void UpdateRowsToProject(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToProject(dataTable);
        }

        public static void UpdateRowsToVideoEvent(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVideoEvent(dataTable);
        }

        public static void UpdateRowsToVideoSegment(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVideoSegment(dataTable);
        }

        public static void UpdateRowsToAudio(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToAudio(dataTable);
        }

        public static void UpdateRowsToDesign(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToDesign(dataTable);
        }

        public static void UpdateRowsToNotes(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToNotes(dataTable);
        }

        public static void UpdateRowsToFinalMp4(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToFinalMp4(dataTable);
        }

        #endregion

    }
}
