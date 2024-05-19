using System;

namespace Timeline.UserControls.Models
{
    public class TimelineEventArgs : EventArgs
    {
        public TimelineEvent TimelineEvent { get; set; }

        public TimelineEventArgs(TimelineEvent timelineEvent)
        {
            TimelineEvent = timelineEvent;
        }
    }
}
