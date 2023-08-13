using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Notes_Wrapper
{
    /// <summary>
    /// Interaction logic for ManageNotes.xaml
    /// </summary>
    public partial class ManageNotes : Window
    {
        private MainWindow parent;
        public ManageNotes(MainWindow mainWin, DataTable dt)
        {
            InitializeComponent();
            this.parent = mainWin;
            notesManagementCtrl.LoadItemsForEdit(dt);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {  
            DataTable dt = notesManagementCtrl.GetSavedData();
            if(dt != null)
                parent.UpdateNotes(dt);
        }

        private DataTable BuildNotesDataTable()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));

            return dtNotes;
        }

        public DataTable AddNoteRow(DataTable dt, int notesId, string notesLine)
        {
            //DataRow dRow = null;

            var dRow = dt.NewRow();

            dRow["notes_id"] = notesId;
            dRow["notes_line"] = notesLine;
            dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            dRow["fk_notes_videoevent"] = 1;
            dRow["notes_wordcount"] = 1;
            dt.Rows.Add(dRow);

            return dt;
        }
    }
}
