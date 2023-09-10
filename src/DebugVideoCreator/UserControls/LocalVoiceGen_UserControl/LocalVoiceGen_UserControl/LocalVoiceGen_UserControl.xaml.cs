using LocalVoiceGen.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LocalVoiceGen_UserControl
{
    public partial class LocalVoiceGenUserControl : UserControl
    {
        private bool IsSetUp = false;
        //int selectionIndex = 0;
        bool showMessageBoxes = false;


        //CBVVideoEvent SelectedEvent = null;
        Window EditWindow = null;

        public LocalVoiceGenUserControl()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NoteStack.Children.Clear();

            if (!IsSetUp)
            {
                InitializeDatabase();
                IsSetUp = true;
            }

            if (ProjectCmbBox.Items.Count > 0)
            {
                ProjectCmbBox.SelectedIndex = 0;
            }
        }

        #region Loading Database
        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                if (showMessageBoxes)
                {
                    MessageBox.Show(message + ", syncing lookup tables !!");
                }
                SyncApp();
                SyncMedia();
                SyncScreen();
                if (showMessageBoxes)
                {
                    MessageBox.Show("lookup tables synced successfully !!");
                }
                RefreshOrLoadComboBoxes(EnumEntity.ALL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //this.Close();
            }
        }
        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(int));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = 1;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = 0;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = 0;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = 0;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = 0;
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SyncMedia()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("media_id", typeof(int));
                datatable.Columns.Add("media_name", typeof(string));
                datatable.Columns.Add("media_color", typeof(string));

                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = "image";
                row["media_color"] = "Tomato";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["media_id"] = -1;
                row2["media_name"] = "video";
                row2["media_color"] = "Thistle";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["media_id"] = -1;
                row3["media_name"] = "audio";
                row3["media_color"] = "Yellow";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["media_id"] = -1;
                row4["media_name"] = "form";
                row4["media_color"] = "LightSalmon";
                datatable.Rows.Add(row4);

                var insertedIds = DataManagerSqlLite.SyncMedia(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SyncScreen()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("screen_id", typeof(int));
                datatable.Columns.Add("screen_name", typeof(string));
                datatable.Columns.Add("screen_color", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "LightSalmon";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["screen_id"] = -1;
                row2["screen_name"] = "prerequisites";
                row2["screen_color"] = "Azure";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["screen_id"] = -1;
                row3["screen_name"] = "screen cast";
                row3["screen_color"] = "Beige";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["screen_id"] = -1;
                row4["screen_name"] = "conclusion";
                row4["screen_color"] = "Aqua";
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["screen_id"] = -1;
                row5["screen_name"] = "next";
                row5["screen_color"] = "LightSteelBlue";
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region ListBox and ComboBox

        private void FillListBoxVideoEvent(List<CBVVideoEvent> source)
        {
            listBoxVideoEvent.SelectedItem = null;
            listBoxVideoEvent.Items.Clear();
            foreach (var item in source)
            {
                var itemExtended = new VideoEventExtended(item)
                {
                    Start = item.videoevent_start,
                    ClipDuration = item.videoevent_duration.ToString() + " sec",
                };
                if (item.audio_data != null && item.audio_data.Count > 0 && item.fk_videoevent_media == 3)
                {
                    itemExtended.MediaName = "Audio";
                }
                else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 1)
                {
                    itemExtended.MediaName = "Image";
                }
                else if (item.videosegment_data != null && item.videosegment_data.Count > 0 && item.fk_videoevent_media == 2)
                {
                    itemExtended.MediaName = "Video";
                }
                else if (item.design_data != null && item.design_data.Count > 0 && item.fk_videoevent_media == 4)
                {
                    itemExtended.MediaName = "Design";
                }
                else
                {
                    itemExtended.MediaName = "None";
                }
                listBoxVideoEvent.Items.Add(itemExtended);
            }
        }
        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
            {
                var data = DataManagerSqlLite.GetProjects(false);
                RefreshComboBoxes<CBVProject>(ProjectCmbBox, data, "project_name");
            }
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

        private void ProjectCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int SelectedIndex = ((ComboBox)sender).SelectedIndex + 1;

            var data = DataManagerSqlLite.GetVideoEvents(SelectedIndex, true);
            FillListBoxVideoEvent(data);
        }
        #endregion

        #region Create Project
        private void CreateProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("project_name", typeof(string));
            dataTable.Columns.Add("project_version", typeof(int));
            dataTable.Columns.Add("project_comments", typeof(string));
            dataTable.Columns.Add("project_createdate", typeof(string));
            dataTable.Columns.Add("project_modifydate", typeof(string));
            dataTable.Columns.Add("fk_project_background", typeof(int));
            dataTable.Columns.Add("project_uploaded", typeof(int));
            dataTable.Columns.Add("project_date", typeof(string));
            dataTable.Columns.Add("project_archived", typeof(int));

            var row = dataTable.NewRow();
            row["project_name"] = ProjectNameTxtBox.Text != "" ? ProjectNameTxtBox.Text : "Project";
            row["project_version"] = 0;
            row["fk_project_background"] = 1;
            row["project_uploaded"] = 0;
            row["project_archived"] = 0;
            row["project_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dataTable.Rows.Add(row);

            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    if (showMessageBoxes)
                    {
                        MessageBox.Show("Project was populated to Database");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            InitializeDatabase();
            ProjectCmbBox.SelectedIndex = ProjectCmbBox.Items.Count - 1;
        }
        #endregion

        internal void Create_Canceled()
        {
            EditWindow.Close();
            EditWindow = null;
            this.IsEnabled = true;
            this.Focus();
        }

        private void listBoxVideoEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxVideoEvent.SelectedIndex != -1)
            {
                AddNoteBtn.IsEnabled = true;

                VideoEventExtended selectedElement = (VideoEventExtended)listBoxVideoEvent.Items[listBoxVideoEvent.SelectedIndex];

                var notes = DataManagerSqlLite.GetNotes(selectedElement.videoevent_id);

                Setup_Notes(notes);
            }
            else
            {
                AddNoteBtn.IsEnabled = false;
                NoteStack.Children.Clear();
            }
        }

        private void Setup_Notes(List<CBVNotes> notes)
        {
            NoteStack.Children.Clear();

            notes.Reverse();

            foreach (var note in notes)
            {
                if (note != null)
                {
                    LocalVoiceGen_Control localVoiceGen_Control = new LocalVoiceGen_Control() { Margin = new Thickness(0,0,5,0), Width = 300 } ;
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

                        VideoEventExtended selectedElement = (VideoEventExtended)listBoxVideoEvent.Items[listBoxVideoEvent.SelectedIndex];

                        row["notes_id"] = e.GeneratedVoiceData.NoteID;
                        row["notes_line"] = e.GeneratedVoiceData.Text;
                        int WordCount = e.GeneratedVoiceData.Text.Split(' ').Length;
                        row["notes_wordcount"] = WordCount;
                        row["notes_index"] = 1;
                        row["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        dataTable.Rows.Add(row);

                        DataManagerSqlLite.UpdateRowsToNotes(dataTable);
                    };

                    localVoiceGen_Control.Save_Event += (s, e) => {
                        GeneratedVoice voice = e.GeneratedVoiceData;

                        if (voice == null)
                        {
                            return;
                        }



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

        private void AddNoteBtn_Click(object sender, RoutedEventArgs e)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            dataTable.Columns.Add("notes_line", typeof(string));

            dataTable.Columns.Add("notes_wordcount", typeof(int));

            dataTable.Columns.Add("notes_createdate", typeof(string));
            dataTable.Columns.Add("notes_modifydate", typeof(string));
            dataTable.Rows.Clear();

            var row = dataTable.NewRow();

            VideoEventExtended selectedElement = (VideoEventExtended)listBoxVideoEvent.Items[listBoxVideoEvent.SelectedIndex];

            row["fk_notes_videoevent"] = selectedElement.videoevent_id;
            row["notes_line"] = "Sample Text...";

            row["notes_wordcount"] = 5;

            row["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            dataTable.Rows.Add(row);

            DataManagerSqlLite.InsertRowsToNotes(dataTable);

            var notes = DataManagerSqlLite.GetNotes(selectedElement.videoevent_id);

            Setup_Notes(notes);
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
