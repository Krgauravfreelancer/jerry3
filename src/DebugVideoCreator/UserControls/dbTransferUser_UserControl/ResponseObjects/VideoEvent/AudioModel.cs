using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class AudioModel
    {
        [JsonPropertyName("audio_id")]
        public int audio_id { get; set; }

        [JsonPropertyName("fk_audio_videoevent")]
        public int fk_audio_videoevent { get; set; }

        [JsonPropertyName("audio_media")]
        public string audio_media { get; set; }

        [JsonPropertyName("fk_audio_createdby")]
        public int fk_audio_createdby { get; set; }

        [JsonPropertyName("fk_audio_modifyby")]
        public int fk_audio_modifyby { get; set; }

        [JsonPropertyName("audio_createdate")]
        public string audio_createdate { get; set; }

        [JsonPropertyName("audio_modifydate")]
        public string audio_modifydate { get; set; }

        public override string ToString()
        {
            return $@"audio_id - {audio_id}, fk_audio_videoevent - {fk_audio_videoevent}, audio_media - {audio_media}, ";
        }
    }
}
