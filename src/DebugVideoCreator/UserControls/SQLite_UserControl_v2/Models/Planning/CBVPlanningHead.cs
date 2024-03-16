namespace Sqllite_Library.Models.Planning
{
    public class CBVPlanningHead
    {
        public int planninghead_id { get; set; }
        public string planninghead_name { get; set; }
        public int planninghead_sort { get; set; }
    }


    public enum EnumPlanningHead
    {
        Title = -1,
        All = 0,
        Introduction = 1,
        Requirements =2,
        Objectives = 3,
        Video = 4,
        Conclusion = 5,
        NextUp = 6,
        Custom = 7,
        Bullet = 8,
        Text = 9,
        Image = 10
    }

    public enum EnumScreen
    {
        Title = 1,
        Intro = 2,
        Requirements = 3,
        Objectives = 4,
        Custom = 5,
        Conclusion = 6,
        Next = 7
    }
}
