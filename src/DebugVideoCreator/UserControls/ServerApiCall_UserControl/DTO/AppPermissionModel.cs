using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO
{
    public class AppPermissionModel
    {
        [JsonPropertyName("draft")]
        public int Draft { get; internal set; }

        [JsonPropertyName("write")]
        public int Write { get; internal set; }

        [JsonPropertyName("talk")]
        public int Talk { get; internal set; }

        [JsonPropertyName("admin")]
        public int Admin { get; internal set; }
    }
}
