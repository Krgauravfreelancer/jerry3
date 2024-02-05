using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.UserControls.Base;
using Timeline.UserControls.Models;

namespace DebugVideoCreator.Models
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
}
