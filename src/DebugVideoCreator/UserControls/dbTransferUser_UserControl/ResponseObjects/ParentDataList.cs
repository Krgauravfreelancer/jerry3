using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects
{
    public class ParentDataList<T>
    {
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
    }
}
