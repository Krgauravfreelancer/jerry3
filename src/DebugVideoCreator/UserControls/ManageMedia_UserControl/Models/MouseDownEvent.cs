using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ManageMedia_UserControl.Models
{
    public class SelectedEvent
    {
        public int EventId;
        public int TrackId;
    }

    public class TrackbarMouseMoveEvent
    {
        public List<int> videoeventIds;
        public string timeAtTheMoment;
        public bool isAnyVideo;
    }

    public class MouseDownEvent
    {
        public SelectedEvent selectedEvent;
        public TrackbarMouseMoveEvent trackbarMouseMoveEvent;
    }
}
