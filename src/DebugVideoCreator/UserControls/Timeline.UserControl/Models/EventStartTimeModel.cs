using System;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    internal class EventStartTimeModel : NotifyPropertyChanged
    {
        private int _hour;
        public int Hour
        {
            get => _hour;
            set => Set(ref _hour, value);
        }


        private int _minute;
        public int Minute
        {
            get => _minute;
            set => Set(ref _minute, value);
        }

        private int _second;
        public int Second
        {
            get => _second;
            set => Set(ref _second, value);
        }


        public EventStartTimeModel(int hour, int minute, int second)
        {
            _hour = hour;
            _minute = minute;
            _second = second;


        }


        public TimeSpan ToTimeSpan() {
            return new TimeSpan(_hour, _minute, _second);
        }
    }
}
