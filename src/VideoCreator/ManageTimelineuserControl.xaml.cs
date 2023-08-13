using Designer_UserControl;
using DesignImager_UserControl;
using ScreenRecording_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace VideoCreator
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimelineUserControl : UserControl
    {
        private int selectedProjectId;
        public ManageTimelineUserControl(int projectId)
        {
            InitializeComponent();
            selectedProjectId = projectId;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;


            GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
            //GridDisplayUserConrol.SetSelectedProjectIdText(subjectText);
            GridDisplayUserConrol.Visibility = Visibility.Visible;


            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectId);
            //TimelineUserConrol.SetSelectedProjectIdText(subjectText);
            TimelineUserConrol.Visibility = Visibility.Visible;
            TimelineUserConrol.BtnInsertVideoEventDataClickEvent += TimelineUserConrol_BtnInsertVideoEventDataClickEvent;

            //Test
            NotesUserConrol.SetSelectedProjectId(selectedProjectId);
            NotesUserConrol.Visibility = Visibility.Visible;
        }

        private void TimelineUserConrol_BtnInsertVideoEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
            NotesUserConrol.SetSelectedProjectId(selectedProjectId);
        }

        private void ContextMenuAddVideoEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            var screenRecorderUserControl = new ScreenRecorderUserControl(selectedProjectId);
            var window = new Window
            {
                Title = "Screen Recorder",
                Content = screenRecorderUserControl,
                ResizeMode = ResizeMode.NoResize,
                Height = 200,
                Width = 600,
                RenderSize = screenRecorderUserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue && screenRecorderUserControl.datatable != null && screenRecorderUserControl.datatable.Rows.Count > 0)
            {
                if (screenRecorderUserControl.UserConsent || MessageBox.Show("Do you want save all recording??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var insertedVideoEventId = DataManagerSqlLite.InsertRowsToVideoEvent(screenRecorderUserControl.datatable);
                    if (insertedVideoEventId.Count > 0)
                    {
                        GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
                        NotesUserConrol.SetSelectedProjectId(selectedProjectId);
                        MessageBox.Show($"{screenRecorderUserControl.datatable.Rows.Count} video event record added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ContextMenuAddFormEventDataClickEvent(object sender, RoutedEventArgs e)
        {

            var backgroundImageData = new Dictionary<int, string>();
            var data = DataManagerSqlLite.GetBackground();
            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                    if (item.background_active)
                        backgroundImageData.Add(item.background_id, "white_background.png");
            }
            var designerUserControl = new DesignerUserControl(selectedProjectId, backgroundImageData);
            var window = new Window
            {
                Title = "Designer",
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
                        var dt = GetVideoEventTable();
                        var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
                        if (insertedVideoEventIds?.Count > 0)
                        {
                            var videoEventId = insertedVideoEventIds[0];
                            foreach (DataRow row in designerUserControl.dataTableAdd.Rows)
                                row["fk_design_videoevent"] = videoEventId;

                            DataManagerSqlLite.InsertRowsToDesign(designerUserControl.dataTableAdd);
                            // DataManagerSqlLite.UpdateRowsToDesign(designerUserControl.dataTableUpdate);

                            //Refresh Grid Data
                            GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
                            NotesUserConrol.SetSelectedProjectId(selectedProjectId);
                            var totalRows = designerUserControl.dataTableAdd.Rows.Count + designerUserControl.dataTableUpdate.Rows.Count;
                            MessageBox.Show($"{totalRows} form event record added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            ContextMenuAddFormEventDataClickEvent_Step2(videoEventId);
                        }
                        else
                        {
                            MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private DataTable GetVideoEventTable()
        {
            var dtVideoEvent = new DataTable();
            dtVideoEvent.Columns.Add("videoevent_id", typeof(int));
            dtVideoEvent.Columns.Add("fk_videoevent_project", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_start", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_duration", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_track", typeof(int));
            dtVideoEvent.Columns.Add("fk_videoevent_media", typeof(int));
            dtVideoEvent.Columns.Add("videoevent_createdate", typeof(string));
            dtVideoEvent.Columns.Add("videoevent_modifydate", typeof(string));
            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_project"] = selectedProjectId;
            row["videoevent_start"] = "00:00:00"; // TBD
            row["videoevent_track"] = 1; // TBD
            row["videoevent_duration"] = 10;
            row["fk_videoevent_media"] = 4;
            row["videoevent_createdate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            row["videoevent_modifydate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }


        private void ContextMenuAddFormEventDataClickEvent_Step2(int videoEventId)
        {
            var designImagerUserControl = new DesignImagerUserControl(videoEventId);
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
                    var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(designImagerUserControl.dtVideoSegment);
                    if (insertedVideoSegmentId > 0)
                    {
                        //GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
                        //NotesUserConrol.SetSelectedProjectId(selectedProjectId);
                        MessageBox.Show($"videosegment record for image added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


    }
}
