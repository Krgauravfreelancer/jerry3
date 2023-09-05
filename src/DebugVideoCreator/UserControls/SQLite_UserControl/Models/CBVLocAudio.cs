using System;

namespace Sqllite_Library.Models
{
    public class CBVLocAudio
    {
        public int locaudio_id { get; set; }
        public int fk_locaudio_notes { get; set; }
        public byte[] locaudio_media { get; set; }
        public DateTime locaudio_createdate { get; set; }
        public DateTime locaudio_modifydate { get; set; }
        public CBVLocAudio()
        {
        }

        public override string ToString()
        {
            return $"{locaudio_id} \t {fk_locaudio_notes} \t {locaudio_media.Length} \r\n";
        }
    }
}
