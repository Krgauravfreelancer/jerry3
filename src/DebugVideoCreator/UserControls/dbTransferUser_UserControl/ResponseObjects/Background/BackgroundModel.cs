using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Background
{
    public class BackgroundModel
    {
        [JsonPropertyName("backgrounds_id")]
        public int backgrounds_id { get; set; }

        [JsonPropertyName("fk_backgrounds_company")]
        public int fk_backgrounds_company { get; set; }

        [JsonPropertyName("backgrounds_media")]
        public string backgrounds_media { get; set; }
        
        [JsonPropertyName("backgrounds_active")]
        public bool backgrounds_active { get; set; }

        public override string ToString()
        {
            return $@"backgrounds_id - {backgrounds_id}, fk_backgrounds_company - {fk_backgrounds_company}, backgrounds_media - {backgrounds_media}, backgrounds_active - {backgrounds_active}";
        }
    }
}
