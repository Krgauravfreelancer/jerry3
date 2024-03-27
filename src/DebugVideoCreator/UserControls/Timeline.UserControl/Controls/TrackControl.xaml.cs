using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Timeline.UserControls.Config;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        public TimelineEvent SelectedEvent { get; set; }
        public event EventHandler TrackEventSelected;
        public event EventHandler<MouseEventArgs> EventMouseMove;
        public event EventHandler EventHoverLeave;
        public event EventHandler TimelineMaxWidthChanged;

        private bool _isEventDragInProgress = false;
        private Models.Timeline _timeline;
        private readonly EventBlockTooltipControl _eventBlockTooltip;

        public TrackControl()
        {
            InitializeComponent();

            // The tooltip will only be visible when a user hovers over an event
            _eventBlockTooltip = new EventBlockTooltipControl();
            _eventBlockTooltip.Visibility = Visibility.Hidden;
            EventsCanvas.Children.Add(_eventBlockTooltip);

            //if the user clicks on empty timeline, set the selectedEvent to null
            TrackControlBackground.MouseLeftButtonDown += (sender, e) =>
            {
                this.SelectedEvent = null;
                OnTrackEventSelected(EventArgs.Empty);
                e.Handled = true;
            };

            this.Loaded += new RoutedEventHandler(TrackControl_Loaded);
        }

        public void ResetSelectedEvent(TimelineEvent selectedEvent)
        {
            foreach (UIElement child in EventsCanvas.Children)
            {
                if (child is EventBlock eventBlock)
                {
                    var eventTimeline = eventBlock.DataContext as TimelineEvent;

                    if (selectedEvent != null)
                    {
                        eventBlock.IsEventSelected =
                            (selectedEvent.TrackNumber == TrackNumber.Notes && selectedEvent.Note?.notes_id == eventTimeline.Note?.notes_id) ||
                            (selectedEvent.TrackNumber != TrackNumber.Notes && selectedEvent.VideoEvent?.videoevent_id == eventTimeline.VideoEvent?.videoevent_id);

                    }
                    else
                    {
                        eventBlock.IsEventSelected = false;
                    }
                }
            }
        }

        //this will generate track events interface
        private void TrackControl_Loaded(object sender, RoutedEventArgs e)
        {
            _timeline = this.DataContext as Models.Timeline;

            if (_timeline != null)
            {
                this.Width = _timeline.TimelineMinWidth;

                this.MouseRightButtonDown += (sender2, e2) =>
                {
                    this.SelectedEvent = null;
                    OnTrackEventSelected(EventArgs.Empty);
                };

                if (this.Width > 0)
                {
                    //add empty grid if timeline has no events
                    if (!_timeline.TimelineEvents.Any())
                    {
                        var tempGrid = new Grid
                        {
                            Width = this.Width,
                            Background = Brushes.Transparent
                        };

                        //when user left-clicks on empty timeline, the selected event should be set to null
                        tempGrid.MouseLeftButtonDown += (tgsender, tge) =>
                        {
                            this.SelectedEvent = null;
                            tge.Handled = true;
                            OnTrackEventSelected(EventArgs.Empty);
                        };

                        //when user right-clicks on empty timeline, the selected event should be set to null
                        tempGrid.MouseRightButtonDown += (tgsender, tge) =>
                        {
                            this.SelectedEvent = null;
                            tge.Handled = true;
                            OnTrackEventSelected(EventArgs.Empty);
                        };

                        EventsCanvas.Children.Add(tempGrid);
                    }

                    foreach (var timelineEvent in _timeline.TimelineEvents)
                    {
                        var mediaBg = timelineEvent.EventMediaColor;
                        double eventStartLocation = (timelineEvent.StartSecond * _timeline.ScaleFactor);
                        double width = timelineEvent.EventDuration_Double * _timeline.ScaleFactor;


                        string eventName = timelineEvent.TrackNumber == TrackNumber.Notes ? $"{timelineEvent.EventMediaName} {timelineEvent.Note.notes_id}"
                                            : timelineEvent.GetTimelineMediaType() == MediaType.form ? timelineEvent.EventScreenName : $"{timelineEvent.EventMediaName} {timelineEvent.VideoEvent.videoevent_id}";



                        Brush eventBackground = timelineEvent.TrackNumber == TrackNumber.Notes
                                             ? timelineEvent.GetMediaBrush()
                                             : timelineEvent.GetTimelineMediaType() == MediaType.form
                                                 ? timelineEvent.GetScreenBrush()
                                                 : timelineEvent.GetMediaBrush();


                        var eventBlock = new EventBlock
                        {
                            Width = width,
                            Height = TimelineDefaultConfig.ItemHeight - 2,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            EventBackgroundColor = eventBackground,
                            EventNameText = eventName,
                            EventNameTextColor = eventBackground.IsDark() ? Brushes.White : Brushes.Black,
                            DataContext = timelineEvent
                        };

                        Canvas.SetLeft(eventBlock, eventStartLocation);

                        ConfigureEventBlockMouseActions(eventBlock, timelineEvent);

                        Panel.SetZIndex(eventBlock, 1);
                        Panel.SetZIndex(_eventBlockTooltip, 2);

                        eventBlock.PreviewMouseRightButtonDown += (sender2, e2) =>
                        {
                            //fire event to timelineGridControl  to set the current selected event  
                            this.SelectedEvent = timelineEvent;
                            e2.Handled = true;
                            OnTrackEventSelected(EventArgs.Empty);
                        };

                        EventsCanvas.Children.Add(eventBlock);
                    }
                }
            }
        }

        private void ConfigureEventBlockMouseActions(EventBlock eventBlock, TimelineEvent timelineEvent)
        {
            #region Mouse Hover

            eventBlock.MouseEnter += (sender, e) =>
            {

            };

            eventBlock.MouseMove += (mouseMoveSender, mouseMoveE) =>
            {
                _eventBlockTooltip.Visibility = Visibility.Visible;

                string tooltipText = $"{timelineEvent.EventBlockName} - {timelineEvent.StartTimeStr}";
                UpdateTooltipPosition(mouseMoveE, tooltipText);
            };

            eventBlock.MouseLeave += (mouseLeaveSender, mouseLeaveE) =>
            {
                _eventBlockTooltip.Visibility = Visibility.Hidden;
            };

            #endregion

            bool extendLeftStarted = false;
            bool extendRightStarted = false;

            eventBlock.ExtendLeftStarted += (sender, e) =>
            {
                Debug.WriteLine("extend left started...");
                extendLeftStarted = true;
            };

            eventBlock.ExtendRightStarted += (sender, e) =>
            {
                Debug.WriteLine("extend right started...");
                extendRightStarted = true;
            };

            eventBlock.PreviewMouseRightButtonDown += (sender2, e2) =>
            {

                //fire event to timelineGridControl  to set the current selected event  
                this.SelectedEvent = timelineEvent;
                e2.Handled = true;
                OnTrackEventSelected(EventArgs.Empty);
            };

            // Define a variable to store the offset between the mouse position and the left edge of the trackEventGrid
            double _mouseOffsetX = 0;

            // Defina a variable to store the previous mouse position when extending or shortening event duration
            double previousMouseLeft = 0;

            // Handle MouseLeftButtonDown event to initiate dragging
            eventBlock.MouseLeftButtonDown += (sender2, e2) =>
            {
                // Capture the mouse to track it even if it moves out of the control's bounds
                eventBlock.CaptureMouse();
                _isEventDragInProgress = true;

                // Calculate the offset between the mouse position and the left edge of the trackEventGrid
                var mousePos = e2.GetPosition(eventBlock);
                _mouseOffsetX = mousePos.X;

                Debug.WriteLine($"Mouse left button down -- MouseOffsetX: {_mouseOffsetX} \t\t EventStart:{timelineEvent.StartSecond * _timeline.ScaleFactor}");

                // Mark the event as selected on MouseLeftClick
                this.SelectedEvent = timelineEvent;
                OnTrackEventSelected(EventArgs.Empty);

                previousMouseLeft = (timelineEvent.StartSecond * _timeline.ScaleFactor);
            };

            eventBlock.MouseMove += (sender2, e2) =>
            {
                if (!_isEventDragInProgress) return;

                // Calculate the new position for the trackEventGrid based on the mouse position and offset
                var mousePos = e2.GetPosition(this);
                double mouseLeft = ((int)Math.Floor(mousePos.X) - _mouseOffsetX);

                Debug.WriteLine($"MouseLeft:{mouseLeft}\t\tX:{mousePos.X}\t\tOffset:{_mouseOffsetX}");

                // Calculate where the event will end based on its duration
                double eventEndLocation = mouseLeft + (timelineEvent.EventDuration_Double * _timeline.ScaleFactor);

                if (_mouseOffsetX >= 0)
                {
                    if (extendLeftStarted)
                    {
                        if (DraggableToLeft(ref timelineEvent, mousePos.X))
                        {
                            // Left-border extend to left
                            if (mouseLeft < previousMouseLeft)
                            {
                                double extended = (previousMouseLeft) - mouseLeft;
                                double prevWidth = eventBlock.Width;
                                double newWidth = eventBlock.Width + extended;

                                eventBlock.Width = newWidth;

                                Canvas.SetLeft(eventBlock, mouseLeft);

                                previousMouseLeft = mouseLeft;

                                UpdateEventStartAndDuration(ref timelineEvent, eventBlock, eventEndLocation, mouseLeft);

                                Debug.WriteLine($"Extend Left --- Left:{mouseLeft}\t\tExtended: {extended} \t\t{eventEndLocation}\tEVENT={eventBlock.ToString()}");
                            }

                            // Left-border shorten to right
                            else if (mouseLeft > previousMouseLeft)
                            {
                                double shrinkAmount = Math.Max(mouseLeft - previousMouseLeft, 0);
                                eventBlock.Width = Math.Max(eventBlock.Width - shrinkAmount, 1);
                                Canvas.SetLeft(eventBlock, mouseLeft);

                                previousMouseLeft = mouseLeft;

                                UpdateEventStartAndDuration(ref timelineEvent, eventBlock, eventEndLocation, mouseLeft);

                                Debug.WriteLine($"Shrink to right --- Left:{mouseLeft}\t\tExtended: {shrinkAmount} \t\t{eventEndLocation}\tEVENT={eventBlock.ToString()}");

                            }

                            // If the mouse position remains unchanged while the left-click is held
                            else
                            {
                                Debug.WriteLine("Mouse not moving");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Not draggable to left");
                        }
                    }
                    else if (extendRightStarted)
                    {
                        double endPoint = timelineEvent.StartSecond + ((timelineEvent.EventDuration_Double) * _timeline.ScaleFactor);

                        if (DraggableToRight(ref timelineEvent, mousePos.X))
                        {
                            // Right-border extend duration
                            if (mouseLeft > previousMouseLeft)
                            {
                                double extended = mouseLeft - (previousMouseLeft);
                                double newWidth = eventBlock.Width + extended;
                                eventBlock.Width = newWidth;

                                previousMouseLeft = mouseLeft;

                                UpdateEventDuration(ref timelineEvent, eventBlock, endPoint);

                                Debug.WriteLine($"Extend to right --- Left:{mouseLeft}\t\tExtended: {extended} \t\t{eventEndLocation}\tEVENT={eventBlock.ToString()}");
                            }

                            // Right-border shorten duration
                            else if (mouseLeft < previousMouseLeft)
                            {
                                double shrink = (previousMouseLeft) - mouseLeft;

                                double newWidth = Math.Max(shrink, 1);
                                eventBlock.Width = Math.Max(eventBlock.Width - newWidth, 1);

                                previousMouseLeft = mouseLeft;

                                UpdateEventDuration(ref timelineEvent, eventBlock, endPoint);

                                Debug.WriteLine($"Shrink to Left --- Left:{mouseLeft}\t\tExtended: {shrink} \t\t{eventEndLocation}\tEVENT={eventBlock.ToString()}");
                            }
                            else
                            {
                                Debug.WriteLine("Mouse not moving");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Not draggable to right");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Drag only check");

                        // This will only drag the event to left or right, duration will not change
                        if (IsDraggable(ref timelineEvent, mouseLeft, eventEndLocation))
                        {
                            Debug.WriteLine("Dragging event...");

                            Canvas.SetLeft(eventBlock, mouseLeft);
                            UpdateEventStartAndDuration(ref timelineEvent, eventBlock, eventEndLocation, mouseLeft);
                        }
                        else
                        {
                            Debug.WriteLine("Not draggable");
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Cannot drag below 0");
                }
            };

            // Handle MouseLeftButtonUp event to stop dragging
            eventBlock.MouseLeftButtonUp += (sender2, e2) =>
            {
                _isEventDragInProgress = false;
                extendLeftStarted = false;
                extendRightStarted = false;

                Debug.WriteLine($"Mouse Left Button Up. EventBlockWidth:{eventBlock.Width}");

                // Release the mouse capture when the dragging operation is complete
                eventBlock.ReleaseMouseCapture();

                eventBlock.Cursor = Cursors.Arrow;
            };
        }

        private bool IsDraggable(ref TimelineEvent timelineEvent, double mouseLeft, double eventEndLocation)
        {
            // Check if the event can be dragged to the left and to the right
            return DraggableToLeft(ref timelineEvent, mouseLeft) && DraggableToRight(ref timelineEvent, eventEndLocation);
        }


        private void UpdateEventStartAndDuration(ref TimelineEvent timelineEvent, EventBlock eventBlock, double eventEndLocation, double mouseLeft)
        {
            Debug.WriteLine($"Updating event start and duration...");

            // Calculate the new position based on mouse left position and timeline scale factor
            var newPositionTimeSpan = TimeSpan.FromSeconds(mouseLeft / _timeline.ScaleFactor);

            // Set a base date for reference (e.g., 1990-01-01) and add the calculated time span
            DateTime baseDate = new DateTime(1990, 01, 01);
            DateTime newStartTime = baseDate + newPositionTimeSpan;

            // Update the timeline event with the new start time, duration and mark it as modified
            timelineEvent.StartTime = newPositionTimeSpan;
            timelineEvent.Modified = true;

            var timeSpan = TimeSpan.FromSeconds(eventBlock.Width / _timeline.ScaleFactor);
            timelineEvent.EventDuration = $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";


            // Check if the event has been dragged beyond the current width of the timeline
            if (eventEndLocation >= this.Width)
            {
                // Adjust the width of the timeline to accommodate the extended event position
                this.Width = eventEndLocation;

                // Notify subscribers about the change in timeline's maximum width
                OnTimelineMaxWidthChanged(EventArgs.Empty);
            }
        }

        private bool DraggableToLeft(ref TimelineEvent timelineEvent, double mouseLeft)
        {
            if (_timeline.TimelineEvents.Any())
            {
                var sortedTimelineEvents = _timeline.TimelineEvents.OrderBy(e => e.StartTime).ToArray();

                TimelineEvent leftEvent = null;
                TimelineEvent currentEvent = null;

                for (int i = 0; i < sortedTimelineEvents.Length; i++)
                {
                    currentEvent = sortedTimelineEvents[i];

                    bool isSelectedEvent = timelineEvent.TrackNumber == TrackNumber.Notes ? (currentEvent.Note.notes_id == timelineEvent.Note.notes_id)
                                            : (currentEvent.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);

                    if (isSelectedEvent)
                    {
                        leftEvent = (i > 0) ? sortedTimelineEvents[i - 1] : null;

                        if (mouseLeft >= 0)
                        {
                            if (leftEvent != null)
                            {
                                double leftEventEndpoint = (leftEvent.StartSecond + leftEvent.EventDuration_Double) * _timeline.ScaleFactor;

                                Debug.WriteLine($"Dragging to left at point {mouseLeft}\tLeftEndpoint:{leftEventEndpoint}\tCheck:{mouseLeft}>={leftEventEndpoint}");

                                return (mouseLeft >= leftEventEndpoint);
                            }
                            else
                            {
                                Debug.WriteLine("Dragged event is the last event");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool DraggableToRight(ref TimelineEvent timelineEvent, double endLocation)
        {
            if (_timeline.TimelineEvents.Any())
            {
                var timelineEventsArray = _timeline.TimelineEvents.OrderBy(e => e.StartTime).ToArray();

                TimelineEvent rightEvent = null;
                TimelineEvent currentEvent = null;

                for (int i = 0; i < timelineEventsArray.Length; i++)
                {
                    currentEvent = timelineEventsArray[i];

                    bool isSelectedEvent = timelineEvent.TrackNumber == TrackNumber.Notes ? (currentEvent.Note.notes_id == timelineEvent.Note.notes_id)
                                             : (currentEvent.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);        

                    if (isSelectedEvent)
                    {
                        if (i < timelineEventsArray.Length - 1)
                            rightEvent = timelineEventsArray[i + 1];
                        else
                            rightEvent = null;

                        if (rightEvent != null)
                        {
                            Debug.WriteLine("Dragged event has another event on the right");

                            double rightEventStartPoint = ((rightEvent.StartSecond) * _timeline.ScaleFactor);

                            Debug.WriteLine($"Dragging to right at point {endLocation} \tRightStartPoint:{rightEventStartPoint} \tCheck: {endLocation} <= {rightEventStartPoint}");

                            if (endLocation <= rightEventStartPoint)
                            {
                                return true;
                            }

                            else return false;
                        }

                        else
                        {
                            Debug.WriteLine("Dragged event is the last event");
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        private void UpdateEventDuration(ref TimelineEvent timelineEvent, EventBlock eventBlock, double eventEndLocation)
        {
            Debug.WriteLine($"Updating event duration...");

            timelineEvent.Modified = true;
            var timeSpan = TimeSpan.FromSeconds(eventBlock.Width / _timeline.ScaleFactor);
            timelineEvent.EventDuration = $"{(int)timeSpan.TotalHours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";

            // Check if the event has been dragged beyond the current width of the timeline
            if (eventEndLocation >= this.Width)
            {
                // Adjust the width of the timeline to accommodate the extended event position
                this.Width = eventEndLocation;

                // Notify subscribers about the change in timeline's maximum width
                OnTimelineMaxWidthChanged(EventArgs.Empty);
            }
        }

        private void UpdateTooltipPosition(MouseEventArgs e, string tooltipText)
        {
            // Update the tooltip position based on the mouse pointer's position
            Point mousePosition = e.GetPosition(EventsCanvas);

            // Set the position of the tooltip relative to the main control
            Canvas.SetLeft(_eventBlockTooltip, mousePosition.X + 10); // Adjust the offset as needed
            Canvas.SetTop(_eventBlockTooltip, mousePosition.Y - 10); // Adjust the offset as needed

            // Update the text property of the tooltip using the Dispatcher
            Dispatcher.Invoke(() => _eventBlockTooltip.EventBlockTooltipText = tooltipText);
        }

        protected virtual void OnEventHoverLeave(EventArgs e)
        {
            EventHoverLeave?.Invoke(this, e);
        }

        protected virtual void OnEventMouseMove(MouseEventArgs e)
        {
            EventMouseMove?.Invoke(this, e);
        }

        protected virtual void OnTrackEventSelected(EventArgs e)
        {
            TrackEventSelected?.Invoke(this, e);
        }

        protected virtual void OnTimelineMaxWidthChanged(EventArgs e)
        {
            TimelineMaxWidthChanged?.Invoke(this, e);
        }
    }
}
