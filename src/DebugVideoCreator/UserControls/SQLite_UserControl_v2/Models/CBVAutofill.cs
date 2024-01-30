using System.Collections.Generic;

namespace Sqllite_Library.Models
{
    public class CBVAutofill
    {
        public List<CBVNextAutofill> next_autofill { get; set; }
        public List<CBVObjectiveAutofill> objective_autofill { get; set; }
        public List<CBVRequireAutofill> require_autofill { get; set; }
    }
}
