using Sqllite_Library.Data;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sqllite_Library.Business
{
    public static class DataManagerSqlLite
    {

        #region == Database Methods ==
        public static string CreateDatabaseIfNotExist(bool encryptFlag, bool canCreateRegistryIfNotExists = false)
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

            return message;
        }

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
                //var projectFlag = SqlLiteData.ReferentialKeyPresent("cbv_project", "project_id", (int)rowMain["fk_videoevent_projdet"]);
                //if (!projectFlag)
                //    throw new Exception("projdet_Id foreign key constraint not successful ");

                var mediaId = (int)rowMain["fk_videoevent_media"];
                //var mediaFlag = SqlLiteData.ReferentialKeyPresent("cbv_media", "media_id", mediaId);
                //if (!mediaFlag)
                //    throw new Exception("media_Id foreign key constraint not successful ");

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

        public static int InsertRowsToVideoSegment(DataTable data, int fk_value)
        {
            // For Johan
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_serverid", fk_value);
            //if (!videoeventFlag)
            //    throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToVideoSegment(data);
        }

        public static int InsertRowsToDesign(DataTable data)
        {
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_design_videoevent"]);
            //if (!videoeventFlag)
            //    throw new Exception("videoevent_Id foreign key constraint not successful ");

            //var bgFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)data?.Rows[0]["fk_design_background"]);
            //if (!bgFlag)
            //    throw new Exception("background_Id foreign key constraint not successful ");

            //var screenFlag = SqlLiteData.ReferentialKeyPresent("cbv_screen", "screen_id", (int)data?.Rows[0]["fk_design_screen"]);
            //if (!screenFlag)
            //    throw new Exception("screen_Id foreign key constraint not successful ");


            return SqlLiteData.InsertRowsToDesign(data);
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

        public static int InsertRowsToNotes(DataTable data)
        {
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_videoevent", "videoevent_id", (int)data?.Rows[0]["fk_notes_videoevent"]);
            //if (!videoeventFlag)
            //    throw new Exception("videoevent_Id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToNotes(data);
        }

        public static List<int> InsertRowsToLocAudio(DataTable data)
        {
            //var videoeventFlag = SqlLiteData.ReferentialKeyPresent("cbv_notes", "notes_id", (int)data?.Rows[0]["fk_locaudio_notes"]);
            //if (!videoeventFlag)
            //    throw new Exception("notes_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToLocAudio(data);
        }

        public static int InsertRowsToFinalMp4(DataTable data)
        {
            //var projectFlag = SqlLiteData.ReferentialKeyPresent("cbv_project", "project_id", (int)data?.Rows[0]["fk_finalmp4_project"]);
            //if (!projectFlag)
            //    throw new Exception("project_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToFinalMp4(data);
        }

        public static int InsertRowsToCompany(DataTable data)
        {
            return SqlLiteData.InsertRowsToCompany(data);
        }

        public static int InsertRowsToBackground(DataTable data)
        {
            //var companyFlag = SqlLiteData.ReferentialKeyPresent("cbv_company", "company_id", (int)data?.Rows[0]["fk_background_company"]);
            //if (!companyFlag)
            //    throw new Exception("company_id foreign key constraint not successful ");

            return SqlLiteData.InsertRowsToBackground(data);
        }

        public static List<int> InsertRowsToVoiceTimer(DataTable data)
        {
            return SqlLiteData.InsertRowsToVoiceTimer(data);
        }

        public static List<int> InsertRowsToVoiceAverage(DataTable data)
        {
            return SqlLiteData.InsertRowsToVoiceAverage(data);
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

        public static List<CBVPlanningHead> GetPlanningHead()
        {
            return SqlLiteData.GetPlanningHead();
        }

        public static List<CBVPlanning> GetPlanning(int projectId, bool dependentFlag = true)
        {
            return SqlLiteData.GetPlanning(projectId, dependentFlag);
        }

        public static List<CBVProjectForJoin> GetDownloadedProjectList()
        {
            return SqlLiteData.GetDownloadedProjectList();
        }

        public static CBVProject GetProjectById(int projectId = -1, bool projdetFlag = false)
        {
            return SqlLiteData.GetProjectById(projectId, projdetFlag).FirstOrDefault();
        }

        public static int GetProjectsCount()
        {
            return SqlLiteData.GetCount("cbv_project");
        }

        public static int GetBackgroundsCount()
        {
            return SqlLiteData.GetCount("cbv_background");
        }

        public static int GetVoiceTimerCount()
        {
            return SqlLiteData.GetCount("cbv_voicetimer");
        }

        public static int GetVoiceAverageCount() // For Testing Purpose
        {
            return SqlLiteData.GetCount("cbv_voiceaverage");
        }

        public static int GetVideoEventCountProjectAndDetailId(int ProjectId, int ProjectdetailId)
        {
            return SqlLiteData.GetVideoEventCountProjectAndDetailId(ProjectId, ProjectdetailId);
        }

        // Lets not fetch dependent tables be default and thats why flag is false
        public static List<CBVVideoEvent> GetVideoEvents(int projdetId = 0, bool dependentDataFlag = false, bool designFlag = false)
        {
            return SqlLiteData.GetVideoEvents(projdetId, dependentDataFlag, designFlag);
        }


        public static List<CBVVideoEvent> GetNotSyncedVideoEvents(int projdetId = 0, bool dependentDataFlag = true)
        {
            return SqlLiteData.GetNotSyncedVideoEvents(projdetId, dependentDataFlag);
        }

        public static List<CBVVideoEvent> GetVideoEventbyId(int videoeventId = 0, bool dependentDataFlag = false, bool designFlag = false)
        {
            return SqlLiteData.GetVideoEventbyId(videoeventId, dependentDataFlag, designFlag);
        }


        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyEndTime(int fk_videoevent_projdet, string endTime, EnumTrack track = EnumTrack.IMAGEORVIDEO)
        {
            return SqlLiteData.GetShiftVideoEventsbyTime(fk_videoevent_projdet, endTime, track);
        }

        public static List<CBVShiftVideoEvent> GetShiftVideoEventsbyStartTime(int fk_videoevent_projdet, string startTime, EnumTrack track = EnumTrack.IMAGEORVIDEO)
        {
            return SqlLiteData.GetShiftVideoEventsbyTime(fk_videoevent_projdet, startTime, track);
        }

        public static List<CBVVideoEvent> GetOverlappingCalloutsByTime(int fk_videoevent_projdet, string startTime, string endtime)
        {
            return SqlLiteData.GetOverlappingCalloutsByTime(fk_videoevent_projdet, startTime, endtime);
        }

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

        public static List<CBVNotes> GetNotesbyId(int notesId)
        {
            return SqlLiteData.GetNotesbyId(notesId);
        }

        public static int GetMaxIndexForNotes(int fkVideoEventId)
        {
            return SqlLiteData.GetMaxIndexForNotes(fkVideoEventId);
        }

        public static List<CBVLocAudio> GetLocAudio(int notesId = -1)
        {
            return SqlLiteData.GetLocAudio(notesId);
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

        public static List<CBVVoiceTimer> GetVoiceTimers()
        {
            return SqlLiteData.GetVoiceTimers();
        }

        public static List<CBVVoiceAvergae> GetVoiceAverage()
        {
            return SqlLiteData.GetVoiceAverage();
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

        public static void UpdateVideoEventDataTableServerId(int localId, Int64 serverId, string ErrorMessage = "")
        {
            SqlLiteData.UpdateServerId("videoevent", localId, serverId, ErrorMessage);
        }

        public static void UpdateRowsToVideoSegment(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVideoSegment(dataTable);
        }

        public static void UpdateVideoSegmentDataTableServerId(int localId, Int64 serverId, string ErrorMessage = "")
        {
            SqlLiteData.UpdateServerId("videosegment", localId, serverId, ErrorMessage);
        }

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

        public static void UpdateRowsToDesign(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToDesign(dataTable);
        }

        public static void UpdateRowsToNotes(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToNotes(dataTable);
        }

        public static void UpdateRowsToNotes(DataRow dr)
        {
            SqlLiteData.UpdateRowsToNotes(dr);
        }

        public static void UpdateRowsToLocAudio(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToLocAudio(dataTable);
        }

        public static void UpdateRowsToFinalMp4(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToFinalMp4(dataTable);
        }

        public static void UpdateRowsToVoiceTimer(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVoiceTimer(dataTable);
        }

        public static void UpdateRowsToVoiceAverage(DataTable dataTable)
        {
            SqlLiteData.UpdateRowsToVoiceAverage(dataTable);
        }

        public static void ShiftVideoEvents(DataTable dataTable)
        {
            SqlLiteData.ShiftVideoEvents(dataTable);
        }

        #endregion


        #region == DELETE Methods ==

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
        public static void UpsertRowsToApp(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToApp(dataTable);
        }

        public static void UpsertRowsToMedia(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToMedia(dataTable);
        }

        public static void UpsertRowsToScreen(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToScreen(dataTable);
        }

        public static void UpsertRowsToCompany(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToCompany(dataTable);
        }

        public static void UpsertRowsToBackground(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToBackground(dataTable);
        }

        public static void UpsertRowsToPlanningHead(DataTable dataTable)
        {
            SqlLiteData.UpsertRowsToPlanningHead(dataTable);
        }

        public static int IsProjectAvailable(int projectServerId)
        {
            return SqlLiteData.IsProjectAvailable(projectServerId);
        }

        public static int UpsertRowsToProjectbyId(DataTable data, int projectServerId, bool projdetAvailable)
        {
            //foreach (DataRow rowMain in data.Rows)
            //{
            //    //var backgroundFlag = SqlLiteData.ReferentialKeyPresent("cbv_background", "background_id", (int)rowMain["fk_project_background"]);
            //    //if (!backgroundFlag)
            //    //    throw new Exception("background_id foreign key constraint not successful ");
            //}

            var projectId = SqlLiteData.IsProjectAvailable(projectServerId);
            if (projectId == -1)
                projectId = SqlLiteData.UpsertRowsToProject(data);

            if (projdetAvailable)
                SqlLiteData.InsertRowsToProjectDetail(data, projectId);
            return projectId;
        }

        public static List<int> UpsertRowsToPlanning(DataTable data, int projectId)
        {
            var planningId = SqlLiteData.IsProjectPlanningAvailable(projectId);
            if (planningId == -1)
            {
                var planningIds = SqlLiteData.InsertRowsToPlanning(data);
                return planningIds;
            }
            return null;
        }

        #endregion


        public static string GetNextStart(int fk_videoevent_media, int projdetId)
        {
            return SqlLiteData.GetNextStart(fk_videoevent_media, projdetId);
        }


        public static string CalcNextEnd(string start, string duration)
        {
            return SqlLiteData.ShiftRight(start, duration);
        }

        public static string ShiftRight(string start, string duration)
        {
            return SqlLiteData.ShiftRight(start, duration);
        }

        public static string ShiftLeft(string time, string durationToShift)
        {
            return SqlLiteData.ShiftLeft(time, durationToShift);
        }

        public static int GetMillisecondsFromTimespan(string timespan)
        {
            return SqlLiteData.GetMillisecondsFromTimespan(timespan);
        }

        public static string GetTimespanFromSeconds(int seconds)
        {
            return SqlLiteData.GetTimespanFromSeconds(seconds);
        }

    }
}
