using FullScreenPlayer_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FSP_UserControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FSPUserControl : UserControl
    {
        private int selectedProjectId;

        public FSPUserControl()
        {
            InitializeComponent();
        }

        public void SetSelectedProjectIdAndReset(int project_id)
        {
            selectedProjectId = project_id;
            SetUp();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
        }

        private void SetUp()
        {
            var data = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
            List<PlaylistItem> playlist = new List<PlaylistItem>();
            foreach (var item in data)
            {
                TimeSpan start;
                bool successful = TimeSpan.TryParse(item.videoevent_start, out start);

                if (!successful)
                {
                    break;
                }

                TimeSpan duration = TimeSpan.FromSeconds(item.videoevent_duration);
                MediaType mediaType = MediaType.Image;

                if (item.fk_videoevent_media == 1)
                {
                    mediaType = MediaType.Image;
                }
                else if (item.fk_videoevent_media == 2)
                {
                    mediaType = MediaType.Video;
                }
                else if (item.fk_videoevent_media == 3)
                {
                    mediaType = MediaType.Audio;
                }

                if (item.videosegment_data.Count > 0)
                {
                    byte[] MediaData = item.videosegment_data[0].videosegment_media;
                    PlaylistItem playlistItem = new PlaylistItem(mediaType, start, duration, MediaData);
                    playlist.Add(playlistItem);
                }
            }
            Player.Init(playlist);
            List<TimeLineItem> timeline = Player.GetPlaylist();
            Timeline.SetTimeline(timeline);
        }

        private void Player_Position_Changed(object sender, FullScreenPlayer_UserControl.PositionChangedArgs e)
        {
            PositionTxt.Text = e.Position.ToString("mm':'ss");
            Timeline.Set_Elapsed(e.Position);
        }
    }
}
