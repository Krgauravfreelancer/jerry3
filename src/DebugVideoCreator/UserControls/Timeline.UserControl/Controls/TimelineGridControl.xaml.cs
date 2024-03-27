using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Timeline.UserControls.Config;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for TimelineGridControl.xaml
    /// </summary>
    public partial class TimelineGridControl : UserControl, ITimelineGridControl
    {
        public TimeSpan TrackbarPosition;

        /// <summary>
        /// This triggers when a user moves the trackbar on mouse click.
        /// </summary>
        public event EventHandler TrackbarMouseMoved;

        /// <summary>
        /// This triggers when a user checks or unchecks any trackbar checkboxes.
        /// </summary>
        public event EventHandler TimelineCheckboxSelected;

        /// <summary>
        /// This triggers when the data of the timeline datatables have been changed.
        /// </summary>
        public event EventHandler TimelineDatabaseModelChanged;

        /// <summary>
        /// This triggers when a user right-clicks on a timeline or event.
        /// </summary>
        public event EventHandler TimelineSelectionChanged;

        /// <summary>
        /// This triggers when a video event is deleted.
        /// </summary>
        public event EventHandler<TimelineEventArgs> TimelineVideoEventDeleted;

        public bool _isTrackbarLineDragInProg { get; private set; }

        private TimelineViewModel _timelineViewModel;
        private TimelineLayoutModel _timelineLayoutModel;
        private TimelineSettings _timelineSettings;

        private readonly TimelineDatabaseModel _timelineDatabaseModel;
        private readonly TimelineSelectionModel _timelineSelectionModel;

        public TimelineGridControl()
        {
            _timelineDatabaseModel = new TimelineDatabaseModel();
            _timelineLayoutModel = new TimelineLayoutModel();
            _timelineViewModel = new TimelineViewModel();
            _timelineSelectionModel = new TimelineSelectionModel();
            _timelineSettings = new TimelineSettings();

            InitializeComponent();

            SetTrackbarMouseEvents();

            //tracks changes on datatable
            ((System.ComponentModel.INotifyPropertyChanged)_timelineDatabaseModel).PropertyChanged += (sender, e) =>
            {
                ReloadTimelineData();
                OnTimelineDatabaseModelChanged(EventArgs.Empty);
            };

            //tracks changes on left-click and right-click selection
            _timelineSelectionModel.PropertyChanged += (sender, e) =>
            {
                OnTimelineSelectionChanged(EventArgs.Empty);
            };

            _timelineSettings.PropertyChanged += (sender, e) =>
            {
                RebuildTimeline();
            };

            _timelineLayoutModel.PropertyChanged += (sender, e) =>
            {


                //set the column2 width to the minimum width for the scrollviewer to work
                Column2BaseLayer.Width = _timelineLayoutModel.MinWidth;
                _timelineViewModel.SetTimelinesMinWidth(_timelineLayoutModel.MinWidth);

                BuildTimelineHeader(_timelineLayoutModel.MinWidth, _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor);


                //set the new width for all trackControls
                foreach (UIElement child in Column2TrackItems.Children)
                {
                    if (child is TrackControl trackControlChild)
                    {
                        trackControlChild.Width = _timelineLayoutModel.MinWidth;
                    }
                }
            };

            this.Loaded += new RoutedEventHandler(TimelineGridControl_Loaded);

            // Update the ScrollableWidth after setting the initial timeline scale factor
            ScrollToTrackbar();
        }

        private void TimelineGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentWindow != null)
            {
                //double width = ParentWindow.Width;
                double width = EventScroll.ViewportWidth;

                if (_timelineLayoutModel?.MinWidth < width)
                {
                    _timelineLayoutModel.MinWidth = width;
                }

                // If the window is maximized or minized, then adjust the minimum width of the timeline layout
                ParentWindow.SizeChanged += ParentWindowSizeChanged;


                Column2BaseLayer_Events();
            }


        }





        private void ParentWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Handle the event when the parent window's size changes.
            double newWidth = e.NewSize.Width;

            if (_timelineLayoutModel.MinWidth < newWidth)
            {
                // Set the new minimum width to the parent window width
                _timelineLayoutModel.MinWidth = newWidth;
            }
        }

        protected virtual void OnTimelineDatabaseModelChanged(EventArgs e)
        {
            TimelineDatabaseModelChanged?.Invoke(this, e);
        }

        protected virtual void OnTimelineSelectionChanged(EventArgs e)
        {
            TimelineSelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnCheckboxSelected(EventArgs e)
        {
            TimelineCheckboxSelected?.Invoke(this, e);
        }

        protected virtual void OnTrackbarMouseMoved(EventArgs e, double mouseX)
        {
            try
            {
                var newPositionTimeSpan = TimeSpan.FromSeconds(mouseX / _timelineSettings.ScaleFactor);
                TrackbarPosition = newPositionTimeSpan;

                TrackbarMouseMoved?.Invoke(this, e);
            }
            catch
            {

            }
        }

        protected virtual void OnTimelineVideoEventDeleted(TimelineEvent timelineVideoEvent)
        {
            TimelineVideoEventDeleted?.Invoke(this, new TimelineEventArgs(timelineVideoEvent));
        }

        private void BuildTimelineHeader(double newWidth, double scaleFactor)
        {
            double tenSecondsWidth = 10 * scaleFactor;
            double oneMinuteWidth = 60 * scaleFactor;

            HeaderScaleBars.Children.Clear();
            HeaderScaleLabel.Children.Clear();

            for (double i = 0; i < (newWidth); i++)
            {
                //for every minute
                if ((i * scaleFactor) % oneMinuteWidth == 0)
                {
                    var timespan = TimeSpan.FromSeconds(i);
                    HeaderScaleBars.Children.Add(new Border { Margin = new Thickness(0, 0, 0, 0), Padding = new Thickness(0, 0, 0, 0), Width = tenSecondsWidth, Height = TimelineDefaultConfig.Column2_HeaderOneMinuteScaleHeight, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new Thickness(1, 0, 0, 0), BorderBrush = Brushes.LightGray });
                    HeaderScaleLabel.Children.Add(new TextBlock { Text = timespan.ToString(), TextAlignment = TextAlignment.Left, FontSize = 10, Width = oneMinuteWidth });
                }

                //for every 10 seconds
                else if ((i * scaleFactor) % tenSecondsWidth == 0)
                    HeaderScaleBars.Children.Add(new Border { Margin = new Thickness(0, 0, 0, 0), Padding = new Thickness(0, 0, 0, 0), Width = tenSecondsWidth, Height = TimelineDefaultConfig.Column2_HeaderTenSecondsScaleHeight, VerticalAlignment = VerticalAlignment.Top, BorderThickness = new Thickness(1, 0, 0, 0), BorderBrush = Brushes.LightGray });
            }
        }

        private void SetTrackbarMouseEvents()
        {
            TrackbarLine2.MouseLeftButtonDown += (sender, e) =>
            {
                _isTrackbarLineDragInProg = true;
                TrackbarLine2.CaptureMouse();
            };

            TrackbarLine2.MouseLeftButtonUp += (sender, e) =>
            {
                _isTrackbarLineDragInProg = false;
                TrackbarLine2.ReleaseMouseCapture();
            };

            TrackbarLine2.MouseMove += (sender, e) =>
            {
                if (!_isTrackbarLineDragInProg) return;

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(Column2BaseLayer);

                // center the trackbarLine on the mouse
                double mouseX = mousePos.X;

                if (mouseX >= 0)
                {
                    Canvas.SetLeft(TrackbarLine2, mouseX);
                    OnTrackbarMouseMoved(EventArgs.Empty, mouseX);
                }
            };

            Canvas.SetTop(TrackbarLine2, 0);
            Canvas.SetZIndex(TrackbarLine2, 3);

            //if user left-clicks on content panel - trackbar should move to the location of mouse
            Column2Header.MouseLeftButtonDown += (sender, e) =>
            {
                if (_isTrackbarLineDragInProg) return;

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(Column2BaseLayer);

                // center the trackbarLine on the mouse
                int mouseX = (int)mousePos.X;

                if (mouseX >= 0 && mouseX <= (_timelineLayoutModel.MinWidth * _timelineSettings.ScaleFactor))
                {
                    Canvas.SetLeft(TrackbarLine2, mouseX);
                    OnTrackbarMouseMoved(EventArgs.Empty, mouseX);
                }
            };
        }

        /// <summary>
        /// This will rebuild the timeline with the added/modified events
        /// </summary>
        private void RebuildTimeline()
        {
            Column1TrackItems.Children.Clear();
            Column2TrackItems.Children.Clear();

            var timelines = new List<Models.Timeline>
            {
                //adjust the order of timelines as preferred
                _timelineViewModel.NotesTimeline,
                _timelineViewModel.Callout2Timeline,
                _timelineViewModel.Callout1Timeline,
                _timelineViewModel.VideoTimeline,
                _timelineViewModel.AudioTimeline,
            };

            //column 1 track items
            foreach (var timeline in timelines)
            {
                var column1TrackItem = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = TimelineDefaultConfig.Column1ItemWidth,
                    Height = TimelineDefaultConfig.ItemHeight,
                    Orientation = Orientation.Horizontal
                };

                column1TrackItem.Children.Add(new TextBlock { Text = timeline.TrackNumber.ToString(), Margin = new Thickness(10, 3, 0, 0) });

                var column1TrackItemBorder = new Border
                {
                    DataContext = timeline,
                    Style = (Style)FindResource("SelectedTimelineStyle"),
                    Child = column1TrackItem
                };

                Column1TrackItems.Children.Add(column1TrackItemBorder);
            }

            //column 2 track items
            foreach (var timeline in timelines)
            {
                var trackControl = new TrackControl
                {
                    DataContext = timeline,
                    Height = TimelineDefaultConfig.ItemHeight + 1,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                // Sets the SelectionModel when the user left-click or right-clicks on the event
                trackControl.TrackEventSelected += (sender, e) =>
                {
                    var senderTrackControl = sender as TrackControl;
                    _timelineSelectionModel.SelectedEvent = senderTrackControl.SelectedEvent;

                    foreach (UIElement child in Column2TrackItems.Children)
                    {
                        if (child is TrackControl trackControlChild)
                        {
                            trackControlChild.ResetSelectedEvent(senderTrackControl.SelectedEvent);
                        }
                    }
                };

                // Adjust the width on all timelines when the minimumWidth changes
                trackControl.TimelineMaxWidthChanged += (sender, e) =>
                {
                    var senderTrackControl = sender as TrackControl;
                    _timelineLayoutModel.MinWidth = senderTrackControl.Width;

                    Debug.WriteLine($"NewMinWidth changed! NewMinWidth: {senderTrackControl.Width}");
                };

                Column2TrackItems.Children.Add(trackControl);
            }

            _timelineLayoutModel.TrackbarHeight = (TimelineDefaultConfig.Column2HeaderHeight + (TimelineDefaultConfig.ItemHeight * timelines.Count));
            TrackbarLine2.SetHeight(_timelineLayoutModel.TrackbarHeight);
            VerticalLineGuide.Y2 = _timelineLayoutModel.TrackbarHeight;

            //set the column2 width to the minimum width for the scrollviewer to work
            double newWidth = _timelineLayoutModel.MinWidth * _timelineSettings.ScaleFactor;
            Column2BaseLayer.Width = newWidth;
            _timelineViewModel.SetTimelinesMinWidth(newWidth);

            BuildTimelineHeader(newWidth, _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor);
        }

        public Window ParentWindow
        {
            get
            {
                return Window.GetWindow(this);
            }
        }

        /// <summary>
        /// Clears everything from the timeline view model and rebuilds the timeline from the datatable
        /// </summary>
        private void ReloadTimelineData()
        {
            InitializeComponent();

            _timelineViewModel.ClearTimelines();

            Column1TrackItems.Children.Clear();
            Column2TrackItems.Children.Clear();

            // Build track models from TimelineDataTable
            if (_timelineDatabaseModel.TimelineVideoEvents.Any())
            {
                foreach (var row in _timelineDatabaseModel.TimelineVideoEvents)
                {
                    TimelineMedia media = _timelineDatabaseModel.GetMedia(row.fk_videoevent_media);
                    TimeSpan startTime = TimeSpan.Parse(row.videoevent_start);

                    var timelineEvent = new TimelineEvent
                    {
                        EventMediaName = media.media_name,
                        EventMediaColor = media.media_color,
                        EventScreenName = null,
                        EventScreenColor = null,
                        EventScreen = null,
                        StartTime = startTime,
                        TrackNumber = (TrackNumber)Enum.Parse(typeof(TrackNumber), row.videoevent_track.ToString()),
                        EventDuration = row.videoevent_duration,

                        VideoEvent = new TimelineVideoEvent
                        {
                            videoevent_id = row.videoevent_id,
                            fk_videoevent_media = row.fk_videoevent_media,
                            fk_videoevent_projdet = row.fk_videoevent_projdet,
                            videoevent_createdate = row.videoevent_createdate,
                            videoevent_duration = row.videoevent_duration,
                            videoevent_origduration = row.videoevent_origduration,
                            videoevent_start = row.videoevent_start,
                            videoevent_end = row.videoevent_end,
                            videoevent_isdeleted = row.videoevent_isdeleted,
                            videoevent_issynced = row.videoevent_issynced,
                            videoevent_modifydate = row.videoevent_modifydate,
                            videoevent_serverid = row.videoevent_serverid,
                            videoevent_syncerror = row.videoevent_syncerror,
                            videoevent_track = row.videoevent_track,
                            fk_design_background = row.fk_design_background,
                            fk_design_screen = row.fk_design_screen,
                        },
                    };

                    // For form events, we will use the screen_color from the cbv_screen table
                    if (row.fk_videoevent_media == (int)MediaType.form)
                    {
                        TimelineScreen screen = _timelineDatabaseModel.GetScreen(row.fk_design_screen);
                        if (screen != null)
                        {
                            timelineEvent.EventScreenName = screen.screen_name;
                            timelineEvent.EventScreenColor = screen.screen_hexcolor;
                            timelineEvent.EventScreen = screen.screen_id;
                        }

                        _timelineViewModel.AddToTimeline(ref _timelineSettings, ref _timelineLayoutModel, timelineEvent);
                    }
                    else
                    {
                        _timelineViewModel.AddToTimeline(ref _timelineSettings, ref _timelineLayoutModel, timelineEvent);
                    }
                }
            }

            if (_timelineDatabaseModel.TimelineNotes.Any())
            {
                foreach (var note in _timelineDatabaseModel.TimelineNotes)
                {
                    TimeSpan startTime = TimeSpan.Parse(note.notes_start);

                    var timelineEvent = new TimelineEvent
                    {
                        EventMediaName = "notes",
                        EventMediaColor = "#FF0C90A2",
                        EventScreenName = null,
                        EventScreenColor = null,
                        EventScreen = null,
                        StartTime = startTime,
                        EventDuration = note.notes_duration,
                        Note = note,
                        TrackNumber = TrackNumber.Notes
                    };

                    _timelineViewModel.AddToTimeline(ref _timelineSettings, ref _timelineLayoutModel, timelineEvent);
                }
            }

            RebuildTimeline();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Get the current value of the Slider
            double sliderValue = e.NewValue;

            // Call ZoomIn or ZoomOut based on the Slider value
            if (sliderValue > _timelineSettings.ScaleFactor)
            {
                while (_timelineSettings.ScaleFactor < sliderValue)
                {
                    ZoomIn();
                }
            }
            else if (sliderValue < _timelineSettings.ScaleFactor)
            {
                while (_timelineSettings.ScaleFactor > sliderValue)
                {
                    ZoomOut();
                }
            }
        }


        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomIn();
            zoomSlider.Value = _timelineSettings.ScaleFactor;

            // Use Dispatcher.Invoke to ensure that the UI is updated before proceeding
            Dispatcher.Invoke(() =>
            {
                // Update the TrackbarLine2 position after adjusting the timeline scale factor
                MoveTrackbarToTimeSpan(TrackbarPosition, true);

                // Update the ScrollableWidth after adjusting the timeline scale factor
                ScrollToTrackbar();

            }, DispatcherPriority.Render);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomOut();
            zoomSlider.Value = _timelineSettings.ScaleFactor;

            // Use Dispatcher.Invoke to ensure that the UI is updated before proceeding
            Dispatcher.Invoke(() =>
            {
                // Update the TrackbarLine2 position after adjusting the timeline scale factor
                MoveTrackbarToTimeSpan(TrackbarPosition, true);

                // Scroll to the position of the trackbar
                ScrollToTrackbar();

            }, DispatcherPriority.Render);
        }

        private void ScrollToTrackbar()
        {
            var scrollViewer = EventScroll;
            if (scrollViewer != null)
            {
                double trackbarLineLeft = TrackbarLine2.TranslatePoint(new Point(0, 0), Column2BaseLayer).X;
                double newHorizontalOffset = trackbarLineLeft - (scrollViewer.ViewportWidth) / 2;

                // Ensure the TrackbarLine2 is within the visible area
                if (newHorizontalOffset < 0)
                {
                    newHorizontalOffset = 0;
                }
                else if (newHorizontalOffset > scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                {
                    newHorizontalOffset = scrollViewer.ExtentWidth - scrollViewer.ViewportWidth;
                }

                // Set the horizontal offset of the ScrollViewer
                scrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
            }
        }

        private void ScrollToVerticalLineGuide()
        {
            var scrollViewer = EventScroll;
            if (scrollViewer != null)
            {
                // Store the current horizontal offset
                double previousHorizontalOffset = scrollViewer.HorizontalOffset;

                double left = VerticalLineGuide.TranslatePoint(new Point(0, 0), Column2BaseLayer).X;
                double newHorizontalOffset = left - (scrollViewer.ViewportWidth / 2);

                // Ensure the new horizontal offset is within the valid range
                if (newHorizontalOffset < 0)
                {
                    newHorizontalOffset = 0;
                }
                else if (newHorizontalOffset > scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                {
                    newHorizontalOffset = scrollViewer.ExtentWidth - scrollViewer.ViewportWidth;
                }

                // Adjust the horizontal offset to maintain the same position of content
                double deltaOffset = newHorizontalOffset - previousHorizontalOffset;
                double finalHorizontalOffset = previousHorizontalOffset + deltaOffset;

                // Set the adjusted horizontal offset of the ScrollViewer
                scrollViewer.ScrollToHorizontalOffset(finalHorizontalOffset);
            }
        }

        private TimeSpan previousMouseTimeSpan = new TimeSpan();

        private void Column2BaseLayer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            Point mouseLocation = Mouse.GetPosition(Column2BaseLayer);
            previousMouseTimeSpan = MouseTimeSpan;

            // Check if the mouse wheel was scrolled up
            if (e.Delta > 0)
            {
                if (ZoomIn())
                {
                    Dispatcher.Invoke(() =>
                    {
                        Canvas.SetLeft(VerticalLineGuide, previousMouseTimeSpan.TotalSeconds * _timelineSettings.ScaleFactor);
                        EventScroll.ScrollToHorizontalOffset(mouseLocation.X);
                        MoveTrackbarToTimeSpan(TrackbarPosition, false);
                    }, DispatcherPriority.Render);
                }
            }
            // Check if the mouse wheel was scrolled down
            else if (e.Delta < 0)
            {
                if (ZoomOut())
                {
                    Dispatcher.Invoke(() =>
                    {
                        Canvas.SetLeft(VerticalLineGuide, previousMouseTimeSpan.TotalSeconds * _timelineSettings.ScaleFactor);
                        EventScroll.ScrollToHorizontalOffset(previousMouseTimeSpan.TotalSeconds * (_timelineSettings.ScaleFactor - 1));
                        MoveTrackbarToTimeSpan(TrackbarPosition, false);
                    }, DispatcherPriority.Render);
                }
            }

            zoomSlider.Value = _timelineSettings.ScaleFactor;
        }

        private void EventScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //double horizontalOffset = e.HorizontalOffset;
            //double verticalOffset = e.VerticalOffset;
            //double extentWidth = e.ExtentWidth;
            //double extentHeight = e.ExtentHeight;
            //double viewportWidth = e.ViewportWidth;
            //double viewportHeight = e.ViewportHeight;

            //Debug.WriteLine($"HorizontalOffset: {horizontalOffset}");
        }

        TimeSpan MouseTimeSpan = new TimeSpan(0, 0, 0, 0);
        private void Column2BaseLayer_Events()
        {
            Column2BaseLayer.MouseMove += (sender, e) =>
            {
                // Update the tooltip position based on the mouse pointer's position
                Point mousePosition = e.GetPosition(Column2BaseLayer);

                MouseTimeSpan = TimeSpan.FromSeconds(mousePosition.X / _timelineSettings.ScaleFactor);
            };

            Column2BaseLayer.MouseLeave += (sender, e) =>
            {
                //VerticalLineGuide.Visibility = Visibility.Hidden;
            };
        }
    }
}
