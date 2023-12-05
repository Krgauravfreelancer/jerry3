using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Threading;

namespace Notes_UserControl
{
    /// <summary>
    /// Interaction logic for Notes_UserControl.xaml
    /// </summary>
    public partial class Notes_UserControl : UserControl
    {
        private int selectedproject_id;
        private int selectedVideoEventId = -1;
        public event EventHandler locAudioAddedEvent;
        public event EventHandler locAudioShowEvent;
        public event EventHandler<int> locAudioManageEvent;

        // Events added for handling to server first
        public event EventHandler<DataTable> saveNotesEvent;
        public event EventHandler<DataTable> saveSingleNoteEvent;
        public event EventHandler<DataTable> updateSingleNoteEvent;
        public event EventHandler<CBVNotes> deleteSingleNoteEvent;

        private bool ReadOnly;
        public Notes_UserControl()
        {
            InitializeComponent();
        }

        #region == Public Functions ==

        

        public void InitializeNotes(int project_id, int videoEvent_id = -1, bool readonlyMode = false)
        {
            selectedproject_id = project_id;
            selectedVideoEventId = videoEvent_id;
            ReadOnly = readonlyMode;
            ResetContextMenu();
            DisplayAllNotesForSelectedVideoEvent();
        }


        private void ResetContextMenu()
        {
            if (ReadOnly)
            {
                var contextMenu = NotesContextMenu as ContextMenu;
                for (int i = 0; i < contextMenu.Items.Count; i++)
                {
                    MenuItem item = contextMenu.Items[i] as MenuItem;
                    if (item != null)
                    {
                        item.IsEnabled = false;
                    }
                }
            }
        }

        public static void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                action.Invoke();
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        public void HandleVideoEventSelectionChanged(int videoEventId = -1)
        {
            if (videoEventId > 0) selectedVideoEventId = videoEventId;
            if (selectedVideoEventId > -1)
            {
                var notes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
                if (notes.Count <= 0)
                {
                    var dtNotes = BuildNotesDataTableForDB();
                    dtNotes = AddNoteRowForDBTable(dtNotes, 1, "Launch Visual Studio, and open the New Project dialog box");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 2, "In the Window category, select the \"WPF User control Library\"");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 3, "Name the new project");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 4, "Click OK to create the project");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 5, "In Solution Explorer, rename UserControl1 to an appropriate name");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 6, "Add System.Windows.forms");
                    if (saveNotesEvent != null)
                        saveNotesEvent.Invoke(this, dtNotes);
                    else
                        DelayAction(1000, new Action(() => { saveNotesEvent.Invoke(this, dtNotes); }));
                }
                else
                    DisplayAllNotesForSelectedVideoEvent();
            }

        }

        public void DisplayAllNotesForSelectedVideoEvent()
        {
            var allNotes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
            stackPanelNotes.Children.Clear();
            int i = 0;
            foreach (var note in allNotes)
                stackPanelNotes.Children.Add(AddNotes(note, ++i));
            if (locAudioAddedEvent != null)
                locAudioAddedEvent.Invoke(this, new EventArgs());
        }

        #endregion

        #region === ContextMenu Events ===

        private void ContextMenu_ManageNotesClickEvent(object sender, RoutedEventArgs e)
        {
            if (selectedVideoEventId <= -1)
            {
                MessageBox.Show("No Video Event Selected", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var dt = GetNotesDataToView(selectedVideoEventId);
            if (dt?.Rows != null && dt?.Rows?.Count > 0)
            {
                var manageNotesUserControl = new ManageNotesUserControl(this, dt);

                var window = new Window
                {
                    Title = "Manage Notes",
                    Content = manageNotesUserControl,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    RenderSize = manageNotesUserControl.RenderSize,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show("No Notes Rows to Manage", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ContextMenu_AddNewNoteClickEvent(object sender, RoutedEventArgs e)
        {
            var addOrEditNotes = new AddOrEditNotes(selectedVideoEventId);
            var window = new Window
            {
                Title = "Add or Edit Notes",
                Content = addOrEditNotes,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();

            addOrEditNotes.saveSingleNoteEvent += AddOrEditNotes_SaveSingleNoteEvent;
            addOrEditNotes.updateSingleNoteEvent += AddOrEditNotes_updateSingleNoteEvent;

            DisplayAllNotesForSelectedVideoEvent();
        }

        private void AddOrEditNotes_updateSingleNoteEvent(object sender, DataTable datatable)
        {
            updateSingleNoteEvent.Invoke(sender, datatable);
        }

        private void AddOrEditNotes_SaveSingleNoteEvent(object sender, DataTable datatable)
        {
            saveSingleNoteEvent.Invoke(sender, datatable);
        }

        private void ContextMenu_GenerateVoiceAllNotesClickEvent(object sender, RoutedEventArgs e)
        {
            if (locAudioShowEvent != null)
                locAudioShowEvent.Invoke(this, new EventArgs());
        }


        #endregion

        #region == Event to dynamically manage notes ==

        private Border AddNotes(CBVNotes note, int index)
        {
            string text = $"{index}. {note.notes_line}";
            var myBorder1 = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 5)
            };

            TextBlock txt1 = NotesHelpers.GetEnhancedNotes(text);
            txt1.TextWrapping = TextWrapping.WrapWithOverflow;
            txt1.Foreground = Brushes.Black;
            txt1.FontSize = 12;
            txt1.Padding = new Thickness(5);
            //txt1.Width = 350;
            txt1.HorizontalAlignment = HorizontalAlignment.Left;
            myBorder1.ContextMenu = GetItemContextMenu(note);
            myBorder1.Child = txt1;
            return myBorder1;
        }

        public ContextMenu GetItemContextMenu(CBVNotes note)
        {
            var contextMenu = new ContextMenu();

            //1st Item
            MenuItem manageNoteQuery = new MenuItem
            {
                Header = "Manage All Notes"
            };
            manageNoteQuery.Click += ContextMenu_ManageNotesClickEvent;
            manageNoteQuery.IsEnabled = !ReadOnly;
            contextMenu.Items.Add(manageNoteQuery);



            MenuItem addNewNoteQuery = new MenuItem
            {
                Header = "Add New Note"
            };
            addNewNoteQuery.Click += ContextMenu_AddNewNoteClickEvent;
            addNewNoteQuery.IsEnabled = !ReadOnly; 
            contextMenu.Items.Add(addNewNoteQuery);

            var voiceGenerateAllNotesQuery = new MenuItem
            {
                Header = "Generate Voice For All Notes"
            };
            voiceGenerateAllNotesQuery.Click += ContextMenu_GenerateVoiceAllNotesClickEvent;
            voiceGenerateAllNotesQuery.IsEnabled = !ReadOnly; 
            contextMenu.Items.Add(voiceGenerateAllNotesQuery);

            contextMenu.Items.Add(new Separator());

            MenuItem editNoteQuery = new MenuItem
            {
                Header = "Advanced Edit"
            };
            editNoteQuery.Click += (sender, e) =>
            {
                var uc = new AddOrEditNotes(selectedVideoEventId, note);
                var window = new Window
                {
                    Title = $"Edit Note with Id - {note.notes_id}",
                    Content = uc,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    ResizeMode = ResizeMode.NoResize,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                var result = window.ShowDialog();
                DisplayAllNotesForSelectedVideoEvent();
            };
            editNoteQuery.IsEnabled = !ReadOnly;
            contextMenu.Items.Add(editNoteQuery);

            var deleteNoteQuery = new MenuItem
            {
                Header = "Delete"
            };
            deleteNoteQuery.Click += (sender, e) =>
            {
                var result = MessageBox.Show($"Are you sure you want to delete notes \n \n{note.notes_line}", $"Delete Note - {note.notes_id}", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    deleteSingleNoteEvent.Invoke(sender, note);
                }

            };
            deleteNoteQuery.IsEnabled = !ReadOnly;
            contextMenu.Items.Add(deleteNoteQuery);

            var voiceGenerateNoteQuery = new MenuItem
            {
                Header = "Manage Audio"
            };
            voiceGenerateNoteQuery.Click += (sender, e) =>
            {
                if (locAudioManageEvent != null)
                    locAudioManageEvent.Invoke(this, note.notes_id);

            };
            voiceGenerateNoteQuery.IsEnabled = !ReadOnly;
            contextMenu.Items.Add(voiceGenerateNoteQuery);
            return contextMenu;
        }


        #endregion


        private DataTable BuildNotesDataTableForDB()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_start", typeof(string));
            dtNotes.Columns.Add("notes_duration", typeof(int));
            dtNotes.Columns.Add("notes_index", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));

            dtNotes.Columns.Add("notes_isdeleted", typeof(bool));
            dtNotes.Columns.Add("notes_issynced", typeof(bool));
            dtNotes.Columns.Add("notes_serverid", typeof(Int64));
            dtNotes.Columns.Add("notes_syncerror", typeof(string));


            return dtNotes;
        }

        private DataTable AddNoteRowForDBTable(DataTable dt, int notesId, string notesLine)
        {
            var dRow = dt.NewRow();
            dRow["notes_index"] = 0;
            dRow["notes_id"] = notesId;
            dRow["notes_line"] = notesLine;
            dRow["notes_start"] = "00:00:00.000";
            dRow["notes_duration"] = 0;
            dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["fk_notes_videoevent"] = selectedVideoEventId;
            dRow["notes_wordcount"] = notesLine.Split(' ').Length;

            dRow["notes_isdeleted"] = false;
            dRow["notes_issynced"] = true;
            dRow["notes_serverid"] = 1;
            dRow["notes_syncerror"] = "";


            dt.Rows.Add(dRow);
            return dt;
        }

        private DataTable GetNotesDataToView(int selectedVideoEventId)
        {
            var notes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
            var dtable = TableViewBuilder.BuildNotesDataTableView();
            foreach (var note in notes)
            {
                dtable = TableViewBuilder.AddNoteRowView(dtable, note.notes_id, note.notes_line, note.notes_index);
            }
            return dtable;
        }

        public void UpdateNotes(DataTable dt)
        {
            // notesReadCtrl.SetNotesList(dt);
            // Lets update the data in DB as well for next time as well.
            var notes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
            var notesTable = BuildNotesDataTableForDB();
            int i = 1;

            foreach (DataRow dr in dt.Rows)
            {
                var note = notes.Find(x => x.notes_id == Convert.ToInt32(dr["notes_id"]));

                var dRow = notesTable.NewRow();
                dRow["notes_index"] = i++;
                dRow["notes_id"] = note.notes_id;
                dRow["notes_line"] = note.notes_line;
                dRow["notes_start"] = note.notes_start;
                dRow["notes_duration"] = note.notes_duration;
                dRow["notes_createdate"] = note.notes_createdate;
                dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["fk_notes_videoevent"] = note.fk_notes_videoevent;
                dRow["notes_wordcount"] = note.notes_wordcount;


                dRow["notes_isdeleted"] = false;
                dRow["notes_issynced"] = true;
                dRow["notes_serverid"] = 1;
                dRow["notes_syncerror"] = "";

                notesTable.Rows.Add(dRow);
            }
            DataManagerSqlLite.UpdateRowsToNotes(notesTable);
            DisplayAllNotesForSelectedVideoEvent();
        }
    }
}
