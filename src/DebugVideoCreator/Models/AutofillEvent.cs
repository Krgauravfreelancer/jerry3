using Sqllite_Library.Models;

namespace VideoCreator.Models
{
    public class AutofillEvent
    {
        public AutofillEnumType AutofillType;
        public string timeAtTheMoment;
        public string Duration { get; set; } = "00:00:10.000";
        public int DurationInSec { get; set; } = 10;
    }

}
