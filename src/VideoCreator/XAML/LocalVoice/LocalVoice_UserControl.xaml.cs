using LocalVoiceGen.Models;
using LocalVoiceGen_UserControl;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace VideoCreator.XAML
{
    public partial class LocalVoice_UserControl : UserControl
    {
        int selectedVideoEvent;
        int selectedNotesId;
        Window EditWindow = null;

        public LocalVoice_UserControl(int videoEvent)
        {
            InitializeComponent();
            selectedVideoEvent = videoEvent;
            if (selectedVideoEvent > 0)
                Setup_Notes(true);
            else
                NoteStack.Children.Clear();
        }

        public LocalVoice_UserControl(int videoEvent, int notesId)
        {
            InitializeComponent();
            selectedVideoEvent = videoEvent;
            selectedNotesId = notesId;
            if (selectedNotesId > 0)
                Setup_Notes(false);
            else
                NoteStack.Children.Clear();
        }

        internal void Create_Canceled()
        {
            EditWindow.Close();
            EditWindow = null;
            this.IsEnabled = true;
            this.Focus();
        }

        private void Setup_Notes(bool allNotesFlag = true)
        {
            NoteStack.Children.Clear();
            var notes = new List<CBVNotes>();
            if (allNotesFlag)
                notes = DataManagerSqlLite.GetNotes(selectedVideoEvent);
            else if (selectedNotesId > 0)
                notes = DataManagerSqlLite.GetNotesbyId(selectedNotesId);

            foreach (var note in notes)
            {
                if (note != null)
                {
                    var localVoiceGen_Control = new LocalVoiceGen_Control() { Margin = new Thickness(0, 0, 5, 0), Width = allNotesFlag ? 300 : 600 };
                    localVoiceGen_Control.Set_Text(note.notes_line);
                    localVoiceGen_Control.set_NoteID(note.notes_id);
                    localVoiceGen_Control.Update_Event += (s, e) =>
                    {
                        var dataTable = new DataTable();
                        dataTable.Columns.Add("notes_id", typeof(int));
                        dataTable.Columns.Add("notes_line", typeof(string));
                        dataTable.Columns.Add("notes_wordcount", typeof(int));
                        dataTable.Columns.Add("notes_index", typeof(int));
                        dataTable.Columns.Add("notes_modifydate", typeof(string));
                        dataTable.Rows.Clear();

                        var row = dataTable.NewRow();
                        row["notes_id"] = e.GeneratedVoiceData.NoteID;
                        row["notes_line"] = e.GeneratedVoiceData.Text;
                        int WordCount = e.GeneratedVoiceData.Text.Split(' ').Length;
                        row["notes_wordcount"] = WordCount;
                        row["notes_index"] = 1;
                        row["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        dataTable.Rows.Add(row);

                        DataManagerSqlLite.UpdateRowsToNotes(dataTable);
                    };

                    localVoiceGen_Control.Save_Event += (s, e) =>
                    {
                        GeneratedVoice voice = e.GeneratedVoiceData;

                        if (voice == null)
                            return;

                        try
                        {
                            var LocAudios = DataManagerSqlLite.GetLocAudio(voice.NoteID);

                            if (LocAudios.Count == 0)
                            {
                                var dataTable = new DataTable();
                                dataTable.Columns.Add("locaudio_id", typeof(int));
                                dataTable.Columns.Add("fk_locaudio_notes", typeof(int));
                                dataTable.Columns.Add("locaudio_media", typeof(byte[]));
                                dataTable.Columns.Add("locaudio_createdate", typeof(string));
                                dataTable.Columns.Add("locaudio_modifydate", typeof(string));
                                dataTable.Rows.Clear();

                                var row = dataTable.NewRow();

                                row["locaudio_id"] = -1;
                                row["fk_locaudio_notes"] = voice.NoteID;
                                row["locaudio_media"] = voice.Data;
                                row["locaudio_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                row["locaudio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                dataTable.Rows.Add(row);

                                DataManagerSqlLite.InsertRowsToLocAudio(dataTable);
                            }
                            else
                            {
                                var dataTable = new DataTable();
                                dataTable.Columns.Add("locaudio_id", typeof(int));
                                dataTable.Columns.Add("fk_locaudio_notes", typeof(int));
                                dataTable.Columns.Add("locaudio_media", typeof(byte[]));
                                dataTable.Columns.Add("locaudio_createdate", typeof(string));
                                dataTable.Columns.Add("locaudio_modifydate", typeof(string));
                                dataTable.Rows.Clear();

                                var row = dataTable.NewRow();

                                row["locaudio_id"] = LocAudios[0].locaudio_id;
                                row["fk_locaudio_notes"] = voice.NoteID;
                                row["locaudio_media"] = voice.Data;
                                row["locaudio_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                                dataTable.Rows.Add(row);

                                DataManagerSqlLite.UpdateRowsToLocAudio(dataTable);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    NoteStack.Children.Add(localVoiceGen_Control);
                }
            }
        }

        private void AddNewNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            //var dataTable = new DataTable();
            //dataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            //dataTable.Columns.Add("notes_line", typeof(string));

            //dataTable.Columns.Add("notes_wordcount", typeof(int));

            //dataTable.Columns.Add("notes_createdate", typeof(string));
            //dataTable.Columns.Add("notes_modifydate", typeof(string));
            //dataTable.Rows.Clear();

            //var row = dataTable.NewRow();

            //VideoEventExtended selectedElement = (VideoEventExtended)listBoxVideoEvent.Items[listBoxVideoEvent.SelectedIndex];

            //row["fk_notes_videoevent"] = selectedElement.videoevent_id;
            //row["notes_line"] = "Sample Text...";

            //row["notes_wordcount"] = 5;

            //row["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //row["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            //dataTable.Rows.Add(row);

            //DataManagerSqlLite.InsertRowsToNotes(dataTable);

            //var notes = DataManagerSqlLite.GetNotes(selectedElement.videoevent_id);

            //Setup_Notes(notes);
        }
    }
}
