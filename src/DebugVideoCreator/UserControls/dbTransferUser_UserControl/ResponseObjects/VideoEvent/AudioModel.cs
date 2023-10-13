using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class AudioModel
    {
        public int audio_id { get; set; }
        public int fk_audio_videoevent { get; set; }
        public string audio_media { get; set; }

        //Optional Fields for POST
        public int fk_audio_createdby { get; set; }
        public int fk_audio_modifyby { get; set; }
        public string audio_createdate { get; set; }
        public string audio_modifydate { get; set; }
    }
}
