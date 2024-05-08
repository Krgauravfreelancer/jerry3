using System;

namespace Sqllite_Library.Models
{
    public class CBVProjdet
    {
        public int projdet_id { get; set; }
        public int fk_projdet_project { get; set; }
        public string projdet_version { get; set; }
        public bool projdet_currver { get; set; }
        public string projdet_comments { get; set; }
        public Int64 projdet_serverid { get; set; }
        public DateTime projdet_createdate { get; set; }
        public DateTime projdet_modifydate { get; set; }
    }
}
