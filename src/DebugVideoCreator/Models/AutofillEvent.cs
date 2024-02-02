using Sqllite_Library.Models;

namespace DebugVideoCreator.Models
{
    public class AutofillEvent
    {
        public AutofillEnumType AutofillType;
        public string timeAtTheMoment;
        public int Duration { get; set; } = 10;
    }

}
