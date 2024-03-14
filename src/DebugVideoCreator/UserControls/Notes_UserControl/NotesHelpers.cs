using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Notes_UserControl
{
    public static class NotesHelpers
    {
        public static string SHORTPAUSE = "{pause-250ms}";
        public static string MEDIUMPAUSE = "{pause-500ms}";
        public static string LONGPAUSE = "{pause-1000ms}";

        public static string InsertPause(string pauseText, string stringText, int caretIndex = -1)
        {
            string returnText;
            if (!string.IsNullOrEmpty(stringText))
            {
                var index = (caretIndex == -1 || caretIndex == stringText.Length) ? stringText.Length : caretIndex;
                var text1 = stringText.Substring(0, index);
                if (index < stringText.Length)
                {
                    var text2 = stringText.Substring(index);
                    returnText = text1 + $" {pauseText} " + text2;
                }
                else
                    returnText = text1 + $" {pauseText} ";
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
