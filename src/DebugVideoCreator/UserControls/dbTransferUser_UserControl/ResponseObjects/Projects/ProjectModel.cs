using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Projects
{
    public class ProjectModel
    {
        [JsonPropertyName("project_id")]
        public int project_id { get; set; }

        [JsonPropertyName("project_name")]
        public string project_name { get; set; }
        
        [JsonPropertyName("fk_project_projstatus")]
        public int fk_project_projstatus { get; set; }

        [JsonPropertyName("fk_project_section")]
        public int fk_project_section { get; set; }

        [JsonPropertyName("project_version")]
        public int project_version { get; set; }

        [JsonPropertyName("project_comments")]
        public string project_comments { get; set; }

        [JsonPropertyName("project_archived")]
        public bool project_archived { get; set; }


        [JsonPropertyName("fk_project_createdby")]
        public int fk_project_createdby { get; set; }

        [JsonPropertyName("fk_project_modifyby")]
        public int? fk_project_modifyby { get; set; }

        [JsonPropertyName("project_modifydate")]
        public string project_modifydate { get; set; }

        [JsonPropertyName("project_createdate")]
        public string project_createdate { get; set; }
        public override string ToString()
        {
            return $@"project_id - {project_id}, 
                        project_name - {project_name}, 
                        fk_project_projstatus - {fk_project_projstatus}, 
                        fk_project_section - {fk_project_section}, 
                        project_version - {project_version}, 
                        project_comments - {project_comments}, 
                        project_archived - {project_archived}";
        }
    }

    public enum ProjectStatusEnum
    {
        All = 0,
        WIP = 1,
        Completed = 2,
        Cancelled = 3,
        Pending = 4,
        Archived = 5
    }
}


