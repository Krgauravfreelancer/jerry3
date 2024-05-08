namespace Sqllite_Library.Models
{
    public class CBVVoiceAvergae
    {
        public int voiceaverage_id { get; set; }
        public string voiceaverage_average { get; set; }
        public CBVVoiceAvergae()
        { }

        public override string ToString()
        {
            return $"Id - {voiceaverage_id} \t    " +
                    $"voiceaverage_average - {voiceaverage_average}\t ";
        }
    }
}
