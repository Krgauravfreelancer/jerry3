using System.Windows.Controls;
using System.Windows.Documents;
using System;
using System.Windows.Media;

namespace Notes_UserControl
{
    public static class NotesHelpers
    {
        public static string SHORTPAUSE = "{pause-250ms}";
        public static string MEDIUMPAUSE = "{pause-500ms}";
        public static string LONGPAUSE = "{pause-1000ms}";

        public static string InsertPause(string pauseText, string stringText)
        {
            var returnText = "";
            if (!string.IsNullOrEmpty(stringText))
            {
                stringText += $" {pauseText} ";
                returnText = stringText;
            }
            else
            {
                returnText = $"{pauseText} ";
            }
            return returnText;
        }

        public static TextBlock GetEnhancedNotes(string text)
        {
            var tbNotes = new TextBlock();
            tbNotes.Inlines.Clear();

            if (!string.IsNullOrEmpty(text))
            {
                var array = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in array)
                {
                    if (NotesHelpers.SHORTPAUSE == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.LightBlue });
                    else if (NotesHelpers.MEDIUMPAUSE == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Blue });
                    else if (NotesHelpers.LONGPAUSE == word)
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Red });
                    else
                        tbNotes.Inlines.Add(new Run(word + " ") { Foreground = Brushes.Black });
                }
            }
            return tbNotes;
        }

    }

}
