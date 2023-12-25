using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO
{
    public class LogoutResponseModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
