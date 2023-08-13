using System;

namespace Sqllite_Library.Models
{
    public class CBVVideoSegment
    {
        public int videosegment_id { get; set; }
        public int fk_videosegment_videoevent { get; set; }
        public byte[] videosegment_media { get; set; }
        public DateTime videosegment_createdate { get; set; }
        public DateTime videosegment_modifydate { get; set; }
        public CBVVideoSegment()
        {
        }

        public override string ToString()
        {
            return $"{videosegment_id} \t {fk_videosegment_videoevent} [videoeventId] \t {videosegment_media.Length} [media]";
        }
    }
}
