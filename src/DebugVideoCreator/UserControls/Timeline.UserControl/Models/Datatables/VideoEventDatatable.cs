using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;
using Timeline.UserControls.Config;

namespace Timeline.UserControls.Models.Datatables
{
    public class VideoEventDatatable : DataTable
    {

        public VideoEventDatatable()
        {
            Build();
        }

        public void Build()
        {
            this.Clear();

            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_id), typeof(int));
            Columns.Add(nameof(TimelineEvent.VideoEvent.fk_videoevent_projdet), typeof(int));
            Columns.Add(nameof(TimelineEvent.VideoEvent.fk_videoevent_media), typeof(int));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_track), typeof(int));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_start), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_duration), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_origduration), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_createdate), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_modifydate), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_isdeleted), typeof(bool));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_issynced), typeof(bool));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_serverid), typeof(Int64));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_syncerror), typeof(string));
            Columns.Add(nameof(TimelineEvent.VideoEvent.videoevent_end), typeof(string));
            //optional
            //Columns.Add("media", typeof(byte[])); // Media Column

            Columns.Add(nameof(TimelineEvent.VideoEvent.fk_design_screen), typeof(int));
            Columns.Add(nameof(TimelineEvent.VideoEvent.fk_design_background), typeof(int));
        }




        public void AddVideoEventRow(TimelineEvent timelineEvent)
        {
            DataRow dRow = this.NewRow();

            dRow[nameof(TimelineEvent.VideoEvent.videoevent_id)] = timelineEvent.VideoEvent.videoevent_id;
            dRow[nameof(TimelineEvent.VideoEvent.fk_videoevent_projdet)] = timelineEvent.VideoEvent.fk_videoevent_projdet;
            dRow[nameof(TimelineEvent.VideoEvent.fk_videoevent_media)] = timelineEvent.VideoEvent.fk_videoevent_media;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_track)] = timelineEvent.VideoEvent.videoevent_track;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_duration)] = timelineEvent.VideoEvent.videoevent_origduration;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_createdate)] = timelineEvent.VideoEvent.videoevent_createdate.ToString(TimelineDefaultConfig.DateTimeStringFormat) ?? DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_modifydate)] = timelineEvent.VideoEvent.videoevent_modifydate.ToString(TimelineDefaultConfig.DateTimeStringFormat) ?? DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_isdeleted)] = timelineEvent.VideoEvent.videoevent_isdeleted;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_issynced)] = timelineEvent.VideoEvent.videoevent_issynced;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_serverid)] = timelineEvent.VideoEvent.videoevent_serverid;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_syncerror)] = timelineEvent.VideoEvent.videoevent_syncerror ?? string.Empty;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_end)] = timelineEvent.VideoEvent.videoevent_end;
            dRow[nameof(TimelineEvent.VideoEvent.fk_design_screen)] = timelineEvent.VideoEvent.fk_design_screen;
            dRow[nameof(TimelineEvent.VideoEvent.fk_design_background)] = timelineEvent.VideoEvent.fk_design_background;

            //modified events
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_start)] = timelineEvent.EventStart;
            dRow[nameof(TimelineEvent.VideoEvent.videoevent_duration)] = timelineEvent.EventDuration;

            this.Rows.Add(dRow);
        }

        public List<int> GetVideoEventsIds()
        {
            var idList = new List<int>();
            if (this != null)
            {
                foreach (DataRow row in this.Rows)
                {
                    int videoevent_id = Convert.ToInt32(row[nameof(TimelineEvent.VideoEvent.videoevent_id)]);
                    idList.Add(videoevent_id);
                }
            }

            return idList;
        }
    }
}
