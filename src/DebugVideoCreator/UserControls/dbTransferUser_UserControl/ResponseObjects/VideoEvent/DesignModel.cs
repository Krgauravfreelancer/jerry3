using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class DesignModel: DesignModelPost
    {
        public int design_id { get; set; }
        public int fk_design_videoevent { get; set; }

        //Optional Fields for POST
        public int fk_videosegment_createdby { get; set; }
        public int fk_videosegment_modifyby { get; set; }
        public string videosegment_createdate { get; set; }
        public string videosegment_modifydate { get; set; }
    }

    public class DesignModelPost
    {
        public string fk_design_screen { get; set; }
        public string design_xml { get; set; }
    }
}
