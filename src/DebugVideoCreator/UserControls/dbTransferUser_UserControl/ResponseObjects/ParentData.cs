using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects
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
