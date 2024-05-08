using System;
using System.Collections.Generic;

namespace Sqllite_Library.Models.Planning
{
    public class CBVPlanning
    {
        public int planning_id { get; set; }
        public int fk_planning_project { get; set; }
        public int fk_planning_screen { get; set; }
        public string planning_customname { get; set; }
        public string planning_notesline { get; set; }
        public int planning_medialibid { get; set; }
        public int planning_sort { get; set; }
        public string planning_suggestnotesline { get; set; }

        public DateTime planning_createdate { get; set; }
        public DateTime planning_modifydate { get; set; }

        public Int64 planning_serverid { get; set; }
        public bool planning_issynced { get; set; }
        public string planning_syncerror { get; set; }
        public bool planning_isedited { get; set; }

        public List<CBVPlanningDesc> planning_desc { get; set; }
        public List<CBVPlanningMedia> planning_media { get; set; }
    }
}
