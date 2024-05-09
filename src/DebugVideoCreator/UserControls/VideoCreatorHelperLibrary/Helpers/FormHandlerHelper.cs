using Newtonsoft.Json;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VideoCreatorHelperLibrary.Auth;
using VideoCreatorHelperLibrary.Loader;
using VideoCreatorHelperLibrary.Models;
using VideoCreatorHelperLibrary.XAML;

namespace VideoCreatorHelperLibrary.Helpers
{
    public static class FormHandlerHelper
    {
        #region === Form/Design Functions ==

        private static int RetryIntervalInSeconds = 300;

        public static async Task<string> PreprocessAndGetBackgroundImage(FormOrCloneEvent calloutEvent)
        {
            var outputFolder = PathHelper.GetTempPath("form");
            var videoEvents = DataManagerSqlLite.GetVideoEventbyId(calloutEvent.timelineVideoEvent.VideoEventID, true, false);
            var videoEvent = videoEvents.Where(x => x.fk_videoevent_media == 2).FirstOrDefault();
            if (videoEvent != null)
            {
                var VideoFileName = $"{outputFolder}\\video_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.mp4";

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
                var imagePath = $"{outputFolder}\\image_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.png";

                Stream t = new FileStream(imagePath, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(imageEvent.videosegment_data[0].videosegment_media);
                t.Close();
                return imagePath;
            }
            return null;
        }

        public static async Task<string> PreprocessAndGetBackgroundImageForEdit(CBVVideoEvent editVideoEvent, SelectedProjectEvent selectedProjectEvent)
        {
            var outputFolder = PathHelper.GetTempPath("editform");
            var events = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true, false);
            var timeAtTheMoment = TimeSpan.Parse(editVideoEvent.videoevent_start);
            var evnt = events.Where(x => TimeSpan.Parse(x.videoevent_start) <= timeAtTheMoment && TimeSpan.Parse(x.videoevent_end) > timeAtTheMoment && x.videoevent_track == (int)EnumTrack.IMAGEORVIDEO);
            var videoItem = evnt.Where(x => x.fk_videoevent_media == (int)EnumMedia.VIDEO).FirstOrDefault();
            if (videoItem != null)
            {
                var VideoFileName = $"{outputFolder}\\video_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.mp4";

                Stream t = new FileStream(VideoFileName, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(videoItem.videosegment_data[0].videosegment_media);
                t.Close();

                var video2image = new VideoToImage_UserControl.VideoToImage_UserControl(VideoFileName, outputFolder);
                var convertedImageFile = await video2image.ConvertVideoToImage();
                return convertedImageFile;
            }
            var imageEvent = evnt.Where(x => x.fk_videoevent_media == (int)EnumMedia.IMAGE || x.fk_videoevent_media == (int)EnumMedia.FORM).FirstOrDefault();
            if (imageEvent != null)
            {
                var imagePath = $"{outputFolder}\\image_{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")}.png";
                Stream t = new FileStream(imagePath, FileMode.Create);
                BinaryWriter b = new BinaryWriter(t);
                b.Write(imageEvent.videosegment_data[0].videosegment_media);
                t.Close();
                return imagePath;
            }
            return null;
        }

        public static async Task<bool?> CallOut(FormOrCloneEvent calloutEvent, string title, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track, UserControl uc, LoadingAnimation loader, string imagePath = null, bool isFormEvent = false)
        {
            Designer_UserControl designerUserControl;
            if (string.IsNullOrEmpty(imagePath))
            {
                var data = DataManagerSqlLite.GetBackground();
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, JsonConvert.SerializeObject(data), -1, false, isFormEvent);
            }
            else
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, -1, true, isFormEvent);

            var designUCWindow = new Window
            {
                Title = string.IsNullOrEmpty(title) ? "Designer" : title,
                Content = designerUserControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            LoaderHelper.ShowLoader(uc, loader, "Another window is opened ..");
            var result = designUCWindow.ShowDialog();
            if (result.HasValue && designerUserControl.dataTableObject.Rows.Count > 0)
            {
                if (designerUserControl.UserConsent || MessageBox.Show("Do you want save all designs??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (designerUserControl.dataTableObject.Rows.Count > 0)
                    {
                        var designImagerUserControl = CallOut_XML2Image(designerUserControl.dataTableObject, uc, loader);
                        if (designImagerUserControl != null)
                        {
                            return await CalloutSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, calloutEvent, selectedProjectEvent, authApiViewModel, track);
                        }
                    }
                }
            }
            return null;
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


        #region == Success And Failure API Calls Logic ==


        private static async Task<bool?> CalloutSaveToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, FormOrCloneEvent calloutEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            string duration = "00:00:10.000";
            var addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableObject, blob, selectedProjectEvent, track, authApiViewModel, calloutEvent?.timeAtTheMoment, duration);
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await CalloutSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, calloutEvent, selectedProjectEvent, authApiViewModel, track);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForCallout(designerUserControl.dataTableObject, designImagerUserControl.dtVideoSegment, calloutEvent?.timeAtTheMoment, duration, (int)track, selectedProjectEvent);
                else
                    return null;
            }
            else
            {
                SuccessFlowForCallout(addedData, selectedProjectEvent.projdetId, blob);
                return true;
            }
        }

        private static bool FailureFlowForCallout(DataTable dtDesignMaster, DataTable dtVideoSegmentMaster, string timeAtTheMoment, string duration, int track, SelectedProjectEvent selectedProjectEvent)
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

        private static void SuccessFlowForCallout(VideoEventResponseModel addedData, int selectedProjectId, byte[] blob)
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


        #endregion == Success And Failure API Calls Logic ==


        #endregion


        #region == Edit Callouts ==
        
        public static async Task<bool?> EditCallOut(int editVideoEventId, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader)
        {
            Designer_UserControl designerUserControl;
            var editVideoEvent = DataManagerSqlLite.GetVideoEventbyId(editVideoEventId).FirstOrDefault();
            var imagePath = await PreprocessAndGetBackgroundImageForEdit(editVideoEvent, selectedProjectEvent);

            if (string.IsNullOrEmpty(imagePath))
            {
                var data = DataManagerSqlLite.GetBackground();
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, JsonConvert.SerializeObject(data), editVideoEventId, false, false);
            }
            else
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, editVideoEventId, true, false);

            var designUCWindow = new Window
            {
                Title = $"Edit Event eith id - {editVideoEvent.videoevent_id}",
                Content = designerUserControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            LoaderHelper.ShowLoader(uc, loader, "Another window is opened ..");
            var result = designUCWindow.ShowDialog();
            if (result.HasValue && designerUserControl.dataTableObject.Rows.Count > 0)
            {
                if (designerUserControl.UserConsent || MessageBox.Show("Do you want save all designs??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (designerUserControl.dataTableObject.Rows.Count > 0)
                    {
                        var designImagerUserControl = CallOut_XML2Image(designerUserControl.dataTableObject, uc, loader);
                        if (designImagerUserControl != null)
                        {
                            var designData = DataManagerSqlLite.GetDesign(editVideoEventId).FirstOrDefault();
                            await EditToServerAndLocalDB(designImagerUserControl, designerUserControl, selectedProjectEvent, designData, authApiViewModel, editVideoEvent);
                            return true;
                        }
                    }
                }
            }
            return null;
        }

        public static async Task<bool?> EditFormEvent(int editVideoEventId, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader)
        {
            Designer_UserControl designerUserControl;
            var editVideoEvent = DataManagerSqlLite.GetVideoEventbyId(editVideoEventId).FirstOrDefault();
            string imagePath = "";

            if (string.IsNullOrEmpty(imagePath))
            {
                var data = DataManagerSqlLite.GetBackground();
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, JsonConvert.SerializeObject(data), editVideoEventId, false, true);
            }
            else
                designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, editVideoEventId, true, true);

            var designUCWindow = new Window
            {
                Title = $"Edit Event eith id - {editVideoEvent.videoevent_id}",
                Content = designerUserControl,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            LoaderHelper.ShowLoader(uc, loader, "Another window is opened ..");
            var result = designUCWindow.ShowDialog();
            if (result.HasValue && designerUserControl.dataTableObject.Rows.Count > 0)
            {
                if (designerUserControl.UserConsent || MessageBox.Show("Do you want save all designs??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (designerUserControl.dataTableObject.Rows.Count > 0)
                    {
                        var designImagerUserControl = CallOut_XML2Image(designerUserControl.dataTableObject, uc, loader);
                        if (designImagerUserControl != null)
                        {
                            var designData = DataManagerSqlLite.GetDesign(editVideoEventId).FirstOrDefault();
                            await EditToServerAndLocalDB(designImagerUserControl, designerUserControl, selectedProjectEvent, designData, authApiViewModel, editVideoEvent);
                            return true;
                        }
                    }
                }
            }
            return null;
        }
        #endregion


        private static async Task EditToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, CBVDesign design, AuthAPIViewModel authApiViewModel, CBVVideoEvent videoEvent)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];

            var isUpdated = await DesignEventHandlerHelper.UpdateDesign(designerUserControl.dataTableObject, blob, selectedProjectEvent, design.design_serverid, authApiViewModel, videoEvent);
            if (isUpdated == true)
            {
                // Update the design and blob table
                UpdateDesignToDB(designerUserControl.dataTableObject, design);
                UpdateVideoSegmentnToDB(blob, videoEvent);
            }
            else
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! ", "Failure", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    await EditToServerAndLocalDB(designImagerUserControl, designerUserControl, selectedProjectEvent, design, authApiViewModel, videoEvent);
            }
        }

        private static void UpdateDesignToDB(DataTable designDataTable, CBVDesign design)
        {
            var dt = new DataTable();
            dt.Columns.Add("design_id", typeof(int));
            dt.Columns.Add("fk_design_screen", typeof(int));
            dt.Columns.Add("fk_design_background", typeof(int));
            dt.Columns.Add("fk_design_videoevent", typeof(int));
            dt.Columns.Add("design_xml", typeof(string));
            
            dt.Columns.Add("design_modifydate", typeof(string));
            dt.Columns.Add("design_isdeleted", typeof(bool));
            dt.Columns.Add("design_issynced", typeof(bool));
            dt.Columns.Add("design_serverid", typeof(Int64));
            dt.Columns.Add("design_syncerror", typeof(string));

            var row = dt.NewRow();
            row["design_id"] = design.design_id;
            row["fk_design_screen"] = design.fk_design_screen;
            row["fk_design_background"] = design.fk_design_background;
            row["fk_design_videoevent"] = design.fk_design_videoevent;
            row["design_xml"] = Convert.ToString(designDataTable.Rows[0]["design_xml"]);
            row["design_isdeleted"] = false;
            row["design_issynced"] = true;
            row["design_serverid"] = design.design_serverid;
            row["design_syncerror"] = "";
            dt.Rows.Add(row);
            DataManagerSqlLite.UpdateRowsToDesign(dt);
        }

        private static void UpdateVideoSegmentnToDB(byte[] blob, CBVVideoEvent videoevent)
        {
            var dt = new DataTable();
            dt.Columns.Add("videosegment_id", typeof(int));
            dt.Columns.Add("videosegment_media", typeof(byte[]));
            dt.Columns.Add("videosegment_modifydate", typeof(string));
            
            var row = dt.NewRow();
            row["videosegment_id"] = videoevent.videoevent_id;
            row["videosegment_media"] = blob;
            dt.Rows.Add(row);

            DataManagerSqlLite.UpdateRowsToVideoSegment(dt);
        }



    }
}
