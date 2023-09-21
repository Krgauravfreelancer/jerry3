using System.Text.Json.Serialization;

namespace Authentication_UserControl.ResponseObjects
{
    public class LogoutResponseModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
