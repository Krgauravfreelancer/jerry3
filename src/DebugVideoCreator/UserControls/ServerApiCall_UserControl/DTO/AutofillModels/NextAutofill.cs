using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.DTO.AutofillModels
{
    public class NextAutofill
    {
        public int nextautofill_id { get; set; }
        public int fk_nextautofill_project { get; set; }
        public string nextautofill_name { get; set; }
        public int nextautofill_importance { get; set; }
        public bool nextautofill_active { get; set; }
    }
}
