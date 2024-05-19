using Newtonsoft.Json;
using ScreenRecorder_UserControl.Models;
using Sqllite_Library.Models;
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
        public EnumMedia mediaType { get; set; }
        public EnumScreen screenType { get; set; }
        public List<TextItem> RecordedTextList { get; set; }
        public Color Color { get; set; }

        public bool IsSelected { get; set; }
    }
}
