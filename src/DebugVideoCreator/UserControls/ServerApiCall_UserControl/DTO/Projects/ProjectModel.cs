namespace ServerApiCall_UserControl.DTO.Projects
{
    public class ProjectList : ProjectListUI
    {
        public int fk_project_section { get; set; }
        public int projstatus_id { get; set; }
        public string project_modifydate { get; set; }
        public string project_createdate { get; set; }
        public int projdet_id { get; set; }
        public string project_currwfstep { get; set; }

    }

    public class ProjectListUI
    {
        public int project_id { get; set; }
        public string project_videotitle { get; set; }
        public string projstatus_name { get; set; }
        public string project_downloaded { get; set; }
        public bool current_version { get; set; }
        public string projdet_version { get; set; }
        public int project_localId { get; set; }
    }


    public enum ProjectStatusEnum
    {
        All = 0,
        WIP = 1,
        Completed = 2,
        Cancelled = 3,
        Pending = 4,
        Archived = 5
    }
}


