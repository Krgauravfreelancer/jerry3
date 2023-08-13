using GalaSoft.MvvmLight;
using NotesRead_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesRead_UserControl
{
    public class NotesReadViewModel:ViewModelBase
    {
        private ObservableCollection<NoteItem> _noteItems;

        public ObservableCollection<NoteItem> NoteItems 
        {   
            get => _noteItems; 
            set => Set(nameof(NoteItems),ref _noteItems,value); 
        }

        public NotesReadViewModel()
        {
           NoteItems = new ObservableCollection<NoteItem>();
        }

        public void SetNoteItems(DataTable dt) 
        {
            NoteItems.Clear();
            List<NoteItem> items = new List<NoteItem>();

            foreach (DataRow item in dt.Rows) 
            {
                items.Add(new NoteItem()
                {
                    Id = Convert.ToInt32(item["id"]),
                    Item = Convert.ToString(item["noteitem"])
                });
            }
            NoteItems = new ObservableCollection<NoteItem>(items);             
        }
    }
}
