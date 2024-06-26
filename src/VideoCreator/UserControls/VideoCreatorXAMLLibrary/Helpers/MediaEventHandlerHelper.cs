﻿using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Models;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class MediaEventHandlerHelper
    {
        #region === Post to server and then save locally ==

        public static async Task<VideoEventResponseModel> PostVideoEventToServerForVideoOrImage(DataRow row, DataTable dtNotes, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)row["fk_videoevent_media"];
            objToSync.videoevent_track = (int)EnumTrack.IMAGEORVIDEO; // TBD
            objToSync.videoevent_start = Convert.ToString(row["videoevent_start"]);
            objToSync.videoevent_duration = Convert.ToString(row["videoevent_duration"]);
            objToSync.videoevent_origduration = Convert.ToString(row["videoevent_origduration"]);
            if(row.Table.Columns["videoevent_planning"] != null && row["videoevent_planning"] != DBNull.Value)
                objToSync.videoevent_planning = Convert.ToInt32(row["videoevent_planning"]);
            else
                objToSync.videoevent_planning = 0;
            objToSync.videoevent_end = DataManagerSqlLite.CalcNextEnd(objToSync.videoevent_start, objToSync.videoevent_duration);
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.videosegment_media_bytes = row["media"] == null ? new byte[0] : (byte[])row["media"];
            if (dtNotes != null && dtNotes.Rows.Count > 0)
            {
                objToSync.notes = new List<NotesModelPost>();
                foreach (DataRow dr in dtNotes.Rows)
                {
                    var note = new NotesModelPost
                    {
                        notes_line = Convert.ToString(dr["notes_line"]),
                        notes_index = Convert.ToString(dr["notes_index"]),
                        notes_wordcount = Convert.ToInt32(dr["notes_wordcount"]),
                        notes_start = Convert.ToString(dr["notes_start"]),
                        notes_duration = Convert.ToString(dr["notes_duration"]),
                        notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    objToSync.notes.Add(note);
                }
            }
            var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
            return result;
        }

        public static async Task<VideoEventModel> UpdateVideoEventOnlyToServer(CBVVideoEvent videoevent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = videoevent.fk_videoevent_media;
            objToSync.videoevent_track = videoevent.videoevent_track;
            objToSync.videoevent_start = videoevent.videoevent_start;
            objToSync.videoevent_duration = Convert.ToString(videoevent.videoevent_duration);
            objToSync.videoevent_origduration = Convert.ToString(videoevent.videoevent_duration); // TBD
            //objToSync.videoevent_origduration = Convert.ToString(videoevent.videoevent_origduration);
            objToSync.videoevent_end = videoevent.videoevent_end;
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var result = await authApiViewModel.UpdateVideoEventOnlyToServer(selectedProjectEvent, videoevent.videoevent_serverid, objToSync);
            return result;
        }

        public static async Task<VideoEventResponseModel> PostVideoEventToServerBackground(CBVVideoEvent videoEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = videoEvent.fk_videoevent_media;
            objToSync.videoevent_track = videoEvent.videoevent_track;
            objToSync.videoevent_start = videoEvent.videoevent_start;
            objToSync.videoevent_duration = videoEvent.videoevent_duration;
            objToSync.videoevent_origduration = videoEvent.videoevent_origduration;
            objToSync.videoevent_end = videoEvent.videoevent_end;
            objToSync.videoevent_modifylocdate = videoEvent.videoevent_createdate.ToString("yyyy-MM-dd HH:mm:ss");
            if (videoEvent?.videosegment_data?.Count > 0)
                objToSync.videosegment_media_bytes = (byte[])videoEvent?.videosegment_data[0]?.videosegment_media;
            if (videoEvent?.design_data?.Count > 0)
                objToSync.design.AddRange(DesignEventHandlerHelper.GetDesignModelList(videoEvent.design_data));
            var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
            return result;
        }

        public static async Task<List<ShiftVideoEventModel>> ShiftVideoEventsToServer(SelectedProjectEvent selectedProjectEvent, List<ShiftVideoEventModel> shiftVideoEvents, AuthAPIViewModel authApiViewModel)
        {
            var result = await authApiViewModel.ShiftVideoEvent(selectedProjectEvent, shiftVideoEvents);
            return result;
        }

        public static DataTable GetVideoEventDataTableForVideoOrImage(VideoEventResponseModel addedData, int selectedProjdetId)
        {
            var dtVideoEvent = GetVideoEventDataTable();

            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = selectedProjdetId;
            row["videoevent_start"] = addedData.videoevent.videoevent_start;
            row["videoevent_track"] = addedData.videoevent.videoevent_track;
            row["videoevent_duration"] = addedData.videoevent.videoevent_duration;
            row["videoevent_origduration"] = addedData.videoevent.videoevent_origduration;
            row["videoevent_planning"] = addedData.videoevent.videoevent_planning;
            row["fk_videoevent_media"] = addedData.videoevent.fk_videoevent_media;
            row["videoevent_createdate"] = addedData.videoevent.videoevent_createdate;
            row["videoevent_modifydate"] = addedData.videoevent.videoevent_modifydate;
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = true;
            row["videoevent_serverid"] = addedData.videoevent.videoevent_id;
            row["videoevent_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }


        public static DataTable GetVideoEventTableWithData(int selectedProjdetId, AllVideoEventResponseModel videoevent)
        {
            var dt = GetVideoEventDataTable();
            var row = dt.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = selectedProjdetId;
            row["videoevent_start"] = videoevent.videoevent_start;
            row["videoevent_track"] = videoevent.videoevent_track;
            row["videoevent_duration"] = videoevent.videoevent_duration;
            row["videoevent_origduration"] = videoevent.videoevent_origduration;
            row["fk_videoevent_media"] = videoevent.fk_videoevent_media;
            row["videoevent_createdate"] = videoevent.videoevent_createdate;
            row["videoevent_modifydate"] = videoevent.videoevent_modifydate;
            row["videoevent_isdeleted"] = videoevent.videoevent_isdeleted;
            row["videoevent_issynced"] = true;
            row["videoevent_serverid"] = videoevent.videoevent_id;
            row["videoevent_syncerror"] = "";
            dt.Rows.Add(row);
            return dt;
        }


        public static DataTable GetVideoSegmentDataTableForVideoOrImage(byte[] blob, int videoeventId, VideoSegmentModel videosegment)
        {
            var dt = GetVideoSegmentDataTable();

            var row = dt.NewRow();
            row["videosegment_id"] = videoeventId;
            row["videosegment_media"] = blob;

            row["videosegment_createdate"] = videosegment.videosegment_createdate;
            row["videosegment_modifydate"] = videosegment.videosegment_modifydate;
            row["videosegment_isdeleted"] = false;
            row["videosegment_issynced"] = true;
            row["videosegment_serverid"] = videosegment.videosegment_id;
            row["videosegment_syncerror"] = "";
            dt.Rows.Add(row);
            return dt;
        }
        #endregion

        #region == Save to Local ==
        public static DataTable GetVideoEventDataTableForVideoOrImageLocally(DataRow datarow, Int64 serverVideoEventId, Int64 selectedProjdetId)
        {
            var dtVideoEvent = GetVideoEventDataTable();

            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = selectedProjdetId;
            row["videoevent_start"] = "00:00:00.000"; // TBD
            row["videoevent_track"] = 2; // TBD
            row["videoevent_duration"] = Convert.ToString(datarow["videoevent_duration"]);
            row["videoevent_origduration"] = Convert.ToString(datarow["videoevent_origduration"]);
            row["videoevent_origduration"] = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            row["fk_videoevent_media"] = (int)datarow["fk_videoevent_media"];
            row["videoevent_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = false;
            row["videoevent_serverid"] = serverVideoEventId;
            row["videoevent_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        public static DataTable GetVideoSegmentDataTableForVideoOrImageLocally(byte[] blob, int videoeventId, Int64 serverVideosegmentId)
        {
            var dt = GetVideoSegmentDataTable();

            var row = dt.NewRow();
            row["videosegment_id"] = videoeventId;
            row["videosegment_media"] = blob;

            row["videosegment_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videosegment_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videosegment_isdeleted"] = false;
            row["videosegment_issynced"] = false;
            row["videosegment_serverid"] = serverVideosegmentId;
            row["videosegment_syncerror"] = "";
            dt.Rows.Add(row);
            return dt;
        }

        #endregion

        #region == Private Methods ==

        private static DataTable GetVideoSegmentDataTable()
        {
            var dtVideoSegment = new DataTable();
            dtVideoSegment.Columns.Add("videosegment_id", typeof(int));
            dtVideoSegment.Columns.Add("videosegment_media", typeof(byte[]));
            dtVideoSegment.Columns.Add("videosegment_createdate", typeof(string));
            dtVideoSegment.Columns.Add("videosegment_modifydate", typeof(string));
            dtVideoSegment.Columns.Add("videosegment_isdeleted", typeof(bool));
            dtVideoSegment.Columns.Add("videosegment_issynced", typeof(bool));
            dtVideoSegment.Columns.Add("videosegment_serverid", typeof(Int64));
            dtVideoSegment.Columns.Add("videosegment_syncerror", typeof(string));
            return dtVideoSegment;
        }

        private static DataTable GetVideoEventDataTable()
        {
            var dtVideoEvent = new DataTable();
            dtVideoEvent.Columns.Add("videoevent_id", typeof(int));
            dtVideoEvent.Columns.Add("fk_videoevent_projdet", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_start", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_duration", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_origduration", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_planning", typeof(Int64));
            dtVideoEvent.Columns.Add("videoevent_track", typeof(int));
            dtVideoEvent.Columns.Add("fk_videoevent_media", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_createdate", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_modifydate", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_isdeleted", typeof(bool));
            dtVideoEvent.Columns.Add("videoevent_issynced", typeof(bool));
            dtVideoEvent.Columns.Add("videoevent_serverid", typeof(Int64));
            dtVideoEvent.Columns.Add("videoevent_syncerror", typeof(string));
            return dtVideoEvent;
        }

        #endregion

    }
}
