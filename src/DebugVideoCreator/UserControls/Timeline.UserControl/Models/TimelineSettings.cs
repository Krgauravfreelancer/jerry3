using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    /// <summary>
    /// Any changes made on the TimelineSettings properties will automatically rebuild the timeline. (Data not modified)
    /// </summary>
    internal class TimelineSettings : NotifyPropertyChanged
    {
        private double _scaleFactor;
        public double ScaleFactor
        {
            get => _scaleFactor;
            set => Set(ref _scaleFactor, value);
        }

        private Brush _eventBg;
        public Brush TrackBg { 
            get => _eventBg;
            set => Set(ref _eventBg, value);
        }

        private string _timeFormat;
        public string TimeFormat
        {
            get => _timeFormat;
            set => Set(ref _timeFormat, value);
        }

        private string _dateTimeFormat;
        public string DateTimeFormat
        {
            get => _dateTimeFormat;
            set => Set(ref _dateTimeFormat, value);
        }

        public TimelineSettings()
        {
            //set default settings;
            ScaleFactor = 1;
            TrackBg = Brushes.LightBlue;
        }
    }
}
