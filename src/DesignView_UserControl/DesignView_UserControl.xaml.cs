using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq;

namespace DesignView_UserControl
{
    public partial class DesignView_UserControl : UserControl
    {
        private int selectedProjectId;
        private int selectedVideoEventId;


        public DesignView_UserControl()
        {
            InitializeComponent();
        }
        

        #region == Public Functions ==

        public void SetSelectedProjectId(int _project_id, int _selectedVideoEventId)
        {
            selectedImage.Source = null;
            selectedProjectId = _project_id;
            selectedVideoEventId= _selectedVideoEventId;
            lblSelectedVideoEvent.Content = "Selected Video Event - " + _selectedVideoEventId.ToString();
            var videoEvents = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
            winFormHost.Visibility = Visibility.Hidden;
            selectedImage.Visibility = Visibility.Hidden;
            if (videoEvents != null && videoEvents.Count > 0) 
            {
                var videoEvent = videoEvents.Where(x=>x.videoevent_id == selectedVideoEventId).FirstOrDefault();
                if(videoEvent != null)
                {
                    if (videoEvent.fk_videoevent_media == 1 && videoEvent.videosegment_data.Count > 0)
                    {
                        byte[] blob = videoEvent.videosegment_data[0].videosegment_media;
                        BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
                        selectedImage.Source = x;
                        selectedImage.Visibility = Visibility.Visible;
                    }
                    else if (videoEvent.fk_videoevent_media == 4 && videoEvent.design_data.Count > 0 && videoEvent.videosegment_data.Count > 0)
                    {
                        
                        byte[] blob = videoEvent.videosegment_data[0].videosegment_media;
                        BitmapSource x = (BitmapSource)((new ImageSourceConverter()).ConvertFrom(blob));
                        selectedImage.Source = x;
                        selectedImage.Visibility = Visibility.Visible;
                    }
                    else if (videoEvent.fk_videoevent_media == 2 && videoEvent.videosegment_data.Count > 0)
                    {
                        var filename = $"video_{DateTime.Now.ToString("yyyyMMddhhmmss")}.mp4";
                        Stream t = new FileStream(filename, FileMode.Create);
                        BinaryWriter b = new BinaryWriter(t);
                        b.Write(videoEvent.videosegment_data[0].videosegment_media);
                        t.Close();
                        winFormHost.Visibility = Visibility.Visible;
                        AxWMPLib.AxWindowsMediaPlayer axWmp = winFormHost.Child as AxWMPLib.AxWindowsMediaPlayer;
                        axWmp.URL = filename;
                    }
                    if (videoEvent.fk_videoevent_media == 1)
                        lblSelectedVideoEvent.Content = $"Selected Video Event - {_selectedVideoEventId}, with type - Image";
                    else if (videoEvent.fk_videoevent_media == 2)
                        lblSelectedVideoEvent.Content = $"Selected Video Event - {_selectedVideoEventId}, with type - Video";
                    else if (videoEvent.fk_videoevent_media == 3)
                        lblSelectedVideoEvent.Content = $"Selected Video Event - {_selectedVideoEventId}, with type - Audio";
                    else if (videoEvent.fk_videoevent_media == 4)
                        lblSelectedVideoEvent.Content = $"Selected Video Event - {_selectedVideoEventId}, with type - Design";
                }
            }

            
        }

        #endregion
        
    }
}