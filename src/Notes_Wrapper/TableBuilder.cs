using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notes_Wrapper
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
    }
}
