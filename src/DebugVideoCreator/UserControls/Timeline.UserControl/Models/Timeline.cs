using System;
using System.Collections.ObjectModel;
using System.Linq;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    public class Timeline : NotifyPropertyChanged
    {
        private int _timelineNumber;
        public int TimelineNumber
        {
            get => _timelineNumber;
            set => Set(ref _timelineNumber, value);
        }

        private double _timelineMinWidth;
        public double TimelineMinWidth
        {
            get => _timelineMinWidth;
            set => Set(ref _timelineMinWidth, value);
        }


        private bool _modeEvent;
        public bool ModeEvent
        {
            get => _modeEvent;
            set => Set(ref _modeEvent, value);
        }

        private ObservableCollection<TimelineEvent> _timelineEvents;
        public ObservableCollection<TimelineEvent> TimelineEvents
        {
            get => _timelineEvents;
            set => Set(ref _timelineEvents, value);
        }

        private bool _selectedTracker;
        public bool SelectedTracker
        {
            get => _selectedTracker;
            set => Set(ref _selectedTracker, value);
        }

        public TrackNumber TrackNumber { get; private set; }

        public Timeline(TrackNumber trackNumber)
        {
            _timelineEvents = new ObservableCollection<TimelineEvent>();
            TrackNumber = trackNumber;
            ScaleFactor = 1;
        }


        /// helper properties
        private double _scaleFactor;
        public double ScaleFactor
        {
            get => _scaleFactor;
            set => Set(ref _scaleFactor, value);
        }


        public TimeSpan GetNextVideoEventStart()
        {
            // Order events by StartTime in descending order
            var orderedEvents = _timelineEvents.OrderByDescending(t => t.StartTime);

            // Find the first event with a valid StartTime and add its duration to get the next start time
            var nextVideoEventStart = orderedEvents
                //.Select(e => e.StartTime.AddSeconds(e.videoevent_duration)) // Add duration in seconds to the StartTime
                //.Select(e => e.StartTime.AddSeconds(e.VideoEventDuration_Double)) // Add duration in seconds to the StartTime
                .Select(e => e.StartTime.Add(TimeSpan.FromSeconds(e.EventDuration_Double))) // Add duration in seconds to the StartTime
                .FirstOrDefault();

            return nextVideoEventStart;
        }


    }

    public enum TrackNumber
    {
        Notes = -1,
        Audio = 1,
        Video = 2,
        Callout1 = 3,
        Callout2 = 4
    }
}
