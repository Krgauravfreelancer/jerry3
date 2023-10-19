using System;

namespace Sqllite_Library.Models
{
    public class CBVDesign
    {
        public int design_id { get; set; }
        public int fk_design_videoevent { get; set; }
        public int fk_design_background { get; set; }
        public int fk_design_screen { get; set; }
        public string design_xml { get; set; }
        public DateTime design_createdate { get; set; }
        public DateTime design_modifydate { get; set; }
        public bool design_isdeleted { get; set; }

        public CBVDesign()
        {
        }

        public override string ToString()
        {
            return $"{design_id} \t {fk_design_videoevent} [videoeventId] \t {fk_design_background} [backgroundId] \t {fk_design_screen} [screenId] \t {design_xml}";
        }
    }
}
