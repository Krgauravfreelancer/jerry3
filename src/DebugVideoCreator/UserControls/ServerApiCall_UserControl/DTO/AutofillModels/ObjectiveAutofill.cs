using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.DTO.AutofillModels
{
    public class ObjectiveAutofill
    {
        public int objectiveautofill_id { get; set; }
        public int fk_objectiveautofill_project { get; set; }
        public string objectiveautofill_name { get; set; }
        public bool objectiveautofill_active { get; set; }
    }
}
