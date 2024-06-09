using LocalVoiceGen_UserControl.Helpers;
using ScreenRecorder_UserControl.Windows.Controls;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using Sqllite_Library.Models.Planning;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using VideoCreatorXAMLLibrary.Auth;
using VideoCreatorXAMLLibrary.Loader;
using VideoCreatorXAMLLibrary.Models;
using VideoCreatorXAMLLibrary.XAML;

namespace VideoCreatorXAMLLibrary.Helpers
{
    public static class PlanningHandlerHelper
    {
        #region === Planning Functions ==

        private static int RetryIntervalInSeconds = 300;
        private static string EventStartTime = "00:00:00.000";
        private static string EventEndTime = "00:00:00.000";

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

        public static string CheckIfBackgroundPresent(SQLiteConnection sqlCon)
        {
            var directory = CreateOrReturnEmptyDirectory();
            var backgrounds = DataManagerSqlLite.GetBackgroundForTransaction(sqlCon);
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

        public static async Task ProcessForTransaction(PlanningEvent planningEvent, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, UserControl uc, LoadingAnimation loader, SQLiteConnection sqlCon, string imagePath = null)
        {
            if (uc != null)
                LoaderHelper.ShowLoader(uc, loader, "Starting ...");
            EventStartTime = DataManagerSqlLite.GetNextStartForTransaction((int)EnumMedia.IMAGE, selectedProjectEvent.projdetId, sqlCon);

            Designer_UserControl designerUserControl = new Designer_UserControl(selectedProjectEvent.projectId, imagePath, -1, true, false);
            var designElements = designerUserControl.PlanningSetup();

            if (planningEvent.Type == EnumScreen.All)
            {
                var data = DataManagerSqlLite.GetPlanningForTransaction(selectedProjectEvent.projectId, sqlCon, dependentFlag: true)?.OrderBy(x => x.planning_sort).ToList();
                loader.setTextBlockMessage($"Processing 0/{data.Count} ...");
                //Add Title
                //await AddProjectNameSlide(designElements, designerUserControl, selectedProjectEvent, authApiViewModel);
                int cntr = 1;
                foreach (var item in data)
                {
                    loader.setTextBlockMessage($"Processing {(EnumScreen)Enum.ToObject(typeof(EnumScreen), item.fk_planning_screen)} {cntr++}/{data.Count} ...");
                    switch (item.fk_planning_screen)
                    {
                        case (int)EnumScreen.Text:
                        case (int)EnumScreen.Introduction:
                            // code block
                            await AddIntroductionOrTextSlide((EnumScreen)item.fk_planning_screen, item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, sqlCon);
                            break;
                        case (int)EnumScreen.Requirements:
                        case (int)EnumScreen.Objectives:
                        case (int)EnumScreen.Conclusion:
                        case (int)EnumScreen.NextUp:
                        case (int)EnumScreen.Bullet:
                            await AddPlanningElementWithDesign(item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, (EnumScreen)item.fk_planning_screen, sqlCon);
                            break;
                        case (int)EnumScreen.Video:
                            await AddPlaceholder((EnumScreen)item.fk_planning_screen, item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, sqlCon);
                            break;
                        case (int)EnumScreen.Custom:
                            await AddPlaceholder((EnumScreen)item.fk_planning_screen, item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, sqlCon);
                            break;
                        case (int)EnumScreen.Image:
                            // code block
                            if (item?.planning_media?.Count > 0)
                                await AddPlanningImage(item, selectedProjectEvent, authApiViewModel, (EnumScreen)item.fk_planning_screen, sqlCon);
                            else
                                await AddPlaceholder((EnumScreen)item.fk_planning_screen, item, designElements, designerUserControl, selectedProjectEvent, authApiViewModel, sqlCon);
                            break;
                        default:
                            // code block
                            break;
                    }
                }
            }
        }


        private static async Task<bool?> AddPlaceholder(EnumScreen screen, CBVPlanning item, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            var text = Enum.GetName(screen.GetType(), screen) + " Placeholder";
            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, screen);
            DataRow rowTitle = designerUserControl.dataTableObject.Rows[0];
            rowTitle["design_xml"] += Environment.NewLine + GetTitleElement(text).Replace("'", "''");

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableObject);
            var finalBlob = designImagerUserControl.XMLToImage(designerUserControl.dataTableObject);
            designImagerUserControl.SaveToDataTable(finalBlob);

            string notes_duration = "00:00:10.000";
            DataTable dtNotes = null;
            if (!string.IsNullOrEmpty(item.planning_notesline))
                dtNotes = GetNotesDatatable(item.planning_notesline, out notes_duration);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: dtNotes, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, notes_duration, item.planning_serverid, sqlCon);
            return result;
        }

        private static async Task<bool?> AddProjectNameSlide(DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            var title = DataManagerSqlLite.GetProjectByIdForTransaction(sqlCon, selectedProjectEvent.projectId, false)?.project_videotitle;

            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, EnumScreen.Title);
            DataRow rowTitle = designerUserControl.dataTableObject.Rows[0];
            rowTitle["design_xml"] += Environment.NewLine + GetTitleElement(title).Replace("'", "''");

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableObject);
            var finalBlob = designImagerUserControl.XMLToImage(designerUserControl.dataTableObject);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: null, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, "00:00:10.000", planningServerId: 0, sqlCon);
            return result;
        }

        private static async Task<bool?> AddIntroductionOrTextSlide(EnumScreen screen, CBVPlanning item, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            if (item?.planning_desc?.Count > 0)
            {
                var text = item?.planning_desc[0]?.planningdesc_line;
                designerUserControl.ClearDatatable();
                FillBackgroundRowForPlanning(designElements, designerUserControl, screen);
                DataRow rowTitle = designerUserControl.dataTableObject.Rows[0];
                rowTitle["design_xml"] += Environment.NewLine + GetTitleElement(text).Replace("'", "''");

                var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableObject);
                var finalBlob = designImagerUserControl.XMLToImage(designerUserControl.dataTableObject);
                designImagerUserControl.SaveToDataTable(finalBlob);

                var dtNotes = GetNotesDatatable(item.planning_notesline, out string notes_duration);
                var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: dtNotes, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, notes_duration, item.planning_serverid, sqlCon);
                return result;
            }
            return null;
        }

        private static async Task<bool?> AddPlanningImage(CBVPlanning item, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumScreen screen, SQLiteConnection sqlCon)
        {
            var notesText = item.planning_notesline;
            var notedataTable = GetNotesDatatable(notesText, out string notes_duration);
            EventEndTime = DataManagerSqlLite.CalcNextEnd(EventStartTime, string.IsNullOrEmpty(notes_duration) ? "00:00:10.000" : notes_duration);
            var dataTable = new DataTable();
            dataTable.Columns.Add("videoevent_id", typeof(int));
            dataTable.Columns.Add("fk_videoevent_projdet", typeof(int));
            dataTable.Columns.Add("fk_videoevent_media", typeof(int));
            dataTable.Columns.Add("videoevent_track", typeof(int));
            dataTable.Columns.Add("videoevent_start", typeof(string));
            dataTable.Columns.Add("videoevent_duration", typeof(string));
            dataTable.Columns.Add("videoevent_origduration", typeof(string));
            dataTable.Columns.Add("videoevent_planning", typeof(Int64));
            dataTable.Columns.Add("videoevent_createdate", typeof(string));
            dataTable.Columns.Add("videoevent_modifydate", typeof(string));
            dataTable.Columns.Add("media", typeof(byte[])); // Media Column
            dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen    
            dataTable.Columns.Add("videoevent_isdeleted", typeof(bool));
            dataTable.Columns.Add("videoevent_issynced", typeof(bool));
            dataTable.Columns.Add("videoevent_serverid", typeof(Int64));
            dataTable.Columns.Add("videoevent_syncerror", typeof(string));
            dataTable.Columns.Add("videoevent_notes", typeof(DataTable));

            // Since this table has Referential Integrity, so lets push one by one
            dataTable.Rows.Clear();

            var row = dataTable.NewRow();
            //row["videoevent_id"] = -1;
            row["fk_videoevent_projdet"] = selectedProjectEvent.projdetId;
            row["fk_videoevent_media"] = 1;
            row["videoevent_track"] = EnumTrack.IMAGEORVIDEO;
            row["videoevent_start"] = EventStartTime;
            row["videoevent_duration"] = notes_duration;
            row["videoevent_origduration"] = notes_duration;
            row["videoevent_planning"] = item.planning_serverid;
            row["videoevent_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = false;
            row["videoevent_serverid"] = -1;
            row["videoevent_syncerror"] = string.Empty;
            row["fk_videoevent_screen"] = (int)screen;
            row["videoevent_notes"] = notedataTable;
            var byteArrayIn = item?.planning_media[0]?.planningmedia_mediafull;
            row["media"] = byteArrayIn;
            dataTable.Rows.Add(row);
            foreach (DataRow itemRow in dataTable.Rows)
            {
                var insertedVideoeventId = await ProcessVideoSegmentDataRowByRow(row, selectedProjectEvent, authApiViewModel, sqlCon);
            }
            EventStartTime = EventEndTime; // Reset it for next time
            return true;

        }

        private static async Task<bool?> AddPlanningElementWithDesign(CBVPlanning item, DataTable designElements, Designer_UserControl designerUserControl, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumScreen screen, SQLiteConnection sqlCon)
        {
            if (item == null || item.planning_desc?.Count == 0 || item.planning_desc[0].planningdesc_bullets?.Count == 0) { return false; }

            var heading = item?.planning_desc[0]?.planningdesc_line ?? "No heading Found";

            designerUserControl.ClearDatatable();
            FillBackgroundRowForPlanning(designElements, designerUserControl, screen);

            DataRow rowTitle = designerUserControl.dataTableObject.Rows[0];
            // Design Elements - Heading
            rowTitle["design_xml"] += Environment.NewLine + GetHeading($"{heading}").Replace("'", "''");

            int j = 0;
            var bullets = string.Empty;
            var bulletCount = item.planning_desc[0].planningdesc_bullets.Count;
            for (var i = 0; i < bulletCount; i++) // Bullet Circle + text
            {
                var bulletText = item.planning_desc[0].planningdesc_bullets[i].planningbullet_line;
                if (string.IsNullOrEmpty(bulletText)) continue;
                bullets += GetBulletCircle(j + 1).Replace("'", "''") + Environment.NewLine;
                if (i < bulletCount - 1)
                    bullets += GetBulletPoints($"{bulletText}", j + 1).Replace("'", "''") + Environment.NewLine;
                else
                    bullets += GetBulletPoints($"{bulletText}", j + 1).Replace("'", "''");
                j++;
            }
            rowTitle["design_xml"] += Environment.NewLine + bullets;
            var notesText = item.planning_notesline;
            var notedataTable = GetNotesDatatable(notesText, out string notes_duration);

            var designImagerUserControl = new DesignImager_UserControl(designerUserControl.dataTableObject);
            var finalBlob = designImagerUserControl.XMLToImage(designerUserControl.dataTableObject);
            designImagerUserControl.SaveToDataTable(finalBlob);
            var result = await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: notedataTable, selectedProjectEvent, authApiViewModel, EnumTrack.IMAGEORVIDEO, notes_duration, item.planning_serverid, sqlCon);
            return result;
        }



        private static void FillBackgroundRowForPlanning(DataTable designElements, Designer_UserControl designerUserControl, EnumScreen screen)
        {
            foreach (DataRow row in designElements.Rows)
            {
                var rowDesign = designerUserControl.GetNewRow();
                rowDesign["design_id"] = row["id"];
                rowDesign["fk_design_videoevent"] = -1;
                rowDesign["fk_design_screen"] = (int)screen;
                rowDesign["fk_design_background"] = 1;
                rowDesign["design_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                rowDesign["design_xml"] = row["xaml"];
                designerUserControl.AddNewRowToDatatable(rowDesign);
            }
        }


        private static DataTable GetNotesDatatable(string data, out string notes_duration)
        {
            // Lets add notes
            var notedataTable = GetNotesRawTable();
            if (string.IsNullOrEmpty(data))
            {
                notes_duration = "00:00:10.000";
                return null;
            }

            var notes = data.Split(new string[] { "$$$NEWNOTES$$$" }, StringSplitOptions.None).ToList();
            TimeSpan measuredTimespan = new TimeSpan(0, 0, 0);

            foreach (var noteItem in notes)
            {
                if (string.IsNullOrEmpty(noteItem))
                    continue;
                var NotesStartTime = measuredTimespan.ToString(@"hh\:mm\:ss\.fff");

                var notesTimespan = new TimeSpan(0, 0, 0, 10);
                if (noteItem?.Length > 0)
                    notesTimespan = TextMeasurement.Measure(noteItem);

                measuredTimespan += notesTimespan;
                notes_duration = notesTimespan.ToString(@"hh\:mm\:ss\.fff");

                var noteRow = notedataTable.NewRow();
                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = -1;
                noteRow["notes_line"] = noteItem?.Replace("'", "''");
                noteRow["notes_wordcount"] = noteItem?.Split(' ').Length ?? 0;
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
            }
            notes_duration = measuredTimespan.ToString(@"hh\:mm\:ss\.fff");
            return notedataTable;
        }

        private static string GetTitleElement(string Text)
        {
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" FontFamily=\"Tahoma\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"300\" Canvas.Top=\"450\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{Text.ToUpper()}</TextBox>";
            return title;
        }

        private static string GetHeading(string HeadingText)
        {
            var top = 250;
            var title = $"<TextBox BorderThickness=\"0,0,0,0\" FontFamily=\"Tahoma\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontWeight = \"800\" FontSize=\"60\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"350\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{HeadingText.ToUpper()}</TextBox>";
            return title;
        }

        private static string GetBulletPoints(string BulletText, int bulletNumber = 1)
        {
            if (BulletText.Length <= 80)
            {
                var top = 250 + (bulletNumber * 150);
                var title = $"<TextBox AcceptsReturn=\"True\" FontFamily=\"Tahoma\" TextWrapping=\"Wrap\" BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"45\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{BulletText}</TextBox>";
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
                    title += $"<TextBox AcceptsReturn=\"True\" FontFamily=\"Tahoma\" TextWrapping=\"Wrap\" BorderThickness=\"0,0,0,0\" Background=\"#00FFFFFF\" Foreground=\"#FFF0F8FF\" FontSize=\"35\" Cursor=\"Arrow\" AllowDrop=\"False\" Focusable=\"False\" Canvas.Left=\"430\" Canvas.Top=\"{top}\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><TextBox.RenderTransform><RotateTransform Angle=\"0\" /></TextBox.RenderTransform>{text}</TextBox>" + Environment.NewLine;
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

        private static async Task<int> ProcessVideoSegmentDataRowByRow(DataRow row, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, SQLiteConnection sqlCon)
        {
            DataTable dtNotes = null;
            if (row != null && row["videoevent_notes"] != DBNull.Value)
                dtNotes = (DataTable)row["videoevent_notes"];
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerForVideoOrImage(row, dtNotes, selectedProjectEvent, authApiViewModel);
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await ProcessVideoSegmentDataRowByRow(row, selectedProjectEvent, authApiViewModel, sqlCon);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForSaveImageorVideo(row, selectedProjectEvent, sqlCon);
                else
                    return -1;
            }
            else
                return SuccessFlowForSaveImageorVideo(row, addedData, selectedProjectEvent, sqlCon);
        }

        private static int SuccessFlowForSaveImageorVideo(DataRow row, VideoEventResponseModel addedData, SelectedProjectEvent selectedProjectEvent, SQLiteConnection sqlCon)
        {
            var insertedVideoSegmentId = -1;
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImage(addedData, selectedProjectEvent.projdetId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEventForTransaction(dt, sqlCon, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImage(blob, localVideoEventId, addedData.videosegment);
                insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegmentForTransaction(dtVideoSegment, addedData.videoevent.videoevent_id, sqlCon);
                if (addedData?.notes?.Count > 0)
                {
                    var dtNotes = CloneEventHandlerHelper.GetNotesDataTableServer(addedData.notes, localVideoEventId);
                    DataManagerSqlLite.InsertRowsToNotesForTransaction(dtNotes, sqlCon);
                }
            }
            return insertedVideoSegmentId;
        }

        private static int FailureFlowForSaveImageorVideo(DataRow row, SelectedProjectEvent selectedProjectEvent, SQLiteConnection sqlCon)
        {
            // Save the record locally with server Id = temp and issynced = false
            var localServerVideoEventId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImageLocally(row, localServerVideoEventId, selectedProjectEvent.projdetId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEventForTransaction(dt, sqlCon, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var videoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var localServerVideoSegmentId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImageLocally(blob, videoEventId, localServerVideoSegmentId);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegmentForTransaction(dtVideoSegment, videoEventId, sqlCon);
            }
            return -1;
        }

        private static async Task<bool?> SavePlanningToServerAndLocalDB(DesignImager_UserControl designImagerUserControl, Designer_UserControl designerUserControl, DataTable dtNotes, SelectedProjectEvent selectedProjectEvent, AuthAPIViewModel authApiViewModel, EnumTrack track, string duration, Int64 planningServerId, SQLiteConnection sqlCon)
        {
            var blob = designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[];
            VideoEventResponseModel addedData;

            EventEndTime = DataManagerSqlLite.CalcNextEnd(EventStartTime, duration);
            addedData = await PostVideoEventToServer(designerUserControl.dataTableObject, dtNotes, blob, selectedProjectEvent, track, authApiViewModel, EventStartTime, duration, planningServerId);

            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    return await SavePlanningToServerAndLocalDB(designImagerUserControl, designerUserControl, dtNotes: dtNotes, selectedProjectEvent, authApiViewModel, track, duration, planningServerId, sqlCon);
                else if (confirmation == MessageBoxResult.No)
                    return FailureFlowForPlanning(designerUserControl.dataTableObject, designImagerUserControl.dtVideoSegment, EventStartTime, duration, (int)track, selectedProjectEvent, sqlCon);
                else
                    return null;
            }
            else
            {
                SuccessFlowForPlanning(addedData, selectedProjectEvent.projdetId, blob, sqlCon);
                EventStartTime = EventEndTime;
                return true;
            }
        }

        private static async Task<VideoEventResponseModel> PostVideoEventToServer(DataTable dtDesign, DataTable dtNotes, byte[] blob, SelectedProjectEvent selectedProjectEvent, EnumTrack track, AuthAPIViewModel authApiViewModel, string startTime = "00:00:00.000", string duration = "00:00:10.000", Int64 videoevent_planning = 0)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)EnumMedia.FORM;
            objToSync.videoevent_track = (int)track;
            objToSync.videoevent_start = startTime;
            objToSync.videoevent_duration = duration;
            objToSync.videoevent_origduration = duration;
            objToSync.videoevent_planning = (int)videoevent_planning;
            objToSync.videoevent_end = DataManagerSqlLite.CalcNextEnd(startTime, duration); // TBD
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.design.AddRange(DesignEventHandlerHelper.GetDesignModelList(dtDesign));
            if (dtNotes != null && dtNotes.Rows.Count > 0)
                objToSync.notes.AddRange(CloneEventHandlerHelper.GetNotesModelList(dtNotes));
            objToSync.videosegment_media_bytes = blob;
            var result = await authApiViewModel.POSTVideoEvent(selectedProjectEvent, objToSync);
            return result;
        }

        private static void SuccessFlowForPlanning(VideoEventResponseModel addedData, int selectedProjectId, byte[] blob, SQLiteConnection sqlCon)
        {
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForDesign(addedData, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEventForTransaction(dt, sqlCon, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCallout(addedData.design, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesignForTransaction(dtDesign, sqlCon);

                // Insert Notes
                if (addedData.notes?.Count > 0)
                {
                    var dtNotes = CloneEventHandlerHelper.GetNotesDataTableServer(addedData.notes, localVideoEventId);
                    var insertedNotesIdLast = DataManagerSqlLite.InsertRowsToNotesForTransaction(dtNotes, sqlCon);
                }


                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForCallout(blob, localVideoEventId, addedData.videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegmentForTransaction(dtVideoSegment, addedData.videosegment.videosegment_id, sqlCon);
            }
        }

        private static bool FailureFlowForPlanning(DataTable dtDesignMaster, DataTable dtVideoSegmentMaster, string timeAtTheMoment, string duration, int track, SelectedProjectEvent selectedProjectEvent, SQLiteConnection sqlCon)
        {
            // Save the record locally with server Id = temp and issynced = false
            var blob = dtVideoSegmentMaster.Rows[0]["videosegment_media"] as byte[];
            var localServerVideoEventId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            var dt = DesignEventHandlerHelper.GetVideoEventDataTableForCalloutLocally(dtDesignMaster, dtVideoSegmentMaster, timeAtTheMoment, duration, track, selectedProjectEvent.projdetId, localServerVideoEventId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEventForTransaction(dt, sqlCon, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var localVideoEventId = insertedVideoEventIds[0];
                var dtDesign = DesignEventHandlerHelper.GetDesignDataTableForCalloutLocally(dtDesignMaster, dtVideoSegmentMaster, timeAtTheMoment, duration, track, localVideoEventId);
                DataManagerSqlLite.InsertRowsToDesignForTransaction(dtDesign, sqlCon);

                var dtVideoSegment = DesignEventHandlerHelper.GetVideoSegmentDataTableForCalloutLocally(blob, localVideoEventId);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegmentForTransaction(dtVideoSegment, localVideoEventId, sqlCon);
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
