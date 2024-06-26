﻿using System;
using System.Collections.Generic;

namespace ServerApiCall_UserControl.DTO.VideoEvent
{
    public class VideoEventModel
    {
        public int videoevent_id { get; set; }
        public int fk_videoevent_projdet { get; set; }
        public int fk_videoevent_media { get; set; }
        public int videoevent_track { get; set; }
        public string videoevent_start { get; set; }
        public string videoevent_end { get; set; }
        public string videoevent_duration { get; set; }
        public string videoevent_origduration { get; set; }
        public string videoevent_modifylocdate { get; set; }
        public int videoevent_planning { get; set; }

        public bool videoevent_isdeleted { get; set; }
        public bool videoevent_issynced { get; set; }
        public Int64 videoevent_serverid { get; set; }
        public string videoevent_syncerror { get; set; }

        public List<NotesModelPost> notes { get; set; }
        public List<DesignModelPost> design { get; set; }
        public byte[] videosegment_media_bytes { get; set; }
        //public List<AudioModel> audios { get; set; }


        //Below data will be returned, no need to send
        public int fk_videoevent_createdby { get; set; }
        public int fk_videoevent_modifyby { get; set; }
        public string videoevent_createdate { get; set; }
        public string videoevent_modifydate { get; set; }

        public VideoEventModel()
        {
            notes = new List<NotesModelPost>();
            design = new List<DesignModelPost>();
        }

    }

    public class AllVideoEventResponseModel : VideoEventResponseObject
    {
        public List<NotesModel> notes { get; set; }
        public List<DesignModel> design { get; set; }
        public VideoSegmentModel videosegment { get; set; }
        public AllVideoEventResponseModel()
        {
            notes = new List<NotesModel>();
            design = new List<DesignModel>();
        }
    }



    public class VideoEventResponseModel
    {
        public List<NotesModel> notes { get; set; }
        public List<DesignModel> design { get; set; }
        public VideoSegmentModel videosegment { get; set; }
        public VideoEventResponseObject videoevent { get; set; }
        public VideoEventResponseModel()
        {
            notes = new List<NotesModel>();
            design = new List<DesignModel>();
        }
    }

    public class VideoEventResponseObject
    {
        public int videoevent_id { get; set; }
        public int fk_videoevent_projdet { get; set; }
        public int fk_videoevent_media { get; set; }
        public int videoevent_track { get; set; }
        public string videoevent_start { get; set; }
        public string videoevent_duration { get; set; }
        public string videoevent_origduration { get; set; }
        public string videoevent_modifylocdate { get; set; }
        public int videoevent_planning { get; set; }

        public bool videoevent_isdeleted { get; set; }
        public bool videoevent_issynced { get; set; }
        public Int64 videoevent_serverid { get; set; }
        public string videoevent_syncerror { get; set; }

        //Below data will be returned, no need to send
        public int fk_videoevent_createdby { get; set; }
        public int fk_videoevent_modifyby { get; set; }
        public string videoevent_createdate { get; set; }
        public string videoevent_modifydate { get; set; }

    }



}
