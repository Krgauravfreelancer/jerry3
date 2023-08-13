namespace Designer_UserControl
{
    public class CBVBackground
    {
        public int background_id { get; set; }
        public string background_image_name { get; set; }
        public int fk_background_company { get; set; }
        public byte[] background_media { get; set; }
        public bool background_active { get; set; }
    }
}
