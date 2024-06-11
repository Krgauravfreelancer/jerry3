using ManageMedia_UserControl.Controls.Items;
using System.Collections.Generic;

namespace ManageMedia_UserControl.Models
{
    public class LocationChangedEventModel
    {
        public List<TrackCalloutItem> CallOutItems { get; set; }
        public List<TrackVideoEventItem> VideoEventItems;
    }

    
}
