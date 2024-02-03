using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.VideoEvent;
using DebugVideoCreator.Models;
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

namespace VideoCreator.Helpers
{
    public static class AutofillHandlerHelper
    {
        #region === Form/Design Functions ==

        private static int RetryIntervalInSeconds = 300;

        private static string CreateOrReturnDirectory()
        {
            var directory = Directory.GetCurrentDirectory() + "\\BackgroundImageForAutofill";
            if(!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }

        public static string CheckIfBackgroundPresent()
        {
            var directory = CreateOrReturnDirectory();


            var backgrounds = DataManagerSqlLite.GetBackground();
            if(backgrounds != null && backgrounds.Count > 0)
            {
                var backgroundBytes = backgrounds.FirstOrDefault().background_media;

                var imagePath = $"{directory}\\backgroundimage_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.png";
                Stream t = new FileStream(imagePath, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(backgroundBytes);
                t.Close();
                return imagePath;
            }
            return null;
        }

        public static async Task<bool?> Process(AutofillEvent autofillEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader, string imagePath = null)
        {
            Designer_UserControl designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, true);
            designerUserControl.AutofillSetup(autofillEvent);


            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            designImagerUserControl.AutofillSetup(designerUserControl.dataTableAdd);

            return await AutofillSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, autofillEvent, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO);

        }

        private static async Task<bool?> AutofillSaveToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, AutofillEvent autofillEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            var addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableAdd, blob, selectedProjectEvent, track, authApiViewModel, autofillEvent?.timeAtTheMoment, autofillEvent.Duration);
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await AutofillSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, autofillEvent, selectedProjectEvent, authApiViewModel, track);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForAutofill(designerUserControl.dataTableAdd, designImagerUserControl.dtVideoSegment, autofillEvent.timeAtTheMoment, autofillEvent.Duration, (int)track, selectedProjectEvent);
                else
                    return null;
            }
            else
            {
                SuccessFlowForAutofill(addedData, selectedProjectEvent.projdetId, blob);
                MessageBox.Show($"videosegment record for image added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }


            
        }

        private static bool FailureFlowForAutofill(DataTable dtDesignMaster, DataTable dtVideoSegmentMaster, string timeAtTheMoment, int duration, int track, SelectedProjectEvent selectedProjectEvent)
        {
            // Save the record locally with server Id = temp and issynced = false
            var blob = dtVideoSegmentMaster.Rows[0]["videosegment_media"] as byte[];
            var localServerVideoEventId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForCalloutLocally(dtDesignMaster, dtVideoSegmentMaster, timeAtTheMoment, duration, track, selectedProjectEvent.projdetId, localServerVideoEventId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCalloutLocally(dtDesignMaster, dtVideoSegmentMaster, timeAtTheMoment, duration, track, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesign(dtDesign);

                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForCalloutLocally(blob, localVideoEventId);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, localVideoEventId);
                if (insertedVideoSegmentId > 0)
                    MessageBox.Show($"Record saved locally, background process will try to sync at an interval of {RetryIntervalInSeconds / 60} min.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return false;
        }


        private static void SuccessFlowForAutofill(VideoEventResponseModel addedData, int selectedProjectId, byte[] blob)
        {
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForDesign(addedData, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCallout(addedData.design, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesign(dtDesign);

                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForCallout(blob, localVideoEventId, addedData.videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, addedData.videosegment.videosegment_id);
            }
        }


        #endregion
    }
}
