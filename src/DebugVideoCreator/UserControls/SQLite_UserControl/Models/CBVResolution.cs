namespace Sqllite_Library.Models
{
    public class CBVResolution
    {
        public int resolution_id { get; set; }
        public string resolution_name { get; set; }
        public CBVResolution()
        {
        }

        public override string ToString()
        {
            return $"{resolution_id} \t {resolution_name}";
        }
    }
}
