using System;
using System.Collections.Generic;

namespace Sqllite_Library.Models
{
    public class CBVVideoEvent
    {
        public int videoevent_id { get; set; }
        public int fk_videoevent_project { get; set; }
        public int fk_videoevent_media { get; set; }
        public int videoevent_track { get; set; }
        public string videoevent_start { get; set; }
        public int videoevent_duration { get; set; }
        public string videoevent_end { get; set; }
        public DateTime videoevent_createdate { get; set; }
        public DateTime videoevent_modifydate { get; set; }
        public List<CBVAudio> audio_data { get; set; }
        public List<CBVVideoSegment> videosegment_data { get; set; }
        public List<CBVDesign> design_data { get; set; }
        public List<CBVNotes> notes_data { get; set; }
        public CBVVideoEvent()
        {
            audio_data =  new List<CBVAudio>();
            videosegment_data = new List<CBVVideoSegment>();
            design_data = new List<CBVDesign>();
            notes_data = new List<CBVNotes>();
        }

        public override string ToString()
        {
            return $"{videoevent_id} \t {fk_videoevent_project} [projectId] \t {fk_videoevent_media} [mediaId] \t {videoevent_track} [track] " +
                $"\t {videoevent_start} [start] \t {videoevent_duration} [duration]";
        }
    }

    public class VideoEventExtended : CBVVideoEvent
    {
        public string MediaName { get; set; }
        public string Start { get; set; }
        public string ClipDuration { get; set; }
        public VideoEventExtended(CBVVideoEvent ch)
        {
            foreach (var prop in ch.GetType().GetProperties())
            {
                this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(ch, null), null);
            }
        }
    }
}
