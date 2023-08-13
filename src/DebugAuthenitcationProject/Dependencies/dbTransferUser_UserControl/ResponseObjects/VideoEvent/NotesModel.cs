using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class NotesModel
    {
        [JsonPropertyName("notes_id")]
        public int notes_id { get; set; }

        [JsonPropertyName("fk_notes_videoevent")]
        public int fk_notes_videoevent { get; set; }

        [JsonPropertyName("notes_line")]
        public string notes_line { get; set; }

        [JsonPropertyName("notes_wordcount")]
        public int notes_wordcount { get; set; }

        [JsonPropertyName("notes_index")]
        public int notes_index { get; set; }

        [JsonPropertyName("fk_notes_createdby")]
        public int fk_notes_createdby { get; set; }

        [JsonPropertyName("fk_notes_modifyby")]
        public int fk_notes_modifyby { get; set; }

        [JsonPropertyName("notes_createdate")]
        public string notes_createdate { get; set; }

        [JsonPropertyName("notes_modifydate")]
        public string notes_modifydate { get; set; }

        public override string ToString()
        {
            return $@"notes_id - {notes_id}, fk_notes_videoevent - {fk_notes_videoevent}, notes_index - {notes_index}, notes_line - {notes_line}, notes_wordcount - {notes_wordcount}, ";
        }
    }
}
