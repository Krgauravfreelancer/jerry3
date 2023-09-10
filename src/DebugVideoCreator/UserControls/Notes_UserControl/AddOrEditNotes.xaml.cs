using Sqllite_Library.Business;
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
        public AddOrEditNotes(int videoEventId)
        {
            InitializeComponent();
            selectedVideoEventId = videoEventId;
        }

        private void txtNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNotes == null) return;
            var text = txtNotes.Text;
            var tbNotes = NotesHelpers.GetEnhancedNotes(text);
            tbNotes.HorizontalAlignment = HorizontalAlignment.Left;
            tbNotes.TextWrapping = TextWrapping.Wrap;
            tbNotes.Width = 550;
            scrollviewerTb.Content = NotesHelpers.GetEnhancedNotes(text);
        }

        private void btnInsertShortPause_Click(object sender, RoutedEventArgs e)
        {
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.SHORTPAUSE, txtNotes.Text);
            txtNotes.Focus();
            txtNotes.CaretIndex = txtNotes.Text.Length;
        }


        private void btnInsertMediumPause_Click(object sender, RoutedEventArgs e)
        {
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.MEDIUMPAUSE, txtNotes.Text);
            txtNotes.Focus();
            txtNotes.CaretIndex = txtNotes.Text.Length;
        }

        private void btnInsertLongPause_Click(object sender, RoutedEventArgs e)
        {
            txtNotes.Text = NotesHelpers.InsertPause(NotesHelpers.LONGPAUSE, txtNotes.Text);
            txtNotes.Focus();
            txtNotes.CaretIndex = txtNotes.Text.Length;
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
            dRow["notes_index"] = 0;
            dRow["notes_id"] = -1;
            dRow["notes_line"] = txtNotes.Text;
            dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["fk_notes_videoevent"] = selectedVideoEventId;
            dRow["notes_wordcount"] = txtNotes.Text.Split(' ').Length;
            dtNotes.Rows.Add(dRow);


            DataManagerSqlLite.InsertRowsToNotes(dtNotes);
            ((Window)this.Parent).Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ((Window)this.Parent).Close();
        }
    }
}
