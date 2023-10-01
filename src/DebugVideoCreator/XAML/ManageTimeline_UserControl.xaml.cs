using AudioEditor_UserControl;
using AudioRecorder_UserControl;
using Newtonsoft.Json;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Windows = System.Windows.Controls;
using Forms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using AudioPlayer_UserControl;

namespace DebugVideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimeline_UserControl : UserControl, IDisposable
    {
        private int selectedProjectId;
        private int selectedVideoEventId = -1;
        private int voiceAvgCount = -1;
        private string audioMinutetext, audioSecondtext, audioSaveButtonText;
        private bool isWavAudio = true;
        private CBVVideoEvent selectedVideoEvent;
        private PopupWindow popup;
        private AudioEditor editor;
        public ManageTimeline_UserControl(int projectId)
        {
            InitializeComponent();

            popup = new PopupWindow();
            ResetAudioMenuOptions();
            RefreshOrLoadComboBoxes();
            

            selectedProjectId = projectId;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;

            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectId);
            TimelineUserConrol.Visibility = Visibility.Visible;
            TimelineUserConrol.ContextMenu_AddVideoEvent_Success += TimelineUserConrol_ContextMenu_AddVideoEvent_Success;
            TimelineUserConrol.ContextMenu_AddForm_Clicked += TimelineUserConrol_ContextMenu_AddForm_Clicked;
            TimelineUserConrol.ContextMenu_Run_Clicked += TimelineUserConrol_ContextMenu_Run_Clicked;

            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
            NotesUserConrol.Visibility = Visibility.Visible;
            
        
            // Reload Control
            //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            TimelineUserConrol.LoadTimelineDataFromDb_Click();
            AudioUserConrol.SetSelected(selectedProjectId, selectedVideoEventId, selectedVideoEvent);
            NotesUserConrol.locAudioAddedEvent += NotesUserConrol_locAudioAddedEvent;
            //FSPClosed.

            //FSPClosed = new EventHandler(this.Parent, new EventArgs());
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

        private void NotesUserConrol_locAudioAddedEvent(object sender, EventArgs e)
        {
            ResetAudio();
        }


        #region Video Event Context Menu

        private void TimelineUserConrol_ContextMenu_Run_Clicked(object sender, EventArgs e)
        {
            var fsp_uc = new FullScreen_UserControl(true);
            fsp_uc.SetSelectedProjectIdAndReset(selectedProjectId);
            var window = new Window
            {
                Title = "Full Screen Player",
                Content = fsp_uc,
                ResizeMode = ResizeMode.CanResize,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
        }

        private void TimelineUserConrol_ContextMenu_AddVideoEvent_Success(object sender, EventArgs e)
        {
            RefreshOrLoadComboBoxes();
            TimelineUserConrol.LoadTimelineDataFromDb_Click();
            //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
        }
        
        private void TimelineUserConrol_ContextMenu_AddForm_Clicked(object sender, EventArgs e)
        {
            var data = DataManagerSqlLite.GetBackground();
            var designerUserControl = new Designer_UserControl(selectedProjectId, JsonConvert.SerializeObject(data));
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
                        var dt = GetVideoEventDataTableForDesignVideoSegment();
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

        private DataTable GetVideoEventDataTableForDesignVideoSegment()
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
            var designImagerUserControl = new DesignImager_UserControl(videoEventId);
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
                        TimelineUserConrol.LoadTimelineDataFromDb_Click();
                        //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
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

        #endregion

        #region == Audio Context Menu ==

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var window = AudioUserConrol.GetCreateEventWindow(selectedProjectId);

            var result = window.ShowDialog();
            if (result.HasValue)
            {
                RefreshOrLoadComboBoxes();
                TimelineUserConrol.LoadTimelineDataFromDb_Click();
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
                        MessageBox.Show($"Voice Average Added to DB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var isSuccess = UpdateVoiceAverageToDatabase(vcForm.newAverage);
                    if (isSuccess)
                    {
                        ResetAudioMenuOptions();
                        MessageBox.Show($"Voice Average updated to DB.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            popup?.Close();
            popup = new PopupWindow();
            AudioRecorder recorder = new AudioRecorder();
            recorder.RecordingStoppedEvent += Recorder_RecordingStoppedEvent;
            popup.Content = recorder;
            popup.ShowDialog();
        }

        private void ContextMenuManageAudioClickEvent(object sender, RoutedEventArgs e)
        {
            popup?.Close();
            if (selectedVideoEvent?.audio_data != null)
                CreateAudioEditWindow(selectedVideoEvent?.audio_data[0]?.audio_media, "Update");
        }

        private void Recorder_RecordingStoppedEvent(object sender, RecordingStoppedArgs e)
        {
            popup?.Close();

            CreateAudioEditWindow(e.RecordedStream, "Save");
        }

        private void CreateAudioEditWindow(byte[] media, string ButtonText)
        {
            audioSaveButtonText = ButtonText;
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

            var SMinTxt = new Windows.TextBox() { Margin = new Thickness(5, 5, 0, 5), MaxLength = 2, MinWidth = 25, Text = ButtonText == "Save" ? "00" : selectedVideoEvent.videoevent_start.Split(':')[1].ToString() };
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
                audioMinutetext = ((Windows.TextBox)se).Text;
            };

            var label = new Windows.Label() { Content = "min" };
            var SSecTxt = new Windows.TextBox() { Margin = new Thickness(5, 5, 0, 5), MaxLength = 2, MinWidth = 25, Text = ButtonText == "Save" ? "00" : selectedVideoEvent.videoevent_start.Split(':')[2].ToString() };

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
                audioSecondtext = ((Windows.TextBox)se).Text;
            };

            var label2 = new Windows.Label() { Content = "sec" };

            Windows.StackPanel stackPanel2 = new Windows.StackPanel() { Orientation = Windows.Orientation.Horizontal };
            stackPanel2.Children.Add(SMinTxt);
            stackPanel2.Children.Add(label);
            stackPanel2.Children.Add(SSecTxt);
            stackPanel2.Children.Add(label2);

            Windows.RadioButton radioButtonWav = new Windows.RadioButton() { Content = "As Wav", IsChecked = true, VerticalAlignment = VerticalAlignment.Center };
            Windows.RadioButton radioButtonMp3 = new Windows.RadioButton() { Content = "As Mp3", Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };


            radioButtonWav.Checked += (se, ev) =>
            {
                isWavAudio = radioButtonWav.IsChecked ?? false;
            };
            radioButtonMp3.Checked += (se, ev) =>
            {
                isWavAudio = !(radioButtonMp3.IsChecked ?? false);
            };


            Windows.StackPanel stackPanel = new Windows.StackPanel() { Orientation = Windows.Orientation.Horizontal };
            stackPanel.Children.Add(radioButtonWav);
            stackPanel.Children.Add(radioButtonMp3);

            var button = new Windows.Button() { Content = ButtonText + " To DB", Padding = new Thickness(5, 0, 5, 0), Margin = new Thickness(5), MinWidth = 100 };
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

        private DataTable GetVideoEventTableForAudio()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("videoevent_id", typeof(int));
            dataTable.Columns.Add("fk_videoevent_project", typeof(int));
            dataTable.Columns.Add("fk_videoevent_media", typeof(int));
            dataTable.Columns.Add("videoevent_track", typeof(int));
            dataTable.Columns.Add("videoevent_start", typeof(string));
            dataTable.Columns.Add("videoevent_duration", typeof(int));
            dataTable.Columns.Add("videoevent_end", typeof(int));
            dataTable.Columns.Add("videoevent_createdate", typeof(string));
            dataTable.Columns.Add("videoevent_modifydate", typeof(string));
            //optional column
            dataTable.Columns.Add("media", typeof(byte[])); // Media Column
            dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen
                                                                       // Since this table has Referential Integrity, so lets push one by one
            return dataTable;
        }

        private DataTable GetAudioTableForAudio()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("audio_id", typeof(int));
            dataTable.Columns.Add("audio_modifydate", typeof(string));
            dataTable.Columns.Add("audio_media", typeof(byte[])); // Media Column
            return dataTable;
        }



        private void SaveAudioToDatabase(object sender, RoutedEventArgs e)
        {
            try
            {
               var dataTable = GetVideoEventTableForAudio();
                var row = dataTable.NewRow();
                row["videoevent_id"] = audioSaveButtonText == "Save" ? -1 : selectedVideoEvent.videoevent_id;
                row["fk_videoevent_project"] = selectedProjectId;
                row["videoevent_track"] = 1;

                audioMinutetext = audioMinutetext ?? (audioSaveButtonText == "Save" ? "00" : selectedVideoEvent.videoevent_start.Split(':')[1].ToString());
                audioSecondtext = audioSecondtext ?? (audioSaveButtonText == "Save" ? "00" : selectedVideoEvent.videoevent_start.Split(':')[2].ToString());

                row["videoevent_start"] = $"00:{audioMinutetext}:{audioSecondtext}";
                row["videoevent_duration"] = 0;
                row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["fk_videoevent_screen"] = -1; // Not needed for this case
                var mediaId = 3;
                row["fk_videoevent_media"] = mediaId;
                //Fill Media in case image, video or audio is selected
                byte[] mediaByte;
                if (isWavAudio == true)
                    mediaByte = editor.GetAudioSelectionAsWav();
                else
                    mediaByte = editor.GetAudioSelectionAsMp3();
                row["media"] = mediaByte;
                
                dataTable.Rows.Add(row);
                //if (audioSaveButtonText == "Save")
                //{
                //    var inserted = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);
                //    if (inserted?.Count > 0 && inserted[0] > 0)
                //        MessageBox.Show("Audio Successfully saved to DB", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                //    popup.Close();
                //}
                //else
                //{
                //    var audioDataTable = GetAudioTableForAudio();
                //    var audioRow = audioDataTable.NewRow();
                //    audioRow["audio_id"] = selectedVideoEvent.audio_data[0]?.audio_id;
                //    audioRow["audio_media"] = mediaByte;
                //    audioRow["audio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //    audioDataTable.Rows.Add(audioRow);
                //    DataManagerSqlLite.UpdateRowsToVideoEvent(dataTable);
                //    DataManagerSqlLite.UpdateRowsToAudio(audioDataTable);
                //    MessageBox.Show("Audio Successfully updated to DB", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                //    popup.Close();
                //}
                //RefreshOrLoadComboBoxes();
                //TimelineUserConrol.LoadTimelineDataFromDb_Click();
                MessageBox.Show("Not saved to DB, Coming Soon", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                popup.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.VIDEOEVENT)
            {
                var data = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
                RefreshComboBoxes(cmbVideoEvent, data, "videoevent_id");
                selectedVideoEvent = selectedVideoEvent != null ? selectedVideoEvent : (data?.Count > 0 ? data[0] : null);
                cmbVideoEvent.SelectedItem = selectedVideoEvent;
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
            if (selectedVideoEvent != null)
            {
                selectedVideoEventId = ((CBVVideoEvent)cmbVideoEvent?.SelectedItem).videoevent_id;
                NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                ResetAudio();
            }
        }

        private void ResetAudio()
        {
            AudioUserConrol.LoadSelectedAudio(selectedVideoEvent);
            ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[3]).IsEnabled = true;

            //if (selectedVideoEvent?.fk_videoevent_media == 3)
            //{
            //    AudioUserConrol.LoadSelectedAudio(selectedVideoEvent);
            //    ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[3]).IsEnabled = true;
            //}
            //else
            //{
            //    //AudioUserConrol.LoadSelectedAudio(null);
            //    ((Windows.MenuItem)AudioUserConrol.ContextMenu.Items[3]).IsEnabled = false;

            //}
        }

        public void Dispose()
        {
            Console.WriteLine("The ManageTimeline_UserControl > dispose() function has been called and the resources have been released!");
        }
    }
}
