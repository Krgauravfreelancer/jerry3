namespace Sqllite_Library.Models
{
    public class CBVVoiceTimer
    {
        public int voicetimer_id { get; set; }
        public string voicetimer_line { get; set; }
        public int voicetimer_wordcount { get; set; }
        public int voicetimer_index { get; set; }
        public CBVVoiceTimer()
        {
        }

        public override string ToString()
        {
            return $"Id - {voicetimer_id} \t    " +
                    $"voicetimer_line - {voicetimer_line}\t " +
                    $"voicetimer_wordcount - {voicetimer_wordcount} \t " +
                    $"voicetimer_index - {voicetimer_index} \t";
        }
    }
}
