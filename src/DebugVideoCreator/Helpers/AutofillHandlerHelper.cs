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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Xml;

namespace VideoCreator.Helpers
{
    public static class AutofillHandlerHelper
    {
        #region === Form/Design Functions ==

        private static int RetryIntervalInSeconds = 300;

        private static string CreateOrReturnEmptyDirectory()
        {
            var directoryName = Directory.GetCurrentDirectory() + "\\BackgroundImageForAutofill";
            if(!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            else
            {
                DirectoryInfo di = new DirectoryInfo(directoryName);
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    foreach (FileInfo file in dir.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            return directoryName;
        }

        public static string CheckIfBackgroundPresent()
        {
            var directory = CreateOrReturnEmptyDirectory();
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

        public static async Task Process(AutofillEvent autofillEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader, string imagePath = null)
        {
            Designer_UserControl designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, true);
            var designElements = designerUserControl.AutofillSetup();

            if (autofillEvent.AutofillType == AutofillEnumType.Title)
            {
                await AddProjectNameAutoFill(autofillEvent, designElements, designerUserControl, selectedProjectEvent, authApiViewModel);
            }
            else if (autofillEvent.AutofillType == AutofillEnumType.Requirement)
            {
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Requirement", selectedProjectEvent, authApiViewModel, 0);
            }
            else if (autofillEvent.AutofillType == AutofillEnumType.Objective)
            {
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Objective", selectedProjectEvent, authApiViewModel, 0);
            }
            else if (autofillEvent.AutofillType == AutofillEnumType.Next)
            {
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Next", selectedProjectEvent, authApiViewModel, 0);
            }
            else if (autofillEvent.AutofillType == AutofillEnumType.All)
            {
                await AddProjectNameAutoFill(autofillEvent, designElements, designerUserControl, selectedProjectEvent, authApiViewModel);
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Requirement", selectedProjectEvent, authApiViewModel, 1);
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Objective", selectedProjectEvent, authApiViewModel, 2);
                await AddOthersAutofill(autofillEvent, designElements, designerUserControl, "Next", selectedProjectEvent, authApiViewModel, 3);
            }
            MessageBox.Show($"Autofill events added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private static async Task<bool?> AddProjectNameAutoFill(AutofillEvent autofillEvent, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            FillBackgroundForAutofill(autofillEvent, designElements, designerUserControl);
            var rowTitle = designerUserControl.GetNewRow();

            rowTitle["design_id"] = -1;
            rowTitle["fk_design_videoevent"] = -1;
            rowTitle["fk_design_screen"] = 1;
            rowTitle["fk_design_background"] = 1;
            rowTitle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_xml"] = GetTitleElement("Sample Video Title for the first Video element");
            designerUserControl.AddNewRowToDatatable(rowTitle);

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            var finalBlob = XMLToImageAutofillSetup(designerUserControl.dataTableAdd);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await AutofillSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, autofillEvent, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, 0);
            return result;
        }

        private static async Task<bool?> AddOthersAutofill(AutofillEvent autofillEvent, DataTable designElements, Designer_UserControl designerUserControl, string Typetext, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, int IncrementBy)
        {
            FillBackgroundForAutofill(autofillEvent, designElements, designerUserControl);
            var rowHeading = designerUserControl.GetNewRow();

            var designScreen = 1;
            if (Typetext == "Requirement")
                designScreen = 2;
            else if (Typetext == "Objective")
                designScreen = 3;
            else if (Typetext == "Next")
                designScreen = 4;

            rowHeading["design_id"] = -1;
            rowHeading["fk_design_videoevent"] = -1;
            rowHeading["fk_design_screen"] = designScreen;
            rowHeading["fk_design_background"] = 1;
            rowHeading["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_xml"] = GetHeading($"{Typetext} heading is here -");
            designerUserControl.AddNewRowToDatatable(rowHeading);

            for (var i = 0; i < 3; i++)
            {
                var rowCircle = designerUserControl.GetNewRow();

                rowCircle["design_id"] = -1;
                rowCircle["fk_design_videoevent"] = -1;
                rowCircle["fk_design_screen"] = 1;
                rowCircle["fk_design_background"] = 1;
                rowCircle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_xml"] = GetBulletCircle(i + 1);
                designerUserControl.AddNewRowToDatatable(rowCircle);


                var rowText = designerUserControl.GetNewRow();

                rowText["design_id"] = -1;
                rowText["fk_design_videoevent"] = -1;
                rowText["fk_design_screen"] = 1;
                rowText["fk_design_background"] = 1;
                rowText["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_xml"] = GetBulletPoints($"{Typetext} Bullet point - {i + 1}", i + 1);
                designerUserControl.AddNewRowToDatatable(rowText);

                if (Typetext == "Next")
                    break;
            }

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            var finalBlob = XMLToImageAutofillSetup(designerUserControl.dataTableAdd);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await AutofillSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, autofillEvent, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, IncrementBy);
            return result;
        }

        private static void FillBackgroundForAutofill(AutofillEvent autofillEvent, DataTable designElements, Designer_UserControl designerUserControl)
        {
            // background Image
            foreach (DataRow row in designElements.Rows)
            {
                var rowDesign = designerUserControl.GetNewRow();
                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = 1;
                rowDesign["fk_design_background"] = 1;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];
                designerUserControl.AddNewRowToDatatable(rowDesign);
            }
        }

        private static string GetTitleElement(string Text)
        {
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"300\" Canvas.Top=\"450\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{Text.ToUpper()}</TextBox>";
            return title;
        }

        private static string GetHeading(string HeadingText)
        {
            var top = 250;
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"350\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{HeadingText.ToUpper()}</TextBox>";
            return title;
        }

        private static string GetBulletPoints(string BulletText, int bulletNumber = 1)
        {
            var top = 250 + (bulletNumber * 150);
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"50\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{BulletText}</TextBox>";
            return title;
        }

        private static string GetBulletCircle(int bulletNumber = 1)
        {
            var top = 280 + (bulletNumber * 150);
            var circle = $"<Ellipse Fill=\"#00000000\" Stroke=\"#FFF0F8FF\" StrokeThickness=\"10\" StrokeDashArray=\"\" Width=\"20\" Height=\"20\" Opacity=\"1\" Canvas.Left=\"380\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Ellipse.RenderTransform><RotateTransform Angle=\"0\" /></Ellipse.RenderTransform></Ellipse>";
            return circle;
        }


        public static byte[] XMLToImageAutofillSetup(DataTable dataTable)
        {
            // Get the Image as byte stream 
            Canvas container = new Canvas();
            container.RenderSize = new Size(1920, 1080);

            //Canvas canvas = new Canvas();
            //canvas.RenderSize = new Size(1920, 1080);
            string text = $@"<Canvas
                                xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string text2 = "</Canvas>";
            foreach (DataRow row in dataTable.Rows)
            {
                var xaml = (string)row["design_xml"];
                string s = text + xaml + text2;
                StringReader input = new StringReader(s);
                XmlReader reader = XmlReader.Create(input);
                var canvas = (Canvas)XamlReader.Load(reader);
                canvas.RenderSize = new Size(1920, 1080);
                UIElement element = canvas.Children[0];
                canvas.Children.RemoveAt(0);
                container.Children.Add(element);
            }

            container.Measure(new Size(1920, 1080));
            Rect rect = new Rect(0, 0, 1920, 1080);
            container.Arrange(rect);

            //Rect rect = new Rect(canvas.RenderSize);
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96.0, 96.0, PixelFormats.Default);
            renderTargetBitmap.Render(container);

            BitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            MemoryStream memoryStream = new MemoryStream();
            bitmapEncoder.Save(memoryStream);
            // Process 
            byte[] blob = (byte[])memoryStream.ToArray();
            BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
            memoryStream.Close();
            return blob;
        }

        
        private static async Task<bool?> AutofillSaveToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, AutofillEvent autofillEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track, int IncrementBy)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            VideoEventResponseModel addedData;

            if (IncrementBy > 0)
            {
                var startTime = DataManagerSqlLite.CalcNextEnd(autofillEvent?.timeAtTheMoment, autofillEvent.Duration*IncrementBy);
                addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableAdd, blob, selectedProjectEvent, track, authApiViewModel, startTime, autofillEvent.Duration);
            }
            else
                addedData = await DesignEventHandlerHelper.PostVideoEventToServerForDesign(designerUserControl.dataTableAdd, blob, selectedProjectEvent, track, authApiViewModel, autofillEvent?.timeAtTheMoment, autofillEvent.Duration);
            
            
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await AutofillSaveToServerAndLocalDB(designImagerUserControl, designerUserControl, autofillEvent, selectedProjectEvent, authApiViewModel, track, IncrementBy);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForAutofill(designerUserControl.dataTableAdd, designImagerUserControl.dtVideoSegment, autofillEvent.timeAtTheMoment, autofillEvent.Duration, (int)track, selectedProjectEvent);
                else
                    return null;
            }
            else
            {
                SuccessFlowForAutofill(addedData, selectedProjectEvent.projdetId, blob);
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
