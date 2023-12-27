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
using ServerApiCall_UserControl.DTO.VideoEvent;
using VideoCreator.Auth;
using System.Threading.Tasks;
using NAudio.CoreAudioApi.Interfaces;
using VideoCreator.Helpers;
using System.Windows.Threading;
using System.Diagnostics.Contracts;
using System.Windows.Controls;
using System.Net;
using SixLabors.ImageSharp.PixelFormats;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using DebugVideoCreator.Models;
using System.IO;
using VideoCreator.MediaLibraryData;

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
        public event EventHandler closeTheEditWindow;
        private bool ReadOnly;
        private int RetryIntervalInSeconds = 300;
        private readonly DispatcherTimer dispatcherTimer;
        private readonly DispatcherTimer dispatcherTimerToCheckIfUnSyncDataPresent;
        private bool isBackgroundProcessRunning = false;

        public ManageTimeline_UserControl(int projectId, Int64 _selectedServerProjectId, AuthAPIViewModel _authApiViewModel)
        {
            InitializeComponent();
            selectedProjectId = projectId;
            selectedServerProjectId = _selectedServerProjectId;
            authApiViewModel = _authApiViewModel;
            var subjectText = "Selected Project Id - " + selectedProjectId;
            lblSelectedProjectId.Content = subjectText;
            //Task.Run(async () => { await checkIfProjectIsLocked(); });

            checkIfProjectIsLocked();


            // To Check if unsync data present
            dispatcherTimerToCheckIfUnSyncDataPresent = new DispatcherTimer();
            dispatcherTimerToCheckIfUnSyncDataPresent.Tick += new EventHandler(RunBackgroundProcessFrequently);
            dispatcherTimerToCheckIfUnSyncDataPresent.Interval = TimeSpan.FromSeconds(30);
            dispatcherTimerToCheckIfUnSyncDataPresent.Start();
            FrequentlyCheckOfflineData();

            // Logic to invoke the BG thread to send not saved data from local DATA to server and update the server Id.
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(RunBackgroundProcess);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(RetryIntervalInSeconds);
            dispatcherTimer.Start();
            loader.Visibility = Visibility.Hidden;
        }

        private void RunBackgroundProcessFrequently(object sender, EventArgs e)
        {
            FrequentlyCheckOfflineData();
        }

        private void FrequentlyCheckOfflineData()
        {
            var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectId, true);
            if (notSyncedData?.Count > 0)
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data - {notSyncedData?.Count} events";
                btnUploadNotSyncedData.Width = 210;
                btnUploadNotSyncedData.IsEnabled = true;
                btnDownloadServerData.IsEnabled = false;
            }
            else
            {
                btnUploadNotSyncedData.Content = $"Upload Not Synced Data";
                btnUploadNotSyncedData.Width = 160;
                btnUploadNotSyncedData.IsEnabled = false;
                btnDownloadServerData.IsEnabled = true;
            }
        }


        private async void RunBackgroundProcess(object sender, EventArgs e)
        {
            await PeriodicallyCheckOfflineDataAndSync();
            FrequentlyCheckOfflineData();
        }

        private async Task PeriodicallyCheckOfflineDataAndSync()
        {
            if (isBackgroundProcessRunning) return;
            isBackgroundProcessRunning = true;
            try
            {
                var notSyncedData = DataManagerSqlLite.GetNotSyncedVideoEvents(selectedProjectId, true);
                foreach (var notSyncedRow in notSyncedData)
                {
                    if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.IMAGE || notSyncedRow.fk_videoevent_media == (int)EnumMedia.VIDEO) // for image or video
                    {
                        await ProcessVideoSegmentDataRowByRowInBackground(notSyncedRow);

                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.AUDIO) // for Audio
                    {
                        // Coming Soon !!!
                    }
                    else if (notSyncedRow.fk_videoevent_media == (int)EnumMedia.FORM) // for DESIGN + IMAGE
                    {
                        // TBD
                    }
                }
            }
            catch (Exception) { }
            finally { isBackgroundProcessRunning = false; }

        }

        private async Task ProcessVideoSegmentDataRowByRowInBackground(CBVVideoEvent videoEvent)
        {
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerForVideoOrImageBackground(videoEvent, selectedServerProjectId, authApiViewModel);
            if (addedData != null)
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, addedData.videoevent.videoevent_id, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, addedData.videosegment.videosegment_id, authApiViewModel.GetError());
            }
            else
            {
                DataManagerSqlLite.UpdateVideoEventDataTableServerId(videoEvent.videoevent_id, videoEvent.videoevent_serverid, authApiViewModel.GetError());
                if (videoEvent?.videosegment_data?.Count > 0)
                    DataManagerSqlLite.UpdateVideoSegmentDataTableServerId(videoEvent.videosegment_data[0].videosegment_id, videoEvent.videosegment_data[0].videosegment_serverid, authApiViewModel.GetError());

            }

        }

        private void InitializeChildren()
        {
            popup = new PopupWindow();
            ResetAudioMenuOptions();
            RefreshOrLoadComboBoxes();

            //Timeline
            TimelineUserConrol.SetSelectedProjectId(selectedProjectId, authApiViewModel, ReadOnly);
            TimelineUserConrol.Visibility = Visibility.Visible;
            TimelineUserConrol.ContextMenu_AddVideoEvent_Clicked += TimelineUserConrol_ContextMenu_AddVideoEvent_Clicked;
            TimelineUserConrol.ContextMenu_AddVideoEvent_Success += TimelineUserConrol_ContextMenu_AddVideoEvent_Success;

            TimelineUserConrol.ContextMenu_AddImageEventUsingCBLibrary_Clicked += TimelineUserConrol_ContextMenu_AddImageEventFromCBLibrary_Clicked;
            TimelineUserConrol.ContextMenu_AddImageEventUsingCBLibrary_Success += TimelineUserConrol_ContextMenu_AddImageEventFromCBLibrary_Success;


            TimelineUserConrol.ContextMenu_AddCallout1_Clicked += TimelineUserConrol_ContextMenu_AddCallout1_Clicked;
            TimelineUserConrol.ContextMenu_AddCallout2_Clicked += TimelineUserConrol_ContextMenu_AddCallout2_Clicked;
            TimelineUserConrol.ContextMenu_Run_Clicked += TimelineUserConrol_ContextMenu_Run_Clicked;

            NotesUserConrol.InitializeNotes(selectedProjectId, selectedVideoEventId, ReadOnly);

            NotesUserConrol.Visibility = Visibility.Visible;


            // Reload Control
            //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
            AudioUserConrol.SetSelected(selectedProjectId, selectedVideoEventId, selectedVideoEvent, ReadOnly);
            ResetAudioContextMenu();
            NotesUserConrol.locAudioAddedEvent += NotesUserConrol_locAudioAddedEvent;
            NotesUserConrol.locAudioShowEvent += NotesUserConrol_locAudioShowEvent;
            NotesUserConrol.locAudioManageEvent += NotesUserConrol_locAudioManageEvent;

            NotesUserConrol.saveNotesEvent += NotesUserConrol_saveNotesEvent;
            NotesUserConrol.saveSingleNoteEvent += NotesUserConrol_saveSingleNoteEvent;
            NotesUserConrol.updateSingleNoteEvent += NotesUserConrol_updateSingleNoteEvent;
            NotesUserConrol.deleteSingleNoteEvent += NotesUserConrol_deleteSingleNoteEvent;
            // NotesUserConrol.HandleVideoEventSelectionChanged();
            //FSPClosed = new EventHandler(this.Parent, new EventArgs());
        }


        private void ResetAudioContextMenu()
        {
            if (ReadOnly)
            {
                var contextMenu = AudioUserConrol.ContextMenu as ContextMenu;
                for (int i = 0; i < contextMenu.Items.Count; i++)
                {
                    MenuItem item = contextMenu.Items[i] as MenuItem;
                    if (item != null)
                    {
                        if (item?.Name != null && item?.Name == "MenuItem_Run")
                        {
                            item.IsEnabled = true; //.Visibility = Visibility.Hidden
                        }
                        else
                            item.IsEnabled = false;
                    }
                }
            }
        }


        private async void checkIfProjectIsLocked()
        {
            var response = await authApiViewModel.GetLockStatus(selectedServerProjectId);
            if (response?.project_islocked == true && response?.permission_status == 1)
            {
                //MessageBox.Show($"Project with {selectedServerProjectId} is open for read-write as its locked by you - " + response.lockedby_username);
                ReadOnly = false;
                btnlock.IsEnabled = false;
                btnunlock.IsEnabled = true;
                InitializeChildren();
            }
            else
            {
                // MessageBox.Show($"Project is locked by some other user - '{response.lockedby_username}', every option is read only !! ", "Read Only Mode - Info", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReadOnly = true;
                btnlock.IsEnabled = true;
                btnunlock.IsEnabled = false;
                //closeTheEditWindow.Invoke(null, null);
                InitializeChildren();
            }

        }


        #region === Notes Event Handler event ==

        private async void NotesUserConrol_saveNotesEvent(object sender, DataTable datatable)
        {
            // Step 1. Save to server
            var notes = NotesEventHandlerHelper.GetNotesModelList(datatable);
            var savedNotes = await authApiViewModel.POSTNotes(selectedVideoEvent.videoevent_serverid, notes);
            if (savedNotes != null)
            {
                // Step 2. Now save the notes to local DB
                var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(savedNotes.Notes, selectedVideoEventId);
                DataManagerSqlLite.InsertRowsToNotes(notesDatatable);
            }
            NotesUserConrol.DisplayAllNotesForSelectedVideoEvent();
        }

        private async void NotesUserConrol_deleteSingleNoteEvent(object sender, CBVNotes notes)
        {
            var result = await authApiViewModel.DeleteNotesById(selectedVideoEvent.videoevent_serverid, notes.notes_serverid);
            if (result?.Status == "success")
            {
                DataManagerSqlLite.DeleteNotesById(notes.notes_id);
                NotesUserConrol.DisplayAllNotesForSelectedVideoEvent();
            }

        }


        private async void NotesUserConrol_saveSingleNoteEvent(object sender, DataTable datatable)
        {
            // Step 1. Save to server
            var notes = NotesEventHandlerHelper.GetNotesModelList(datatable);
            var savedNotes = await authApiViewModel.POSTNotes(selectedVideoEvent.videoevent_serverid, notes);

            // Step 2. Now save the notes to local DB
            var notesDatatable = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(savedNotes.Notes, selectedVideoEventId);
            DataManagerSqlLite.InsertRowsToNotes(notesDatatable);

            NotesUserConrol.DisplayAllNotesForSelectedVideoEvent();
        }

        private void NotesUserConrol_updateSingleNoteEvent(object sender, DataTable datatable)
        {
            throw new NotImplementedException();
        }

        private void NotesUserConrol_locAudioManageEvent(object sender, int notesId)
        {
            //var uc = new LocalVoice_UserControl(selectedVideoEventId, notesId);
            //var window = new Window
            //{
            //    Title = $"Manage Audio For {notesId} notes",
            //    Content = uc,
            //    SizeToContent = SizeToContent.WidthAndHeight,
            //    ResizeMode = ResizeMode.NoResize,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen
            //};
            //var result = window.ShowDialog();
            //NotesUserConrol.DisplayAllNotesForSelectedVideoEvent();
        }

        private void NotesUserConrol_locAudioShowEvent(object sender, EventArgs e)
        {
            //var uc = new LocalVoice_UserControl(selectedVideoEventId);
            //var window = new Window
            //{
            //    Title = "Generate Local Voice",
            //    Content = uc,
            //    SizeToContent = SizeToContent.WidthAndHeight,
            //    ResizeMode = ResizeMode.NoResize,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen
            //};
            //var result = window.ShowDialog();
            //NotesUserConrol.DisplayAllNotesForSelectedVideoEvent();
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


        #endregion

        #region == Video Event Context Menu ==

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

        private void TimelineUserConrol_ContextMenu_AddVideoEvent_Clicked(object sender, EventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            var screenRecordingUserControl = new ScreenRecordingWindow(1, selectedProjectId);
            screenRecordingUserControl.BtnSaveClickedEvent += ScreenRecordingUserControl_BtnSaveClickedEvent; //+= ScreenRecorderUserConrol_ContextMenu_SaveEvent_Clicked;
            var screenRecorderWindow = new System.Windows.Window
            {
                Title = "Screen Recorder",
                Content = screenRecordingUserControl,
                ResizeMode = ResizeMode.CanResize,
                WindowState = WindowState.Maximized,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            LoaderHelper.ShowLoader(screenRecorderWindow, loader);
            var result = screenRecorderWindow.ShowDialog();
            if (result.HasValue)
            {
                TimelineUserConrol_ContextMenu_AddVideoEvent_Success(this, new EventArgs());
            }
            LoaderHelper.HideLoader(this, loader);
        }

        private async void ScreenRecordingUserControl_BtnSaveClickedEvent(DataTable dataTable)
        {
            // We need to insert the Data to server here and once it is success, then to local DB
            foreach (DataRow row in dataTable.Rows)
                await ProcessVideoSegmentDataRowByRow(row);
        }

        private async Task ProcessVideoSegmentDataRowByRow(DataRow row)
        {
            var addedData = await MediaEventHandlerHelper.PostVideoEventToServerForVideoOrImage(row, selectedServerProjectId, authApiViewModel);
            if (addedData == null)
            {
                var confirmation = MessageBox.Show($"Something went wrong, Do you want to retry !! " +
                    $"{Environment.NewLine}{Environment.NewLine}Press 'Yes' to retry now, " +
                    $"{Environment.NewLine}'No' for retry later at an interval of {RetryIntervalInSeconds / 60} minutes and " +
                    $"{Environment.NewLine}'Cancel' to discard", "Failure", MessageBoxButton.YesNoCancel, MessageBoxImage.Error);
                if (confirmation == MessageBoxResult.Yes)
                    await ProcessVideoSegmentDataRowByRow(row);
                else if (confirmation == MessageBoxResult.No)
                    FailureFlowForSaveImageorVideo(row);
                else
                    return;
            }
            else
            {
                SuccessFlowForSaveImageorVideo(row, addedData);
            }
        }


        private void SuccessFlowForSaveImageorVideo(DataRow row, VideoEventResponseModel addedData)
        {
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImage(addedData, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var videoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImage(blob, videoEventId, addedData.videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, addedData.videoevent.videoevent_id);
                if (insertedVideoSegmentId > 0)
                {
                    RefreshOrLoadComboBoxes();
                    TimelineUserConrol.ClearTimeline();
                    TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
                    //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
                    //NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                    MessageBox.Show($"videosegment record added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void FailureFlowForSaveImageorVideo(DataRow row)
        {
            // Save the record locally with server Id = temp and issynced = false
            var localServerVideoEventId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
            var dt = MediaEventHandlerHelper.GetVideoEventDataTableForVideoOrImageLocally(row, localServerVideoEventId, selectedProjectId);
            var insertedVideoEventIds = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            if (insertedVideoEventIds?.Count > 0)
            {
                var videoEventId = insertedVideoEventIds[0];
                var blob = row["media"] as byte[];
                var localServerVideoSegmentId = Convert.ToInt64(DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImageLocally(blob, videoEventId, localServerVideoSegmentId);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, videoEventId);
                if (insertedVideoSegmentId > 0)
                {
                    RefreshOrLoadComboBoxes();
                    TimelineUserConrol.ClearTimeline();
                    TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
                    //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
                    //NotesUserConrol.SetSelectedProjectId(selectedProjectId, selectedVideoEventId);
                    MessageBox.Show($"videosegment record saved locally, background process will try to sync at an interval of {RetryIntervalInSeconds / 60} min.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void TimelineUserConrol_ContextMenu_AddVideoEvent_Success(object sender, EventArgs e)
        {
            RefreshOrLoadComboBoxes();
            TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
            //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            NotesUserConrol.InitializeNotes(selectedProjectId, selectedVideoEventId);
        }
        #endregion


        #region == ContextMenu > AddImageEvent > Using CB Library ==
        private void TimelineUserConrol_ContextMenu_AddImageEventFromCBLibrary_Clicked(object sender, EventArgs e)
        {
            var mediaLibraryUserControl = new MediaLibrary_UserControl(2, selectedProjectId, selectedServerProjectId, authApiViewModel);
            mediaLibraryUserControl.BtnUseAndSaveClickedEvent += MediaLibraryUserControl_BtnUseAndSaveClickedEvent;
            var window = new Window
            {
                Title = "Media Library",
                Content = mediaLibraryUserControl,
                WindowState = WindowState.Normal,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            LoaderHelper.ShowLoader(this, loader);
            LoaderHelper.ShowLoader(window, loader);
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                TimelineUserConrol_ContextMenu_AddImageEventFromCBLibrary_Success(this, new EventArgs());
                mediaLibraryUserControl.Dispose();
            }

        }

        private async void MediaLibraryUserControl_BtnUseAndSaveClickedEvent(DataTable dataTable)
        {
            // We need to insert the Data to server here and once it is success, then to local DB
            foreach (DataRow row in dataTable.Rows)
                await ProcessVideoSegmentDataRowByRow(row);
            LoaderHelper.HideLoader(this, loader);
        }

        private void TimelineUserConrol_ContextMenu_AddImageEventFromCBLibrary_Success(object sender, EventArgs e)
        {
            RefreshOrLoadComboBoxes();
            TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
            //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
            NotesUserConrol.InitializeNotes(selectedProjectId, selectedVideoEventId);
        }


        #endregion

        #region == Design/Form Context Menu Option ==

        private async Task HandleCalloutLogic(EnumTrack track, string imagePath)
        {
            var isSuccess = await CallOutHandlerHelper.CallOut("Designer", selectedProjectId, selectedServerProjectId, authApiViewModel, track, this, loader, imagePath);
            if (isSuccess)
            {
                RefreshOrLoadComboBoxes();
                TimelineUserConrol.LoadVideoEventsFromDb(selectedProjectId);
                //FSPUserConrol.SetSelectedProjectIdAndReset(selectedProjectId);
                NotesUserConrol.InitializeNotes(selectedProjectId, selectedVideoEventId);
                LoaderHelper.HideLoader(this, loader);
                MessageBox.Show($"videosegment record for image added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            LoaderHelper.HideLoader(this, loader);

            //else
            //    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void TimelineUserConrol_ContextMenu_AddCallout1_Clicked(object sender, CalloutEvent calloutEvent)
        {
            if (calloutEvent.timelineVideoEvent != null && calloutEvent.timeAtTheMoment != "00:00:00")
            {
                var convertedImage = await CallOutHandlerHelper.Preprocess(calloutEvent);
                await HandleCalloutLogic(EnumTrack.CALLOUT1, convertedImage);
            }
            else
                await HandleCalloutLogic(EnumTrack.CALLOUT1, null);
        }

        private async void TimelineUserConrol_ContextMenu_AddCallout2_Clicked(object sender, CalloutEvent calloutEvent)
        {
            if (calloutEvent.timelineVideoEvent != null && calloutEvent.timeAtTheMoment != "00:00:00")
            {
                var convertedImage = await CallOutHandlerHelper.Preprocess(calloutEvent);
                await HandleCalloutLogic(EnumTrack.CALLOUT2, convertedImage);
            }
            else
                await HandleCalloutLogic(EnumTrack.CALLOUT2, null);
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
                //TimelineUserConrol.LoadVideoEventsFromDb();
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
            dataTable.Columns.Add("videoevent_end", typeof(string));
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

        private void CancelSaveAudio(object sender, RoutedEventArgs e)
        {
            popup.Close();
        }


        private int SaveVoiceAverageToDatabase(string average)
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

        private bool UpdateVoiceAverageToDatabase(string average)
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
                NotesUserConrol.HandleVideoEventSelectionChanged(selectedVideoEventId);
                ResetAudio();
            }
        }



        private async void btnReleaseLock_Click(object sender, RoutedEventArgs e)
        {
            var response = await authApiViewModel.LockProject(selectedServerProjectId, false);
            if (response?.Status == "success")
            {
                MessageBox.Show($"Lock on Project with Id - {selectedServerProjectId} is released successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                ReadOnly = true;
                btnlock.IsEnabled = true;
                btnunlock.IsEnabled = false;
                InitializeChildren();
            }

        }

        private async void btnLockForEdit_Click(object sender, RoutedEventArgs e)
        {
            var response = await authApiViewModel.LockProject(selectedServerProjectId, true);
            if (response?.Status == "success")
            {
                MessageBox.Show($"Project with Id - {selectedServerProjectId} is locked successfully, waiting for sync", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                ReadOnly = false;
                btnlock.IsEnabled = false;
                btnunlock.IsEnabled = true;
                await SyncServerDataToLocalDB();
                InitializeChildren();
            }
            else
            {
                MessageBox.Show($"{response?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReadOnly = true;
                btnlock.IsEnabled = true;
                btnunlock.IsEnabled = false;
                //closeTheEditWindow.Invoke(null, null);
                InitializeChildren();
            }
        }

        private async void btnUploadNotSyncedData_Click(object sender, RoutedEventArgs e)
        {
            await PeriodicallyCheckOfflineDataAndSync();
        }


        private async void btnDownloadServer_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show($"This will overwrite all local changes and server data will be synchronised to local DB", "Please confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                LoaderHelper.ShowLoader(this, loader);
                await SyncServerDataToLocalDB();

            }
            InitializeChildren();
            LoaderHelper.HideLoader(this, loader);
            MessageBox.Show($"Sync successfull !!!", "Success", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async Task SyncServerDataToLocalDB()
        {
            // Step1: Lets clear the local DB

            DataManagerSqlLite.DeleteAllVideoEventsByProjectId(selectedProjectId, true);


            //Step2: Lets fetch the data from 
            var serverVideoEventData = await authApiViewModel.GetAllVideoEventsbyProjectId(selectedServerProjectId);

            if (serverVideoEventData?.Data != null)
            {
                foreach (var videoEvent in serverVideoEventData?.Data)
                {
                    var localVideoEventId = SaveVideoEvent(videoEvent);
                    if (videoEvent?.design?.Count > 0)
                        SaveDesign(localVideoEventId, videoEvent?.design);
                    if (videoEvent?.notes?.Count > 0)
                        SaveNotes(localVideoEventId, videoEvent?.notes);
                    if (videoEvent?.videosegment != null)
                        await SaveVideoSegment(localVideoEventId, videoEvent?.videosegment);
                }
            }
        }


        private int SaveVideoEvent(AllVideoEventResponseModel videoevent)
        {
            var dt = MediaEventHandlerHelper.GetVideoEventTableWithData(selectedProjectId, videoevent);
            var result = DataManagerSqlLite.InsertRowsToVideoEvent(dt, false);
            return result?.Count > 0 ? result[0] : -1;
        }

        private void SaveDesign(int localVideoEventId, List<DesignModel> allDesigns)
        {
            var dtDesign = DesignEventHandlerHelper.GetDesignDataTable(allDesigns, localVideoEventId);
            DataManagerSqlLite.InsertRowsToDesign(dtDesign);
        }

        private async Task SaveVideoSegment(int localVideoEventId, VideoSegmentModel videosegment)
        {
            var downloadUrl = videosegment.videosegment_download_url;

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                byte[] bytes = await authApiViewModel.GetSecuredFileByteArray(downloadUrl);
                var dtVideoSegment = MediaEventHandlerHelper.GetVideoSegmentDataTableForVideoOrImage(bytes, localVideoEventId, videosegment);
                var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(dtVideoSegment, localVideoEventId);

            }
        }

        private void SaveNotes(int localVideoEventId, List<NotesModel> notes)
        {
            var dtNotes = NotesEventHandlerHelper.GetNotesDataTableForLocalDB(notes, localVideoEventId);
            DataManagerSqlLite.InsertRowsToNotes(dtNotes);
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
