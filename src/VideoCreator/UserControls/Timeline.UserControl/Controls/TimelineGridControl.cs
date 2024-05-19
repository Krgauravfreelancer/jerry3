using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Timeline.UserControls.Config;
using Timeline.UserControls.Exceptions;
using Timeline.UserControls.Models;
using Timeline.UserControls.Models.Datatables;

namespace Timeline.UserControls.Controls
{
    public partial class TimelineGridControl : UserControl, ITimelineGridControl
    {
        /// <summary>
        /// This will prevent the app to make changes on a particular timeline based on the app value.
        /// </summary>
        private void CheckAppAccess(MediaType mediaType, TimelineAppAccess appAccess)
        {
            if (mediaType == MediaType.image || mediaType == MediaType.video || mediaType == MediaType.form)
            {
                if (appAccess.TalkAccess)
                {
                    throw new UnauthorizedAppActionException(mediaType.ToString());
                }
            }
            else if (mediaType == MediaType.audio)
            {
                if (appAccess.DraftAccess || appAccess.WriteAccess)
                {
                    throw new UnauthorizedAppActionException(mediaType.ToString());
                }
            }
        }

        public void DeleteSelectedEvent()
        {
            try
            {
                if (_timelineSelectionModel.SelectedEvent != null)
                {

                    if (_timelineSelectionModel.SelectedEvent.TrackNumber != TrackNumber.Notes)
                        CheckAppAccess(_timelineSelectionModel.SelectedEvent.GetTimelineMediaType(), _timelineDatabaseModel.AppAccess);

                    OnTimelineVideoEventDeleted(_timelineSelectionModel.SelectedEvent);

                    _timelineViewModel.RemoveFromTimeline(_timelineSelectionModel.SelectedEvent);
                    _timelineSelectionModel.SelectedEvent = null;

                    RebuildTimeline();
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAppActionException)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else throw;
            }
        }

        public DataTable BuildTimelineDataTable()
        {
            return new VideoEventDatatable();
        }

        public void ClearTimeline()
        {
            _timelineViewModel.ClearTimelines();
            _timelineDatabaseModel.Clear();
            ReloadTimelineData();
        }

        public List<TimelineEvent> GetAddedTimelineEvents()
        {
            var addedEvents = new List<TimelineEvent>();

            addedEvents.AddRange(_timelineViewModel.AudioTimeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id < 0).ToList());
            addedEvents.AddRange(_timelineViewModel.VideoTimeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id < 0).ToList());
            addedEvents.AddRange(_timelineViewModel.Callout1Timeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id < 0).ToList());
            addedEvents.AddRange(_timelineViewModel.Callout2Timeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id < 0).ToList());
            addedEvents.AddRange(_timelineViewModel.NotesTimeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id < 0).ToList());

            return addedEvents;
        }

        public List<TimelineEvent> GetModifiedTimelineEvents()
        {
            var modifiedEvents = new List<TimelineEvent>();

            modifiedEvents.AddRange(_timelineViewModel.AudioTimeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id > 0 && p.Modified).ToList());
            modifiedEvents.AddRange(_timelineViewModel.VideoTimeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id > 0 && p.Modified).ToList());
            modifiedEvents.AddRange(_timelineViewModel.Callout1Timeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id > 0 && p.Modified).ToList());
            modifiedEvents.AddRange(_timelineViewModel.Callout2Timeline.TimelineEvents.Where(p => p.VideoEvent.videoevent_id > 0 && p.Modified).ToList());
            modifiedEvents.AddRange(_timelineViewModel.NotesTimeline.TimelineEvents.Where(p => p.Note.notes_id > 0 && p.Modified).ToList());

            return modifiedEvents;
        }

        public TimelineEvent GetSelectedEvent()
        {
            return _timelineSelectionModel.SelectedEvent;
        }

        public List<TimelineEvent> GetTrackbarVideoEvents()
        {
            var trackbarDate = TrackbarPosition;
            var events = new List<TimelineEvent>();

            foreach (var e in _timelineViewModel.AudioTimeline.TimelineEvents)
            {
                if (EventInTrackbarPosition(trackbarDate, e))
                    events.Add(e);
            }

            foreach (var e in _timelineViewModel.VideoTimeline.TimelineEvents)
            {
                if (EventInTrackbarPosition(trackbarDate, e))
                    events.Add(e);
            }

            foreach (var e in _timelineViewModel.Callout1Timeline.TimelineEvents)
            {
                if (EventInTrackbarPosition(trackbarDate, e))
                    events.Add(e);
            }

            foreach (var e in _timelineViewModel.Callout2Timeline.TimelineEvents)
            {
                if (EventInTrackbarPosition(trackbarDate, e))
                    events.Add(e);
            }

            foreach (var e in _timelineViewModel.NotesTimeline.TimelineEvents)
            {
                if (EventInTrackbarPosition(trackbarDate, e))
                    events.Add(e);
            }

            return events;
        }

        public bool HasData() => _timelineViewModel.HasData;

        public void MoveTrackbarToTimeSpan(TimeSpan timeSpan, bool scrollToTrackbar)
        {
            double totalSeconds = timeSpan.TotalSeconds * _timelineSettings.ScaleFactor;

            if (totalSeconds >= 0)
                Canvas.SetLeft(TrackbarLine2, totalSeconds);

            // Use Dispatcher.Invoke to ensure that the UI is updated before proceeding
            Dispatcher.Invoke(() =>
            {
                if (scrollToTrackbar)
                    ScrollToTrackbar();
            }, DispatcherPriority.Render);
        }

        public void SetAppControl(List<TimelineApp> apps)
        {
            _timelineDatabaseModel.TimelineApps = apps;

            foreach (var app in _timelineDatabaseModel.TimelineApps)
            {
                switch (app.app_id)
                {
                    case (int)AppControlType.draft:
                        _timelineDatabaseModel.AppAccess.DraftAccess = app.app_active;
                        break;
                    case (int)AppControlType.write:
                        _timelineDatabaseModel.AppAccess.WriteAccess = app.app_active;
                        break;
                    case (int)AppControlType.talk:
                        _timelineDatabaseModel.AppAccess.TalkAccess = app.app_active;
                        break;
                    case (int)AppControlType.admin:
                    case (int)AppControlType.superadmin:
                        _timelineDatabaseModel.AppAccess.AdminAccess = app.app_active;
                        break;
                    default:
                        throw new UnknownAppAccessException(app.app_id);
                }
            }
        }

        public void SetDesigns(List<TimelineDesign> designs)
        {
            _timelineDatabaseModel.TimelineDesigns = designs;
        }

        public void SetMediaList(List<TimelineMedia> mediaList)
        {
            _timelineDatabaseModel.TimelineMediaList = mediaList;
        }

        public void SetNotes(List<TimelineNote> notes)
        {
            _timelineDatabaseModel.TimelineNotes = notes;
        }

        public void SetVideoEvents(List<TimelineVideoEvent> videoEvents)
        {
            _timelineDatabaseModel.TimelineVideoEvents = videoEvents;
        }

        public void SetScreenList(List<TimelineScreen> screenList)
        {
            _timelineDatabaseModel.ScreenList = screenList;
        }

        public bool ZoomIn()
        {
            if (_timelineSettings.ScaleFactor <= TimelineDefaultConfig.ZoomLimit)
            {
                _timelineSettings.ScaleFactor++;
                _timelineViewModel.VideoTimeline.ScaleFactor = _timelineSettings.ScaleFactor;
                _timelineViewModel.AudioTimeline.ScaleFactor = _timelineSettings.ScaleFactor;
                _timelineViewModel.Callout1Timeline.ScaleFactor = _timelineSettings.ScaleFactor;
                _timelineViewModel.Callout2Timeline.ScaleFactor = _timelineSettings.ScaleFactor;
                _timelineViewModel.NotesTimeline.ScaleFactor = _timelineSettings.ScaleFactor;

                return true;
            }

            return false;
        }

        public bool ZoomOut()
        {
            if (_timelineSettings.ScaleFactor >= 2)
            {
                _timelineSettings.ScaleFactor--;
                _timelineViewModel.VideoTimeline.ScaleFactor = _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor;
                _timelineViewModel.AudioTimeline.ScaleFactor = _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor;
                _timelineViewModel.Callout1Timeline.ScaleFactor = _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor;
                _timelineViewModel.Callout2Timeline.ScaleFactor = _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor;
                _timelineViewModel.NotesTimeline.ScaleFactor = _timelineSettings.ScaleFactor == 0 ? 1 : _timelineSettings.ScaleFactor;

                return true;
            }

            return false;
        }

        private bool EventInTrackbarPosition(TimeSpan trackbarDate, TimelineEvent timelineEvent)
        {
            var startTime = timelineEvent.StartTime;
            var endTime = timelineEvent.EndTime;
            var trackbarTime = trackbarDate;

            if (endTime.HasValue)
            {
                // If endTime is not null, check if trackbarTime is within the range [startTime, endTime]
                return trackbarTime >= startTime && trackbarTime <= endTime;
            }
            else
            {
                // If endTime is null, only compare trackbarTime with startTime
                return trackbarTime >= startTime;
            }
        }
    }
}
