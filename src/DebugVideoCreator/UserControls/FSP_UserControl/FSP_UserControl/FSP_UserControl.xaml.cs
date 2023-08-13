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


        public FSPUserControl()
        {
            InitializeComponent();


            Refresh();
        }

        private void Refresh()
        {
            var data = DataManagerSqlLite.GetProjects(false);
            RefreshComboBoxes<CBVProject>(cmbProject, data, "project_name");
        }

        private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }

        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
        }

        private void cmbProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int SelectedIndex = ((ComboBox)sender).SelectedIndex + 1;

            var data = DataManagerSqlLite.GetVideoEvents(SelectedIndex, true);

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

                List<CBVVideoSegment> videoSegments = item.videosegment_data;
                List<CBVAudio> AudioSegments = item.audio_data;

                MediaType mediaType = MediaType.Image;

                if (item.fk_videoevent_media == 1)
                {
                    //Its an image
                    mediaType = MediaType.Image;
                }

                if (item.fk_videoevent_media == 2)
                {
                    //Its a video
                    mediaType = MediaType.Video;
                }

                if (item.fk_videoevent_media == 3)
                {
                    //Its an audio
                    mediaType = MediaType.Audio;
                }

                if (videoSegments.Count > 0)
                {
                    byte[] MediaData = videoSegments[0].videosegment_media;

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
