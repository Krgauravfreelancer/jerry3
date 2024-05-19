using System.Collections.Generic;

namespace Sqllite_Library.Models.Planning
{
    public class CBVPlanningDesc
    {
        public int planningdesc_id { get; set; }
        public string planningdesc_line { get; set; }
        public List<CBVPlanningBullet> planningdesc_bullets { get; set; }
    }
}
