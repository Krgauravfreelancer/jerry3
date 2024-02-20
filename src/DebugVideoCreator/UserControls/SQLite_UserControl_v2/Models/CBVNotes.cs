using System;

namespace Sqllite_Library.Models
{
    public class CBVNotes
    {
        public int notes_id { get; set; }
        public int fk_notes_videoevent { get; set; }
        public string notes_line { get; set; }
        public int notes_wordcount { get; set; }
        public int notes_index { get; set; }
        public string notes_start { get; set; }
        public string notes_duration { get; set; }
        public DateTime notes_createdate { get; set; }
        public DateTime notes_modifydate { get; set; }
        public bool notes_isdeleted { get; set; }
        public bool notes_issynced { get; set; }
        public Int64 notes_serverid { get; set; }
        public string notes_syncerror { get; set; }
        public CBVNotes()
        {
        }

        public override string ToString()
        {
            return $"{notes_id} \t {fk_notes_videoevent} [videoeventId] \t {notes_line} \t {notes_wordcount} [wordcount]";
        }
    }
}
