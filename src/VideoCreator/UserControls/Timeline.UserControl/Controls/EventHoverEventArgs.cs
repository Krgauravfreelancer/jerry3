using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Timeline.UserControls.Controls
{
    public class EventHoverEventArgs : EventArgs
    {
        public Grid TimelineEventGrid { get; }

        public EventHoverEventArgs(Grid timelineEventGrid)
        {
            TimelineEventGrid = timelineEventGrid;
        }
    }

}
