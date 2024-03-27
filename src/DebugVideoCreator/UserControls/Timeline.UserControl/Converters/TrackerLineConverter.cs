using System;
using System.Globalization;
using System.Windows.Data;

namespace Timeline.UserControls.Converters
{
    public class TrackerLineConverter : OneWayConverter
    {
        public override object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {

            if (Value is double height)
            {
                return height * -1;
            }

            return Binding.DoNothing;
        }

    }
}
