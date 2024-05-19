using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO
{
    public class ParentDataList<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
