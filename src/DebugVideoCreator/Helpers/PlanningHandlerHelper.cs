using LocalVoiceGen_UserControl.Helpers;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using VideoCreator.Auth;
using VideoCreator.Loader;
using VideoCreator.Models;
using VideoCreator.XAML;

namespace VideoCreator.Helpers
{
    public static class PlanningHandlerHelper
    {
        #region === Planning Functions ==

        private static int RetryIntervalInSeconds = 300;
        private static string EventStartTime = "00:00:00.000";
        private static string EventEndTime = "00:00:00.000";
        private static string NotesStartTime = "00:00:00.000";
        //private static string NoteEndTime = "00:00:00.000";
        private static int notesIndex = 1;
        private static string CreateOrReturnEmptyDirectory()
        {
            var directoryName = PathHelper.GetTempPath("Planning");
            DirectoryInfo di = new DirectoryInfo(directoryName);
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                foreach (FileInfo file in dir.GetFiles())
                    file.Delete();
            }
            return directoryName;
        }

        public static string CheckIfBackgroundPresent()
        {
            var directory = CreateOrReturnEmptyDirectory();
            var backgrounds = DataManagerSqlLite.GetBackground();
            if (backgrounds != null && backgrounds.Count > 0)
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

        public static async Task Process(PlanningEvent planningEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader, string imagePath = null)
        {
            LoaderHelper.ShowLoader(uc, loader, "Starting ...");
            EventStartTime = DataManagerSqlLite.GetNextStart((int)EnumMedia.IMAGE, selectedProjectEvent.projdetId);

            Designer_UserControl designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, true);
            var designElements = designerUserControl.PlanningSetup();

            if (planningEvent.Type == EnumPlanningHead.All)
            {
                var allPlanningData = DataManagerSqlLite.GetPlanning(selectedProjectEvent.projectId)?.OrderBy(x => x.planning_sort).ToList();
                var data = allPlanningData?.Where(x => x.fk_planning_head != (int)EnumPlanningHead.Video && x.fk_planning_head != (int)EnumPlanningHead.Custom).ToList();
                LoaderHelper.ShowLoader(uc, loader, $"Processing 0/{data.Count} ...");
                //Add Title
                await AddProjectNameSlide(designElements, designerUserControl, selectedProjectEvent, authApiViewModel);
                int cntr = 1;
                foreach (var item in data)
                {
                    LoaderHelper.ShowLoader(uc, loader, $"Processing {(EnumPlanningHead)Enum.ToObject(typeof(EnumPlanningHead), item.fk_planning_head)} {cntr++}/{data.Count} ...");
                    switch (item.fk_planning_head)
                    {
                        case (int)EnumPlanningHead.Text:
                        case (int)EnumPlanningHead.Introduction:
                            // code block
                            await AddIntroductionOrTextSlide((EnumPlanningHead)item.fk_planning_head, item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel);
                            break;
                        case (int)EnumPlanningHead.Requirements:
                        case (int)EnumPlanningHead.Objectives:
                        case (int)EnumPlanningHead.Conclusion:
                        case (int)EnumPlanningHead.NextUp:
                        case (int)EnumPlanningHead.Bullet:
                            // code block - requirements
                            await AddPlanningElementWithDesign(item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, (EnumPlanningHead)item.fk_planning_head);
                            break;
                        case (int)EnumPlanningHead.Video:
                            // code block
                            break;
                        case (int)EnumPlanningHead.Custom:
                            // code block
                            break;
                        case (int)EnumPlanningHead.Image:
                            // code block
                            break;
                        default:
                            // code block
                            break;
                    }
                }
            }
        }


        private static async Task<bool?> AddProjectNameSlide(DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            var title = DataManagerSqlLite.GetProjectById(selectedProjectEvent.projectId, false)?.project_videotitle;

            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, EnumPlanningHead.Title);
            var rowTitle = designerUserControl.GetNewRow();

            rowTitle["design_id"] = -1;
            rowTitle["fk_design_videoevent"] = -1;
            rowTitle["fk_design_screen"] = (int)EnumScreen.Title;
            rowTitle["fk_design_background"] = 1;
            rowTitle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_xml"] = GetTitleElement(title);
            designerUserControl.AddNewRowToDatatable(rowTitle);

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            var finalBlob = XMLToImagePlanningSetup(designerUserControl.dataTableAdd);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: null, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, "00:00:10.000");
            return result;
        }

        private static async Task<bool?> AddIntroductionOrTextSlide(EnumPlanningHead planningHead, CBVPlanning item, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel)
        {
            var text = item?.planning_desc[0]?.planningdesc_line;
            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, planningHead);
            var rowTitle = designerUserControl.GetNewRow();

            rowTitle["design_id"] = -1;
            rowTitle["fk_design_videoevent"] = -1;
            rowTitle["fk_design_screen"] = (int)EnumScreen.Intro;
            rowTitle["fk_design_background"] = 1;
            rowTitle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowTitle["design_xml"] = GetTitleElement(text);
            designerUserControl.AddNewRowToDatatable(rowTitle);

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            var finalBlob = XMLToImagePlanningSetup(designerUserControl.dataTableAdd);
            designImagerUserControl.SaveToDataTable(finalBlob);
            
            var dtNotes = GetNotesDatatable(item.planning_notesline.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList(), out string notes_duration);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: dtNotes, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, notes_duration);
            return result;
        }


        private static async Task<bool?> AddPlanningElementWithDesign(CBVPlanning item, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumPlanningHead planningType)
        {
            if (item == null || item.planning_desc?.Count == 0 || item.planning_desc[0].planningdesc_bullets?.Count == 0) { return false; }

            var heading = item?.planning_desc[0]?.planningdesc_line ?? "No heading Found" ;

            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, planningType);
            var screen = GetScreenTypeId(planningType);

            // Design Elements - Heading
            var rowHeading = designerUserControl.GetNewRow();
            rowHeading["design_id"] = -1;
            rowHeading["fk_design_videoevent"] = -1;
            rowHeading["fk_design_screen"] = screen;
            rowHeading["fk_design_background"] = 1;
            rowHeading["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            rowHeading["design_xml"] = GetHeading($"{heading}");
            designerUserControl.AddNewRowToDatatable(rowHeading);
            
            int j = 0;
            for (var i = 0; i < item.planning_desc[0].planningdesc_bullets.Count; i++) // Bullet Circle + text
            {
                var bulletText = item.planning_desc[0].planningdesc_bullets[i].planningbullet_line;
                if (string.IsNullOrEmpty(bulletText)) continue;

                var rowCircle = designerUserControl.GetNewRow();

                rowCircle["design_id"] = -1;
                rowCircle["fk_design_videoevent"] = -1;
                rowCircle["fk_design_screen"] = screen;
                rowCircle["fk_design_background"] = 1;
                rowCircle["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowCircle["design_xml"] = GetBulletCircle(j + 1);
                designerUserControl.AddNewRowToDatatable(rowCircle);


                var rowText = designerUserControl.GetNewRow();

                rowText["design_id"] = -1;
                rowText["fk_design_videoevent"] = -1;
                rowText["fk_design_screen"] = screen;
                rowText["fk_design_background"] = 1;
                rowText["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowText["design_xml"] = GetBulletPoints($"{bulletText}", j + 1);
                designerUserControl.AddNewRowToDatatable(rowText);
                j++;
            }

            var notesText = item.planning_notesline.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            var notedataTable = GetNotesDatatable(notesText, out string notes_duration);

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableAdd);
            var finalBlob = XMLToImagePlanningSetup(designerUserControl.dataTableAdd);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: notedataTable, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, notes_duration);
            return result;
        }

        private static void FillBackgroundRowForPlanning(DataTable designElements, Designer_UserControl designerUserControl, EnumPlanningHead planningType)
        {
            var screen = GetScreenTypeId(planningType);
            // background Image
            foreach (DataRow row in designElements.Rows)
            {
                var rowDesign = designerUserControl.GetNewRow();
                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = screen;
                rowDesign["fk_design_background"] = 1;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];
                designerUserControl.AddNewRowToDatatable(rowDesign);
            }
        }


        private static DataTable GetNotesDatatable(List<string> data, out string notes_duration)
        {
            // Lets add notes
            var notedataTable = GetNotesRawTable();
            var notes = string.Join(Environment.NewLine, data);
            TimeSpan measuredTimespan = TextMeasurement.Measure(notes);
            notes_duration = measuredTimespan.ToString(@"hh\:mm\:ss\.fff");

            var noteRow = notedataTable.NewRow();
            noteRow["notes_id"] = -1;
            noteRow["fk_notes_videoevent"] = -1;
            noteRow["notes_line"] = notes?.Replace("'", "''");
            noteRow["notes_wordcount"] = notes?.Split(' ').Length ?? 0;
            noteRow["notes_index"] = notesIndex++;
            noteRow["notes_start"] = NotesStartTime;
            noteRow["notes_duration"] = notes_duration;
            noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            noteRow["notes_isdeleted"] = false;
            noteRow["notes_issynced"] = true;
            noteRow["notes_serverid"] = -1;
            noteRow["notes_syncerror"] = string.Empty;
            notedataTable.Rows.Add(noteRow);

            return notedataTable;
        }


        private static int GetScreenTypeId(EnumPlanningHead planningType)
        {
            var screen = (int)EnumScreen.Custom;
            if (planningType == EnumPlanningHead.Title)
                screen = (int)EnumScreen.Title;
            else if (planningType == EnumPlanningHead.Requirements)
                screen = (int)EnumScreen.Requirements;
            else if (planningType == EnumPlanningHead.Objectives)
                screen = (int)EnumScreen.Objectives;
            else if (planningType == EnumPlanningHead.NextUp)
                screen = (int)EnumScreen.Next;
            else if (planningType == EnumPlanningHead.Introduction || planningType == EnumPlanningHead.Text)
                screen = (int)EnumScreen.Intro;
            else if (planningType == EnumPlanningHead.Custom || planningType == EnumPlanningHead.Image || planningType == EnumPlanningHead.Video)
                screen = (int)EnumScreen.Custom;
            else if (planningType == EnumPlanningHead.Conclusion)
                screen = (int)EnumScreen.Conclusion;
            return screen;
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
            if (BulletText.Length <= 80)
            {
                var top = 250 + (bulletNumber * 150);
                var title = $"<TextBox AcceptsReturn=\"True\" TextWrapping=\"Wrap\" BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"45\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{BulletText}</TextBox>";
                return title;
            }
            else
            {
                var converted = Regex.Replace(BulletText, "(.{80})", "$1###");
                var arr = converted.Split(new string[] { "###" }, StringSplitOptions.None);
                var title = "";
                int i = 0;
                foreach (var item in arr)
                {
                    var top = 250 + (bulletNumber * 150) + i;
                    var text = i >= 120 ? item + "..." : item;
                    title += $"<TextBox AcceptsReturn=\"True\" TextWrapping=\"Wrap\" BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"35\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{text}</TextBox>" + Environment.NewLine;
                    i = i + 40;
                    if (i >= 160) break;
                }
                return title;
            }

        }

        private static string GetBulletCircle(int bulletNumber = 1)
        {
            var top = 280 + (bulletNumber * 150);
            var circle = $"<Ellipse Fill=\"#00000000\" Stroke=\"#FFF0F8FF\" StrokeThickness=\"10\" StrokeDashArray=\"\" Width=\"20\" Height=\"20\" Opacity=\"1\" Canvas.Left=\"380\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Ellipse.RenderTransform><RotateTransform Angle=\"0\" /></Ellipse.RenderTransform></Ellipse>";
            return circle;
        }

        public static byte[] XMLToImagePlanningSetup(DataTable dataTable)
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
                if (canvas.Children.Count == 1)
                {
                    UIElement element = canvas.Children[0];
                    canvas.Children.RemoveAt(0);
                    container.Children.Add(element);
                }
                else
                {
                    for (int i = canvas.Children.Count - 1; i >= 0; i--)
                    {
                        UIElement element = canvas.Children[i];
                        canvas.Children.RemoveAt(i);
                        container.Children.Add(element);
                    }
                }
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

        private static async Task<bool?> SavePlanningToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, DataTable dtNotes, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track, string duration)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            VideoEventResponseModel addedData;

            EventEndTime = DataManagerSqlLite.CalcNextEnd(EventStartTime, duration);
            addedData = await PostVideoEventToServer(designerUserControl.dataTableAdd, dtNotes, blob, selectedProjectEvent, track, authApiViewModel, EventStartTime, duration);

            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: dtNotes, selectedProjectEvent, authApiViewModel, track, duration);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForPlanning(designerUserControl.dataTableAdd, designImagerUserControl.dtVideoSegment, EventStartTime, duration, (int)track, selectedProjectEvent);
                else
                    return null;
            }
            else
            {
                SuccessFlowForPlanning(addedData, selectedProjectEvent.projdetId, blob);
                EventStartTime = EventEndTime;
                return true;
            }
        }

        private static async Task<VideoEventResponseModel> PostVideoEventToServer(DataTable dtDesign, DataTable dtNotes, byte[] blob, SelectedProjectEvent selectedProjectEvent, EnumTrack track, AuthAPIViewModel authApiViewModel, string startTime = "00:00:00.000", string duration = "00:00:10.000")
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)EnumMedia.FORM;
            objToSync.videoevent_track = (int)track;
            objToSync.videoevent_start = startTime;
            objToSync.videoevent_duration = duration;
            objToSync.videoevent_origduration = duration;
            objToSync.videoevent_end = DataManagerSqlLite.CalcNextEnd(startTime, duration); // TBD
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.design.AddRange(DesignEventHandlerHelper.GetDesignModelList(dtDesign));
            if (dtNotes != null && dtNotes.Rows.Count > 0)
                objToSync.notes.AddRange(CloneEventHandlerHelper.GetNotesModelList(dtNotes));
            objToSync.videosegment_media_bytes = blob;
            var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
            return result;
        }

        private static void SuccessFlowForPlanning(VideoEventResponseModel addedData, int selectedProjectId, byte[] blob)
        {
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForDesign(addedData, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCallout(addedData.design, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesign(dtDesign);

                // Insert Notes
                if (addedData.notes?.Count > 0)
                {
                    var dtNotes = CloneEventHandlerHelper.GetNotesDataTableServer(addedData.notes, localVideoEventId);
                    var insertedNotesIdLast = DataManagerSqlLite.InsertRowsToNotes(dtNotes);
                }


                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForCallout(blob, localVideoEventId, addedData.videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, addedData.videosegment.videosegment_id);
            }
        }


        private static bool FailureFlowForPlanning(DataTable dtDesignMaster, DataTable dtVideoSegmentMaster, string timeAtTheMoment, string duration, int track, SelectedProjectEvent selectedProjectEvent)
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




        #endregion

        #region == Notes ==
        private static DataTable GetNotesRawTable()
        {
            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_createdate", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));
            return notedataTable;
        }
        #endregion
    }
}
