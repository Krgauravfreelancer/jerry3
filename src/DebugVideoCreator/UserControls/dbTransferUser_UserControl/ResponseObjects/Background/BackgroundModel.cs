using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Background
{
    public class BackgroundModel
    {
        public int backgrounds_id { get; set; }
        public int fk_backgrounds_company { get; set; }
        public string backgrounds_media { get; set; }
        public bool backgrounds_active { get; set; }
        public string backgrounds_url { get; set; }
        public string backgrounds_thumb { get; set; }

        public override string ToString()
        {
            return $@"backgrounds_id - {backgrounds_id}, fk_backgrounds_company - {fk_backgrounds_company}, backgrounds_media - {backgrounds_media}, backgrounds_active - {backgrounds_active}";
        }
    }
}
