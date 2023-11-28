using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;

namespace VideoToImage_UserControl
{
    /// <summary>
    /// Interaction logic for VideoToImage_UserControl.xaml
    /// </summary>
    public partial class VideoToImage_UserControl : UserControl
    {
        double TotalFrames, Period;
        double fps;
        bool CancelSignal;
        MediaFile Mp4Media;
        string OutputFolder;


        public VideoToImage_UserControl(string pathToVideo, string outputFilenamePrefix)
        {
            InitializeComponent();
            txtVideo.Text = pathToVideo;
            txtOutput.Text = outputFilenamePrefix;
            CancelSignal = false;
        }

        private void Initiate()
        {
            if (string.IsNullOrEmpty(txtVideo.Text) || string.IsNullOrEmpty(txtOutput.Text))
                return;

            Period = int.Parse(txtTime.Text) > 0 ? int.Parse(txtTime.Text): 1;
            try
            {
                Mp4Media = new MediaFile { Filename = txtVideo.Text };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(Mp4Media);
                }

                TotalFrames = Mp4Media.Metadata.Duration.TotalSeconds * Mp4Media.Metadata.VideoData.Fps;

                fps = Mp4Media.Metadata.VideoData.Fps;
                TitleLabel.Content = "Title : " + Path.GetFileNameWithoutExtension(txtVideo.Text);
                FrameRateLabel.Content = "Frame Rate : " + fps.ToString() + " FPS";
                DurationLabel.Content = "Duration : " + Mp4Media.Metadata.Duration.ToString();
                ExpectedFramesLabel.Content = $"Expected Images at an interval of {Period} seconds : " + (int)(TotalFrames/(Period* fps));
                btnCreateImage.IsEnabled = true;
            }
            catch 
            {
                btnCreateImage.IsEnabled = false;


                TitleLabel.Content = "Something Went wrong, please check Input and output values";
                FrameRateLabel.Content = string.Empty;
                DurationLabel.Content = string.Empty;
                ExpectedFramesLabel.Content = string.Empty;
            }
            
        }

        private void txtOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Validations())
                Initiate();
            OutputFolder = txtOutput.Text;
        }
        private void txtVideo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Validations())
                Initiate();
        }

        private void txtPeriod_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Validations())
                Initiate();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void btnCreateImage_Click(object sender, EventArgs e)
        {
            if (Period <= 0 || (int)(TotalFrames / (Period * fps)) < 1)
            {
                MessageBox.Show($"Video Duration is not that long. Max Length is - {Mp4Media.Metadata.Duration}", "Error");
                return;
            }
            Directory.CreateDirectory(txtOutput.Text);
            txtOutput.IsEnabled = false;
            txtVideo.IsEnabled = false;
            txtTime.IsEnabled = false;

            await StartExtraction();

            txtOutput.IsEnabled = true;
            txtVideo.IsEnabled = true;
            txtTime.IsEnabled = true;

            if (CancelSignal)
            {
                CancelSignal = false;
                MessageBox.Show("Operation was aborted.", "Aborted", MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
            else
            {
                var r = MessageBox.Show("Extracted Image successfully, do you want to open folder?", "Operation Completed Successfully", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(OutputFolder);
            }
        }

        private async Task StartExtraction()
        {
            await Task.Run(delegate
            {
                using (var engine = new Engine())
                {
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(Period) };
                    var outputFile = new MediaFile { Filename = $"{OutputFolder}\\Image.jpeg" };
                    engine.GetThumbnail(Mp4Media, outputFile, options);
                }
            });
            
        }

        

        private bool Validations()
        {
            try
            {
                if(btnCreateImage == null) return false;
                if (string.IsNullOrEmpty(txtVideo.Text) || string.IsNullOrEmpty(txtOutput.Text) || string.IsNullOrEmpty(txtTime.Text))
                {
                    btnCreateImage.IsEnabled = false;
                    return false;
                }
                else
                {
                    btnCreateImage.IsEnabled = true;
                    return true;
                }
            }catch(Exception) { }
            return false;
        }
    }
}
