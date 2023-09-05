using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using AudioPlayer_UserControl;
using Sqllite_Library.Models;
using Audio_UserControl.Windows;

namespace Audio_UserControl
{

    public partial class AudioUserControl : UserControl
    {

        List<AudioPlayer> AudioPlayerList = new List<AudioPlayer>();

        CBVVideoEvent selectedEvent = null;
        private int selectedproject_id;
        private int selectedVideoEventId = -1;

        public AudioUserControl()
        {
            InitializeComponent();
        }

        #region == Public Functions ==


        public void SetSelected(int projectId, int videoEventId = -1, CBVVideoEvent _selectedEvent = null)
        {
            selectedproject_id = projectId;
            selectedVideoEventId = videoEventId;
            selectedEvent = _selectedEvent;
            VideoEventSelectionChanged();
        }

        public void LoadSelectedAudio(CBVVideoEvent videoevent)
        {
            if (videoevent != null && videoevent?.fk_videoevent_media == 3)
            {
                byte[] audioData = videoevent?.audio_data[0]?.audio_media;
                WavePlayer_UC.LoadAudio(audioData);
            }
            //else
            //{
            //    InitializeComponent();
            //    // WavePlayer_UC = new WavePlayer_UserControl.WavePlayer();
            //}
        }

        public CreateEventWindow GetCreateEventWindow(int project_id)
        {
            selectedproject_id = project_id;
            var EventWindow = new CreateEventWindow(selectedproject_id);
            return EventWindow;
            //EventWindow.ShowDialog();
            //this.IsEnabled = false;
        }


        private void VideoEventSelectionChanged()
        {
            LoadSelectedAudio(selectedEvent);
        }



        #endregion

        #region == Events ==

        private void TxtVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            ((TextBox)sender).Text = s;
        }

        private void TxtIncrement_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            ((TextBox)sender).Text = s;
        }

        #endregion

        private void FillListBoxVideoEvent(List<CBVVideoEvent> source)
        {
            //listBoxAudioEvent.SelectedItem = null;
            //listBoxAudioEvent.Items.Clear();
            //AudioPlayerList.Clear();

            //if (cmbProject.SelectedIndex != -1)
            //{
            //    CreateAudioBtn.IsEnabled = true;

            //    foreach (var item in source)
            //    {
            //        var itemExtended = new VideoEventExtended(item)
            //        {
            //            Start = item.videoevent_start,
            //            ClipDuration = item.videoevent_duration.ToString() + " sec",
            //        };
            //        if (item.audio_data != null && item.audio_data.Count > 0 && item.fk_videoevent_media == 3)
            //        {
            //            itemExtended.MediaName = "Audio";
            //        }
            //        else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 1)
            //        {
            //            itemExtended.MediaName = "Image";
            //        }
            //        else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 2)
            //        {
            //            itemExtended.MediaName = "Video";
            //        }
            //        else if (item.design_data != null && item.design_data.Count > 0 && item.fk_videoevent_media == 4)
            //        {
            //            itemExtended.MediaName = "Design";
            //        }
            //        else
            //        {
            //            itemExtended.MediaName = "None";
            //        }

            //        if (item.fk_videoevent_media == 3)
            //        {
            //            listBoxAudioEvent.Items.Add(itemExtended);
            //        }

            //    }
            //}
        }

        PopupWindow popup = new PopupWindow();

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var EventWindow = new CreateEventWindow(selectedproject_id);
            EventWindow.ShowDialog();
            this.IsEnabled = false;
        }


        private void AudioPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            AudioPlayer myPlayer = (AudioPlayer)sender;
            myPlayer.SetMini();
            myPlayer.DataRequest += MyPlayer_DataRequest;
            AudioPlayerList.Add(myPlayer);
        }

        private void MyPlayer_DataRequest(object sender, EventArgs e)
        {
            AudioPlayer myPlayer = (AudioPlayer)sender;
            int i = AudioPlayerList.IndexOf((AudioPlayer)sender);

            //byte[] audioData = ((VideoEventExtended)listBoxAudioEvent.Items[i]).audio_data[0].audio_media;
            //myPlayer.Init(audioData, "Mini Player", true);
        }

        private void listBoxAudioEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int i = listBoxAudioEvent.SelectedIndex;

            //if (i != -1)
            //{
            //    byte[] audioData = ((VideoEventExtended)listBoxAudioEvent.Items[i]).audio_data[0].audio_media;
            //    SelectedEvent = (VideoEventExtended)listBoxAudioEvent.Items[i];

            //    WavePlayer_UC.LoadAudio(audioData);
            //}
        }
    }

    public class VideoEventExtended : CBVVideoEvent
    {
        public string MediaName { get; set; }
        public string Start { get; set; }
        public string ClipDuration { get; set; }
        public VideoEventExtended(CBVVideoEvent ch)
        {
            foreach (var prop in ch.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(ch, null), null);
            }
        }
    }
}
