using System;

namespace Sqllite_Library.Models
{
    public class CBVProject
    {
        public int project_id { get; set; }
        public string project_name { get; set; }
        public int project_version { get; set; }
        public string project_comments { get; set; }
        public bool project_uploaded { get; set; }
        public DateTime? project_date { get; set; }
        public bool project_archived { get; set; }
        public int fk_project_background { get; set; }
        public DateTime project_createdate { get; set; }
        public DateTime project_modifydate { get; set; }
        public bool project_isdeleted { get; set; }
        public bool project_issynced { get; set; }
        public Int64 project_serverid { get; set; }
        public string project_syncerror { get; set; }
        public bool project_started { get; set; } // computed column, based upon  If we have any rows in Video events table for this project, then "Project Started" will be "Yes". If we have a project with no video events dependent rows, then "Project Started" will be "No".
        public int project_videoeventcount { get; set; }
        public bool project_downloaded { get; set; } // computed column, based upon  If we have any rows in finalmp4 table for this project, then "Project Downloaded" will be "Yes". If we have a project with no video events dependent rows, then "Project Downloaded" will be "No".
        public int project_finalmp4count { get; set; }
        public CBVProject()
        {
        }

        public override string ToString()
        {
            return $"{project_id} \t {project_name} \t {project_version} \t {project_uploaded}  [uploaded] " +
                $"\t {project_date} [project_date] \t {project_archived} [archived] \t {fk_project_background} background \t {project_comments} [comments] \t {project_started}[Started]";
        }
    }

    public class CBVWIPOrArchivedProjectList
    {
        public int project_id { get; set; }
        public string project_name { get; set; }
        public int project_version { get; set; }
        public DateTime? project_date { get; set; }
        public bool project_started { get; set; } // computed column, based upon  If we have any rows in Video events table for this project, then "Project Started" will be "Yes". If we have a project with no video events dependent rows, then "Project Started" will be "No".
        public int project_videoeventcount { get; set; }
        public CBVWIPOrArchivedProjectList()
        {
        }

        public override string ToString()
        {
            return $"Id: {project_id} \tName: {project_name} \tVersion: {project_version} \tDate: {project_date} \tStarted: {project_started} \tEventCount: {project_videoeventcount}";
        }
    }

    public class CBVPendingProjectList
    {
        public int project_id { get; set; }
        public string project_name { get; set; }
        public int project_version { get; set; }

        public DateTime? project_date { get; set; }

        public bool project_accepted { get; set; }
        public CBVPendingProjectList()
        {
        }

        public override string ToString()
        {
            return $"Id: {project_id} \tName: {project_name} \tVersion: {project_version} \tDate: {project_date}";
        }
    }
}
