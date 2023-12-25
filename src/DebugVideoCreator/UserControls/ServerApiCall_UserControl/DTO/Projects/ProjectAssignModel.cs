using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.Projects
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
