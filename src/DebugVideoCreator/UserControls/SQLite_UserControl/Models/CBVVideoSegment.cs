using System;

namespace Sqllite_Library.Models
{
    public class CBVVideoSegment
    {
        public int videosegment_id { get; set; }
        public byte[] videosegment_media { get; set; }
        public DateTime videosegment_createdate { get; set; }
        public DateTime videosegment_modifydate { get; set; }
        public bool videosegment_isdeleted { get; set; }
        public bool videosegment_issynced { get; set; }
        public Int64 videosegment_serverid { get; set; }
        public string videosegment_syncerror { get; set; }
        public CBVVideoSegment()
        {
        }

        public override string ToString()
        {
            return $"{videosegment_id} \t {videosegment_media.Length} [media]";
        }
    }
}
