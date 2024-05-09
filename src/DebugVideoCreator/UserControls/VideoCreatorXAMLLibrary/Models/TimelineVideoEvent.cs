using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoCreatorXAMLLibrary.Models
{
    public class TimelineVideoEvent
    {
        public int videoevent_id { get; set; }
        public int fk_videoevent_media { get; set; }
        public int fk_videoevent_projdet { get; set; }
        public int videoevent_track { get; set; }

        public string videoevent_start { get; set; }
        public string videoevent_end { get; set; }
        public string videoevent_duration { get; set; }
        public string videoevent_origduration { get; set; }

        public DateTime videoevent_createdate { get; set; }
        public DateTime videoevent_modifydate { get; set; }

        public bool videoevent_isdeleted { get; set; }
        public bool videoevent_issynced { get; set; }

        public Int64 videoevent_serverid { get; set; }
        public string videoevent_syncerror { get; set; }

        public int fk_design_screen { get; set; }
        public int fk_design_background { get; set; }

    }
}
