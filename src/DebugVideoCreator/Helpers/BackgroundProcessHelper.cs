using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using VideoCreator.Auth;
using VideoCreator.Helpers;

namespace DebugVideoCreator.Helpers
{
    public static class BackgroundProcessHelper
    {
        private static readonly DispatcherTimer dispatcherTimerToCheckIfUnSyncDataPresent = new DispatcherTimer();
        private static readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private static int RetryIntervalInSeconds = 300;
        private static bool isBackgroundProcessRunning = false;

        private static int selectedProjectId;
        private static Int64 selectedServerProjectId;
        private static AuthAPIViewModel authApiViewModel;
        private static Button btnUploadNotSyncedData, btnDownloadServerData;

        public static void SetBackgroundProcess(int projectId, Int64 serverProjectId, AuthAPIViewModel _authApiViewModel, Button btnUpload, Button btnDownload)
        {
            selectedProjectId = projectId;
            selectedServerProjectId = serverProjectId;
            authApiViewModel = _authApiViewModel;
            btnUploadNotSyncedData = btnUpload;
            btnDownloadServerData = btnDownload;

            // To Check if unsync data present
            dispatcherTimerToCheckIfUnSyncDataPresent.Tick += new EventHandler(RunBackgroundProcessFrequently);
            dispatcherTimerToCheckIfUnSyncDataPresent.Interval = TimeSpan.FromSeconds(30);
            dispatcherTimerToCheckIfUnSyncDataPresent.Start();
            FrequentlyCheckOfflineData();

            // Logic to invoke the BG thread to send not saved data from local DATA to server and update the server Id.
            dispatcherTimer.Tick += new EventHandler(RunBackgroundProcess);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(RetryIntervalInSeconds);
            dispatcherTimer.Start();
            
        }

        private static void RunBackgroundProcessFrequently(object sender, EventArgs e)
        {
            FrequentlyCheckOfflineData();
        }

        private static void FrequentlyCheckOfflineData()
        {
            var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectId, true);
            if (notSyncedData?.Count > 0)
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data - {notSyncedData?.Count} events";
                btnUploadNotSyncedData.Width = 210;
                btnUploadNotSyncedData.IsEnabled = true;
                btnDownloadServerData.IsEnabled = false;
            }
            else
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data";
                btnUploadNotSyncedData.Width = 160;
                btnUploadNotSyncedData.IsEnabled = false;
                btnDownloadServerData.IsEnabled = true;
            }
        }


        private static async void RunBackgroundProcess(object sender, EventArgs e)
        {
            await PeriodicallyCheckOfflineDataAndSync();
            FrequentlyCheckOfflineData();
        }

        public static async Task PeriodicallyCheckOfflineDataAndSync()
        {
            if (isBackgroundProcessRunning) return;
            isBackgroundProcessRunning = true;
            try
            {
                var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectId, true);
                foreach (var notSyncedRow in notSyncedData)
                {
                    if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.IMAGE || notSyncedRow.fk_videoevent_media == (int)EnumMedia.VIDEO) // for image or video
                    {
                        await ProcessVideoSegmentDataRowByRowInBackground(notSyncedRow);

                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.AUDIO) // for Audio
                    {
                        // Coming Soon !!!
                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.FORM) // for DESIGN + IMAGE
                    {
                        // TBD
                    }
                }
            }
            catch (Exception) { }
            finally { isBackgroundProcessRunning = false; }

        }

        private static async Task ProcessVideoSegmentDataRowByRowInBackground(CBVVideoEvent videoEvent)
        {
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerForVideoOrImageBackground(videoEvent, selectedServerProjectId, authApiViewModel);
            if (addedData != null)
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, addedData.videoevent.videoevent_id, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, addedData.videosegment.videosegment_id, authApiViewModel.GetError());
            }
            else
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, videoEvent.videoevent_serverid, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, videoEvent.videosegment_data[0].videosegment_serverid, authApiViewModel.GetError());

            }

        }
    }
}
