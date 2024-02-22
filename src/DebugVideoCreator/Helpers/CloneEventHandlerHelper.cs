using VideoCreator.Models;
using NAudio.CoreAudioApi.Interfaces;
using ServerApiCall_UserControl.DTO;
using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using VideoCreator.Auth;

namespace VideoCreator.Helpers
{
    public static class CloneEventHandlerHelper
    {
        #region === Clone Functions ==
        public static async Task<VideoEventResponseModel> PostVideoEventToServerForClone(List<CBVVideoEvent> videoEventList, byte[] blob, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, string startTime = "00:00:00.000")
        {
            if (videoEventList != null && videoEventList?.Count > 0)
            {
                var videoEvent = videoEventList[0];
                var dtNotes = GetNotesDataTableClient(videoEvent.notes_data, -1);
                var dtDesign = GetDesignDataTableClient(videoEvent.design_data, -1);
                var dtVideoSegment = GetVideoSegmentDataTableClient(videoEvent.videosegment_data, selectedProjectEvent.serverProjectId, -1);

                var objToSync = new VideoEventModel();
                objToSync.fk_videoevent_media = videoEvent.fk_videoevent_media;
                objToSync.videoevent_track = videoEvent.videoevent_track;
                objToSync.videoevent_start = startTime;
                objToSync.videoevent_duration = videoEvent.videoevent_duration;
                objToSync.videoevent_end = DataManagerSqlLite.CalcNextEnd(startTime, videoEvent.videoevent_duration);
                objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                objToSync.design.AddRange(GetDesignModelList(dtDesign));
                objToSync.notes.AddRange(GetNotesModelList(dtNotes));
               
                if (blob != null)
                    objToSync.videosegment_media_bytes = blob;

                var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
                return result;
            }
            return null;
        }
        
        public static byte[] GetBlobBytes(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
                return (byte[])row["videosegment_media"];
            return null;
        }

        #endregion

        #region == Notes Section ==

        public static DataTable GetNotesDataTableServer(List<NotesModel> notes, int localVideoEventId)
        {
            var dt = GetRawNotesDataTable();
            foreach (var item in notes)
            {
                var dRow = dt.NewRow();
                dRow["notes_index"] = item.notes_index;
                dRow["notes_id"] = -1;
                dRow["notes_line"] = item.notes_line;
                dRow["notes_start"] = item.notes_start;
                dRow["notes_duration"] = item.notes_duration;
                dRow["notes_createdate"] = item.notes_createdate;
                dRow["notes_modifydate"] = item.notes_modifydate;
                dRow["fk_notes_videoevent"] = localVideoEventId;
                dRow["notes_wordcount"] = item.notes_wordcount;
                dRow["notes_isdeleted"] = item.notes_isdeleted;
                dRow["notes_issynced"] = true;
                dRow["notes_serverid"] = item.notes_id;
                dRow["notes_syncerror"] = "";
                dt.Rows.Add(dRow);
            }

            return dt;
        }
        
        private static DataTable GetNotesDataTableClient(List<CBVNotes> notes, int localVideoEventId = -1)
        {
            var dtNotes = GetRawNotesDataTable();
            foreach (var item in notes)
            {
                var dRow = dtNotes.NewRow();
                dRow["notes_index"] = item.notes_index;
                dRow["notes_id"] = -1;
                dRow["notes_line"] = item.notes_line;
                dRow["notes_start"] = item.notes_start; // TBD
                dRow["notes_duration"] = item.notes_duration;
                dRow["notes_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["notes_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["fk_notes_videoevent"] = localVideoEventId;
                dRow["notes_wordcount"] = item.notes_wordcount;
                dRow["notes_isdeleted"] = item.notes_isdeleted;
                dRow["notes_issynced"] = false;
                dRow["notes_serverid"] = -1;
                dRow["notes_syncerror"] = "";
                dtNotes.Rows.Add(dRow);
            }
            return dtNotes;
        }

        private static DataTable GetRawNotesDataTable()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_start", typeof(string));
            dtNotes.Columns.Add("notes_duration", typeof(string));
            dtNotes.Columns.Add("notes_index", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));
            dtNotes.Columns.Add("notes_isdeleted", typeof(bool));
            dtNotes.Columns.Add("notes_issynced", typeof(bool));
            dtNotes.Columns.Add("notes_serverid", typeof(Int64));
            dtNotes.Columns.Add("notes_syncerror", typeof(string));
            return dtNotes;
        }

        private static List<NotesModelPost> GetNotesModelList(DataTable dt)
        {
            var data = new List<NotesModelPost>();
            foreach (DataRow note in dt.Rows)
            {
                var notesModel = new NotesModelPost();
                notesModel.notes_line = Convert.ToString(note["notes_line"]);
                notesModel.notes_wordcount = Convert.ToInt16(note["notes_wordcount"]);
                notesModel.notes_index = Convert.ToString(note["notes_index"]);
                notesModel.notes_start = Convert.ToString(note["notes_start"]);
                notesModel.notes_duration = Convert.ToString(note["notes_duration"]);
                notesModel.notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(notesModel);
            }
            return data;
        }


        #endregion


        #region == Design Section ==

        public static DataTable GetDesignDataTableServer(List<DesignModel> alldesigns, int localVideoEventId)
        {
            var dt = GetRawDesignDataTable();

            foreach (var item in alldesigns)
            {
                var row = dt.NewRow();
                row["design_id"] = -1;
                row["fk_design_screen"] = item.fk_design_screen;
                row["fk_design_background"] = 1; // TBD
                row["fk_design_videoevent"] = localVideoEventId;
                row["design_xml"] = item.design_xml;

                row["design_createdate"] = item.design_createdate;
                row["design_modifydate"] = item.design_modifydate;
                row["design_isdeleted"] = item.design_isdeleted;
                row["design_issynced"] = true;
                row["design_serverid"] = item.design_id;
                row["design_syncerror"] = "";
                dt.Rows.Add(row);
            }

            return dt;
        }

        private static DataTable GetDesignDataTableClient(List<CBVDesign> alldesigns, int localVideoEventId = -1)
        {
            var dt = GetRawDesignDataTable();
            foreach (var item in alldesigns)
            {
                var row = dt.NewRow();
                row["design_id"] = -1;
                row["fk_design_screen"] = item.fk_design_screen;
                row["fk_design_background"] = item.fk_design_background;
                row["fk_design_videoevent"] = localVideoEventId;
                row["design_xml"] = item.design_xml;

                row["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["design_isdeleted"] = item.design_isdeleted;
                row["design_issynced"] = false;
                row["design_serverid"] = -1;
                row["design_syncerror"] = "";
                dt.Rows.Add(row);
            }
            return dt;
        }

        private static DataTable GetRawDesignDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("design_id", typeof(int));
            dt.Columns.Add("fk_design_screen", typeof(int));
            dt.Columns.Add("fk_design_background", typeof(int));
            dt.Columns.Add("fk_design_videoevent", typeof(int));
            dt.Columns.Add("design_xml", typeof(string));
            dt.Columns.Add("design_createdate", typeof(string));
            dt.Columns.Add("design_modifydate", typeof(string));
            dt.Columns.Add("design_isdeleted", typeof(bool));
            dt.Columns.Add("design_issynced", typeof(bool));
            dt.Columns.Add("design_serverid", typeof(Int64));
            dt.Columns.Add("design_syncerror", typeof(string));

            return dt;
        }

        private static List<DesignModelPost> GetDesignModelList(DataTable dtDesign)
        {
            var data = new List<DesignModelPost>();
            foreach (DataRow design in dtDesign.Rows)
            {
                var designModel = new DesignModelPost();
                designModel.fk_design_screen = Convert.ToInt16(design["fk_design_screen"]);
                designModel.design_xml = Convert.ToString(design["design_xml"]);
                data.Add(designModel);
            }
            return data;
        }

        #endregion


        #region == VideoSegment Section ==

        public static DataTable GetVideoSegmentDataTableServer(byte[] blob, VideoSegmentModel postResponse, int fk_videosegment_videoevent)
        {
            var dtVideoEvent = GetRawVideoSegmentDataTable();
            var row = dtVideoEvent.NewRow();
            row["videosegment_id"] = fk_videosegment_videoevent;
            row["videosegment_media"] = blob;

            row["videosegment_createdate"] = postResponse.videosegment_createdate;
            row["videosegment_modifydate"] = postResponse.videosegment_modifydate;
            row["videosegment_isdeleted"] = false;
            row["videosegment_issynced"] = true;
            row["videosegment_serverid"] = postResponse.videosegment_id;
            row["videosegment_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        public static DataTable GetVideoSegmentDataTableClient(List<CBVVideoSegment> videosegments, Int64 selectedProjectId, int localId = -1)
        {
            var dt = GetRawVideoSegmentDataTable();
            foreach (var item in videosegments)
            {
                var row = dt.NewRow();
                row["videosegment_id"] = localId;
                row["videosegment_media"] = item.videosegment_media;
                row["videosegment_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["videosegment_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["videosegment_isdeleted"] = item.videosegment_isdeleted;
                row["videosegment_issynced"] = false;
                row["videosegment_serverid"] = -1;
                row["videosegment_syncerror"] = "";
                dt.Rows.Add(row);
            }
            return dt;
        }

        private static DataTable GetRawVideoSegmentDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("videosegment_id", typeof(int));
            dt.Columns.Add("videosegment_media", typeof(byte[]));
            dt.Columns.Add("videosegment_createdate", typeof(string));
            dt.Columns.Add("videosegment_modifydate", typeof(string));
            dt.Columns.Add("videosegment_isdeleted", typeof(bool));
            dt.Columns.Add("videosegment_issynced", typeof(bool));
            dt.Columns.Add("videosegment_serverid", typeof(Int64));
            dt.Columns.Add("videosegment_syncerror", typeof(string));

            return dt;
        }

        #endregion


        #region == VideoEvent Section ==

        public static DataTable GetVideoEventDataTableServer(VideoEventResponseModel addedData, int projdetId)
        {
            var dtVideoEvent = GetRawVideoEventDataTable();
            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = projdetId;
            row["videoevent_start"] = addedData.videoevent.videoevent_start;
            row["videoevent_track"] = addedData.videoevent.videoevent_track;
            row["videoevent_duration"] = addedData.videoevent.videoevent_duration;
            row["fk_videoevent_media"] = addedData.videoevent.fk_videoevent_media;
            row["videoevent_createdate"] = addedData.videoevent.videoevent_createdate;
            row["videoevent_modifydate"] = addedData.videoevent.videoevent_modifydate;
            row["videoevent_isdeleted"] = addedData.videoevent.videoevent_isdeleted;
            row["videoevent_issynced"] = true;
            row["videoevent_serverid"] = addedData.videoevent.videoevent_id;
            row["videoevent_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        private static DataTable GetRawVideoEventDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("videoevent_id", typeof(int));
            dt.Columns.Add("fk_videoevent_projdet", typeof(int));
            dt.Columns.Add("videoevent_start", typeof(string));
            dt.Columns.Add("videoevent_duration", typeof(string));
            dt.Columns.Add("videoevent_track", typeof(int));
            dt.Columns.Add("fk_videoevent_media", typeof(int));
            dt.Columns.Add("videoevent_createdate", typeof(string));
            dt.Columns.Add("videoevent_modifydate", typeof(string));
            dt.Columns.Add("videoevent_isdeleted", typeof(bool));
            dt.Columns.Add("videoevent_issynced", typeof(bool));
            dt.Columns.Add("videoevent_serverid", typeof(Int64));
            dt.Columns.Add("videoevent_syncerror", typeof(string));

            return dt;
        }
        #endregion

        
    }
}
