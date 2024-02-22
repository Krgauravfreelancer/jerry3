using ManageMedia_UserControl;
using ManageMedia_UserControl.Classes;
using ManageMedia_UserControl.Controls;
using ManageMedia_UserControl.Models;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VideoCreator.Loader;
using ManageMedia = ManageMedia_UserControl.Classes.Media;
using ManageMediaType = ManageMedia_UserControl.Classes.MediaType;

namespace VideoCreator.Helpers
{
    public class ManageMediaWindowManager
    {
        Window _Window;
        ManageMedia_Control _ManageMedia;
        SelectedProjectEvent selectedProjectEvent;
        public LoadingAnimation loader;
        public Window CreateWindow(SelectedProjectEvent _selectedProjectEvent)
        {
            _Window = new Window();
            _Window.Width = 1200;
            _Window.Height = 620;
            _Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            _ManageMedia = new ManageMedia_Control();
            selectedProjectEvent = _selectedProjectEvent;
            //ProjectID = Selected_ID;

            //if (Selected_ID != -1)
            if (selectedProjectEvent != null)
            {
                _ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);

                RefreshData();

                //Events Go Here
                _ManageMedia.NotesChanged += _ManageMedia_NotesChanged;
                _ManageMedia.NotesDeleted += _ManageMedia_NotesDeleted;
                _ManageMedia.NotesCreated += _ManageMedia_NotesCreated;
            }

            _Window.Content = AddLoaderAndReturnContent(_ManageMedia);
            //window.Show();
            return _Window;
        }

        private Grid AddLoaderAndReturnContent(ManageMedia_Control _ManageMedia)
        {
            var grid = new Grid
            {
                Name = "parentGrid"
            };
            grid.Children.Add(_ManageMedia);

            loader = new LoadingAnimation
            {
                Name = "loader",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Hidden,
            };
            grid.Children.Add(loader);
            return grid;
        }

        public bool? ShowWindow(Window window)
        {
            LoaderHelper.HideLoader(window, loader);
            var result = window.ShowDialog();
            return result;
        }

        private void _ManageMedia_NotesCreated(object sender, ManageMedia_UserControl.Controls.NotesChangedArgs e)
        {
            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_createdate", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));

            foreach (var CreatedElement in e.ChangedNotes)
            {
                notedataTable.Rows.Clear();
                var noteRow = notedataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = CreatedElement.VideoEventID;
                noteRow["notes_line"] = CreatedElement.Item.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = CreatedElement.Item.WordCount;
                noteRow["notes_start"] = CreatedElement.Item.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = CreatedElement.Item.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = false;
                noteRow["notes_serverid"] = 1;
                noteRow["notes_syncerror"] = string.Empty;

                notedataTable.Rows.Add(noteRow);

                int ItemIndex = CreateNotes(notedataTable);
                _ManageMedia.SetNoteID(ItemIndex, CreatedElement.Item);
            }
        }

        private void _ManageMedia_NotesDeleted(object sender, ManageMedia_UserControl.Controls.NotesChangedArgs e)
        {
            foreach (var DeletedElement in e.ChangedNotes)
            {
                DeletedNote(DeletedElement.Item);
            }
        }

        private void _ManageMedia_NotesChanged(object sender, ManageMedia_UserControl.Controls.NotesChangedArgs e)
        {

            foreach (var DeletedElement in e.ChangedNotes)
            {
                DeletedNote(DeletedElement.Item);
            }


            var notedataTable = new DataTable();
            notedataTable.Columns.Add("notes_id", typeof(int));
            notedataTable.Columns.Add("fk_notes_videoevent", typeof(int));
            notedataTable.Columns.Add("notes_line", typeof(string));
            notedataTable.Columns.Add("notes_wordcount", typeof(int));
            notedataTable.Columns.Add("notes_index", typeof(int));
            notedataTable.Columns.Add("notes_start", typeof(string));
            notedataTable.Columns.Add("notes_duration", typeof(string));
            notedataTable.Columns.Add("notes_createdate", typeof(string));
            notedataTable.Columns.Add("notes_modifydate", typeof(string));
            notedataTable.Columns.Add("notes_isdeleted", typeof(bool));
            notedataTable.Columns.Add("notes_issynced", typeof(bool));
            notedataTable.Columns.Add("notes_serverid", typeof(long));
            notedataTable.Columns.Add("notes_syncerror", typeof(string));


            foreach (var CreatedElement in e.ChangedNotes)
            {
                notedataTable.Rows.Clear();
                var noteRow = notedataTable.NewRow();

                noteRow["notes_id"] = -1;
                noteRow["fk_notes_videoevent"] = CreatedElement.VideoEventID;
                noteRow["notes_line"] = CreatedElement.Item.Text.Replace("'", "''"); ;
                noteRow["notes_wordcount"] = CreatedElement.Item.WordCount;
                noteRow["notes_start"] = CreatedElement.Item.Start.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_duration"] = CreatedElement.Item.Duration.ToString(@"hh\:mm\:ss\.fff");
                noteRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                noteRow["notes_isdeleted"] = false;
                noteRow["notes_issynced"] = false;
                noteRow["notes_serverid"] = 1;
                noteRow["notes_syncerror"] = string.Empty;

                notedataTable.Rows.Add(noteRow);

                int ItemIndex = CreateNotes(notedataTable);
                _ManageMedia.SetNoteID(ItemIndex, CreatedElement.Item);
            }

        }

        private void DeletedNote(TextItem textItem)
        {
            try
            {
                DataManagerSqlLite.DeleteNotesById(textItem.NoteID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CreateNotes(DataTable dataTable)
        {
            try
            {
                var insertedId = DataManagerSqlLite.InsertRowsToNotes(dataTable);
                return insertedId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return -1;
        }

        public void SetProjectID(int projectID)
        {
            //_ProjectID = projectID;
            RefreshData();
        }

        private void RefreshData()
        {
            #region Planned Text

            List<CBVPlanningHead> PlanningHead = DataManagerSqlLite.GetPlanningHead();
            List<CBVPlanning> PlanningData = DataManagerSqlLite.GetPlanning(selectedProjectEvent.projectId);

            if (PlanningHead?.Count > 0 && PlanningData?.Count > 0)
            {
                List<PlannedTextGroup> PlannedTextGroups = new List<PlannedTextGroup>();
                for (int i = 0; i < PlanningHead.Count; i++)
                {
                    PlannedTextGroup PlannedGroup = new PlannedTextGroup()
                    {
                        GroupID = PlanningHead[i].planninghead_id,
                        GroupName = PlanningHead[i].planninghead_name,
                        PlannedTexts = new List<PlannedText>()
                    };
                    PlannedTextGroups.Add(PlannedGroup);
                }

                for (int i = 0; i < PlanningData.Count; i++)
                {
                    CBVPlanning CurrentPlanning = PlanningData[i];
                    for (int x = 0; x < PlannedTextGroups.Count; x++)
                    {
                        PlannedTextGroup CurrentGroup = PlannedTextGroups[x];
                        if (CurrentPlanning.fk_planning_head == CurrentGroup.GroupID)
                        {
                            CurrentGroup.PlannedTexts.Add(
                            new PlannedText()
                            {
                                PlannedTextID = CurrentPlanning.planning_id,
                                Text = CurrentPlanning.planning_notesline,
                                SortKey = CurrentPlanning.planning_sort
                            });
                        }
                    }
                }

                _ManageMedia.LoadPlannedText(PlannedTextGroups);
            }


            #endregion

            #region Video Events

            List<CBVMedia> MediaHead = DataManagerSqlLite.GetMedia();
            List<CBVVideoEvent> VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true);


            List<ManageMedia> mediaList = new List<ManageMedia>();
            foreach (var item in VideoEventData)
            {
                ManageMedia media = new ManageMedia();

                media.Color = (Color)ColorConverter.ConvertFromString(MediaHead[item.fk_videoevent_media - 1].media_color);

                if (item.videosegment_data != null && item.videosegment_data.Count > 0)
                {
                    media.mediaData = item.videosegment_data[0].videosegment_media;
                }

                if (item.fk_videoevent_media == 1 || item.fk_videoevent_media == 4)
                {
                    media.mediaType = ManageMediaType.Image;
                }
                else if (item.fk_videoevent_media == 2)
                {
                    media.mediaType = ManageMediaType.Video;
                }

                media.VideoEventID = item.videoevent_id;
                media.ProjectId = selectedProjectEvent.projectId;
                media.TrackId = item.videoevent_track;
                if (item.notes_data != null)
                {
                    List<TextItem> textItems = new List<TextItem>();
                    foreach (var note in item.notes_data)
                    {
                        TextItem textItem = new TextItem();
                        textItem.NoteID = note.notes_id;
                        textItem.Text = note.notes_line;
                        textItem.Start = TimeSpan.Parse(note.notes_start);
                        textItem.Duration = TimeSpan.Parse(note.notes_duration);

                        textItems.Add(textItem);
                    }
                    media.RecordedTextList = textItems;
                }
                media.Duration = TimeSpan.Parse(item.videoevent_duration);
                media.StartTime = TimeSpan.Parse(item.videoevent_start);

                mediaList.Add(media);
            }

            _ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);

            _ManageMedia.LoadEvents(mediaList);

            #endregion
        }
        

    }
}
