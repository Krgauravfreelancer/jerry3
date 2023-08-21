using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using DebugVideoCreator.Helpers;


namespace DebugVideoCreator
{
    public partial class MainWindow : Window, IDisposable
    {
        private bool IsSetUp = false;
        private List<CBVVoiceTimer> voiceAverageData;
        private int voiceAverageIndex = 0;
        private Recorder_Speed objRecord;
        string audioFilename;
        int totalWords = 0;
        double totalTime = 0.1;

        public MainWindow()
        {
            InitializeComponent();
            voiceAverageData = new List<CBVVoiceTimer>();

        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsSetUp)
                {
                    InitialiseDbHelper.InitializeDatabase();
                    IsSetUp = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            SetWelcomeMessage();
            GetReadingText();
            txtAverageNote.Text = voiceAverageData[voiceAverageIndex].voicetimer_line;
            // LoadProjectDataGrid();

        }

        #region == Events ==

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetWelcomeMessage()
        {
            var encryptedUserId = TestRegisteryHelper.GetUsername();
            if (string.IsNullOrEmpty(encryptedUserId))
                this.Title = "Video Creator, Not Logged in - Username not present in registry !!!";
            else
            {
                var userId = TestEncryptionHelper.DecryptString(TestEncryptionHelper.SecuredKey, encryptedUserId);
                this.Title = $"Video Creator, Logged in as - {userId}";
            }
        }

        private void BtnManageTimeline_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one recrod from Grid", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int selectedProjectId;
            if ((bool)rbPending.IsChecked)
                selectedProjectId = ((CBVPendingProjectList)datagrid.SelectedItem)?.project_id ?? 0;
            else
                selectedProjectId = ((CBVWIPOrArchivedProjectList)datagrid.SelectedItem)?.project_id ?? 0;

            var manageTimeline_UserControl = new ManageTimeline_UserControl(selectedProjectId);

            var window = new Window
            {
                Title = "Manage Timeline",
                Content = manageTimeline_UserControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                RenderSize = manageTimeline_UserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                datagrid.SelectedItem = null;
                manageTimeline_UserControl.Dispose();
            }

        }

        private void BtnManageAudio_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnManageVideo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnDownloadProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnRecordAudio_Click(object sender, RoutedEventArgs e)
        {
            btnNext.IsEnabled = false;
            btnRecord.IsEnabled = false;
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = true;
            btnRecord.Content = "Recording ...";
            audioFilename = "\\_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".wav";
            objRecord = new Recorder_Speed(0, Directory.GetCurrentDirectory() + "\\VoiceRecordings\\", audioFilename);
            objRecord.StartRecording(sender, e);
        }

        private void BtnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            btnNext.IsEnabled = true;
            btnRecord.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnRecord.Content = "Record";
            objRecord.RecordEnd(sender, e);
            totalWords += voiceAverageData[voiceAverageIndex].voicetimer_wordcount;
            totalTime += objRecord.GetDuration();
            MessageBox.Show($@"Total {voiceAverageData[voiceAverageIndex].voicetimer_wordcount} words in time {objRecord.GetDuration()} seconds !!", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            txtResult.Text = Math.Round(totalWords / totalTime, 2).ToString();
            btnPlay.IsEnabled = true;
        }

        private void BtnPlayRecording_Click(object sender, RoutedEventArgs e)
        {
            if (audioFilename.Length >= 0)
                objRecord.RecordPlay(sender, e);
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (voiceAverageIndex < voiceAverageData.Count - 1)
            {
                voiceAverageIndex++;
                txtAverageNote.Text = voiceAverageData[voiceAverageIndex].voicetimer_line;
            }
            else
            {
                btnNext.IsEnabled = false;
                return;
            }
        }




        #endregion == Events ==

        private void GetReadingText()
        {
            voiceAverageData = DataManagerSqlLite.GetVoiceTimers();

        }

        private void LoadProjectDataGrid()
        {
            var data = DataManagerSqlLite.GetProjects(true, true);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
            SetWelcomeMessage();
        }

        private void rbPending_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetPendingProjectList();
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }
        private void rbWIP_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetWIPOrArchivedProjectList(false, true);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }
        private void rbArchived_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetWIPOrArchivedProjectList(true, false);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public void Dispose()
        {
            Console.WriteLine("The dispose() function has been called and the resources have been released!");
        }
    }
}