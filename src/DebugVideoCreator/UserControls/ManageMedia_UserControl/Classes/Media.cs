using Newtonsoft.Json;
using ScreenRecorder_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ManageMedia_UserControl.Classes
{
    public class Media
    {
        public int ProjectId { get; set; }
        public int TrackId { get; set; }
        public int VideoEventID { get; set; }
        public string ImageType { get; set; }

        [JsonIgnore]
        public byte[] mediaData { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan OriginalDuration { get; set; }
        public MediaType mediaType { get; set; }
        public List<TextItem> RecordedTextList { get; set; }
        public Color Color { get; set; }
    }

    public enum MediaType
    {
        Image = 1,
        Video = 2,
        Audio = 3,
        Form = 4
    }

}
