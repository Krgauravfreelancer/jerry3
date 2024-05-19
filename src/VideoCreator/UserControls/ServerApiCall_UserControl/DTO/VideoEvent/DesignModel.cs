namespace ServerApiCall_UserControl.DTO.VideoEvent
{
    public class DesignModel : DesignModelPost
    {
        //Optional Fields for POST
        public int fk_design_createdby { get; set; }
        public int fk_design_modifyby { get; set; }
        public string design_createdate { get; set; }
        public string design_modifydate { get; set; }
    }

    public class DesignModelPost
    {
        public int design_id { get; set; }
        public int fk_design_videoevent { get; set; }
        public int fk_design_screen { get; set; }
        public string design_xml { get; set; }
        public bool design_isdeleted { get; set; }
    }
}
