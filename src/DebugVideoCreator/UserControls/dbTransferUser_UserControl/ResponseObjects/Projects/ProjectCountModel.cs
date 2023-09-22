using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Projects
{
    public class ProjectCountModel
    {
        [JsonPropertyName("available")]
        public int Available { get; set; }
    }
}
