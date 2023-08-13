using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Projects
{
    public class ProjectAssignModel
    {
        [JsonPropertyName("assigned")]
        public bool Assigned { get; set; }

        public override string ToString()
        {
            return $@"Assigned - {Assigned}";
        }
    }
}
