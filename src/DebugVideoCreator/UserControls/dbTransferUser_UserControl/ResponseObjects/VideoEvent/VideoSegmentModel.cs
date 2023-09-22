using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class VideoSegmentModel
    {
        [JsonPropertyName("videosegment_id")]
        public int videosegment_id { get; set; }

        [JsonPropertyName("fk_videosegment_videoevent")]
        public int fk_videosegment_videoevent { get; set; }

        [JsonPropertyName("videosegment_media")]
        public string videosegment_media { get; set; }

        

        [JsonPropertyName("fk_videosegment_createdby")]
        public int fk_videosegment_createdby { get; set; }

        [JsonPropertyName("fk_videosegment_modifyby")]
        public int fk_videosegment_modifyby { get; set; }

        [JsonPropertyName("videosegment_createdate")]
        public string videosegment_createdate { get; set; }

        [JsonPropertyName("videosegment_modifydate")]
        public string videosegment_modifydate { get; set; }

        public override string ToString()
        {
            return $@"videosegment_id - {videosegment_id}, fk_videosegment_videoevent - {fk_videosegment_videoevent}, videosegment_media - {videosegment_media}";
        }
    }
}
