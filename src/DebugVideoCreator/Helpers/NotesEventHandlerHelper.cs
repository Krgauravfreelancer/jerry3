﻿using ServerApiCall_UserControl.DTO.VideoEvent;
using System;
using System.Collections.Generic;
using System.Data;

namespace VideoCreator.Helpers
{
    public static class NotesEventHandlerHelper
    {
        #region === Notes Functions ==

        public static List<NotesModelPost> GetNotesModelList(DataTable dt)
        {
            var data = new List<NotesModelPost>();
            foreach (DataRow note in dt.Rows)
            {
                var notesModel = new NotesModelPost();
                notesModel.notes_line = Convert.ToString(note["notes_line"]);
                notesModel.notes_index = Convert.ToString(note["notes_index"]);
                notesModel.notes_wordcount = Convert.ToInt16(note["notes_wordcount"]);
                notesModel.notes_start = Convert.ToString(note["notes_start"]);
                notesModel.notes_duration = Convert.ToString(note["notes_duration"]);
                notesModel.notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                data.Add(notesModel);
            }
            return data;
        }

        public static List<NotesModelPost> GetNotesModelList(DataRow note)
        {
            var data = new List<NotesModelPost>();

            var notesModel = new NotesModelPost();
            notesModel.notes_line = Convert.ToString(note["notes_line"]);
            notesModel.notes_index = Convert.ToString(note["notes_index"]);
            notesModel.notes_wordcount = Convert.ToInt16(note["notes_wordcount"]);
            notesModel.notes_start = Convert.ToString(note["notes_start"]);
            notesModel.notes_duration = Convert.ToString(note["notes_duration"]);
            notesModel.notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            data.Add(notesModel);

            return data;
        }


        public static List<NotesModelPut> GetNotesModelListPut(DataTable dt)
        {
            var data = new List<NotesModelPut>();
            foreach (DataRow note in dt.Rows)
            {
                var notesModel = new NotesModelPut();
                notesModel.notes_line = Convert.ToString(note["notes_line"]);
                notesModel.notes_index = Convert.ToString(note["notes_index"]);
                notesModel.notes_wordcount = Convert.ToInt32(note["notes_wordcount"]);
                notesModel.notes_start = Convert.ToString(note["notes_start"]);
                notesModel.notes_duration = Convert.ToString(note["notes_duration"]);
                notesModel.notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                notesModel.notes_id = Convert.ToInt32(note["notes_serverid"]);
                data.Add(notesModel);
            }
            return data;
        }

        public static List<NotesModelPut> GetNotesModelListPut(DataRow note)
        {
            var data = new List<NotesModelPut>();
            var notesModel = new NotesModelPut();
            notesModel.notes_line = Convert.ToString(note["notes_line"]);
            notesModel.notes_index = Convert.ToString(note["notes_index"]);
            notesModel.notes_wordcount = Convert.ToInt32(note["notes_wordcount"]);
            notesModel.notes_start = Convert.ToString(note["notes_start"]);
            notesModel.notes_duration = Convert.ToString(note["notes_duration"]);
            notesModel.notes_modifylocdate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            notesModel.notes_id = Convert.ToInt32(note["notes_serverid"]);
            data.Add(notesModel);
            return data;
        }


        public static DataTable GetNotesDataTableForLocalDB(List<NotesModel> notes, int localVideoEventId)
        {
            var dtNotes = GetNotesRawTable();
            AddNotesRowToDataTable(dtNotes, notes, localVideoEventId);
            return dtNotes;
        }

        private static DataTable GetNotesRawTable()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_start", typeof(string));
            dtNotes.Columns.Add("notes_duration", typeof(string));
            dtNotes.Columns.Add("notes_index", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));
            dtNotes.Columns.Add("notes_isdeleted", typeof(bool));
            dtNotes.Columns.Add("notes_issynced", typeof(bool));
            dtNotes.Columns.Add("notes_serverid", typeof(Int64));
            dtNotes.Columns.Add("notes_syncerror", typeof(string));
            return dtNotes;
        }

        private static DataTable AddNotesRowToDataTable(this DataTable dt, List<NotesModel> notes, int localVideoEventId)
        {
            foreach (var item in notes)
            {
                var dRow = dt.NewRow();
                dRow["notes_index"] = item.notes_index;
                dRow["notes_id"] = -1;
                dRow["notes_line"] = item.notes_line;
                dRow["notes_start"] = item.notes_start;
                dRow["notes_duration"] = item.notes_duration;
                dRow["notes_createdate"] = item.notes_createdate;
                dRow["notes_modifydate"] = item.notes_modifydate;
                dRow["fk_notes_videoevent"] = localVideoEventId;
                dRow["notes_wordcount"] = item.notes_wordcount;
                dRow["notes_isdeleted"] = false;
                dRow["notes_issynced"] = true;
                dRow["notes_serverid"] = item.notes_id;
                dRow["notes_syncerror"] = "";
                dt.Rows.Add(dRow);
            }

            return dt;
        }

        #endregion
    }
}
