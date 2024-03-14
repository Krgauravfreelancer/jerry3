using System.Collections.Generic;

namespace ServerApiCall_UserControl.DTO.Projects
{
    public class ProjectDetail
    {
        public int projdet_id { get; set; }
        public int fk_projdet_project { get; set; }
        public string projdet_version { get; set; }
        public bool projdet_currver { get; set; }
        public string projdet_comments { get; set; }
        public string projdet_createdate { get; set; }
        public string projdet_modifydate { get; set; }
        public int fk_projdet_createdby { get; set; }
        public int fk_projdet_modifyby { get; set; }
        public List<object> videoEvent { get; set; }
    }
}
