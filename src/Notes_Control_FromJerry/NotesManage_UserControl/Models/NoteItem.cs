using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesManage_UserControl.Models
{
    public class NoteItem: ObservableObject
    {
        private int id;
        private string item;

        public int Id { get => id; set => id = value; }
        public string Item { get => item; set => item = value; }
    }
}
