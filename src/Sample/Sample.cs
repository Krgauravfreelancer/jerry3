using System;
using System.Data;

namespace Sample
{
    public static class Sample
    {

        public static DataTable BuildNotesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("noteitem", typeof(string));
            return dt;
        }

        public static DataTable AddNoteRow(DataTable dt, int id, string txt)
        {
            DataRow dRow = null;

            dRow = dt.NewRow();

            dRow["id"] = id;
            dRow["noteitem"] = txt;
            dt.Rows.Add(dRow);

            return dt;
        }

        public static int GetWordsCount(int rows = 10)
        {
            var dt = BuildNotesDataTable();
            for (int i = 0; i < rows; i++)
            {
                AddNoteRow(dt, i + 1, $"This is a sample note {i}");
            }
            var TextField = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                TextField += Convert.ToString(dt.Rows[i]["noteitem"]) + " ";

            }
            string[] words = TextField.Split(' ');
            return words.Length;
        }


        public static int GetCharactersCount(int rows = 10)
        {
            var dt = BuildNotesDataTable();
            for (int i = 0; i < rows; i++)
            {
                AddNoteRow(dt, i + 1, $"This is a sample note {i}");
            }
            var TextField = string.Empty;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                TextField += Convert.ToString(dt.Rows[i]["noteitem"]);

            }
            return TextField.Length;
        }

    }
}
