namespace ServerApiCall_UserControl.DTO.Media
{
    public class MediaModel
    {
        public int media_id { get; set; }
        public string media_name { get; set; }
        public string media_color { get; set; }

        public override string ToString()
        {
            return $@"MediaName - {media_name}, MediaColor - {media_color}";
        }
    }
}
