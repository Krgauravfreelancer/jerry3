using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugVideoCreator
{
    public class VoiceAverageHelper
    {
        WaveIn sourceStream;
        WaveFileWriter waveWriter;
        readonly string FilePath;
        readonly string FileName;
        readonly int InputDeviceIndex;
        WaveOutEvent player;

        public VoiceAverageHelper(int inputDeviceIndex, string filePath, string fileName)
        {
            //InitializeComponent();
            this.InputDeviceIndex = inputDeviceIndex;
            this.FileName = fileName;
            this.FilePath = filePath;
        }

        public void StartRecording(object sender, EventArgs e)
        {
            sourceStream = new WaveIn
            {
                DeviceNumber = this.InputDeviceIndex,
                WaveFormat =
                    new WaveFormat(44100, WaveIn.GetCapabilities(this.InputDeviceIndex).Channels)
            };

            sourceStream.DataAvailable += this.SourceStreamDataAvailable;

            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }

            waveWriter = new WaveFileWriter(FilePath + FileName, sourceStream.WaveFormat);
            sourceStream.StartRecording();
        }

        public void SourceStreamDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        public void RecordEnd(object sender, EventArgs e)
        {
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
            if (this.waveWriter == null)
            {
                return;
            }
            this.waveWriter.Dispose();
            this.waveWriter = null;
        }

        public double GetDuration()
        {
            using (MediaFoundationReader mainOutputStream = new MediaFoundationReader(FilePath + FileName))
            {
                var totalSpan = mainOutputStream.TotalTime;
                return totalSpan.TotalSeconds;
            }

        }

        public void RecordPlay(object sender, EventArgs e)
        {
            using (MediaFoundationReader mainOutputStream = new MediaFoundationReader(FilePath + FileName))
            {
                WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
                player = new WaveOutEvent();
                player.Init(volumeStream);
                player.Play();
            }
        }

        public void RecordStop(object sender, EventArgs e)
        {
            player.Stop();
            
        }
    }
}
