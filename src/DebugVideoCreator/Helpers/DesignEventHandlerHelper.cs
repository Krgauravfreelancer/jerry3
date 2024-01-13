﻿using ServerApiCall_UserControl.DTO;
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

        public static async Task<VideoEventResponseModel> PostVideoEventToServerForDesign(DataTable dtDesign, byte[] blob,  Int64 selectedServerProjectId, EnumTrack track, AuthAPIViewModel authApiViewModel, string startTime = "00:00:00.000", int duration = 10)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)EnumMedia.FORM;
            objToSync.videoevent_track = (int)track;
            objToSync.videoevent_start = startTime;
            objToSync.videoevent_duration = duration;
            objToSync.videoevent_end = GetEndTime(startTime, duration); // TBD
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.design.AddRange(GetDesignModelList(dtDesign));
            objToSync.videosegment_media_bytes = blob;
            var result = await authApiViewModel.POSTVideoEvent(selectedServerProjectId, objToSync);
            return result;
        }

        private static string GetEndTime(string startTime, int duration = 10)
        {
            if (string.IsNullOrEmpty(startTime)) return "00:00:10.000";

            var timeonly = startTime.Split('.')[0];
            var timeArray = timeonly.Split(':');
            var time = (int.Parse(timeArray[0]) * 3600) + (int.Parse(timeArray[1]) * 60) + int.Parse(timeArray[2]);
            var endTime = time + duration;
            TimeSpan endTimeSpan = TimeSpan.FromSeconds(endTime);
            return endTimeSpan.ToString(@"hh\:mm\:ss") + "." + Convert.ToString(startTime.Split('.')[1]);
        }

        public static DataTable GetVideoEventDataTableForDesign(VideoEventResponseModel addedData, int selectedProjectId)
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

        public static DataTable GetDesignDataTable(List<DesignModel> alldesigns, int localVideoEventId)
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

        public static DataTable GetVideoSegmentDataTableForDesign(byte[] blob, int videoeventId, VideoSegmentModel videosegment)
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

        private static List<DesignModel> GetDesignModelListForPut(int videoeventId, CBVVideoEvent videoevent)
        {
            var data = new List<DesignModel>();
            foreach (var design in videoevent.design_data)
            {
                var designModel = new DesignModel();
                designModel.fk_design_screen = design.fk_design_screen;
                designModel.design_xml = design.design_xml;
                designModel.fk_design_videoevent = (int)videoevent.videoevent_serverid;
                designModel.design_id = (int)design.design_serverid;
                data.Add(designModel);
            }
            return data;
        }

        #endregion
    }
}
