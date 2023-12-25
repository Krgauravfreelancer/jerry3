using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO
{
    public class ParentData<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
