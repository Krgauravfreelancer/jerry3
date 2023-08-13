using System;

namespace Sqllite_Library.Models
{
    public class CBVAudio
    {
        public int audio_id { get; set; }
        public int fk_audio_videoevent { get; set; }
        public byte[] audio_media { get; set; }
        public DateTime audio_createdate { get; set; }
        public DateTime audio_modifydate { get; set; }
        public CBVAudio()
        {
        }

        public override string ToString()
        {
            return $"{audio_id} \t {fk_audio_videoevent} videoevent Id \t {audio_media.Length} [medialength]";
        }
    }
}
