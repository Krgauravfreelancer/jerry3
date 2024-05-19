using ManageMedia_UserControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ManageMedia_UserControl.Controls
{
    internal class ThumbNail : ListBoxItem
    {

        internal PlannedImage PlannedImage = null;

        public ThumbNail( PlannedImage plannedImage)
        { 
            PlannedImage = plannedImage;

            Border ImageBorder = new Border();
            ImageBorder.Background = Brushes.Transparent;
            ImageBorder.IsHitTestVisible = false;

            Image ThumbNailImage = new Image();
            ThumbNailImage.Source = LoadImage(plannedImage.ThumbNail);
            ThumbNailImage.Margin = new Thickness(2);
            ThumbNailImage.Width = 80;
            ThumbNailImage.HorizontalAlignment = HorizontalAlignment.Left;
            ImageBorder.Child = ThumbNailImage;
            ImageBorder.IsHitTestVisible = false;

            this.Content = ImageBorder;
            this.Background = Brushes.Transparent;
        }

        internal BitmapImage GetMainImage()
        {
            return LoadImage(PlannedImage.Image);
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
                mem.Close();
            }
            image.Freeze();
            return image;
        }
    }
}
