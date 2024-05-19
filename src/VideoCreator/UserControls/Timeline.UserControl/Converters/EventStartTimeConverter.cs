using System.Globalization;
using System;
using System.Windows.Data;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Converters
{
    internal class EventStartTimeConverter : OneWayConverter
    {
        public override object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {

            if (Value is DateTime startTime)
            {
                var startTimeSpan = startTime.TimeOfDay;
                return new EventStartTimeModel(startTimeSpan.Hours, startTimeSpan.Minutes, startTimeSpan.Seconds);
            }

            return Binding.DoNothing;
        }
    }
}
