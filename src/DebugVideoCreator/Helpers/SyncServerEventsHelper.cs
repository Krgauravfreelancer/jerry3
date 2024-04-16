using ServerApiCall_UserControl.DTO;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VideoCreator.Auth;
using VideoCreator.Loader;
using VideoCreator.XAML;

namespace VideoCreator.Helpers
{
    public static class SyncServerEventsHelper
    {
        public async static Task ConfirmAndSyncServerDataToLocalDB(ManageTimeline_UserControl uc, Button btnDownloadServerData, SelectedProjectEvent selectedProjectEvent, LoadingAnimation loader, AuthAPIViewModel authApiViewModel)
        {
            // Get Local Video Event
            var localVideoEventCount = DataManagerSqlLite.GetVideoEventCountProjectAndDetailId(selectedProjectEvent.projectId, selectedProjectEvent.projdetId);
            // Get Server Video Event
            var serverVideoEventData = await authApiViewModel.GetAllVideoEventsbyProjdetId(selectedProjectEvent);

            if (serverVideoEventData != null && serverVideoEventData.Data?.Count > localVideoEventCount)
            {
                btnDownloadServerData.IsEnabled = true;
                var message = $@"Server has {serverVideoEventData.Data.Count} events while local DB has {localVideoEventCount} events.{Environment.NewLine}Do you want to download server data to local?";
                var result = MessageBox.Show(message, "Sync Data", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    await SyncServerDataToLocalDB(serverVideoEventData, uc, btnDownloadServerData, selectedProjectEvent, loader, authApiViewModel);
                else
                {
                    btnDownloadServerData.Content = $@"Download Server Data{Environment.NewLine}Server {serverVideoEventData.Data.Count} | Local {localVideoEventCount} events";
                    btnDownloadServerData.IsEnabled = true;
                }
            }
            else
            {
                btnDownloadServerData.Content = $@"Download Server Data{Environment.NewLine}Server {serverVideoEventData.Data.Count} | Local {localVideoEventCount} events";
                btnDownloadServerData.IsEnabled = false;
            }
        }

        public static async Task SyncServerDataToLocalDB(ParentDataList<AllVideoEventResponseModel> serverVideoEventData, ManageTimeline_UserControl uc, Button btnDownloadServerData, SelectedProjectEvent selectedProjectEvent, LoadingAnimation loader, AuthAPIViewModel authApiViewModel)
        {
            if (serverVideoEventData == null) serverVideoEventData = await authApiViewModel.GetAllVideoEventsbyProjdetId(selectedProjectEvent);
            LoaderHelper.ShowLoader(uc, loader);

            // Step1: Lets clear the local DB
            DataManagerSqlLite.DeleteAllVideoEventsByProjdetId(selectedProjectEvent.projdetId, true);
            int i = 1;
            foreach (var videoEvent in serverVideoEventData?.Data)
            {
                var localVideoEventId = SaveVideoEvent(videoEvent, selectedProjectEvent);
                if (videoEvent?.design?.Count > 0)
                    SaveDesign(localVideoEventId, videoEvent?.design);
                if (videoEvent?.notes?.Count > 0)
                    SaveNotes(localVideoEventId, videoEvent?.notes);
                if (videoEvent?.videosegment != null)
                    await SaveVideoSegment(localVideoEventId, videoEvent?.videosegment, authApiViewModel);

                LoaderHelper.ShowLoader(uc, loader, $"Processed {i++}/{serverVideoEventData?.Data?.Count} events..");
            }
            //InitializeChildren();
            //Refresh();
            btnDownloadServerData.Content = $@"Download Server Data";
            btnDownloadServerData.IsEnabled = false;
            LoaderHelper.HideLoader(uc, loader);
            MessageBox.Show($"Sync successfull !!!", "Success", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }


        private static int SaveVideoEvent(AllVideoEventResponseModel videoevent, SelectedProjectEvent selectedProjectEvent)
        {
            var dt = MediaEventHandlerHelper.GetVideoEventTableWithData(selectedProjectEvent.projdetId, videoevent);
            var result = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            dt = null;
            return result?.Count > 0 ? result[0] : -1;
        }

        private static void SaveDesign(int localVideoEventId, List<DesignModel> allDesigns)
        {
            var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCallout(allDesigns, localVideoEventId);
            DataManagerSqlLite.InsertRowsToDesign(dtDesign);
            dtDesign = null;
        }

        private static async Task SaveVideoSegment(int localVideoEventId, VideoSegmentModel videosegment, AuthAPIViewModel authApiViewModel)
        {
            var downloadUrl = videosegment.videosegment_download_url;

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                byte[] bytes = await authApiViewModel.GetSecuredFileByteArray(downloadUrl);
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImage(bytes, localVideoEventId, videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, localVideoEventId);
                bytes = null;
                dtVideoSegment = null;
            }
        }

        private static void SaveNotes(int localVideoEventId, List<NotesModel> notes)
        {
            var dtNotes = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(notes, localVideoEventId);
            DataManagerSqlLite.InsertRowsToNotes(dtNotes);
            dtNotes = null;
        }

    }
}
