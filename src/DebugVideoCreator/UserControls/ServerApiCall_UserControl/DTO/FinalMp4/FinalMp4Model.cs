using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.FinalMp4
{
    public class FinalMp4Model
    {
        [JsonPropertyName("finalmp4_id")]
        public int finalmp4_id { get; set; }

        [JsonPropertyName("fk_finalmp4_project")]
        public int fk_finalmp4_project { get; set; }

        [JsonPropertyName("finalmp4_version")]
        public int finalmp4_version { get; set; }

        [JsonPropertyName("finalmp4_comments")]
        public string finalmp4_comments { get; set; }

        [JsonPropertyName("finalmp4_mediamp4")]
        public string finalmp4_mediamp4 { get; set; }

        [JsonPropertyName("finalmp4_mediahlsts")]
        public string finalmp4_mediahlsts { get; set; }




        [JsonPropertyName("fk_finalmp4_createdby")]
        public int fk_finalmp4_createdby { get; set; }

        [JsonPropertyName("fk_finalmp4_modifyby")]
        public int fk_finalmp4_modifyby { get; set; }

        [JsonPropertyName("finalmp4_createdate")]
        public string finalmp4_createdate { get; set; }

        [JsonPropertyName("finalmp4_modifydate")]
        public string finalmp4_modifydate { get; set; }


        public override string ToString()
        {
            return $@"finalmp4_id - {finalmp4_id}, fk_finalmp4_project - {fk_finalmp4_project}, finalmp4_version - {finalmp4_version}, finalmp4_comments - {finalmp4_comments}, finalmp4_mediamp4 - {finalmp4_mediamp4}, finalmp4_mediahlsts - {finalmp4_mediahlsts}";
        }
    }
}
