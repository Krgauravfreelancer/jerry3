using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace dbTransferUser_UserControl.ResponseObjects.Media
{
    public class MediaModel
    {
        [JsonPropertyName("media_name")]
        public string MediaName { get; set; }

        [JsonPropertyName("media_color")]
        public string MediaColor { get; set; }

        public override string ToString()
        {
            return $@"MediaName - {MediaName}, MediaColor - {MediaColor}";
        }
    }
}
