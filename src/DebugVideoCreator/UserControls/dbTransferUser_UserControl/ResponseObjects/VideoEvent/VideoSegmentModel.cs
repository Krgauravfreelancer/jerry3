using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.VideoEvent
{
    public class VideoSegmentPostResponse
    {
        public VideoSegmentModel VideoSegment { get; set; }

        public VideoSegmentPostResponse() { }
    }

    public class VideoSegmentModel
    {
        public int videosegment_id { get; set; }
        public string videosegment_media { get; set; }
        public string videosegment_download_url { get; set; }

        public string videosegment_modifylocdate { get; set; }

        //Optional Fields for POST
        public int fk_videosegment_createdby { get; set; }
        public int fk_videosegment_modifyby { get; set; }
        public string videosegment_createdate { get; set; }
        public string videosegment_modifydate { get; set; }
    }
}
