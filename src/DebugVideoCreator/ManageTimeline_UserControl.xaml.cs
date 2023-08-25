using AudioEditor_UserControl;
using AudioRecorder_UserControl;
using Designer_UserControl;
using DesignImager_UserControl;
using FSP_UserControl;
using Newtonsoft.Json;
using ScreenRecording_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Windows = System.Windows.Controls;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using AudioPlayer_UserControl;

namespace DebugVideoCreator
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimeline_UserControl : UserControl, IDisposable
    {
        private int selectedProjectId;
        private int selectedVideoEventId = -1;
        private int voiceAvgCount = -1;
        private CBVVideoEvent selectedVideoEvent;
        private PopupWindow popup;
        private AudioEditor editor;
        public ManageTimeline_UserControl(int projectId)
        {
            InitializeComponent();

            popup = new PopupWindow();


            selectedProjectId = projectId;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;

            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectId);
            TimelineUserConrol.Visibility = Visibility.Visible;
            //TimelineUserConrol.BtnInsertVideoEventDataClickEvent += TimelineUserConrol_BtnInsertVideoEventDataClickEvent;

            //Test
            RefreshOrLoadComboBoxes();
            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
            NotesUserConrol.Visibility = Visibility.Visible;

            AudioUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);

            // Reload Control
            FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            ResetAudioMenuOptions();
        }

        private void ResetAudioMenuOptions()
        {
            voiceAvgCount = DataManagerSqlLite.GetVoiceAverageCount();
            if (voiceAvgCount > 0)
            {
                ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[1]).IsEnabled = true;
                ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[0]).Header = "Re-Calculate Voice Average";
            }
            else
            {
                ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[1]).IsEnabled = false;
                ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[0]).Header = "Calculate Voice Average";
            }
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
                        //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
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
                        var dt = GetVideoEventDataTable();
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

        private DataTable GetVideoEventDataTable()
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
                        FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
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

        #region == Audio Context Menu ==

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var window = AudioUserConrol.GetCreateEventWindow(selectedProjectId);
            
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                RefreshOrLoadComboBoxes();
            }
        }

        private void ContextMenuCalcVoiceAverage(object sender, RoutedEventArgs e)
        {
            var vcForm = new VoiceAverage_Form();
            var result = vcForm.ShowDialog();
            if (result == Forms.DialogResult.Cancel && vcForm.response == MessageBoxResult.Yes && vcForm.newAverage.Length > 0)
            {
                if (voiceAvgCount <= 0)
                {
                    var insertedVoiceAvergaeId = SaveVoiceAverageToDatabase(vcForm.newAverage);
                    if (insertedVoiceAvergaeId > 0)
                    {
                        ResetAudioMenuOptions();
                        MessageBox.Show($"Vocie Average Added to DB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var isSuccess = UpdateVoiceAverageToDatabase(vcForm.newAverage);
                    if (isSuccess)
                    {
                        ResetAudioMenuOptions();
                        MessageBox.Show($"Vocie Average updated to DB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show($"Voice average not saved to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ContextMenuRecordAudioClickEvent(object sender, RoutedEventArgs e)
        {
            if (popup != null)
            {
                popup.Close();
            }

            popup = new PopupWindow();

            AudioRecorder recorder = new AudioRecorder();
            recorder.RecordingStoppedEvent += Recorder_RecordingStoppedEvent;


            popup.Content = recorder;
            popup.ShowDialog();
        }

        private void ContextMenuManageAudioClickEvent(object sender, RoutedEventArgs e)
        {
            if (popup != null)
            {
                popup.Close();
            }

            CreateAudioEditWindow(selectedVideoEvent?.audio_data[0]?.audio_media);
        }

        private void Recorder_RecordingStoppedEvent(object sender, RecordingStoppedArgs e)
        {
            if (popup != null)
            {
                popup.Close();
            }

            CreateAudioEditWindow(e.RecordedStream);
        }

        private void CreateAudioEditWindow(byte[] media)
        {
            popup = new PopupWindow() { Height = 460 };

            editor = new AudioEditor
            {
                Height = 250,
                Margin = new Thickness(15, 15, 15, 0)
            };


            editor.Init(media);

            var player = new AudioPlayer() { VerticalAlignment = VerticalAlignment.Center };
            editor.Selection_Changed += (se, ev) =>
            {
                byte[] data = editor.GetAudioSelectionAsMp3();
                player.Init(data, "Bounce");  
            };

            player.PositionUpdated += (se, ev) =>
            {
                editor.Wave.SetIndicator(ev.Position);
            };

            player.StoppedPlaying += (se, ev) =>
            {
                editor.Wave.HideIndicator();
            };

            player.PausedPlaying += (se, ev) =>
            {
                editor.Wave.HideIndicator();
            };

            var myStack = new Windows.StackPanel();
            myStack.Children.Add(editor);

            var playerGrid = new Windows.Grid();
            playerGrid.ColumnDefinitions.Add(new Windows.ColumnDefinition());
            playerGrid.ColumnDefinitions.Add(new Windows.ColumnDefinition());


            myStack.Children.Add(playerGrid);

            var groupBoxplayer = new Windows.GroupBox() { Margin = new Thickness(5) };
            groupBoxplayer.Header = "Player";
            playerGrid.Children.Add(groupBoxplayer);
            groupBoxplayer.Content = player;

            var groupBox = new Windows.GroupBox() { Margin = new Thickness(5) };
            groupBox.Header = "Event Parameters";
            var grid = new Windows.Grid();
            grid.ColumnDefinitions.Add(new Windows.ColumnDefinition() { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new Windows.ColumnDefinition());

            grid.RowDefinitions.Add(new Windows.RowDefinition() { Height = new GridLength(30) });
            grid.RowDefinitions.Add(new Windows.RowDefinition() { Height = new GridLength(30) });
            grid.RowDefinitions.Add(new Windows.RowDefinition() { Height = new GridLength(30) });

            var textBlock = new Windows.TextBlock() { Text = "Audio Start Time:", VerticalAlignment = VerticalAlignment.Center };
            var textBlock2 = new Windows.TextBlock() { Text = "Audio Encoding:", VerticalAlignment = VerticalAlignment.Center };

            var SMinTxt = new Windows.TextBox() { Margin = new Thickness(5, 5, 0, 5), MaxLength = 2, MinWidth = 25, Text = "00" };
            SMinTxt.PreviewTextInput += (se, ev) =>
            {
                int i;
                bool IsValid = int.TryParse(((Windows.TextBox)se).Text + ev.Text, out i) && i >= 0 && i <= 60;
                ev.Handled = !IsValid;
            };

            SMinTxt.LostKeyboardFocus += (se, ev) =>
            {
                if (((Windows.TextBox)se).Text == "")
                {
                    ((Windows.TextBox)se).Text = "00";
                }
                else
                {
                    ((Windows.TextBox)se).Text = Convert.ToInt32(((Windows.TextBox)se).Text).ToString("D2");
                }
            };

            var label = new Windows.Label() { Content = "min" };
            var SSecTxt = new Windows.TextBox() { Margin = new Thickness(5, 5, 0, 5), MaxLength = 2, MinWidth = 25, Text = "00" };

            SSecTxt.PreviewTextInput += (se, ev) =>
            {
                int i;
                bool IsValid = int.TryParse(((Windows.TextBox)se).Text + ev.Text, out i) && i >= 0 && i <= 60;
                ev.Handled = !IsValid;
            };

            SSecTxt.LostKeyboardFocus += (se, ev) =>
            {
                if (((Windows.TextBox)se).Text == "")
                {
                    ((Windows.TextBox)se).Text = "00";
                }
                else
                {
                    ((Windows.TextBox)se).Text = Convert.ToInt32(((Windows.TextBox)se).Text).ToString("D2");
                }
            };

            var label2 = new Windows.Label() { Content = "sec" };

            Windows.StackPanel stackPanel2 = new Windows.StackPanel() { Orientation = Windows.Orientation.Horizontal };
            stackPanel2.Children.Add(SMinTxt);
            stackPanel2.Children.Add(label);
            stackPanel2.Children.Add(SSecTxt);
            stackPanel2.Children.Add(label2);

            Windows.RadioButton radioButtonWav = new Windows.RadioButton() { Content = "As Wav", IsChecked = true, VerticalAlignment = VerticalAlignment.Center };
            Windows.RadioButton radioButtonMp3 = new Windows.RadioButton() { Content = "As Mp3", Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };

            Windows.StackPanel stackPanel = new Windows.StackPanel() { Orientation = Windows.Orientation.Horizontal };
            stackPanel.Children.Add(radioButtonWav);
            stackPanel.Children.Add(radioButtonMp3);

            var button = new Windows.Button() { Content = "Save", Padding = new Thickness(5, 0, 5, 0), Margin = new Thickness(5), MinWidth = 100 };
            button.Click += SaveAudioToDatabase;
            

            Windows.Button button2 = new Windows.Button() { Content = "Cancel", Padding = new Thickness(5, 0, 5, 0), Margin = new Thickness(5), MinWidth = 100 };
            button2.Click += CancelSaveAudio;

            Windows.StackPanel stackPanel1 = new Windows.StackPanel() { Orientation = Windows.Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            stackPanel1.Children.Add(button2);
            stackPanel1.Children.Add(button);


            grid.Children.Add(textBlock);
            Windows.Grid.SetColumn(textBlock, 0);
            Windows.Grid.SetRow(textBlock, 0);

            grid.Children.Add(stackPanel2);
            Windows.Grid.SetColumn(stackPanel2, 1);
            Windows.Grid.SetRow(stackPanel2, 0);

            grid.Children.Add(textBlock2);
            Windows.Grid.SetColumn(textBlock2, 0);
            Windows.Grid.SetRow(textBlock2, 1);

            grid.Children.Add(stackPanel);
            Windows.Grid.SetColumn(stackPanel, 1);
            Windows.Grid.SetRow(stackPanel, 1);

            grid.Children.Add(stackPanel1);
            Windows.Grid.SetColumn(stackPanel1, 1);
            Windows.Grid.SetRow(stackPanel1, 2);

            grid.Margin = new Thickness(15, 15, 5, 5);


            groupBox.Content = grid;

            playerGrid.Children.Add(groupBox);
            Windows.Grid.SetColumn(groupBox, 1);
            Windows.Grid.SetRow(groupBox, 0);

            popup.Content = myStack;
            popup.Show();

        }
        private void SaveAudioToDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
                //var dataTable = new DataTable();
                //dataTable.Columns.Add("videoevent_id", typeof(int));
                //dataTable.Columns.Add("fk_videoevent_project", typeof(int));
                //dataTable.Columns.Add("fk_videoevent_media", typeof(int));
                //dataTable.Columns.Add("videoevent_track", typeof(int));
                //dataTable.Columns.Add("videoevent_start", typeof(string));
                //dataTable.Columns.Add("videoevent_duration", typeof(int));
                //dataTable.Columns.Add("videoevent_createdate", typeof(string));
                //dataTable.Columns.Add("videoevent_modifydate", typeof(string));
                ////optional column
                //dataTable.Columns.Add("media", typeof(byte[])); // Media Column
                //dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen
                //                                                           // Since this table has Referential Integrity, so lets push one by one
                //dataTable.Rows.Clear();
                //var row = dataTable.NewRow();
                //row["videoevent_id"] = -1;
                //row["fk_videoevent_project"] = selectedProjectId;
                //row["videoevent_track"] = 1;
                //row["videoevent_start"] = $"00:{SMinTxt.Text}:{SSecTxt.Text}";
                //row["videoevent_duration"] = 0;
                //row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //var mediaId = 3;
                //row["fk_videoevent_media"] = mediaId;
                //if (mediaId == 4) //select screen if media is form
                //{
                //    row["fk_videoevent_screen"] = 1; // Hardcoded 1 for now
                //    row["media"] = null;
                //}
                //else
                //{
                //    //Fill Media in case image, video or audio is selected
                //    row["fk_videoevent_screen"] = -1; // Not needed for this case
                //    byte[] mediaByte;
                //    if (radioButtonWav.IsChecked == true)
                //    {
                //        mediaByte = editor.GetAudioSelectionAsWav();
                //    }
                //    else
                //    {
                //        mediaByte = editor.GetAudioSelectionAsMp3();
                //    }
                //    row["media"] = mediaByte;
                //}
                //dataTable.Rows.Add(row);
                //Create_Event(dataTable);
                MessageBox.Show("Coming Soon", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                popup.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                popup.Close();
            }
        }

        private void CancelSaveAudio(object sender, RoutedEventArgs e)
        {
            popup.Close();
        }


    private int SaveVoiceAverageToDatabase(string average)
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("voiceaverage_id", typeof(int));
                dataTable.Columns.Add("voiceaverage_average", typeof(string));
                var row = dataTable.NewRow();
                row["voiceaverage_id"] = -1;
                row["voiceaverage_average"] = average;
                dataTable.Rows.Add(row);
                var insertedId = DataManagerSqlLite.InsertRowsToVoiceAverage(dataTable);
                return insertedId[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateVoiceAverageToDatabase(string average)
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("voiceaverage_id", typeof(int));
                dataTable.Columns.Add("voiceaverage_average", typeof(string));
                var row = dataTable.NewRow();
                row["voiceaverage_id"] = 1;
                row["voiceaverage_average"] = average;
                dataTable.Rows.Add(row);
                DataManagerSqlLite.UpdateRowsToVoiceAverage(dataTable);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion 


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

        private void cmbVideoEvent_SelectionChanged(object sender, Windows.SelectionChangedEventArgs e)
        {
            selectedVideoEvent = (CBVVideoEvent)cmbVideoEvent?.SelectedItem;
            if(selectedVideoEvent != null)
            {
                selectedVideoEventId = ((CBVVideoEvent)cmbVideoEvent?.SelectedItem).videoevent_id;
                NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                if (selectedVideoEvent.fk_videoevent_media == 3)
                {
                    AudioUserConrol.LoadSelectedAudio(selectedVideoEvent);
                    ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[3]).IsEnabled = true;
                }
                else
                {
                    AudioUserConrol.LoadSelectedAudio(null);
                    ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[3]).IsEnabled = false;

                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
