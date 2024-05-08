namespace Sqllite_Library.Models
{
    public class CBVCompany
    {
        public int company_id { get; set; }
        public string company_name { get; set; }
        public CBVCompany()
        {
        }

        public override string ToString()
        {
            return $"{company_id} \t {company_name}";
        }
    }
}
