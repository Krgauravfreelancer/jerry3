using DebugVideoCreator.Models;
using Sqllite_Library.Business;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VideoCreator.Helpers;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : UserControl
    {
        string imageOutputFolder = $"{Directory.GetCurrentDirectory()}\\ExtractedImages";
        string videoOutputFolder = $"{Directory.GetCurrentDirectory()}\\Media";
        public bool isProcessing = false;
        public Preview()
        {
            InitializeComponent();
            CleanUp();
        }

        private void CleanUp()
        {
            if (Directory.Exists(imageOutputFolder))
            {
                var diImage = new DirectoryInfo(imageOutputFolder);
                foreach (FileInfo file in diImage?.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in diImage?.GetDirectories())
                    dir.Delete(true);
            }

            if (Directory.Exists(videoOutputFolder))
            {
                var diVideo = new DirectoryInfo(videoOutputFolder);
                foreach (FileInfo file in diVideo?.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in diVideo?.GetDirectories())
                    dir.Delete(true);
            }
        }

        public async void Process(TrackbarMouseMoveEvent mouseMovedEvent)
        {
            isProcessing = true;
            LoaderHelper.ShowLoader(this, loader);
            //Console.WriteLine($"Mouse.Captured - {Mouse.LeftButton == MouseButtonState.Pressed}, {Mouse.LeftButton == MouseButtonState.Released}");

            var bmps = new List<System.Drawing.Bitmap>();
            foreach (var id in mouseMovedEvent.videoeventIds)
            {
                var videoevent = DataManagerSqlLite.GetVideoEventbyId(id, true).FirstOrDefault();
                if (videoevent != null && videoevent?.videosegment_data?.Count > 0) 
                {
                    if (videoevent.fk_videoevent_media == 1 || videoevent.fk_videoevent_media == 4)
                    {
                        //Image case
                        var imageBytes = videoevent?.videosegment_data[0]?.videosegment_media;
                        if (imageBytes != null)
                        {
                            using (var ms = new MemoryStream(imageBytes))
                            {
                                var bmp = new Bitmap(ms);
                                bmps.Add(bmp);
                            }
                        }
                    }
                    else
                    {
                        var VideoFileName = $"{videoOutputFolder}\\video_{videoevent?.videoevent_id}.mp4";
                        if (!File.Exists(VideoFileName))
                        {
                            Stream t = new FileStream(VideoFileName, FileMode.Create);
                            BinaryWriter b = new BinaryWriter(t);
                            b.Write(videoevent.videosegment_data[0].videosegment_media);
                            t.Close();
                        }
                        var video2image = new VideoToImage_UserControl.VideoToImage_UserControl(VideoFileName, videoOutputFolder);
                        var convertedImage = await video2image.ConvertVideoToImage();
                        var bmp = new Bitmap(convertedImage);
                        bmps.Add(bmp);
                    }
                }
            }
            Console.WriteLine($"Time - {mouseMovedEvent.timeAtTheMoment} and events - {string.Join(", ", mouseMovedEvent.videoeventIds)} and blobs Found - {bmps?.Count}");
            var finalImage = CombineBitmap(bmps);

            if (finalImage != null)
                previewImage.Source = Convert(finalImage);
             finalImage.Dispose();
            LoaderHelper.HideLoader(this, loader);
            isProcessing = false;
        }

        private static System.Drawing.Bitmap CombineBitmap(List<System.Drawing.Bitmap> images)
        {
            //read all images into memory
            //List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try
            {
                int width = 1920;
                int height = 1080;

                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.White);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap image in images)
                    {
                        g.DrawImage(image,
                          new System.Drawing.Rectangle(offset, 0, image.Width, image.Height));
                        //offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();

                throw ex;
            }
            finally
            {
                //clean up memory
                foreach (System.Drawing.Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        private BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
