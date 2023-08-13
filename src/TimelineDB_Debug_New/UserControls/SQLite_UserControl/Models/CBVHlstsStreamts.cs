namespace Sqllite_Library.Models
{
    public class CBVStreamts
    {
        public int streamts_id { get; set; }
        public int fk_streamts_hlsts { get; set; }
        public int fk_streamts_resolution { get; set; }
        public int streamts_increment { get; set; }
        public byte[] streamts_stream { get; set; }
        public CBVStreamts()
        {
        }

        public override string ToString()
        {
            return $"{streamts_id} \t {fk_streamts_hlsts} [hlstsId] \t {fk_streamts_resolution} [resolutionId] \t {streamts_increment} [increment] \t {streamts_stream.Length} [stream]";
        }
    }
}
