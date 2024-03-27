using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Media;

namespace Timeline.UserControls
{
    public static class TimelineExtensions
    {
        public const double ExtraWidthForDisplay = 0;

      

        public static DateTime ConvertToDatetime(TimeSpan timeSpan) { 
            return new DateTime(timeSpan.Ticks);
        }


        public static bool IsDark(this Brush brush)
        {
            SolidColorBrush solidColorBrush = brush as SolidColorBrush;

            if (solidColorBrush != null)
            {
                Color color = solidColorBrush.Color;

                // Calculate brightness based on luminance
                double luminance = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
                return luminance < 128; // Adjust the threshold to your preference
            }

            return false; // Return false if brush is not a SolidColorBrush
        }


    }
}
