using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using VideoCreator.Auth;

namespace VideoCreator.Helpers
{
    public static class DesignEventHandlerHelper
    {
        #region === Form/Design Functions ==
        public static List<DesignModelPost> GetDesignModelList(DataTable dtDesign)
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

        public static List<DesignModelPost> GetDesignModelList(List<CBVDesign> designs)
        {
            var data = new List<DesignModelPost>();
            foreach (var design in designs)
            {
                var designModel = new DesignModelPost();
                designModel.fk_design_screen = design.fk_design_screen;
                designModel.design_xml = design.design_xml;
                data.Add(designModel);
            }
            return data;
        }

        public static async Task<VideoEventResponseModel> PostVideoEventToServerForDesign(DataTable dtDesign, byte[] blob, SelectedProjectEvent selectedProjectEvent, EnumTrack track, AuthAPIViewModel authApiViewModel, string startTime = "00:00:00.000", string duration = "00:00:10.000")
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)EnumMedia.FORM;
            objToSync.videoevent_track = (int)track;
            objToSync.videoevent_start = startTime;
            objToSync.videoevent_duration = duration;
            objToSync.videoevent_origduration = duration;
            objToSync.videoevent_end = DataManagerSqlLite.CalcNextEnd(startTime, duration); // TBD
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.design.AddRange(GetDesignModelList(dtDesign));
            objToSync.videosegment_media_bytes = blob;
            var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
            return result;
        }

        #endregion

        #region == Video Event ==

        public static DataTable GetVideoEventDataTableForDesign(VideoEventResponseModel addedData, int selectedProjdetId)
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

        public static DataTable GetVideoEventDataTableForCalloutLocally(DataTable design, DataTable videosegment, string timeAtTheMoment, string duration, int track, int projdetId, Int64 serverVideoEventId)
        {
            var dtVideoEvent = GetVideoEventDataTable();
            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = projdetId;
            row["videoevent_start"] = timeAtTheMoment;
            row["videoevent_track"] = track;
            row["videoevent_duration"] = duration;
            row["videoevent_origduration"] = duration;
            row["videoevent_planning"] = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            row["fk_videoevent_media"] = (int)EnumMedia.FORM;
            row["videoevent_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = false;
            row["videoevent_serverid"] = serverVideoEventId;
            row["videoevent_syncerror"] = "API Call Failed !!!";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
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

        #region == Design == 
        public static DataTable GetDesignDataTableForCallout(List<DesignModel> alldesigns, int localVideoEventId)
        {
            var dt = GetDesignDataTable();

            foreach (var post in alldesigns)
            {
                var row = dt.NewRow();
                row["design_id"] = -1;
                row["fk_design_screen"] = post.fk_design_screen;
                row["fk_design_background"] = 1; // TBD
                row["fk_design_videoevent"] = localVideoEventId;
                row["design_xml"] = post.design_xml;

                row["design_createdate"] = post.design_createdate;
                row["design_modifydate"] = post.design_modifydate;
                row["design_isdeleted"] = false;
                row["design_issynced"] = true;
                row["design_serverid"] = post.design_id;
                row["design_syncerror"] = "";
                dt.Rows.Add(row);
            }

            return dt;
        }

        public static DataTable GetDesignDataTableForCalloutLocally(DataTable design, DataTable videosegment, string timeAtTheMoment, string duration, int track, int localVideoEventId)
        {
            var dtVideoEvent = GetDesignDataTable();
            foreach (DataRow rowDesign in design.Rows)
            {
                var localServerDesignId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
                var row = dtVideoEvent.NewRow();
                row["design_id"] = -1;
                row["fk_design_screen"] = Convert.ToInt32(rowDesign["fk_design_screen"]);
                row["fk_design_background"] = 1; // TBD
                row["fk_design_videoevent"] = localVideoEventId;
                row["design_xml"] = Convert.ToString(rowDesign["design_xml"]);

                row["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                row["design_isdeleted"] = false;
                row["design_issynced"] = false;
                row["design_serverid"] = localServerDesignId;
                row["design_syncerror"] = "API Call Failed !!!";
                dtVideoEvent.Rows.Add(row);
            }
            return dtVideoEvent;
        }

        private static DataTable GetDesignDataTable()
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
        #endregion

        #region == VideoSegment == 
        public static DataTable GetVideoSegmentDataTableForCallout(byte[] blob, int videoeventId, VideoSegmentModel videosegment)
        {
            var dtVideoEvent = GetVideoSegmentDataTable();

            var row = dtVideoEvent.NewRow();
            row["videosegment_id"] = videoeventId;
            row["videosegment_media"] = blob;

            row["videosegment_createdate"] = videosegment.videosegment_createdate;
            row["videosegment_modifydate"] = videosegment.videosegment_modifydate;
            row["videosegment_isdeleted"] = false;
            row["videosegment_issynced"] = true;
            row["videosegment_serverid"] = videosegment.videosegment_id;
            row["videosegment_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        public static DataTable GetVideoSegmentDataTableForCalloutLocally(byte[] blob, int videoeventId)
        {
            var dtVideoEvent = GetVideoSegmentDataTable();
            var localServerDesignId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));

            var row = dtVideoEvent.NewRow();
            row["videosegment_id"] = videoeventId;
            row["videosegment_media"] = blob;

            row["videosegment_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videosegment_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videosegment_isdeleted"] = false;
            row["videosegment_issynced"] = false;
            row["videosegment_serverid"] = localServerDesignId;
            row["videosegment_syncerror"] = "API Call Failed !!!";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        private static DataTable GetVideoSegmentDataTable()
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


    }
}
