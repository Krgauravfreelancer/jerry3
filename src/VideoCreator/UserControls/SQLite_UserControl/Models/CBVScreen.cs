namespace Sqllite_Library.Models
{
    public class CBVScreen
    {
        public int screen_id { get; set; }
        public string screen_name { get; set; }
        public string screen_color { get; set; }
        public string screen_hexcolor { get; set; }
    }

    public enum EnumScreen
    {
        Title = -1,
        All = 0,
        Introduction = 1,
        Requirements = 2,
        Objectives = 3,
        Video = 4,
        Conclusion = 5,
        NextUp = 6,
        Custom = 7,
        Bullet = 8,
        Text = 9,
        Image = 10
    }
}
