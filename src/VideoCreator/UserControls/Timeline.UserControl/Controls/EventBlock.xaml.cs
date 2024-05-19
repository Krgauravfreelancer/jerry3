using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for EventBlock.xaml
    /// </summary>
    public partial class EventBlock : UserControl, INotifyPropertyChanged
    {
        private bool _isEventSelected;
        public bool IsEventSelected
        {
            get { return _isEventSelected; }
            set
            {
                if (_isEventSelected != value)
                {
                    _isEventSelected = value;
                    OnPropertyChanged(nameof(IsEventSelected));
                }
            }
        }

        public bool ExtendModeLeft { get; set; }
        public bool ExtendModeRight { get; set; }

        public event EventHandler ExtendLeftStarted;
        public event EventHandler ExtendRightStarted;
        //public event EventHandler EventSelected;


        public EventBlock()
        {

            InitializeComponent();

            this.Loaded += EventBlock_Loaded;
        }

        private void EventBlock_Loaded(object sender, RoutedEventArgs e)
        {
            SetUpSelectionBorderEvents();

            // If the event belongs to a Callout1 or Callout2 track, then allow adjustment of event duration by dragging the left-right borders
            if (IsCalloutTrack())
            {
                SetUpLeftBorderEvents();
                SetUpRightBorderEvents();
            }
        }

        private bool IsCalloutTrack()
        {
            var timelineEvent = this.DataContext as TimelineEvent;

            return timelineEvent.TrackNumber == TrackNumber.Callout1 || timelineEvent.TrackNumber == TrackNumber.Callout2;
        }

        private void SetUpLeftBorderEvents()
        {
            EventLeftBorder.MouseEnter += (sender, e) =>
            {
                this.Cursor = Cursors.SizeWE;
                e.Handled = true;
            };

            EventLeftBorder.MouseLeftButtonDown += (sender, e) =>
            {
                ExtendModeLeft = true;
                this.Cursor = Cursors.SizeWE;

                OnExtendLeftStarted(EventArgs.Empty);
            };

            EventLeftBorder.MouseMove += (sender, e) =>
            {
                this.Cursor = Cursors.SizeWE;
            };

            // Handle MouseLeftButtonUp event to stop dragging
            EventLeftBorder.MouseLeftButtonUp += (sender, e) =>
            {
                ExtendModeLeft = false;
                this.Cursor = Cursors.Arrow;
            };


            EventLeftBorder.MouseLeave += (sender, e) =>
            {
                if (ExtendModeLeft)
                    this.Cursor = Cursors.SizeWE;
                else
                    this.Cursor = Cursors.Arrow;
            };
        }


        private void SetUpRightBorderEvents()
        {
            //RIGHT BORDER
            EventRightBorder.MouseLeftButtonDown += (sender, e) =>
            {
                ExtendModeRight = true;
                this.Cursor = Cursors.SizeWE;

                if (IsCalloutTrack())
                    OnExtendRightStarted(EventArgs.Empty);
            };

            EventRightBorder.MouseMove += (sender, e) =>
            {
                this.Cursor = Cursors.SizeWE;
            };

            EventRightBorder.MouseLeftButtonUp += (sender, e) =>
            {
                ExtendModeRight = false;
                this.Cursor = Cursors.Arrow;
            };

            EventRightBorder.MouseLeave += (sender, e) =>
            {
                if (ExtendModeRight)
                    this.Cursor = Cursors.SizeWE;
                else
                    this.Cursor = Cursors.Arrow;
            };
        }

        private void SetUpSelectionBorderEvents()
        {

            var defaultBorderBrush = Brushes.SlateGray;

            this.MouseLeftButtonDown += (sender, e) =>
            {
                IsEventSelected = true;

                EventLeftBorder.Stroke = defaultBorderBrush;
                EventRightBorder.Stroke = defaultBorderBrush;
                EventTopBorder.Stroke = defaultBorderBrush;
                EventBottomBorder.Stroke = defaultBorderBrush;


            };

            this.MouseEnter += (sender, e) =>
            {
                // Make the borders darker when the mouse hovers over the event
                if (!IsEventSelected)
                {
                    var hoverStrokeBrush = Brushes.DarkSlateGray;

                    EventLeftBorder.Stroke = hoverStrokeBrush;
                    EventRightBorder.Stroke = hoverStrokeBrush;
                    EventTopBorder.Stroke = hoverStrokeBrush;
                    EventBottomBorder.Stroke = hoverStrokeBrush;
                }
            };

            this.MouseLeave += (sender, e) =>
            {
                this.Cursor = Cursors.Arrow;

                if (!IsEventSelected)
                {
                    EventLeftBorder.Stroke = defaultBorderBrush;
                    EventRightBorder.Stroke = defaultBorderBrush;
                    EventTopBorder.Stroke = defaultBorderBrush;
                    EventBottomBorder.Stroke = defaultBorderBrush;
                }


            };

           
        }


        public override string ToString()
        {
            var videoEvent = this.DataContext as TimelineEvent;

            return $"EventId:{videoEvent.VideoEvent.videoevent_id}\tStartTime:{videoEvent.StartTimeStr}\tStartSecond:{videoEvent.StartSecond}\tDur:{videoEvent.EventDuration_Double} s\tWidth:{this.Width}";
        }


        // Implement INotifyPropertyChanged interface
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // Dependency property for EventBackground color
        public static readonly DependencyProperty EventBackgroundColorProperty =
            DependencyProperty.Register("EventBackgroundColor", typeof(Brush), typeof(EventBlock), new PropertyMetadata(Brushes.Red));

        public Brush EventBackgroundColor
        {
            get { return (Brush)GetValue(EventBackgroundColorProperty); }
            set { SetValue(EventBackgroundColorProperty, value); }
        }


        // Dependency property for EventName text
        public static readonly DependencyProperty EventNameTextProperty =
            DependencyProperty.Register("EventNameText", typeof(string), typeof(EventBlock), new PropertyMetadata(String.Empty));

        public string EventNameText
        {
            get { return (string)GetValue(EventNameTextProperty); }
            set { SetValue(EventNameTextProperty, value); }
        }


        // Dependency property for EventName TextColor
        public static readonly DependencyProperty EventNameTextColorProperty =
            DependencyProperty.Register("EventNameTextColor", typeof(Brush), typeof(EventBlock), new PropertyMetadata(Brushes.Black));

        public Brush EventNameTextColor
        {
            get { return (Brush)GetValue(EventNameTextColorProperty); }
            set { SetValue(EventNameTextColorProperty, value); }
        }




        protected virtual void OnExtendLeftStarted(EventArgs e)
        {
            ExtendLeftStarted?.Invoke(this, e);
        }


        protected virtual void OnExtendRightStarted(EventArgs e)
        {
            ExtendRightStarted?.Invoke(this, e);
        }



    }
}
