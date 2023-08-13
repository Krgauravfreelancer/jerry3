using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using NotesRead_UserControl.Models;
using Sqllite_Library.Business;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Notes_Wrapper
{
    internal class NotesViewModel: ViewModelBase
    {
        DataTable dtable = null;
        NotesRead_UserControl.NotesReadCtrl myCtrl;
        public ICommand DisplayNotes { get; private set; }

        //private ObservableCollection<NoteItem> _noteItems;

        //public ObservableCollection<NoteItem> NoteItems
        //{
        //    get => _noteItems;
        //    set => Set(nameof(NoteItems), ref _noteItems, value);
        //}

        public NotesViewModel(NotesRead_UserControl.NotesReadCtrl ctrl)
        {
            DisplayNotes  = new RelayCommand(() => { GetNotes(); });
            myCtrl = ctrl;//new NotesRead_UserControl.NotesReadCtrl();
        }

        private void GetNotes()
        {
            //DataTable dt;

            var notes  = DataManagerSqlLite.GetNotes();
            dtable = TableBuilder.BuildNotesDataTable();
            
            foreach (var note in notes) 
            {
                dtable = TableBuilder.AddNoteRow(dtable, note.notes_id, note.notes_line);
            }


            //pass the dtable to the UserControl public method
            myCtrl.SetNotesList(dtable);
        }

        public DataTable GetCurrentData()
        {
            return dtable;
        }

        public void RefreshNotes(DataTable dt)
        {
            this.dtable = dt;
            myCtrl.SetNotesList(dt);
        }
    }
}
