namespace ServerApiCall_UserControl.DTO.Background
{
    public class PlanningModel
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


        // API Call params
        public int fk_planning_createdby { get; set; }
        public int fk_planning_modifyby { get; set; }
        public bool planning_approved { get; set; }
        public string planning_media_thumb { get; set; }
        public string planning_media_full { get; set; }
    }
}
