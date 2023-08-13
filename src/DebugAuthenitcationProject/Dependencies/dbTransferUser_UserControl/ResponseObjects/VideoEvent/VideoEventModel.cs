using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class VideoEventModel
    {
        [JsonPropertyName("videoevent_id")]
        public int videoevent_id { get; set; }

        [JsonPropertyName("fk_videoevent_project")]
        public int fk_videoevent_project { get; set; }

        [JsonPropertyName("fk_videoevent_media")]
        public int fk_videoevent_media { get; set; }

        [JsonPropertyName("videoevent_track")]
        public int videoevent_track { get; set; }

        [JsonPropertyName("videoevent_start")]
        public string videoevent_start { get; set; }

        [JsonPropertyName("videoevent_duration")]
        public int videoevent_duration { get; set; }

        [JsonPropertyName("fk_videoevent_createdby")]
        public int fk_videoevent_createdby { get; set; }

        [JsonPropertyName("fk_videoevent_modifyby")]
        public int fk_videoevent_modifyby { get; set; }

        [JsonPropertyName("videoevent_createdate")]
        public string videoevent_createdate { get; set; }

        [JsonPropertyName("videoevent_modifydate")]
        public string videoevent_modifydate { get; set; }

        public override string ToString()
        {
            return $@"videoevent_id - {videoevent_id}, fk_videoevent_project - {fk_videoevent_project}, fk_videoevent_media - {fk_videoevent_media}, "+
                        $@"videoevent_track - {videoevent_track}, videoevent_start - {videoevent_start}, videoevent_duration - {videoevent_duration}";
        }
    }
}
