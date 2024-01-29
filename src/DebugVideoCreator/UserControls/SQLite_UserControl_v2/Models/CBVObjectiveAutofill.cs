using System;

namespace Sqllite_Library.Models
{
    public class CBVObjectiveAutofill
    {
        public int objectiveautofill_id { get; set; }
        public int fk_objectiveautofill_project { get; set; }
        public string objectiveautofill_name { get; set; }
        //public int objectiveautofill_importance { get; set; }
        public bool objectiveautofill_active { get; set; }
        public DateTime objectiveautofill_createdate { get; set; }
        public DateTime objectiveautofill_modifydate { get; set; }
        public Int64 objectiveautofill_serverid { get; set; }
        public bool objectiveautofill_issynced { get; set; }
        public string objectiveautofill_syncerror { get; set; }
        public bool objectiveautofill_isdeleted { get; set; }
       
    }
}
