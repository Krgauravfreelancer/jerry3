using Sqllite_Library.Business;
using System;
using System.Windows.Controls;

namespace VideoCreator.Helpers
{
    public static class CommentsHelper
    {
        public static void ShowComments(int videoeventId, TextBlock commentsBlock)
        {
            if (videoeventId == -1)
            {
                commentsBlock.Text = "No VideoEvent Selected";
                return;
            }
            commentsBlock.Text = "Suggestion Notes coming soon";
        }

        public static void ShowNotes(int videoeventId, TextBlock notesBlock)
        {
            if(videoeventId == -1)
            {
                notesBlock.Text = "No VideoEvent Selected";
                return;
            }
            var notes = DataManagerSqlLite.GetNotes(videoeventId);
            var notesText = string.Empty;
            foreach( var note in notes)
                notesText += note.notes_line + Environment.NewLine;
            notesBlock.Text = notesText;
        }

        public static void ShowNotesById(int notesId, TextBlock notesBlock)
        {
            if (notesId == -1)
            {
                notesBlock.Text = "No VideoEvent Selected";
                return;
            }
            var notes = DataManagerSqlLite.GetNotesbyId(notesId);
            var notesText = string.Empty;
            foreach (var note in notes)
                notesText += note.notes_line + Environment.NewLine;
            notesBlock.Text = notesText;
        }

    }
}
