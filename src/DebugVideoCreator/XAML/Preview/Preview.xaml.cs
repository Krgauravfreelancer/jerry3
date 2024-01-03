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

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : UserControl
    {
        public Preview()
        {
            InitializeComponent();
        }

        public void Process(TrackbarMouseMoveEvent mouseMovedEvent)
        {
            var bmps = new List<System.Drawing.Bitmap>();
            foreach (var id in mouseMovedEvent.videoeventIds)
            {
                var videoevent = DataManagerSqlLite.GetVideoEventbyId(id, true).FirstOrDefault();
                if (videoevent != null && (videoevent.fk_videoevent_media == 1 || videoevent.fk_videoevent_media == 4))
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
            }
            Console.WriteLine($"Time - {mouseMovedEvent.timeAtTheMoment} and events - {string.Join(", ", mouseMovedEvent.videoeventIds)} and blobs Found - {bmps?.Count}");
            var finalImage = CombineBitmap(bmps);

            if (finalImage != null)
                previewImage.Source = Convert(finalImage);
            // finalImage.Dispose();
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
