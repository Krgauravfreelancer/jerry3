using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.Projects
{
    public class ProjectCountModel
    {
        [JsonPropertyName("available")]
        public int Available { get; set; }
    }
}
