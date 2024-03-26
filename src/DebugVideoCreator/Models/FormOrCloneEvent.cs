using Sqllite_Library.Models;
using System.Collections.Generic;
using Timeline.UserControls.Models;

namespace VideoCreator.Models
{
    public class FormOrCloneEvent
    {
        public TimelineVideoEvent timelineVideoEvent;
        public string timeAtTheMoment;
    }


    public class TrackbarMouseMoveEvent
    {
        public List<int> videoeventIds;
        public string timeAtTheMoment;
        public bool isAnyVideo;
    }

    public class TimelineSelectedEvent
    {
        public int EventId;
        public TrackNumber Track;
    }
}
