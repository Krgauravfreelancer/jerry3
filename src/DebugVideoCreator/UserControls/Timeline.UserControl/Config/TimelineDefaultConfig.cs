using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timeline.UserControls.Config
{
    internal static class TimelineDefaultConfig
    {
        public const double Column1ItemWidth = 150;
        public const double Column2HeaderHeight = 70;
        public const double ItemHeight = 30;

        public const double Column2_HeaderOneMinuteWidth = 60;
        public const double Column2_HeaderOneMinuteScaleHeight = 30;
        public const double Column2_HeaderTenSecondsScaleHeight = 20;
        public const double Column2_HeaderTenSecondsMinuteWidth = 10;

        public const double TrackControlBackgroundHeight = 20;

        public const double ZoomLimit = 8;
        public const double SliderZoomLimit = ZoomLimit + 1;

        public const string EventTimeFormat = @"hh\:mm\:ss.fff";
        public const string DateTimeStringFormat = "yyyy-MM-dd HH:mm:ss";
    }
}
