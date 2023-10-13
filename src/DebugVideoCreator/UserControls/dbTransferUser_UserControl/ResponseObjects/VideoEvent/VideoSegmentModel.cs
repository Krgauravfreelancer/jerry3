using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class VideoSegmentModel
    {
        public int videosegment_id { get; set; }
        public int fk_videosegment_videoevent { get; set; }
        public string videosegment_media { get; set; }


        //Optional Fields for POST
        public int fk_videosegment_createdby { get; set; }
        public int fk_videosegment_modifyby { get; set; }
        public string videosegment_createdate { get; set; }
        public string videosegment_modifydate { get; set; }
    }
}
