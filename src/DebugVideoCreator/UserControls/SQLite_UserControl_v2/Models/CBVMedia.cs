namespace Sqllite_Library.Models
{
    public class CBVMedia
    {
        public int media_id { get; set; }
        public string media_name { get; set; }
        public string media_color { get; set; }
        public CBVMedia()
        {
        }

        public override string ToString()
        {
            return $"{media_id} \t {media_name} \t {media_color}";
        }
    }
}
