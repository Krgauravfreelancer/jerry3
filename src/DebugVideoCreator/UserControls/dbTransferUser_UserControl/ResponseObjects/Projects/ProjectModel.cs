using System.Text.Json.Serialization;

namespace dbTransferUser_UserControl.ResponseObjects.Projects
{
    public class ProjectModel: ProjectModelUI
    {
        public int? fk_project_createdby { get; set; }
        public int? fk_project_modifyby { get; set; }
        public string project_modifydate { get; set; }
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

    public class ProjectModelUI
    {
        public int project_id { get; set; }
        public string project_name { get; set; }
        public int fk_project_projstatus { get; set; }
        public int fk_project_section { get; set; }
        public int project_version { get; set; }
        public string project_comments { get; set; }
        public bool project_archived { get; set; }
        
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

    public class ProjectAcceptRejectModel
    {
        public string status { get; set; }
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


