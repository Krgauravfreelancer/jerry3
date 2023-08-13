using NotesManage_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NotesManage_UserControl
{
    internal class TableBuilder
    {
        public static DataTable BuildNotesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("noteitem", typeof(string));
            return dt;
        }

        public static DataTable AddNoteRow(DataTable dt, Int32 id, String txt)
        {
            DataRow dRow = null;

            dRow = dt.NewRow();

            dRow["id"] = id;
            dRow["noteitem"] = txt;
            dt.Rows.Add(dRow);

            return dt;
        }

        public static DataTable GetNotesTable(DataTable dt,List<NoteItem> items)
        {
            DataRow dRow = null;

            foreach(var note in items)
            {
                dRow = dt.NewRow();

                dRow["id"] = note.Id;
                dRow["noteitem"] = note.Item;
                dt.Rows.Add(dRow);
            }        

            return dt;
        }
    }
}
