﻿namespace Sqllite_Library.Models
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
        Requirements,
        Objectives,
        Video,
        Conclusion,
        NextUp,
        Custom
    }
}