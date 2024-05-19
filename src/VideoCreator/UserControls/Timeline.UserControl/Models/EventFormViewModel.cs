using System.Collections.Generic;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    internal class EventFormViewModel : NotifyPropertyChanged
    {
        public string MediaDisplayName => $"Media: {TimelineEvent?.EventMediaName}";

        private TimelineEvent _timelineEvent;
        public TimelineEvent TimelineEvent
        {
            get => _timelineEvent;
            set => Set(ref _timelineEvent, value);
        }

        public List<TimelineScreen> ScreenModels { get; set; }
    }
}
