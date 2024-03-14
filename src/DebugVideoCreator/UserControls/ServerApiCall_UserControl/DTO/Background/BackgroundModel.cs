namespace ServerApiCall_UserControl.DTO.Background
{
    public class BackgroundModel
    {
        public int backgrounds_id { get; set; }
        public int fk_backgrounds_company { get; set; }
        public string backgrounds_media { get; set; }
        public bool backgrounds_active { get; set; }
        public string backgrounds_url { get; set; }
        public string backgrounds_thumb { get; set; }
    }
}
