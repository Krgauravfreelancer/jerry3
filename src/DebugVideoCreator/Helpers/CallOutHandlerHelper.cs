using dbTransferUser_UserControl.ResponseObjects.App;
using dbTransferUser_UserControl.ResponseObjects.Background;
using dbTransferUser_UserControl.ResponseObjects.Company;
using dbTransferUser_UserControl.ResponseObjects.Media;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using dbTransferUser_UserControl.ResponseObjects.Screen;
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
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
using System.Windows.Documents;
using System.Windows.Markup;
using VideoCreator.Auth;
using VideoCreator.XAML;

namespace VideoCreator.Helpers
{
    public static class CallOutHandlerHelper
    {
        #region === Form/Design Functions ==
        public static async Task<bool> CallOut(string title, int selectedProjectId, Int64 selectedServerProjectId, AuthAPIViewModel authApiViewModel)
        {
            var data = DataManagerSqlLite.GetBackground();
            var designerUserControl = new Designer_UserControl(selectedProjectId, JsonConvert.SerializeObject(data));
            var window = new Window
            {
                Title = string.IsNullOrEmpty(title) ? "Designer" : title,
                Content = designerUserControl,
                ResizeMode = ResizeMode.NoResize,
                Height = 500,
                Width = 1000,
                RenderSize = designerUserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue && designerUserControl.dataTableAdd.Rows.Count > 0 || designerUserControl.dataTableUpdate.Rows.Count > 0)
            {
                if (designerUserControl.UserConsent || MessageBox.Show("Do you want save all designs??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (designerUserControl.dataTableAdd.Rows.Count > 0)
                    {
                        // We need to insert the Data to server here and once it is success, then to local DB
                        var addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableAdd, selectedServerProjectId, authApiViewModel);
                        // Now we have to save the data locally
                        if (addedData != null)
                        {
                            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForDesign(addedData, selectedProjectId);
                            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
                            if (insertedVideoEventIds?.Count > 0)
                            {
                                var videoEventId = insertedVideoEventIds[0];
                                var dtDesign = DesignEventHandlerHelper.GetDesignDataTable(addedData.design, videoEventId);
                                DataManagerSqlLite.InsertRowsToDesign(dtDesign);
                                var totalRows = dtDesign.Rows.Count;
                                var isSuccess = await CallOut_Step2(videoEventId, addedData.videoevent.videoevent_id, authApiViewModel);
                                return isSuccess;
                            }
                            else
                            {
                                MessageBox.Show($"No data added to database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            return false;

        }

        public static async Task<bool> CallOut_Step2(int videoeventId, int videoevent_serverid, AuthAPIViewModel authApiViewModel)
        {
            var designImagerUserControl = new DesignImager_UserControl(videoeventId);
            var window = new Window
            {
                Title = "Design Image",
                Content = designImagerUserControl,
                ResizeMode = ResizeMode.NoResize,
                Height = 500,
                Width = 850,
                RenderSize = designImagerUserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue && designImagerUserControl.dtVideoSegment != null && designImagerUserControl.dtVideoSegment.Rows.Count > 0)
            {
                if (designImagerUserControl.UserConsent || MessageBox.Show("Do you want save all Image??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // We need to insert the Data to server here and once it is success, then to local DB
                    var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
                    var postResponse = await authApiViewModel.PostVideoSegment(videoevent_serverid, EnumMedia.FORM, blob);
                    if (postResponse.Status == "error")
                        MessageBox.Show($"{postResponse.Status}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForDesign(blob, videoeventId, postResponse.Data);
                    var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, postResponse.Data.VideoSegment.videosegment_id);
                    if (insertedVideoSegmentId > 0)
                    {
                        return true;
                        
                    }
                }
                else
                {
                    
                }
            }
            return false;
        }
        #endregion
    }
}
