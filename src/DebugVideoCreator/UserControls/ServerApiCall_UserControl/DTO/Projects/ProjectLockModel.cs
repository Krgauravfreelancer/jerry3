﻿using System.Text.Json.Serialization;

namespace ServerApiCall_UserControl.DTO.Projects
{
    public class ProjectLockModel
    {
        public bool project_islocked { get; set; }
        public int permission_status { get; set; }
        public string permission_name { get; set; }
        public string lockedby_username { get; set; }
        public int? lockedby_id { get; set; }   
    }
}


