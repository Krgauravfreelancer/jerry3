using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace dbTransferUser_UserControl.ResponseObjects
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
