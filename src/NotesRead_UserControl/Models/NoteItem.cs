namespace NotesRead_UserControl.Models
{
    public class NoteItem
    {
        private int id;
        private string item;
        private int index;

        public int Id { get => id; set => id = value; }
        public string Item { get => item; set => item = value; }
        public int Index { get => index; set => index = value; }
    }
}
