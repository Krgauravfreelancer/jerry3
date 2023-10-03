using FullScreenPlayer_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FullScreen_UserControl : UserControl
    {
        private int selectedProjectId;
        private bool playVideoAudioFlag;
        private bool playAudioFlag;
        private MediaPlayer _backgroundMusic = new MediaPlayer();
        public FullScreen_UserControl(bool playAudio = false, bool videoAudioFlag = false)
        {
            InitializeComponent();
            playAudioFlag = playAudio;
            playVideoAudioFlag = videoAudioFlag;
            Player.Loaded += PlayerLoaded;
        }

        public FullScreen_UserControl()
        {
            InitializeComponent();
            playAudioFlag = false;
            playVideoAudioFlag = false;
            Player.Loaded += PlayerLoaded;
        }

        public void SetSelectedProjectIdAndReset(int project_id)
        {
            selectedProjectId = project_id;
            if (Player.IsLoaded) SetUp();
        }

        private void PlayerLoaded(object sender, RoutedEventArgs e)
        {
            SetUp();
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
            if (playAudioFlag) _backgroundMusic.Play();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop();
            if (playAudioFlag) _backgroundMusic.Stop();
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
            if (playAudioFlag) _backgroundMusic.Pause();
        }

        private void Timeline_TimeLine_Clicked(object sender, SeekEventArgs e)
        {
            Player.Seek(e.Position);
        }

        private void SetUp()
        {
            var data = DataManagerSqlLite.GetVideoEvents(selectedProjectId, true);
            List<PlaylistItem> playlist = new List<PlaylistItem>();
            var finalBytes = new byte[] { };
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


                var allNotes = DataManagerSqlLite.GetNotes(item.videoevent_id);
                

                foreach (var note in allNotes)
                {
                    var aud = DataManagerSqlLite.GetLocAudio(note.notes_id);
                    if (aud != null && aud.Count > 0)
                    {
                        var byteMedia = aud[0].locaudio_media;
                        finalBytes = Combine(finalBytes, byteMedia);
                    }
                }
            }
            Player.Init(playlist);
            List<TimeLineItem> timeline = Player.GetPlaylist();
            Timeline.SetTimeline(timeline);

            if (playAudioFlag && finalBytes.Length > 0)
            {
                var directory = Directory.GetCurrentDirectory() + "\\" + DateTime.UtcNow.ToString("yyyy-MM-dd-hh-mm-ss") + ".mp3";
                File.WriteAllBytes(directory, finalBytes);
                _backgroundMusic.Open(new Uri(directory));
                _backgroundMusic.MediaEnded += new EventHandler(BackgroundMusic_Ended);
                _backgroundMusic.Play();
            }
            if(playVideoAudioFlag)
                Player.Play();
        }

        private void BackgroundMusic_Ended(object sender, EventArgs e)
        {
            _backgroundMusic.Position = TimeSpan.Zero;
        }

        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private void Player_Position_Changed(object sender, FullScreenPlayer_UserControl.PositionChangedArgs e)
        {
            PositionTxt.Text = e.Position.ToString("mm':'ss");
            Timeline.Set_Elapsed(e.Position);
        }

        private void FullScreen_UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _backgroundMusic.Stop();
        }
    }
}
