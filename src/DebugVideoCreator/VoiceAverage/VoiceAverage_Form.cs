using VideoCreator.VoiceAverage;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace VideoCreator
{
    public partial class VoiceAverage_Form : Form
    {
        private List<CBVVoiceTimer> voiceAverageData;
        private VoiceAverageHelper objRecord;
        private readonly DispatcherTimer dt;
        private Stopwatch stopwatch;
        private int totalWords = 0;
        private double totalTime = 0.0;

        public MessageBoxResult response;
        public string newAverage;
        public string audioFilename;
        
        
        public VoiceAverage_Form()
        {
            InitializeComponent();
            voiceAverageData = new List<CBVVoiceTimer>();
            PopulateReadingText();
            dt = new DispatcherTimer();
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
                btnRecord.Text = "Recording ..." + currentTime;
            }
        }

        private void PopulateReadingText()
        {
            voiceAverageData = DataManagerSqlLite.GetVoiceTimers();
            int i = 0;
            foreach (var voiceTimer in voiceAverageData)
            {
                var lbl = new System.Windows.Forms.Label();
                lbl.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                lbl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(129)))), ((int)(((byte)(204)))));
                lbl.Location = new System.Drawing.Point(10, 100 + 70 * i);
                lbl.MaximumSize = new System.Drawing.Size(750, 0);
                lbl.Size = new System.Drawing.Size(750, 70);
                lbl.TabIndex = i;
                lbl.Text = $"{i + 1})    " + voiceTimer.voicetimer_line;
                lbl.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                this.Controls.Add(lbl);
                i++;
            }
        }

        private void voiceAverageForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (audioFilename.Length >= 0)
            {
               response = System.Windows.MessageBox.Show("Do you want to save voice average? ", "Voice Average Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
        }

        #region == Click Events ==

        private async void btnPlayRecording_Click(object sender, EventArgs e)
        {
            if (btnPlayRecording.Text == "Play Recording")
            {
                if (audioFilename.Length >= 0)
                {
                    btnPlayRecording.Text = "Stop Recording"; 
                    await Task.Run(() => {
                        objRecord.RecordPlay(sender, e);
                    });
                    
                    //Thread.Sleep(Convert.ToInt32((objRecord.GetDuration() * 1000) + 1000));
                    //btnPlayRecording.Text = "Play Recording";
                }
            }
            else
            {
                objRecord.RecordStop(sender, e);
                btnPlayRecording.Text = "Play Recording";
            }

        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            btnRecord.Enabled = false;
            btnPlayRecording.Visible = false;
            btnStop.Enabled = true;
            btnRecord.Text = "Recording ...";
            stopwatch = Stopwatch.StartNew();
            dt.Start();
            audioFilename = "\\_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".wav";
            objRecord = new VoiceAverageHelper(0, Directory.GetCurrentDirectory() + "\\VoiceRecordings\\", audioFilename);
            objRecord.StartRecording(sender, e);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnRecord.Enabled = true;
            btnPlayRecording.Visible = true;
            btnStop.Enabled = false;
            btnRecord.Text = "Record";
            objRecord.RecordEnd(sender, e);
            stopwatch.Stop();
            dt.Stop();

            
            CalcAverage();
            SaveToDatabase();
        }
        private void CalcAverage()
        {
            totalWords = 0;
            foreach (var item in voiceAverageData)
                totalWords += item.voicetimer_wordcount;

            totalTime = objRecord.GetDuration();
            newAverage = $"{Math.Round(totalWords * 60 / totalTime, 2)} words per minute";
            lblResult.Text = $@"Total words - {totalWords} in {totalTime} seconds. Average - {newAverage}";
        }

        private void SaveToDatabase()
        {

        }


        #endregion


    }
}
