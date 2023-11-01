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
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
using VideoCreator.Auth;
using System.Threading.Tasks;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for ManageTimeline.xaml
    /// </summary>
    public partial class ManageTimeline_UserControl : UserControl, IDisposable
    {
        private int selectedProjectId;
        private Int64 selectedServerProjectId;
        private int selectedVideoEventId = -1;
        private int voiceAvgCount = -1;
        private string audioMinutetext, audioSecondtext, audioSaveButtonText;
        private bool isWavAudio = true;
        private CBVVideoEvent selectedVideoEvent;
        private PopupWindow popup;
        private AudioEditor editor;
        private readonly AuthAPIViewModel authApiViewModel;

        public ManageTimeline_UserControl(int projectId, Int64 _selectedServerProjectId, AuthAPIViewModel _authApiViewModel)
        {
            InitializeComponent();
            selectedProjectId = projectId;
            selectedServerProjectId = _selectedServerProjectId;
            authApiViewModel = _authApiViewModel;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;

            popup = new PopupWindow();
            ResetAudioMenuOptions();
            RefreshOrLoadComboBoxes();



            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectId);
            TimelineUserConrol.Visibility = Visibility.Visible;
            TimelineUserConrol.ContextMenu_AddVideoEvent_Success += TimelineUserConrol_ContextMenu_AddVideoEvent_Success;
            TimelineUserConrol.ContextMenu_AddForm_Clicked += TimelineUserConrol_ContextMenu_AddForm_Clicked;
            TimelineUserConrol.ContextMenu_Run_Clicked += TimelineUserConrol_ContextMenu_Run_Clicked;

            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
            NotesUserConrol.Visibility = Visibility.Visible;


            // Reload Control
            FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            TimelineUserConrol.LoadVideoEventsFromDb();
            AudioUserConrol.SetSelected(selectedProjectId, selectedVideoEventId, selectedVideoEvent);
            NotesUserConrol.locAudioAddedEvent += NotesUserConrol_locAudioAddedEvent;
            NotesUserConrol.locAudioShowEvent += NotesUserConrol_locAudioShowEvent;
            NotesUserConrol.locAudioManageEvent += NotesUserConrol_locAudioManageEvent;


            //FSPClosed = new EventHandler(this.Parent, new EventArgs());
        }

        private void NotesUserConrol_locAudioManageEvent(object sender, int notesId)
        {
            var uc = new LocalVoice_UserControl(selectedVideoEventId, notesId);
            var window = new Window
            {
                Title = $"Manage Audio For {notesId} notes",
                Content = uc,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            NotesUserConrol.DisplayAllNotes();
        }

        private void NotesUserConrol_locAudioShowEvent(object sender, EventArgs e)
        {
            var uc = new LocalVoice_UserControl(selectedVideoEventId);
            var window = new Window
            {
                Title = "Generate Local Voice",
                Content = uc,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            NotesUserConrol.DisplayAllNotes();
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
            var fsp_uc = new FullScreen_UserControl(true, true);
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
            TimelineUserConrol.LoadVideoEventsFromDb();
            FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
        }

        private List<DesignModelPost> GetDesignModelList(DataTable dtDesign)
        {
            var data = new List<DesignModelPost>();
            foreach (DataRow design in dtDesign.Rows)
            {
                var designModel = new DesignModelPost();
                designModel.fk_design_screen = Convert.ToInt16(design["fk_design_screen"]);
                designModel.design_xml = Convert.ToString(design["design_xml"]);
                data.Add(designModel);
            }
            return data;
        }

        private async Task<VideoEventResponseModel> PostVideoEventToServerForDesign(DataTable dtDesign)
        {
            var objToSync = new VideoEventModel();
            objToSync.fk_videoevent_media = (int)EnumMedia.FORM;
            objToSync.videoevent_track = 1; // TBD
            objToSync.videoevent_start = "00:00:00"; // TBD
            objToSync.videoevent_duration = 10;
            objToSync.videoevent_serverid = selectedServerProjectId;
            objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            objToSync.design.AddRange(GetDesignModelList(dtDesign));
            var result = await authApiViewModel.POSTVideoEvent(objToSync);
            return result;
        }

        private List<DesignModel> GetDesignModelListForPut(int videoeventId, CBVVideoEvent videoevent)
        {
            var data = new List<DesignModel>();
            foreach (var design in videoevent.design_data)
            {
                var designModel = new DesignModel();
                designModel.fk_design_screen = design.fk_design_screen;
                designModel.design_xml = design.design_xml;
                designModel.fk_design_videoevent = (int)videoevent.videoevent_serverid;
                designModel.design_id = (int)design.design_serverid;
                data.Add(designModel);
            }
            return data;
        }

        private async Task<VideoEventResponseModel> PutVideoEventToServerForDesign_Step2(int videoeventId, byte[] blob)
        {
            var dataToSync = DataManagerSqlLite.GetVideoEventbyId(videoeventId, true);
            var objToSync = new VideoEventModel();
            if (dataToSync != null && dataToSync.Count > 0)
            {
                var item = dataToSync[0];

                objToSync.fk_videoevent_media = item.fk_videoevent_media;
                objToSync.videoevent_track = item.videoevent_track;
                objToSync.videoevent_start = item.videoevent_start;
                objToSync.videoevent_duration = item.videoevent_duration;
                objToSync.videoevent_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                objToSync.videoevent_serverid = item.videoevent_serverid;
                objToSync.fk_videoevent_project = (int)selectedServerProjectId;
                if (objToSync.fk_videoevent_media == (int)EnumMedia.FORM) //Id-4 1:1
                {
                    // We need to fill image and design children
                    if (item.design_data?.Count > 0)
                        objToSync.design.AddRange(GetDesignModelListForPut(videoeventId, item));
                    if (item.notes_data?.Count > 0)
                        objToSync.notes.AddRange(GetNotesModelList(item));
                }
                var result = await authApiViewModel.PutVideoEvent((int)objToSync.fk_videoevent_project, objToSync, blob);
                return result;
            }
            return null;
        }

        private async void TimelineUserConrol_ContextMenu_AddForm_Clicked(object sender, EventArgs e)
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
                        // We need to insert the Data to server here and once it is success, then to local DB
                        var addedData = await PostVideoEventToServerForDesign(designerUserControl.dataTableAdd);
                        // Now we have to save the data locally

                        var dt = GetVideoEventDataTableForDesign(addedData);
                        var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
                        if (insertedVideoEventIds?.Count > 0)
                        {
                            var videoEventId = insertedVideoEventIds[0];
                            var dtDesign = GetDesignDataTable(addedData.design, videoEventId);
                            DataManagerSqlLite.InsertRowsToDesign(dtDesign);
                            var totalRows = dtDesign.Rows.Count;
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

        private async void ContextMenuAddFormEventDataClickEvent_Step2(int videoeventId)
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
                    var addedData = await PutVideoEventToServerForDesign_Step2(videoeventId, designImagerUserControl.dtVideoSegment.Rows[0]["videosegment_media"] as byte[]);
                    //var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(designImagerUserControl.dtVideoSegment);
                    //if (insertedVideoSegmentId > 0)
                    //{
                    //    RefreshOrLoadComboBoxes();
                    //    TimelineUserConrol.LoadVideoEventsFromDb();
                    //    FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
                    //    NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                    //    MessageBox.Show($"videosegment record for image added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    //}
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private DataTable GetVideoEventDataTableForDesign(VideoEventResponseModel addedData)
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
            dtVideoEvent.Columns.Add("videoevent_isdeleted", typeof(bool));
            dtVideoEvent.Columns.Add("videoevent_issynced", typeof(bool));
            dtVideoEvent.Columns.Add("videoevent_serverid", typeof(Int64));
            dtVideoEvent.Columns.Add("videoevent_syncerror", typeof(string));

            var row = dtVideoEvent.NewRow();
            row["videoevent_id"] = -1;
            row["fk_videoevent_project"] = selectedProjectId;
            row["videoevent_start"] = addedData.videoevent.videoevent_start;
            row["videoevent_track"] = addedData.videoevent.videoevent_track;
            row["videoevent_duration"] = addedData.videoevent.videoevent_duration;
            row["fk_videoevent_media"] = addedData.videoevent.fk_videoevent_media;
            row["videoevent_createdate"] = addedData.videoevent.videoevent_createdate;
            row["videoevent_modifydate"] = addedData.videoevent.videoevent_modifydate;
            row["videoevent_isdeleted"] = false;
            row["videoevent_issynced"] = true;
            row["videoevent_serverid"] = addedData.videoevent.videoevent_id;
            row["videoevent_syncerror"] = "";
            dtVideoEvent.Rows.Add(row);
            return dtVideoEvent;
        }

        private DataTable GetDesignDataTable(List<DesignModel> alldesigns, int localVideoEventId)
        {
            var dt = new DataTable();
            dt.Columns.Add("design_id", typeof(int));
            dt.Columns.Add("fk_design_screen", typeof(int));
            dt.Columns.Add("fk_design_background", typeof(int));
            dt.Columns.Add("fk_design_videoevent", typeof(int));
            dt.Columns.Add("design_xml", typeof(string));
            dt.Columns.Add("design_createdate", typeof(string));
            dt.Columns.Add("design_modifydate", typeof(string));
            dt.Columns.Add("design_isdeleted", typeof(bool));
            dt.Columns.Add("design_issynced", typeof(bool));
            dt.Columns.Add("design_serverid", typeof(Int64));
            dt.Columns.Add("design_syncerror", typeof(string));

            foreach (var post in alldesigns)
            {
                var row = dt.NewRow();
                row["design_id"] = -1;
                row["fk_design_screen"] = post.fk_design_screen;
                row["fk_design_background"] = 1; // TBD
                row["fk_design_videoevent"] = localVideoEventId;
                row["design_xml"] = post.design_xml;

                row["design_createdate"] = post.design_createdate;
                row["design_modifydate"] = post.design_modifydate;
                row["design_isdeleted"] = false;
                row["design_issynced"] = true;
                row["design_serverid"] = post.design_id;
                row["design_syncerror"] = "";
                dt.Rows.Add(row);
            }

            return dt;
        }

        //private DataTable GetVideoSegmentDataTableForDesign(DataTable dtVideoSegment)
        //{

        //    var dt = new DataTable();
        //    dt.Columns.Add("videosegment_id", typeof(int));
        //    dt.Columns.Add("videosegment_media", typeof(byte[]));
        //    dt.Columns.Add("videosegment_createdate", typeof(string));
        //    dt.Columns.Add("videosegment_modifydate", typeof(string));

        //    dt.Columns.Add("videosegment_isdeleted", typeof(bool));
        //    dt.Columns.Add("videosegment_issynced", typeof(bool));
        //    dt.Columns.Add("videosegment_serverid", typeof(Int64));
        //    dt.Columns.Add("videosegment_syncerror", typeof(string));

        //    foreach (var row in dtVideoSegment.Rows)
        //    {
        //        var row = dt.NewRow();
        //        row["videosegment_id"] = -1;
        //        row["videosegment_media"] = post["fk_design_screen"];


        //        row["videosegment_createdate"] = rowvid;
        //        row["videosegment_modifydate"] = row.design_modifydate;
        //        row["videosegment_isdeleted"] = false;
        //        row["videosegment_issynced"] = true;
        //        row["videosegment_serverid"] = row.video;
        //        row["videosegment_syncerror"] = "";
        //        dt.Rows.Add(row);
        //    }

        //    return dt;
        //}



        #endregion

        #region == Audio Context Menu ==

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var window = AudioUserConrol.GetCreateEventWindow(selectedProjectId);

            var result = window.ShowDialog();
            if (result.HasValue)
            {
                RefreshOrLoadComboBoxes();
                TimelineUserConrol.LoadVideoEventsFromDb();
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

        private async void btnSync_Click(object sender, RoutedEventArgs e)
        {
            var dataToSync = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
            if (dataToSync != null && dataToSync.Count > 0)
            {
                foreach (var item in dataToSync)
                {
                    var objToSync = new VideoEventModel();
                    objToSync.fk_videoevent_media = item.fk_videoevent_media;
                    objToSync.videoevent_track = item.videoevent_track;
                    objToSync.videoevent_start = item.videoevent_start;
                    objToSync.videoevent_duration = item.videoevent_duration;
                    objToSync.fk_videoevent_project = selectedProjectId;
                    if (objToSync.fk_videoevent_media == (int)EnumMedia.FORM) //Id-4 1:1
                    {
                        // We need to fill image and design children
                        if (item.design_data?.Count > 0)
                            objToSync.design.AddRange(GetDesignModelList(item));
                        if (item.videosegment_data?.Count > 0)
                            objToSync.videosegment_media_bytes = item.videosegment_data[0].videosegment_media;
                        if (item.notes_data?.Count > 0)
                            objToSync.notes.AddRange(GetNotesModelList(item));
                    }
                    else if (objToSync.fk_videoevent_media == (int)EnumMedia.IMAGE) //Id-1 1:1
                    {
                        // We need to fill images
                        if (item.videosegment_data?.Count > 0)
                            objToSync.videosegment_media_bytes = item.videosegment_data[0].videosegment_media;
                        if (item.notes_data?.Count > 0)
                            objToSync.notes.AddRange(GetNotesModelList(item));
                    }
                    else if (objToSync.fk_videoevent_media == (int)EnumMedia.VIDEO) //Id-2 1:1
                    {
                        // We need to fill videos
                        if (item.videosegment_data?.Count > 0)
                            objToSync.videosegment_media_bytes = item.videosegment_data[0].videosegment_media;
                        if (item.notes_data?.Count > 0)
                            objToSync.notes.AddRange(GetNotesModelList(item));
                    }
                    else if (objToSync.fk_videoevent_media == (int)EnumMedia.AUDIO) //Id-3 1:1
                    {
                        // TBD
                    }
                    await authApiViewModel.POSTVideoEvent(objToSync);
                    break;
                }
            }

        }

        private List<DesignModelPost> GetDesignModelList(CBVVideoEvent item)
        {
            var data = new List<DesignModelPost>();
            foreach (var design in item.design_data)
            {
                var designModel = new DesignModelPost();
                designModel.fk_design_screen = design.fk_design_screen;
                designModel.design_xml = design.design_xml;
                data.Add(designModel);
            }
            return data;
        }

        private List<NotesModelPost> GetNotesModelList(CBVVideoEvent item)
        {
            var data = new List<NotesModelPost>();
            foreach (var note in item.notes_data)
            {
                var notesModel = new NotesModelPost();
                notesModel.notes_line = note.notes_line;
                notesModel.notes_wordcount = note.notes_wordcount.ToString();
                notesModel.notes_index = note.notes_index.ToString();
                data.Add(notesModel);
            }
            return data;
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
