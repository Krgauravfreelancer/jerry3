using System.Collections.Generic;

namespace ServerApiCall_UserControl.DTO.Background
{
    public class PlanningModel
    {
        public int planning_id { get; set; }
        public int fk_planning_project { get; set; }
        public int fk_planning_screen { get; set; }
        public string planning_customname { get; set; }
        public List<string> planning_notesline { get; set; }
        public string planning_medialibid { get; set; }
        public int planning_sort { get; set; }
        public string planning_suggestnotesline { get; set; }
        public string planning_media_thumb { get; set; }
        public string planning_media_full { get; set; }
        public string planning_createdate { get; set; }
        public string planning_modifydate { get; set; }


        // API Call params
        public int fk_planning_createdby { get; set; }
        public int fk_planning_modifyby { get; set; }
        public bool planning_approved { get; set; }

        public PlanningDesc planningdesc { get; set; }
    }


    public class PlanningBullet
    {
        public int planningbullet_id { get; set; }
        public int fk_planningbullet_desc { get; set; }
        public string planningbullet_line { get; set; }
    }

    public class PlanningDesc
    {
        public int planningdesc_id { get; set; }
        public string planningdesc_line { get; set; }
        public List<PlanningBullet> bullet { get; set; }
    }

    // Only for local DB
    public class PlanningMedia
    {
        public int planningmedia_id { get; set; }
        public string planningmedia_mediathumb { get; set; }
        public string planningmedia_mediafull { get; set; }
    }

}
