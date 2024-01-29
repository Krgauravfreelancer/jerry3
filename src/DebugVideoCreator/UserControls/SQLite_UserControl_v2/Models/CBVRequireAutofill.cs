using System;

namespace Sqllite_Library.Models
{
    public class CBVRequireAutofill
    {
        public int requireautofill_id { get; set; }
        public int fk_requireautofill_project { get; set; }
        public string requireautofill_name { get; set; }
        public int requireautofill_importance { get; set; }
        public bool requireautofill_active { get; set; }
        public DateTime requireautofill_createdate { get; set; }
        public DateTime requireautofill_modifydate { get; set; }
        public Int64 requireautofill_serverid { get; set; }
        public bool requireautofill_issynced { get; set; }
        public string requireautofill_syncerror { get; set; }
        public bool requireautofill_isdeleted { get; set; }
       
    }
}
