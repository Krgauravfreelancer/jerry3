using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotesRead_UserControl.Models
{
    public class NoteItem
    {
        private int id;
        private string item;

        public int Id { get => id; set => id = value; }
        public string Item { get => item; set => item = value; }
    }
}
