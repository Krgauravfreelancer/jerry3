using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    internal class TimelineSelectionModel : NotifyPropertyChanged
    {
        private TimelineEvent _selectedEvent;
        public TimelineEvent SelectedEvent
        {
            get => _selectedEvent;
            set => SetAndAlwaysRaisePropertyChanged(ref _selectedEvent, value);
        }

        public TimelineSelectionModel() { }
    }
}
