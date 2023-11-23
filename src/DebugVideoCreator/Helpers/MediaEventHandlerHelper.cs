using dbTransferUser_UserControl.ResponseObjects.App;
using dbTransferUser_UserControl.ResponseObjects.Background;
using dbTransferUser_UserControl.ResponseObjects.Company;
using dbTransferUser_UserControl.ResponseObjects.Media;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using dbTransferUser_UserControl.ResponseObjects.Screen;
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
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
    public static class MediaEventHandlerHelper
    {
        #region === Image/Video Functions ==

        public static async Task<VideoEventResponseModel> PostVideoEventToServerForVideoOrImage(DataRow row, Int64 selectedServerProjectId, AuthAPIViewModel authApiViewModel)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)row["fk_videoevent_media"];
            objToSync.videoevent_track = 1; // TBD
            objToSync.videoevent_start = "00:00:00"; // TBD
            objToSync.videoevent_duration = (int)row["videoevent_duration"];
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.videosegment_media_bytes = (byte[])row["media"];
            var result = await authApiViewModel.POSTVideoEvent(selectedServerProjectId, objToSync);
            return result;
        }

        public static DataTable GetVideoEventDataTableForVideoOrImage(VideoEventResponseModel addedData, int selectedProjectId)
        {
            var dtVideoEvent = GetVideoEventDataTable();

            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_project"] = selectedProjectId;
            row["videoevent_start"] = addedData.videoevent.videoevent_start;
            row["videoevent_track"] = addedData.videoevent.videoevent_track;
            row["videoevent_duration"] = addedData.videoevent.videoevent_duration;
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

        

        public static DataTable GetVideoEventTableWithData(int selectedProjectId, AllVideoEventResponseModel videoevent)
        {
            var dt = GetVideoEventDataTable();
            var row = dt.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_project"] = selectedProjectId;
            row["videoevent_start"] = videoevent.videoevent_start;
            row["videoevent_track"] = videoevent.videoevent_track;
            row["videoevent_duration"] = videoevent.videoevent_duration;
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
            var dtVideoEvent = new DataTable();
            dtVideoEvent.Columns.Add("videosegment_id", typeof(int));
            dtVideoEvent.Columns.Add("videosegment_media", typeof(byte[]));
            dtVideoEvent.Columns.Add("videosegment_createdate", typeof(string));
            dtVideoEvent.Columns.Add("videosegment_modifydate", typeof(string));
            dtVideoEvent.Columns.Add("videosegment_isdeleted", typeof(bool));
            dtVideoEvent.Columns.Add("videosegment_issynced", typeof(bool));
            dtVideoEvent.Columns.Add("videosegment_serverid", typeof(Int64));
            dtVideoEvent.Columns.Add("videosegment_syncerror", typeof(string));

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

        private static DataTable GetVideoEventDataTable()
        {
            var dtVideoEvent = new DataTable();
            dtVideoEvent.Columns.Add("videoevent_id", typeof(int));
            dtVideoEvent.Columns.Add("fk_videoevent_project", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_start", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_duration", typeof(int));
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
