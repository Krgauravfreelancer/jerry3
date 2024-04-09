using Sqllite_Library.Models;
using System.Collections.Generic;
using MMClass = ManageMedia_UserControl.Classes;

namespace VideoCreator.Models
{
    public class FormOrCloneEvent
    {
        public MMClass.Media timelineVideoEvent;
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
        public EnumTrack Track;
    }
}
