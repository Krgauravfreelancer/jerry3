using Designer_UserControl;
using DesignImager_UserControl;
using Newtonsoft.Json;
using ScreenRecording_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace DebugVideoCreator
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimeline_UserControl : UserControl
    {
        private int selectedProjectId;
        private int selectedVideoEventId = -1;
        public ManageTimeline_UserControl(int projectId)
        {
            InitializeComponent();
            selectedProjectId = projectId;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;

            //Timeline
            //TimelineUserConrol.SetSelectedProjectId(selectedProjectId);
            //TimelineUserConrol.Visibility = Visibility.Visible;
            //TimelineUserConrol.BtnInsertVideoEventDataClickEvent += TimelineUserConrol_BtnInsertVideoEventDataClickEvent;

            //Test
            RefreshOrLoadComboBoxes();
            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
            NotesUserConrol.Visibility = Visibility.Visible;

            AudioUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
        }

        private void TimelineUserConrol_BtnInsertVideoEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
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
                        RefreshOrLoadComboBoxes();
                        DesignViewUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                        NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
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
            
            var data = DataManagerSqlLite.GetBackground();
            var designerUserControl = new DesignerUserControl(selectedProjectId, JsonConvert.SerializeObject(data));
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
                            //GridDisplayUserConrol.SelectionChanged(EnumEntity.VIDEOEVENT, selectedProjectId);
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
                        RefreshOrLoadComboBoxes();
                        DesignViewUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                        NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                        MessageBox.Show($"videosegment record for image added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var window = AudioUserConrol.GetCreateEventWindow(selectedProjectId);
            
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                RefreshOrLoadComboBoxes();
            }
        }

        private void LoadTimelineDataFromDb_Click(object sender, RoutedEventArgs e)
        {
            TimelineUserConrol.LoadTimelineDataFromDb_Click();
        }

        private void ClearTimelines(object sender, RoutedEventArgs e)
        {
            TimelineUserConrol.ClearTimelines();
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            TimelineUserConrol.DeleteSelectedEvent();
        }

        private void EditSelectedEvent(object sender, RoutedEventArgs e)
        {
            TimelineUserConrol.EditSelectedEvent();
        }

        private void SaveTimeline(object sender, RoutedEventArgs e)
        {
            TimelineUserConrol.SaveTimeline();
        }

        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.VIDEOEVENT)
            {
                var data = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
                RefreshComboBoxes(cmbVideoEvent, data, "videoevent_id");
            }
        }

        private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }
        }

        private void cmbVideoEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedVideoEvent = (CBVVideoEvent)cmbVideoEvent?.SelectedItem;
            if(selectedVideoEvent != null)
            {
                selectedVideoEventId = ((CBVVideoEvent)cmbVideoEvent?.SelectedItem).videoevent_id;
                NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                if (selectedVideoEvent.fk_videoevent_media == 3)
                {
                    AudioUserConrol.LoadSelectedAudio(selectedVideoEvent);
                    DesignViewUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                }
                else
                {
                    DesignViewUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                }

            }
           
        }
    }
}
