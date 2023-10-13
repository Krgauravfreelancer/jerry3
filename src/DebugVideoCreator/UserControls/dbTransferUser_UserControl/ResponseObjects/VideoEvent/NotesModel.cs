using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class NotesModel: NotesModelPost
    {
        public int notes_id { get; set; }
        public int fk_notes_videoevent { get; set; }
        
        //Optional Fields for POST
        public int fk_notes_createdby { get; set; }
        public int fk_notes_modifyby { get; set; }
        public string notes_createdate { get; set; }
        public string notes_modifydate { get; set; }
    }


    public class NotesModelPost
    {
        public string notes_line { get; set; }
        public int notes_wordcount { get; set; }
        public int notes_index { get; set; }
    }
}
