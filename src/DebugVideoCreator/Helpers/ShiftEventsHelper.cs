using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.VideoEvent;
using VideoCreator.Models;
using Newtonsoft.Json;
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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using VideoCreator.Auth;
using VideoCreator.Loader;
using VideoCreator.XAML;
using System.Linq;
using Timeline.UserControls.Models;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Xml;
using FullScreenPlayer_UserControl.Controls;

namespace VideoCreator.Helpers
{
    public static class ShiftEventsHelper
    {
        #region == API Calls ==
        private static async Task<VideoEventResponseModel> DeleteVideoEventToServer(SelectedProjectEvent selectedProjectEvent, Int64 videoevent_serverid, AuthAPIViewModel authApiViewModel)
        {
            var result = await authApiViewModel.DeleteVideoEvent(selectedProjectEvent, videoevent_serverid);
            return result;
        }

        private static async Task<object> UndeleteVideoEventToServer(SelectedProjectEvent selectedProjectEvent, Int64 videoevent_serverid, AuthAPIViewModel authApiViewModel)
        {
            var result = await authApiViewModel.UndeleteVideoEventToServer(selectedProjectEvent, videoevent_serverid);
            return result;
        }

        #endregion

        public static bool CheckIfUndeleteCanbeDone(int undoVideoEventId, Timeline_UserControl TimelineUserConrol)
        {
            if (undoVideoEventId == -1)
            {
                MessageBox.Show("No deleted item to undo, please try again !!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                TimelineUserConrol.DisableUndoDeleteAndReset();
                return false;
            }
            return true;
        }

        public static async Task DeleteAndShiftEvent(int videoeventLocalId, CBVVideoEvent videoevent, bool isShift, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            // Step-1 soft-delete from server the video-event and children
            var deletedData = await DeleteVideoEventToServer(selectedProjectEvent, videoevent.videoevent_serverid, authApiViewModel);

            if (isShift)
            {
                // Step-2 Fetch next events of deleted event
                var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyEndTime(selectedProjectEvent.projdetId, videoevent.videoevent_end);

                // Step-3 Call server API to shift video event and then save locally the shifted events
                if (tobeShiftedVideoEvents?.Count > 0)
                {
                    var tobeServerShiftedVideoEvents = new List<ShiftVideoEventModel>();
                    foreach (var item in tobeShiftedVideoEvents)
                    {
                        var model = new ShiftVideoEventModel
                        {
                            videoevent_id = (int)item.videoevent_serverid,
                            videoevent_duration = item.videoevent_duration,
                            videoevent_origduration = item.videoevent_origduration,
                            videoevent_end = DataManagerSqlLite.ShiftLeft(item.videoevent_end, videoevent.videoevent_duration),
                            videoevent_start = DataManagerSqlLite.ShiftLeft(item.videoevent_start, videoevent.videoevent_duration)
                        };
                        tobeServerShiftedVideoEvents.Add(model);
                    }
                    var serverShiftedVideoEvents = await MediaEventHandlerHelper.ShiftVideoEventsToServer(selectedProjectEvent, tobeServerShiftedVideoEvents, authApiViewModel);

                    var dtShiftedVideoEvents = new DataTable();
                    dtShiftedVideoEvents.Columns.Add("videoevent_serverid", typeof(Int64));
                    dtShiftedVideoEvents.Columns.Add("videoevent_start", typeof(string));
                    dtShiftedVideoEvents.Columns.Add("videoevent_end", typeof(string));
                    dtShiftedVideoEvents.Columns.Add("videoevent_duration", typeof(string));
                    dtShiftedVideoEvents.Columns.Add("videoevent_origduration", typeof(string));
                    foreach (var item in serverShiftedVideoEvents)
                    {
                        var row = dtShiftedVideoEvents.NewRow();
                        row["videoevent_serverid"] = item.videoevent_id;
                        row["videoevent_start"] = item.videoevent_start;
                        row["videoevent_end"] = item.videoevent_end;
                        row["videoevent_duration"] = item.videoevent_duration;
                        row["videoevent_origduration"] = item.videoevent_origduration;
                        dtShiftedVideoEvents.Rows.Add(row);
                    }
                    DataManagerSqlLite.ShiftVideoEvents(dtShiftedVideoEvents);
                }
            }

            // Step-4 finally Soft delete event and children from local DB
            DataManagerSqlLite.DeleteVideoEventsById(videoeventLocalId, true); // This will delete from design/notes/videosegment/videoevent
        }

        public static async Task UndeleteAndShiftEvent(int videoeventLocalId, CBVVideoEvent videoevent, bool isShift, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            // Step-1 soft-delete from server the video-event and children
            var undeletedData = await UndeleteVideoEventToServer(selectedProjectEvent, videoevent.videoevent_serverid, authApiViewModel);

            if (isShift)
            {
                // Step-2 Fetch next events of deleted event
                var tobeShiftedVideoEvents = DataManagerSqlLite.GetShiftVideoEventsbyStartTime(selectedProjectEvent.projdetId, videoevent.videoevent_start);

                // Step-3 Call server API to shift video event and then save locally the shifted events
                await ShiftRight(tobeShiftedVideoEvents, videoevent.videoevent_duration, selectedProjectEvent, authApiViewModel);
            }

            // Step-4 finally Soft delete event and children from local DB
            DataManagerSqlLite.UndeleteVideoEventsById(videoeventLocalId, true); // This will delete from design/notes/videosegment/videoevent
        }


        public async static Task ShiftRight(List<CBVShiftVideoEvent> tobeShiftedVideoEvents, string duration, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            // Step-3 Call server API to shift video event and then save locally the shifted events
            if (tobeShiftedVideoEvents?.Count > 0)
            {
                var tobeServerShiftedVideoEvents = new List<ShiftVideoEventModel>();
                foreach (var item in tobeShiftedVideoEvents)
                {
                    var model = new ShiftVideoEventModel
                    {
                        videoevent_id = (int)item.videoevent_serverid,
                        videoevent_duration = item.videoevent_duration,
                        videoevent_origduration = item.videoevent_origduration,
                        videoevent_start = DataManagerSqlLite.ShiftRight(item.videoevent_start, duration),
                        videoevent_end = DataManagerSqlLite.ShiftRight(item.videoevent_end, duration)
                    };
                    tobeServerShiftedVideoEvents.Add(model);
                }
                var serverShiftedVideoEvents = await MediaEventHandlerHelper.ShiftVideoEventsToServer(selectedProjectEvent, tobeServerShiftedVideoEvents, authApiViewModel);

                var dtShiftedVideoEvents = new DataTable();
                dtShiftedVideoEvents.Columns.Add("videoevent_serverid", typeof(Int64));
                dtShiftedVideoEvents.Columns.Add("videoevent_start", typeof(string));
                dtShiftedVideoEvents.Columns.Add("videoevent_end", typeof(string));
                dtShiftedVideoEvents.Columns.Add("videoevent_duration", typeof(string));
                dtShiftedVideoEvents.Columns.Add("videoevent_origduration", typeof(string));
                foreach (var item in serverShiftedVideoEvents)
                {
                    var row = dtShiftedVideoEvents.NewRow();
                    row["videoevent_serverid"] = item.videoevent_id;
                    row["videoevent_start"] = item.videoevent_start;
                    row["videoevent_end"] = item.videoevent_end;
                    row["videoevent_duration"] = item.videoevent_duration;
                    row["videoevent_origduration"] = item.videoevent_origduration;
                    dtShiftedVideoEvents.Rows.Add(row);
                }
                DataManagerSqlLite.ShiftVideoEvents(dtShiftedVideoEvents);
            }

        }

    }
}
