using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using VideoCreatorXAMLLibrary.Auth;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class BackgroundProcessHelper
    {
        private static readonly DispatcherTimer dispatcherTimerToCheckIfUnSyncDataPresent = new DispatcherTimer();
        private static readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private static int RetryIntervalInSeconds = 300;
        private static bool isBackgroundProcessRunning = false;
        private static SelectedProjectEvent selectedProjectEvent;
        private static AuthAPIViewModel authApiViewModel;
        private static Button btnUploadNotSyncedData;

        public static void SetBackgroundProcess(SelectedProjectEvent _selectedProjectEvent, AuthAPIViewModel _authApiViewModel, Button btnUpload)
        {
            selectedProjectEvent = _selectedProjectEvent;
            authApiViewModel = _authApiViewModel;
            btnUploadNotSyncedData = btnUpload;

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
            var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectEvent.projdetId, true);
            if (notSyncedData?.Count > 0)
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data - {notSyncedData?.Count} events";
                btnUploadNotSyncedData.Width = 210;
                btnUploadNotSyncedData.IsEnabled = true;
            }
            else
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data";
                btnUploadNotSyncedData.Width = 160;
                btnUploadNotSyncedData.IsEnabled = false;
            }
        }


        private static async void RunBackgroundProcess(object sender, EventArgs e)
        {
            await PeriodicallyCheckOfflineDataAndSync();
            FrequentlyCheckOfflineData();
        }

        public static async Task<bool?> PeriodicallyCheckOfflineDataAndSync(bool isRunningInUI = false)
        {
            if (isBackgroundProcessRunning) return null;
            isBackgroundProcessRunning = true;
            try
            {
                var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectEvent.projdetId, true);
                foreach (var notSyncedRow in notSyncedData)
                {
                    if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.IMAGE || notSyncedRow.fk_videoevent_media == (int)EnumMedia.VIDEO) // for image or video
                    {
                        return await ProcessVideoSegmentDataRowByRowInBackground(notSyncedRow);

                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.AUDIO) // for Audio
                    {
                        // Coming Soon !!!
                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.FORM) // for DESIGN + IMAGE
                    {
                        return await ProcessVideoSegmentDataRowByRowInBackground(notSyncedRow);
                    }
                }
            }
            catch (Exception) { }
            finally { isBackgroundProcessRunning = false; }
            return null;
        }

        private static async Task<bool> ProcessVideoSegmentDataRowByRowInBackground(CBVVideoEvent videoEvent)
        {
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerBackground(videoEvent, selectedProjectEvent, authApiViewModel);
            if (addedData != null)
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, addedData.videoevent.videoevent_id, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, addedData.videosegment.videosegment_id, authApiViewModel.GetError());

                foreach (var design in videoEvent?.design_data)
                {
                    var serverDesign = addedData.design.Where(y => y.design_xml.Equals(design.design_xml)).FirstOrDefault();
                    if (serverDesign != null)
                        DataManagerSqlLite.UpdateDesignDataTableServerId(design.design_id, serverDesign.design_id, authApiViewModel.GetError());
                }
                return true;
            }
            else
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, videoEvent.videoevent_serverid, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, videoEvent.videosegment_data[0].videosegment_serverid, authApiViewModel.GetError());

                foreach (var design in videoEvent?.design_data)
                    DataManagerSqlLite.UpdateDesignDataTableServerId(design.design_id, design.design_serverid, authApiViewModel.GetError());
                return false;
            }

        }
    }
}
