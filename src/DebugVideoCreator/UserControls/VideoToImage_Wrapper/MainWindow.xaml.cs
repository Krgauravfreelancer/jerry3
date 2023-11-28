using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VideoToImage_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TryImageFromVideo();
        }

        private void TryImageFromVideo()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var VideoFileName = $"{currentDirectory}\\Media\\Screencast1.mp4";
            var outputFolder = $"C:\\commercialBase\\{DateTime.Now.ToString("yyyyMMdd_HHmm")}";
            var video2image = new VideoToImage_UserControl.VideoToImage_UserControl(VideoFileName, outputFolder);

            var window = new Window
            {
                Title = "Video To Image user control",
                Content = video2image,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                RenderSize = video2image.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Closing += Window_Closing;
            try
            {
                var result = window.ShowDialog();
            }
            catch (Exception)
            { }

            //bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Close();
        }
    }
}
