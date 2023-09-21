using System.Text.Json.Serialization;

namespace Authentication_UserControl.ResponseObjects
{
    public class LoginResponseModel
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
}
