using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.UserControls.Models
{
    public class TimelineDesign
    {
        public int design_id { get; set; }
        public int fk_design_videoevent { get; set; }
        public int fk_design_screen { get; set; }
        public string design_xml { get; set; }
        public DateTime design_createdate { get; set; }
        public DateTime design_modifydate { get; set; }
    }
}
