using Sqllite_Library.Business;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Notes_UserControl
{
    /// <summary>
    /// Interaction logic for AddOrEditNotes.xaml
    /// </summary>
    public partial class AddOrEditNotes : System.Windows.Controls.UserControl
    {
        string shortPauseText = "{pause-250ms}";
        string mediumPauseText = "{pause-500ms}";
        string longPauseText = "{pause-1000ms}";
        int selectedVideoEventId = -1;
        public AddOrEditNotes(int videoEventId)
        {
            InitializeComponent();
            selectedVideoEventId = videoEventId;
        }

        private void txtNotes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbNotes == null) return;
            tbNotes.Inlines.Clear();
            var text = txtNotes.Text;

            if (!string.IsNullOrEmpty(text))
            {
                var array = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in array)
                {
                    if (shortPauseText == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.LightBlue });
                    else if (mediumPauseText == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Blue });
                    else if (longPauseText == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Red });
                    else
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Black });
                }
            }
        }

        private void btnInsertShortPause_Click(object sender, RoutedEventArgs e)
        {
            InsertPause(shortPauseText);
        }

        private void InsertPause(string pauseText)
        {
            var stringText = txtNotes.Text;
            if (!string.IsNullOrEmpty(stringText))
            {
                stringText += $" {pauseText} ";
                txtNotes.Text = stringText;
            }
            else
            {
                txtNotes.Text = $"{pauseText} ";
            }
        }

        private void btnInsertMediumPause_Click(object sender, RoutedEventArgs e)
        {
            InsertPause(mediumPauseText);
        }

        private void btnInsertLongPause_Click(object sender, RoutedEventArgs e)
        {
            InsertPause(longPauseText);
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
