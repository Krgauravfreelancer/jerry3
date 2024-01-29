using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.DTO.AutofillModels
{
    public class Autofill
    {
        public List<RequireAutofill> require_autofill { get; set; }
        public List<NextAutofill> next_autofill { get; set; }
        public List<ObjectiveAutofill> objective_autofill { get; set; }

        public Autofill() 
        {
            require_autofill = new List<RequireAutofill>();
            next_autofill = new List<NextAutofill>();
            objective_autofill = new List<ObjectiveAutofill>();
        }
    }
}
