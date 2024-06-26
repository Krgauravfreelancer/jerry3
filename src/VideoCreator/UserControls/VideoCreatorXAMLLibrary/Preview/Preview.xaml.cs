﻿using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VideoCreatorXAMLLibrary.Helpers;
using VideoCreatorXAMLLibrary.Models;

namespace VideoCreatorXAMLLibrary
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : UserControl
    {
        string imagePreviewFolder = PathHelper.GetTempPath("PreviewImages");
        string mediaPreviewFolder = PathHelper.GetTempPath("PreviewMedia");
        public bool isProcessing = false;
        public Preview()
        {
            InitializeComponent();
            CleanUp();
            FillResolutionCombo();
        }

        private void FillResolutionCombo()
        {
            cmbResolution.Items.Clear();
            cmbResolution.Items.Add("Resolution | 4K | 3840:2160");
            cmbResolution.Items.Add("Resolution | Full HD | 1920:1080");
            cmbResolution.Items.Add("Resolution | 1/2 | 960:540");
            cmbResolution.Items.Add("Resolution | 1/4 | 430:270");
            cmbResolution.Items.Add("Resolution | 1/6 | 320:180");
            cmbResolution.Items.Add("Resolution | 1/8 | 215:125");
            cmbResolution.Items.Add("Resolution | 1/10 | 192:108");
            cmbResolution.SelectedIndex = 1;
        }

        private void CleanUp()
        {
            if (Directory.Exists(imagePreviewFolder))
            {
                var diImage = new DirectoryInfo(imagePreviewFolder);
                foreach (FileInfo file in diImage?.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in diImage?.GetDirectories())
                    dir.Delete(true);
            }

            if (Directory.Exists(mediaPreviewFolder))
            {
                var diVideo = new DirectoryInfo(mediaPreviewFolder);
                foreach (FileInfo file in diVideo?.GetFiles())
                    file.Delete();
                foreach (DirectoryInfo dir in diVideo?.GetDirectories())
                    dir.Delete(true);
            }
        }

        public async void Process(ManageMedia_UserControl.Models.TrackbarMouseMoveEventModel mouseMovedEvent)
        {
            isProcessing = true;
            LoaderHelper.ShowLoader(this, loader, "Processing Preview ...", false);
            var bmps = new List<System.Drawing.Bitmap>();
            foreach (var id in mouseMovedEvent.videoeventIds)
            {
                var videoevent = DataManagerSqlLite.GetVideoEventbyId(id, true, false).FirstOrDefault();
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
                                Bitmap original = new Bitmap(ms);
                                double aspectRatio = (double)original.Width / (double)original.Height;
                                double desiredRatio = (double)16 / (double)9;
                                if(desiredRatio > aspectRatio)
                                {
                                    // Width is more, calc new height
                                    var newheight = (double)original.Width * ((double)9 / (double)16);
                                    Bitmap resized = new Bitmap(original, new Size(original.Width, (int)newheight));
                                    bmps.Add(resized);
                                }
                                else if (desiredRatio < aspectRatio)
                                {
                                    // Height is more
                                    var newWidth = (double)original.Height * ((double)16 / (double)9);
                                    Bitmap resized = new Bitmap(original, new Size((int)newWidth, original.Height));
                                    bmps.Add(resized);
                                }
                                else
                                {
                                    bmps.Add(original);
                                }
                            }
                        }
                    }
                    else
                    {
                        var VideoFileName = $"{mediaPreviewFolder}\\video_{videoevent?.videoevent_id}.mp4";
                        if (!File.Exists(VideoFileName))
                        {
                            Stream t = new FileStream(VideoFileName, FileMode.Create);
                            BinaryWriter b = new BinaryWriter(t);
                            b.Write(videoevent.videosegment_data[0].videosegment_media);
                            t.Close();
                        }

                        var timeAtTheMoment = TimeSpan.Parse(mouseMovedEvent.timeAtTheMoment) - TimeSpan.Parse(videoevent?.videoevent_start);

                        var video2image = new VideoToImage_UserControl.VideoToImage_UserControl(VideoFileName, mediaPreviewFolder, timeAtTheMoment.ToString(@"hh\:mm\:ss\.fff"));
                        var convertedImage = await video2image.ConvertVideoToImage(true);
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


        private System.Drawing.Bitmap CombineBitmap(List<System.Drawing.Bitmap> images)
        {
            //read all images into memory
            //List<System.Drawing.Bitmap> images = new List<System.Drawing.Bitmap>();
            System.Drawing.Bitmap finalImage = null;

            try
            {
                int width = 320;
                int height = 180;
                if (cmbResolution.SelectedIndex == 0)
                {
                    width = 3840;
                    height = 2160;
                }
                else if (cmbResolution.SelectedIndex == 1)
                {
                    width = 1920;
                    height = 1080;
                }
                else if (cmbResolution.SelectedIndex == 2)
                {
                    width = 960;
                    height = 540;
                }
                else if (cmbResolution.SelectedIndex == 3)
                {
                    width = 430;
                    height = 270;
                }
                else if (cmbResolution.SelectedIndex == 5)
                {
                    width = 215;
                    height = 125;
                }
                else if (cmbResolution.SelectedIndex == 6)
                {
                    width = 192;
                    height = 108;
                }


                //create a bitmap to hold the combined image
                finalImage = new System.Drawing.Bitmap(width, height);

                //get a graphics object from the image so we can draw on it
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(finalImage))
                {
                    //set background color
                    g.Clear(System.Drawing.Color.Black);

                    //go through each image and draw it on the final image
                    int offset = 0;
                    foreach (System.Drawing.Bitmap bmp in images)
                    {
                        System.Drawing.Image.GetThumbnailImageAbort myCallback = new System.Drawing.Image.GetThumbnailImageAbort(() => false);
                        System.Drawing.Image myThumbnail = bmp.GetThumbnailImage(width, height, myCallback, IntPtr.Zero);
                        g.DrawImage(myThumbnail, new System.Drawing.Rectangle(offset, 0, width, height));
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
