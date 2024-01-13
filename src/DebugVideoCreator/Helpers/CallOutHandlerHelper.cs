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

namespace VideoCreator.Helpers
{
    public static class CallOutHandlerHelper
    {
        #region === Form/Design Functions ==

        public static async Task<string> Preprocess(CalloutOrCloneEvent calloutEvent)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var videoEvents = DataManagerSqlLite.GetVideoEventbyId(calloutEvent.timelineVideoEvent.videoevent_id, true);

            var videoEvent = videoEvents.Where(x => x.fk_videoevent_media == 2).FirstOrDefault();
            if (videoEvent != null)
            {
                var VideoFileName = $"{currentDirectory}\\Media\\video_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.mp4";
                var outputFolder = $"C:\\commercialBase\\ExtractedImages";

                Stream t = new FileStream(VideoFileName, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(videoEvent.videosegment_data[0].videosegment_media);
                t.Close();

                var video2image = new VideoToImage_UserControl.VideoToImage_UserControl(VideoFileName, outputFolder);
                var convertedImage = await video2image.ConvertVideoToImage();
                return convertedImage;
            }

            var imageEvent = videoEvents.Where(x => x.fk_videoevent_media == 1 || x.fk_videoevent_media == 4).FirstOrDefault();
            if (imageEvent != null)
            {
                var imagePath = $"C:\\commercialBase\\ExtractedImages\\image_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.png";

                Stream t = new FileStream(imagePath, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(imageEvent.videosegment_data[0].videosegment_media);
                t.Close();
                return imagePath;
            }
            return null;
        }

        public static async Task<bool> CallOut(CalloutOrCloneEvent calloutEvent, string title, int selectedProjectId, Int64 selectedServerProjectId, AuthAPIViewModel authApiViewModel, EnumTrack track, UserControl uc, LoadingAnimation loader, string imagePath = null)
        {
            Designer_UserControl designerUserControl;
            if (string.IsNullOrEmpty(imagePath))
            {
                var data = DataManagerSqlLite.GetBackground();
                designerUserControl = new Designer_UserControl(selectedProjectId, JsonConvert.SerializeObject(data));
            }
            else
                designerUserControl = new Designer_UserControl(selectedProjectId, imagePath, true);

            var designUCWindow = new Window
            {
                Title = string.IsNullOrEmpty(title) ? "Designer" : title,
                Content = designerUserControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            LoaderHelper.ShowLoader(uc, loader, "Another window is opened ..");
            var result = designUCWindow.ShowDialog();
            if (result.HasValue && designerUserControl.dataTableAdd.Rows.Count > 0 || designerUserControl.dataTableUpdate.Rows.Count > 0)
            {
                if (designerUserControl.UserConsent || MessageBox.Show("Do you want save all designs??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (designerUserControl.dataTableAdd.Rows.Count > 0)
                    {
                        var designImagerUserControl = CallOut_XML2Image(designerUserControl.dataTableAdd, uc, loader);
                        if (designImagerUserControl != null)
                        {
                            var response = await CalloutSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, calloutEvent, selectedProjectId, selectedServerProjectId, authApiViewModel, track);
                            LoaderHelper.HideLoader(uc, loader);
                            return response;
                        }
                    }
                }
            }
            LoaderHelper.HideLoader(uc, loader);
            return false;

        }

        private static DesignImager_UserControl CallOut_XML2Image(DataTable designDataTable, UserControl uc, LoadingAnimation loader)
        {
            var designImagerUserControl = new DesignImager_UserControl(designDataTable);
            var window = new Window
            {
                Title = "Design Image",
                Content = designImagerUserControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue && designImagerUserControl.dtVideoSegment != null && designImagerUserControl.dtVideoSegment.Rows.Count > 0)
                return designImagerUserControl;
            return null;
        }

        private static async Task<bool> CalloutSaveToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, CalloutOrCloneEvent calloutEvent, int selectedProjectId, Int64 selectedServerProjectId, AuthAPIViewModel authApiViewModel, EnumTrack track)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            var addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableAdd, blob, selectedServerProjectId, track, authApiViewModel, calloutEvent?.timeAtTheMoment, 10);
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForDesign(addedData, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTable(addedData.design, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesign(dtDesign);

                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForDesign(blob, localVideoEventId, addedData.videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, addedData.videosegment.videosegment_id);
                if(insertedVideoSegmentId > 0)
                    return true;
            }
            return false;
        }
        #endregion
    }
}
