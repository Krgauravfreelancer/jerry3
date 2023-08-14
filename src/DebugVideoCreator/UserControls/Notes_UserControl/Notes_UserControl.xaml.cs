﻿using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Notes_UserControl
{
    /// <summary>
    /// Interaction logic for Notes_UserControl.xaml
    /// </summary>
    public partial class Notes_UserControl : UserControl
    {
        private int selectedproject_id;
        private int selectedVideoEventId = -1;
        public Notes_UserControl()
        {
            InitializeComponent();
        }

        #region == Public Functions ==

        public void SetSelectedProjectId(int project_id, int videoEvent_id = -1)
        {
            selectedproject_id = project_id;
            selectedVideoEventId = videoEvent_id;
            VideoEventSelectionChanged();
        }

        #endregion

        #region == Events ==


        private void btnSaveNotes_Click(object sender, RoutedEventArgs e)
        {

            var dt = notesReadCtrl.GetUpdatedViewTable();
            
            // Lets update the data in DB as well for next time as well.
            var notes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
            var notesTable = BuildNotesDataTableForDB();

            foreach (DataRow dr in dt.Rows)
            {
                var note = notes.Find(x => x.notes_id == Convert.ToInt32(dr["notes_id"]));

                var dRow = notesTable.NewRow();
                dRow["notes_index"] = Convert.ToInt32(dr["notes_index"]);
                dRow["notes_id"] = note.notes_id;
                dRow["notes_line"] = Convert.ToString(dr["notes_line"]);
                dRow["notes_createdate"] = note.notes_createdate;
                dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["fk_notes_videoevent"] = note.fk_notes_videoevent;
                dRow["notes_wordcount"] = note.notes_wordcount;
                notesTable.Rows.Add(dRow);
            }
            DataManagerSqlLite.UpdateRowsToNotes(notesTable);

            MessageBox.Show("Notes are updated to DB successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ContextMenu_ManageNotes_ClickEvent(object sender, RoutedEventArgs e)
        {
            if(selectedVideoEventId <= -1)
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
                var result = window.ShowDialog();
                if (result.HasValue)
                {
                    // do something on closing !!
                }
            }
            else
            {
                MessageBox.Show("No Notes Rows to Manage", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VideoEventSelectionChanged()
        {
            if (selectedVideoEventId > -1)
            {
                var notes = DataManagerSqlLite.GetNotes(selectedVideoEventId);
                if (notes.Count <= 0)
                {
                    //var result = MessageBox.Show("No Notes present for selected videoevent, do you want to add template notes?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    //if(result == MessageBoxResult.Yes)
                    //{
                    var dtNotes = BuildNotesDataTableForDB();
                    dtNotes = AddNoteRowForDBTable(dtNotes, 1, "Launch Visual Studio, and open the New Project dialog box");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 2, "In the Window category, select the \"WPF User control Library\"");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 3, "Name the new project");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 4, "Click OK to create the project");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 5, "In Solution Explorer, rename UserControl1 to an appropriate name");
                    dtNotes = AddNoteRowForDBTable(dtNotes, 6, "Add System.Windows.forms");
                    var insertedId = DataManagerSqlLite.InsertRowsToNotes(dtNotes);
                    if (insertedId > 0)
                    {
                        var dtable = GetNotesDataToView(selectedVideoEventId);
                        notesReadCtrl.SetNotesList(dtable);
                    }
                    //}
                }
                else
                {
                    var dtable = GetNotesDataToView(selectedVideoEventId);
                    notesReadCtrl.SetNotesList(dtable);
                }
            }
        }

        #endregion


        private DataTable BuildNotesDataTableForDB()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_index", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));
            return dtNotes;
        }

        private DataTable AddNoteRowForDBTable(DataTable dt, int notesId, string notesLine)
        {
            var dRow = dt.NewRow();
            dRow["notes_index"] = 0;
            dRow["notes_id"] = notesId;
            dRow["notes_line"] = notesLine;
            dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["fk_notes_videoevent"] = selectedVideoEventId;
            dRow["notes_wordcount"] = notesLine.Split(' ').Length;
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
            notesReadCtrl.SetNotesList(dt);
            
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
                dRow["notes_createdate"] = note.notes_createdate;
                dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["fk_notes_videoevent"] = note.fk_notes_videoevent;
                dRow["notes_wordcount"] = note.notes_wordcount;
                notesTable.Rows.Add(dRow);
            }
            DataManagerSqlLite.UpdateRowsToNotes(notesTable);
        }
    }
}