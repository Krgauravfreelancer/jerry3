using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO
{
    public class LoginResponseModel
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
