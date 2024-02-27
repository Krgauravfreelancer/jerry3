using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.VideoEvent
{
    public class NotesResponseModel: NotesModel
    {
        public List<NotesModel> Notes { get; set; }
    }

    public class NotesModel : NotesModelPost
    {

        public int notes_id { get; set; }
        public int fk_notes_videoevent { get; set; }


        //Optional Fields for POST
        public int fk_notes_createdby { get; set; }
        public int fk_notes_modifyby { get; set; }
        public string notes_createdate { get; set; }
        public string notes_modifydate { get; set; }
        public bool notes_isdeleted { get; set; }
    }


    public class NotesModelPost
    {
        public string notes_line { get; set; }
        public int notes_wordcount { get; set; }
        public string notes_index { get; set; }
        public string notes_start { get; set; }
        public string notes_duration { get; set; }
        public string notes_modifylocdate { get; set; }

    }

    public class NotesModelPut : NotesModelPost
    {
        public int notes_id { get; set; }

    }
}
