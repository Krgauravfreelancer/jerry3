using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbTransferUser_UserControl.ResponseObjects.MediaLibraryModels
{
    public class MediaLibrary
    {
        public int media_id { get; set; }
        public string thumbnail_download_link { get; set; }
        public string media_download_link { get; set; }
        public List<string> tags { get; set; }
        public ImageMeta image_meta { get; set; }
        public string type { get; set; }
    }

    public class ImageMeta
    {
        public int width { get; set; }
        public int height { get; set; }
        public string mime { get; set; }
        public string size { get; set; }
    }

    public class Links
    {
        public string first { get; set; }
        public string last { get; set; }
        public string prev { get; set; }
        public object next { get; set; }
    }

    public class Meta
    {
        public int current_page { get; set; }
        public int from { get; set; }
        public int last_page { get; set; }
        public int per_page { get; set; }
        public int to { get; set; }
        public int total { get; set; }
    }

    public class MediaLibraryParent<T>
    {
        public List<T> Data { get; set; }
        public Links Links { get; set; }
        public Meta Meta { get; set; }
    }
}
