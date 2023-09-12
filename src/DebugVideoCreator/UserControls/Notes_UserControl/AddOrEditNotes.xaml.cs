using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Notes_UserControl
{
    /// <summary>
    /// Interaction logic for AddOrEditNotes.xaml
    /// </summary>
    public partial class AddOrEditNotes : UserControl
    {
        int selectedVideoEventId = -1;
        CBVNotes selectedNote;
        public AddOrEditNotes(int videoEventId)
        {
            InitializeComponent();
            selectedVideoEventId = videoEventId;
            btnSave.Content = "Save To DB";
        }

        public AddOrEditNotes(int videoEventId, CBVNotes note)
        {
            InitializeComponent();
            selectedVideoEventId = videoEventId;
            selectedNote = note;
            txtNotes.Text = selectedNote.notes_line;
            btnSave.Content = "Update To DB";
        }

        #region == Events ==

        private void txtNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNotes == null) return;
            var text = txtNotes.Text;
            var tbNotes = NotesHelpers.GetEnhancedNotes(text);
            tbNotes.HorizontalAlignment = HorizontalAlignment.Left;
            tbNotes.TextWrapping = TextWrapping.Wrap;
            tbNotes.Width = 550;
            scrollviewerTb.Content = tbNotes;
        }

        private void btnInsertShortPause_Click(object sender, RoutedEventArgs e)
        {
            var caretIndex = txtNotes.CaretIndex;
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.SHORTPAUSE, txtNotes.Text, txtNotes.CaretIndex);
            txtNotes.Focus();
            txtNotes.CaretIndex = caretIndex + NotesHelpers.SHORTPAUSE.Length + 2;
        }

        private void btnInsertMediumPause_Click(object sender, RoutedEventArgs e)
        {
            var caretIndex = txtNotes.CaretIndex;
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.MEDIUMPAUSE, txtNotes.Text, txtNotes.CaretIndex);
            txtNotes.Focus();
            txtNotes.CaretIndex += caretIndex + NotesHelpers.MEDIUMPAUSE.Length + 2;
        }

        private void btnInsertLongPause_Click(object sender, RoutedEventArgs e)
        {
            var caretIndex = txtNotes.CaretIndex; 
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.LONGPAUSE, txtNotes.Text, txtNotes.CaretIndex);
            txtNotes.Focus();
            txtNotes.CaretIndex = caretIndex + NotesHelpers.LONGPAUSE.Length + 2; 
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_index", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));

            var dRow = dtNotes.NewRow();
            
            dRow["notes_line"] = txtNotes.Text;
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["fk_notes_videoevent"] = selectedVideoEventId;
            dRow["notes_wordcount"] = txtNotes.Text.Split(' ').Length;
            dtNotes.Rows.Add(dRow);

            if (Convert.ToString(btnSave.Content) == "Save To DB")
            {
                dRow["notes_id"] = -1;
                dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dRow["notes_index"] = 0;
                DataManagerSqlLite.InsertRowsToNotes(dtNotes);
            }
            else
            {
                dRow["notes_id"] = selectedNote.notes_id;
                dRow["notes_index"] = selectedNote.notes_index;
                DataManagerSqlLite.UpdateRowsToNotes(dtNotes);
            }

            
            ((Window)this.Parent).Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }

        #endregion
    }
}
