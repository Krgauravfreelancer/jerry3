using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    internal class TimelineLayoutModel : NotifyPropertyChanged
    {
        private double _minWidth;
        public double MinWidth
        {
            get => _minWidth;
            set => Set(ref _minWidth, value);
        }

        private double _trackbarHeight;
        public double TrackbarHeight
        {
            get => _trackbarHeight;
            set => Set(ref _trackbarHeight, value);
        }
    }
}
