using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class DesignModel
    {
        [JsonPropertyName("design_id")]
        public int design_id { get; set; }

        [JsonPropertyName("fk_design_videoevent")]
        public int fk_design_videoevent { get; set; }

        [JsonPropertyName("fk_design_screen")]
        public int fk_design_screen { get; set; }

        [JsonPropertyName("design_xml")]
        public string design_xml { get; set; }



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
            return $@"design_id - {design_id}, fk_design_videoevent - {fk_design_videoevent}, fk_design_screen - {fk_design_screen}, design_xml - {design_xml}";
        }
    }
}
