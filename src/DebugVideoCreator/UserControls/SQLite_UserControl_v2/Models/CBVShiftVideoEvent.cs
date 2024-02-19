using System;

namespace Sqllite_Library.Models
{
    public class CBVShiftVideoEvent
    {
        public int videoevent_id { get; set; }
        public string videoevent_start { get; set; }
        public string videoevent_duration { get; set; }
        public string videoevent_end { get; set; }
        public Int64 videoevent_serverid { get; set; }
    }

}
