using ScreenRecorderLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace Sample_App_ScreenRecording
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch stopwatch;
        string videoPath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (stopwatch.IsRunning)
            {
                TimeSpan ts = stopwatch.Elapsed;
                var currentTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                stopwatchText.Text = currentTime;
            }
        }

        Recorder _rec;
        void CreateRecording()
        {
            stopwatch = Stopwatch.StartNew();
            dt.Start();


            var currentDirectory = Directory.GetCurrentDirectory();
            videoPath = Path.Combine(currentDirectory, "ScreenRecordings\\Recordings\\Recording_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".mp4");
            _rec = Recorder.CreateRecorder();
            _rec.OnRecordingComplete += Rec_OnRecordingComplete;
            _rec.OnRecordingFailed += Rec_OnRecordingFailed;
            _rec.OnStatusChanged += Rec_OnStatusChanged;
            _rec.Record(videoPath);
           
        }
        void EndRecording()
        {
            _rec.Stop();
        }
        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            //Get the file path if recorded to a file
            string path = e.FilePath;
        }
        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            string error = e.Error;
        }
        private void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            RecorderStatus status = e.Status;
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (btnRecord.Content.ToString() == "Record")
            {
                // Start recording
                btnRecord.Content = "Stop";
                txtPath.Text = "";
                CreateRecording();
            }
            else
            {
                btnRecord.Content = "Record";
                stopwatch.Stop();
                dt.Stop();
                EndRecording();
                txtPath.Text = videoPath;
            }

        }

        private void btnEndRecord_Click(object sender, RoutedEventArgs e)
        {
            
        }
        
    }
}
