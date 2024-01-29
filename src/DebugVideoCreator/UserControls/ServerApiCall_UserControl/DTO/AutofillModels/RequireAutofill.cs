using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.DTO.AutofillModels
{
    public class RequireAutofill
    {
        public int requireautofill_id { get; set; }
        public int fk_requireautofill_project { get; set; }
        public string requireautofill_name { get; set; }
        public int requireautofill_importance { get; set; }
        public bool requireautofill_active { get; set; }
    }
}
