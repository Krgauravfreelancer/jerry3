using System;
using System.Windows.Media;
using Timeline.UserControls.Base;
using Timeline.UserControls.Config;
using Timeline.UserControls.Exceptions;

namespace Timeline.UserControls.Models
{
    public class TimelineEvent : NotifyPropertyChanged
    {
        public string EventScreenColor { get; set; }

        public bool Modified { get; set; }
        public bool Selected { get; set; }
        public TrackNumber TrackNumber { get; set; }
        public TimelineNote Note { get; set; }
        public TimelineVideoEvent VideoEvent { get; set; }

 

        private string _eventDuration;

        /// <summary>
        /// Used for videoevent_duration and notes_duration
        /// </summary>
        public string EventDuration
        {
            get => _eventDuration;
            set => Set(ref _eventDuration, value);
        }

        /// <summary>
        /// Returns the event_duration in TimeSpan format
        /// </summary>
        public TimeSpan EventDuration_TimeSpan => TimeSpan.Parse(_eventDuration);

        /// <summary>
        /// Returns the event_duration in seconds
        /// </summary>
        public double EventDuration_Double => EventDuration_TimeSpan.TotalSeconds;

        private string _EventBlockName;
        public string EventBlockName
        {
            get
            {
                if (TrackNumber == TrackNumber.Notes)
                    return $"{EventMediaName} {Note.notes_id}";
                else
                    return (GetTimelineMediaType() == MediaType.form) ? EventScreenName : $"{EventMediaName} {VideoEvent.videoevent_id}";
            }
            set => Set(ref _EventBlockName, value);
        }

        private string _EventMediaName;
        public string EventMediaName
        {
            get => _EventMediaName;
            set => Set(ref _EventMediaName, value);
        }

        private int? _eventScreen;
        public int? EventScreen
        {
            get => _eventScreen;
            set => Set(ref _eventScreen, value);
        }

        private string _eventScreenName;
        public string EventScreenName
        {
            get => _eventScreenName;
            set => Set(ref _eventScreenName, value);
        }



        /// <summary>
        /// Use StartTime property to set the value of event_start in TimeSpan data type
        /// </summary>
        public string EventStart
        {
            get
            {
                return StartTimeStr;
            }
        }

        public string StartTimeStr
        {
            get
            {
                var timeSpan = StartTime;
                return $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
            }
        }

        public double StartSecond
        {
            get
            {
                return (StartTime).TotalSeconds;
            }
        }

        public TimeSpan StartTime { get; set; }
        public TimeSpan? EndTime => StartTime.Add(TimeSpan.FromSeconds(EventDuration_Double));

        private string _eventMediaColor;
        public string EventMediaColor
        {
            get => _eventMediaColor;
            set => Set(ref _eventMediaColor, value);
        }

        private double _scaleFactor;
        public double ScaleFactor
        {
            get => _scaleFactor;
            set => Set(ref _scaleFactor, value);
        }

        public MediaType GetTimelineMediaType()
        {

            bool success = Enum.TryParse(VideoEvent.fk_videoevent_media.ToString(), out MediaType mediaType);
            if (success)
                return mediaType;

            else
                throw new UnknownMediaTypeException(VideoEvent.fk_videoevent_media);
        }

        public Brush GetMediaBrush()
        {
            SolidColorBrush mediaColorBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(EventMediaColor);
            return mediaColorBrush;
        }

        public Brush GetScreenBrush()
        {
            SolidColorBrush mediaColorBrush = (SolidColorBrush)new BrushConverter().ConvertFromString(EventScreenColor);
            return mediaColorBrush;
        }

    }
}
