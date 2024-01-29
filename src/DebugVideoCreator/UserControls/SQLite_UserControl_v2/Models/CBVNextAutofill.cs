using System;

namespace Sqllite_Library.Models
{
    public class CBVNextAutofill
    {
        public int nextautofill_id { get; set; }
        public int fk_nextautofill_project { get; set; }
        public string nextautofill_name { get; set; }
        public int nextautofill_importance { get; set; }
        public bool nextautofill_active { get; set; }
        public DateTime nextautofill_createdate { get; set; }
        public DateTime nextautofill_modifydate { get; set; }
        public Int64 nextautofill_serverid { get; set; }
        public bool nextautofill_issynced { get; set; }
        public string nextautofill_syncerror { get; set; }
        public bool nextautofill_isdeleted { get; set; }
       
    }
}
