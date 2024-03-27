using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Converters
{
    internal class BooleanToStrokeThicknessConverter : OneWayConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is bool isSelected && isSelected)
            {
                return 4; // Set the desired StrokeThickness when the event is selected
            }
            else
            {
                return 1; // Set the default StrokeThickness when the event is not selected
            }

        }
    }
}
