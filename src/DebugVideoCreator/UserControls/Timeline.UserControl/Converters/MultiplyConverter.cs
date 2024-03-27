using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Converters
{
    //internal class MultiplyConverter : OneWayConverter
    //{
    //    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {

    //        if (value is double value1 && parameter is double ScaleFactor)
    //        {
    //            return value1 * ScaleFactor;
    //        }

    //        return Binding.DoNothing;
    //    }


    //}

    public class MultiplyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type TargetType, object Parameter, CultureInfo Culture)
        {
            if (values.Length == 2 && values[0] is double value1 && values[1] is double ScaleFactor)
            {
                return (value1 * ScaleFactor);
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
