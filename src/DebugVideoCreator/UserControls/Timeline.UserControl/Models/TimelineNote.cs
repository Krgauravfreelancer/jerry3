using System;

namespace Timeline.UserControls.Models
{
    public class TimelineNote
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
        public long notes_serverid { get; set; }
        public string notes_syncerror { get; set; }
    }
}
