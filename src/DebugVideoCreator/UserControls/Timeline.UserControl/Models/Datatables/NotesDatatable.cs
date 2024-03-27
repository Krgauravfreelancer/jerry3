using System;
using System.Data;
using Timeline.UserControls.Config;

namespace Timeline.UserControls.Models.Datatables
{
    public class NotesDatatable : DataTable
    {
        public NotesDatatable()
        {
            Build();
        }

        public void Build()
        {
            this.Clear();

            Columns.Add(nameof(TimelineNote.notes_id), typeof(int));
            Columns.Add(nameof(TimelineNote.fk_notes_videoevent), typeof(int));
            Columns.Add(nameof(TimelineNote.notes_line), typeof(string));
            Columns.Add(nameof(TimelineNote.notes_wordcount), typeof(int));
            Columns.Add(nameof(TimelineNote.notes_index), typeof(int));
            Columns.Add(nameof(TimelineNote.notes_start), typeof(string));
            Columns.Add(nameof(TimelineNote.notes_duration), typeof(string));
            Columns.Add(nameof(TimelineNote.notes_createdate), typeof(string));
            Columns.Add(nameof(TimelineNote.notes_modifydate), typeof(string));
            Columns.Add(nameof(TimelineNote.notes_isdeleted), typeof(bool));
            Columns.Add(nameof(TimelineNote.notes_issynced), typeof(bool));
            Columns.Add(nameof(TimelineNote.notes_serverid), typeof(Int64));
            Columns.Add(nameof(TimelineNote.notes_syncerror), typeof(string));

        }




        public void AddNoteRow(TimelineNote timelineNote)
        {
            DataRow dRow = this.NewRow();

            dRow[nameof(TimelineNote.notes_id)] = timelineNote.notes_id;
            dRow[nameof(TimelineNote.fk_notes_videoevent)] = timelineNote.fk_notes_videoevent;
            dRow[nameof(TimelineNote.notes_line)] = timelineNote.notes_line;
            dRow[nameof(TimelineNote.notes_wordcount)] = timelineNote.notes_wordcount;
            dRow[nameof(TimelineNote.notes_index)] = timelineNote.notes_index;
            dRow[nameof(TimelineNote.notes_start)] = timelineNote.notes_start;
            dRow[nameof(TimelineNote.notes_duration)] = timelineNote.notes_duration;
            dRow[nameof(TimelineNote.notes_createdate)] =  timelineNote.notes_createdate.ToString(TimelineDefaultConfig.DateTimeStringFormat) ?? DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);
            dRow[nameof(TimelineNote.notes_modifydate)] = timelineNote.notes_modifydate.ToString(TimelineDefaultConfig.DateTimeStringFormat) ?? DateTime.Now.ToString(TimelineDefaultConfig.DateTimeStringFormat);
            dRow[nameof(TimelineNote.notes_isdeleted)] = timelineNote.notes_isdeleted;
            dRow[nameof(TimelineNote.notes_issynced)] = timelineNote.notes_issynced;
            dRow[nameof(TimelineNote.notes_serverid)] = timelineNote.notes_serverid;
            dRow[nameof(TimelineNote.notes_syncerror)] = timelineNote.notes_syncerror;
          

            this.Rows.Add(dRow);
        }

        //public List<int> GetNotesIds()
        //{
        //    var idList = new List<int>();
        //    if (this != null)
        //    {
        //        foreach (DataRow row in this.Rows)
        //        {
        //            int videoevent_id = Convert.ToInt32(row[nameof(TimelineEvent.videoevent_id)]);
        //            idList.Add(videoevent_id);
        //        }
        //    }

        //    return idList;
        //}
    }
}
