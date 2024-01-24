using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.Projects
{
    public class ProjectWithDetailModel
    {
        public int project_id { get; set; }
        public string project_name { get; set; }
        public string project_currwfstep { get; set; }
        public int fk_project_section { get; set; }
        public object fk_project_projstatus { get; set; }
        public string fk_project_currentassigned { get; set; }
        public object project_version { get; set; }
        public object project_comments { get; set; }
        public bool project_archived { get; set; }
        public string project_createdate { get; set; }
        public string project_modifydate { get; set; }
        public int fk_project_createdby { get; set; }
        public object fk_project_modifyby { get; set; }
        public List<ProjectDetail> project_detail { get; set; }

    }
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


