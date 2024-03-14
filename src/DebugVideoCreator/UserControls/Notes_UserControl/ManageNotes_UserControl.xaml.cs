using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Notes_UserControl
{
    /// <summary>
    /// Interaction logic for ManageNotes.xaml
    /// </summary>
    public partial class ManageNotesUserControl : UserControl
    {
        private Notes_UserControl parent;
        public ManageNotesUserControl(Notes_UserControl mainWin, DataTable dt)
        {
            InitializeComponent();
            this.parent = mainWin;
            notesManagementCtrl.LoadItemsForEdit(dt);
            this.Loaded += UserControl1_Loaded;
        }

        void UserControl1_Loaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Closing += UserControl_ContextMenuClosing;
        }


        void UserControl_ContextMenuClosing(object sender, global::System.ComponentModel.CancelEventArgs e)
        {
            DataTable dt = notesManagementCtrl.GetSavedData();
            if (dt != null)
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

            dtNotes.Columns.Add("notes_isdeleted", typeof(bool));
            dtNotes.Columns.Add("notes_issynced", typeof(bool));
            dtNotes.Columns.Add("notes_serverid", typeof(Int64));
            dtNotes.Columns.Add("notes_syncerror", typeof(string));

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

            dRow["notes_isdeleted"] = false;
            dRow["notes_issynced"] = true;
            dRow["notes_serverid"] = 1;
            dRow["notes_syncerror"] = "";


            dt.Rows.Add(dRow);

            return dt;
        }


    }
}
