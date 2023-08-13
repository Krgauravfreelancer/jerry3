using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.App
{
    public class AppModel
    {
        [JsonPropertyName("draft")]
        public int Draft { get; set; }

        [JsonPropertyName("write")]
        public int Write { get; set; }
        
        [JsonPropertyName("talk")]
        public int Talk { get; set; }

        [JsonPropertyName("admin")]
        public int Admin { get; set; }

        [JsonPropertyName("superadmin")]
        public int Superadmin { get; set; }

        public override string ToString()
        {
            return $@"Draft - {Draft}, Write - {Write}, Talk - {Talk}, Admin - {Admin}, Superadmin - {Superadmin}";
        }
    }
}
