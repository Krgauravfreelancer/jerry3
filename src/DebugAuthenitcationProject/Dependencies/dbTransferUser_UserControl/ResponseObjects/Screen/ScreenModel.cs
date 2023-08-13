using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Screen
{
    public class ScreenModel
    {
        [JsonPropertyName("screen_name")]
        public string ScreenName { get; set; }

        [JsonPropertyName("screen_color")]
        public string ScreenColor { get; set; }
        public override string ToString()
        {
            return $@"ScreenName - {ScreenName}, ScreenColor - {ScreenColor}";
        }
    }
}
