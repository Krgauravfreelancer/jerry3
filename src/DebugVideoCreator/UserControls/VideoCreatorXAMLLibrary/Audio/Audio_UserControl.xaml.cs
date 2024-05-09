using AudioPlayer_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace VideoCreatorXAMLLibrary
{

    public partial class Audio_UserControl : UserControl
    {

        List<AudioPlayer> AudioPlayerList = new List<AudioPlayer>();

        CBVVideoEvent selectedEvent = null;
        private int selectedproject_id;
        private int selectedVideoEventId = -1;
        private bool ReadOnly;
        public Audio_UserControl()
        {
            InitializeComponent();
            WavePlayer_UC.Loaded += WavePlayer_UC_Loaded;
        }

        private void WavePlayer_UC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSelectedAudio(selectedEvent);
        }


        #region == Public Functions ==

        public void SetSelected(int projectId, int videoEventId = -1, CBVVideoEvent _selectedEvent = null, bool readonlyMode = false)
        {
            selectedproject_id = projectId;
            selectedVideoEventId = videoEventId;
            selectedEvent = _selectedEvent;
            ReadOnly = readonlyMode;
            LoadSelectedAudio(selectedEvent);
        }

        public void LoadSelectedAudio(CBVVideoEvent videoevent)
        {
            if (videoevent != null && WavePlayer_UC.IsLoaded)
            {
                var allNotes = DataManagerSqlLite.GetNotes(videoevent.videoevent_id);
                var finalBytes = new byte[] { };

                foreach (var note in allNotes)
                {
                    var data = DataManagerSqlLite.GetLocAudio(note.notes_id);
                    if (data != null && data.Count > 0)
                    {
                        var byteMedia = data[0].locaudio_media;
                        finalBytes = Combine(finalBytes, byteMedia);
                    }
                }
                if (finalBytes.Length > 0)
                {
                    WavePlayer_UC.LoadAudio(finalBytes);
                }

            }
        }

        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public Audio_CreateEventWindow GetCreateEventWindow(int project_id)
        {
            selectedproject_id = project_id;
            var EventWindow = new Audio_CreateEventWindow(selectedproject_id);
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

        PopupWindow popup = new PopupWindow();

        private void ContextMenuAddAudioFromFileClickEvent(object sender, RoutedEventArgs e)
        {
            var EventWindow = new Audio_CreateEventWindow(selectedproject_id);
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
}
