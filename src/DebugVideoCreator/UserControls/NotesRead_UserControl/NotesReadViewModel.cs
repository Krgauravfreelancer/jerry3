using GalaSoft.MvvmLight;
using NotesRead_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

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
                    Id = Convert.ToInt32(item["notes_id"]),
                    Item = Convert.ToString(item["notes_line"]),
                    Index = Convert.ToInt32(item["notes_index"]),
                });
            }
            NoteItems = new ObservableCollection<NoteItem>(items);             
        }
    }
}
