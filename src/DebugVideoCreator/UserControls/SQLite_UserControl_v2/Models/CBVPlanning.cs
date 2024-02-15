using System;

namespace Sqllite_Library.Models
{
    public class CBVPlanning
    {
        public int planning_id { get; set; }
        public int fk_planning_project { get; set; }
        public int fk_planning_head { get; set; }
        public string planning_customname { get; set; }
        public string planning_notesline { get; set; }
        public string planning_medialibid { get; set; }
        public int planning_sort { get; set; }
        public string planning_suggestnotesline { get; set; }
        public string planning_createdate { get; set; }
        public string planning_modifydate { get; set; }

        public Int64 planning_serverid { get; set; }
        public bool planning_issynced { get; set; }
        public string planning_syncerror { get; set; }
        public bool planning_isEdited { get; set; }
    }
}
