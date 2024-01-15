namespace Sqllite_Library.Models
{
    public class CBVScreen
    {
        public int screen_id { get; set; }
        public string screen_name { get; set; }
        public string screen_color { get; set; }
        public CBVScreen()
        {
        }

        public override string ToString()
        {
            return $"{screen_id} \t {screen_name} \t {screen_color}";
        }
    }
}
