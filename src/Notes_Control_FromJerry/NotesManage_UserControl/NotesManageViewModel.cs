using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NotesManage_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NotesManage_UserControl
{
    public class NotesManageViewModel: ViewModelBase
    {
        DataTable dt = null;
        public ObservableCollection<NoteItem> AllNotes { get; set; }
        public List<NoteItem> PreviousNotes { get; set; }
        public ICommand SaveNotes { get; private set; }
        public NotesManageViewModel() 
        { 
            AllNotes = new ObservableCollection<NoteItem>();
            //SaveNotes = new RelayCommand(() => { SaveItems(); });
        }

        public DataTable SaveItems()
        {
            dt = TableBuilder.BuildNotesDataTable();

            dt = TableBuilder.GetNotesTable(dt, AllNotes.ToList());

            return dt;
        }

        internal void LoadItems(DataTable dt)
        {
            if(dt!=null)
            { 
                List<NoteItem> items = dt.AsEnumerable().Select(row => 
                                                            new NoteItem { 
                                                                            Id = row.Field<int>("id"),
                                                                            Item = row.Field<string>("noteitem")
                                                                          } 
                                                            ).ToList();
                PreviousNotes = items;
                //PreviousNotes.CopyTo()
                AllNotes = new ObservableCollection<NoteItem>(items);
            }
        } 
        
        internal ObservableCollection<NoteItem> GetAllNotes()
        { return AllNotes; }

        internal void SetNoteItems(ObservableCollection<NoteItem> items) 
        {
            AllNotes = items;
        }
    }
}
