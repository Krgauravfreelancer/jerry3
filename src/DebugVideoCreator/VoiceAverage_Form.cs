using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace DebugVideoCreator
{
    public partial class VoiceAverage_Form : Form
    {
        private List<CBVVoiceTimer> voiceAverageData;
        private VoiceAverageHelper objRecord;
        string audioFilename;
        int totalWords = 0;
        double totalTime = 0.0;

        public VoiceAverage_Form()
        {
            InitializeComponent();
            voiceAverageData = new List<CBVVoiceTimer>();
            PopulateReadingText();
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
                lbl.Location = new System.Drawing.Point(10, 100 + 70*i);
                lbl.MaximumSize = new System.Drawing.Size(750, 0);
                lbl.Size = new System.Drawing.Size(750, 70);
                lbl.TabIndex = i;
                lbl.Text = $"{i+1})    " + voiceTimer.voicetimer_line;
                lbl.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                this.Controls.Add(lbl);
                i++;
            }
        }

        #region == Click Events ==

        private void btnPlayRecording_Click(object sender, EventArgs e)
        {
            if(btnPlayRecording.Text == "Play Recording")
            {
                if (audioFilename.Length >= 0)
                    objRecord.RecordPlay(sender, e);
                btnPlayRecording.Text = "Stop Recording";
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
            btnPlayRecording.Enabled = false;
            btnStop.Enabled = true;
            btnCalcAverage.Enabled = false;
            btnSave.Enabled = false;
            btnRecord.Text = "Recording ...";

            audioFilename = "\\_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".wav";
            objRecord = new VoiceAverageHelper(0, Directory.GetCurrentDirectory() + "\\VoiceRecordings\\", audioFilename);
            objRecord.StartRecording(sender, e);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnRecord.Enabled = true;
            btnStop.Enabled = false;
            btnCalcAverage.Enabled = true;
            btnSave.Enabled = true;
            btnRecord.Text = "Record";
            objRecord.RecordEnd(sender, e);

            btnPlayRecording.Enabled = true;

        }
        private void btnCalcAverage_Click(object sender, EventArgs e)
        {
            totalWords = 0;
            foreach (var item in voiceAverageData)
                totalWords += item.voicetimer_wordcount;

            totalTime = objRecord.GetDuration();
            System.Windows.MessageBox.Show($@"Total {totalWords} words in time {totalTime} seconds !!", "Result", MessageBoxButton.OK, MessageBoxImage.Information);
            lblResult.Text = $@"Total {Math.Round(totalWords * 60 / totalTime, 2)} words per minute";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }


        #endregion


    }
}
