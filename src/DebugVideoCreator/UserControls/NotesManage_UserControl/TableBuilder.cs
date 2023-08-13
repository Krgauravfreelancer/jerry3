using NotesManage_UserControl.Models;
using System.Collections.Generic;
using System.Data;

namespace NotesManage_UserControl
{
    internal class TableViewBuilder
    {
        public static DataTable BuildNotesDataTableView()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("notes_id", typeof(int));
            dt.Columns.Add("notes_line", typeof(string));
            dt.Columns.Add("notes_index", typeof(string));
            return dt;
        }

        public static DataTable AddNoteRowView(DataTable dt, int notes_id, string notes_line, int notes_index)
        {
            var dRow = dt.NewRow();
            dRow["notes_id"] = notes_id;
            dRow["notes_line"] = notes_line;
            dRow["notes_index"] = notes_index;
            dt.Rows.Add(dRow);
            return dt;
        }
        public static DataTable GetNotesTable(DataTable dt, List<NoteItem> items)
        {
            foreach (var note in items)
            {
                var dRow = dt.NewRow();

                dRow["notes_id"] = note.Id;
                dRow["notes_line"] = note.Item;
                dRow["notes_index"] = note.Index;
                dt.Rows.Add(dRow);
            }

            return dt;
        }
    }
}
