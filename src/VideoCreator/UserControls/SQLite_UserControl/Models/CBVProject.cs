using System;
using System.Collections.Generic;

namespace Sqllite_Library.Models
{
    public class CBVProject
    {
        public int project_id { get; set; }
        public string project_videotitle { get; set; }
        public string project_currwfstep { get; set; }
        public bool project_uploaded { get; set; }
        public int fk_project_background { get; set; }
        public DateTime? project_date { get; set; }
        public bool project_archived { get; set; }
        public DateTime project_createdate { get; set; }
        public DateTime project_modifydate { get; set; }
        public bool project_isdeleted { get; set; }
        public bool project_issynced { get; set; }
        public Int64 project_serverid { get; set; }
        public string project_syncerror { get; set; }
        public List<CBVProjdet> projdet_data { get; set; }
        public CBVProject()
        {
            projdet_data = new List<CBVProjdet>();
        }
    }

    public class CBVProjectForJoin
    {
        public int project_id { get; set; }
        public string project_videotitle { get; set; }
        public string project_version { get; set; }
        public Int64 project_serverid { get; set; }
        public DateTime? project_date { get; set; }

    }
}
