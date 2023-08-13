namespace Sqllite_Library.Models
{
    public class CBVApp
    {
        public int app_id { get; set; }
        public string app_name { get; set; }
        public bool app_active { get; set; }
        public CBVApp()
        {
        }

        public override string ToString()
        {
            return $"{app_id} \t {app_name} \t {app_active}[app_active]";
        }
    }
}
