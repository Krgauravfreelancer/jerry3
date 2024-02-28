using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.DTO.VideoEvent
{
    public class ShiftVideoEventModel
    {
        public int videoevent_id { get; set; }
        public string videoevent_start { get; set; }
        public string videoevent_duration { get; set; }
        public string videoevent_origduration { get; set; }
        public string videoevent_end { get; set; }
    }

    public class ShiftVideoEventResponse : ShiftVideoEventModel
    {
        public int fk_videoevent_projdet { get; set; }
        public int fk_videoevent_media { get; set; }
        public int videoevent_track { get; set; }
        public string videoevent_modifylocdate { get; set; }
    }
}
