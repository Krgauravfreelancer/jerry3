using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Timeline.UserControls.Config;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Converters
{
    internal class ScreenVisibilityConverter : OneWayConverter
    {
        public override object Convert(object Value, Type TargetType, object Parameter, CultureInfo Culture)
        {

            if (Value is int mediaId)
            {
                if(mediaId == (int)MediaType.form)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Binding.DoNothing;
        }
    }
}
