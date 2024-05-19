using System;
using System.Linq;
using Timeline.UserControls.Base;

namespace Timeline.UserControls.Models
{
    internal class TimelineViewModel : NotifyPropertyChanged
    {
        private int _newId = -1;
        internal int newId
        {
            get
            {
                return _newId--;
            }
        }

        public bool HasData
        {
            get => (VideoTimeline.TimelineEvents.Any()
                    || AudioTimeline.TimelineEvents.Any()
                    || Callout1Timeline.TimelineEvents.Any()
                    || Callout2Timeline.TimelineEvents.Any()
                    || NotesTimeline.TimelineEvents.Any());
        }

        private Timeline _videoTimeline;
        public Timeline VideoTimeline
        {
            get => _videoTimeline;
            set => Set(ref _videoTimeline, value);
        }

        private Timeline _audioTimeline;
        public Timeline AudioTimeline
        {
            get => _audioTimeline;
            set => Set(ref _audioTimeline, value);
        }

        private Timeline _notesTimeline;
        public Timeline NotesTimeline
        {
            get => _notesTimeline;
            set => Set(ref _notesTimeline, value);
        }

        private Timeline _callout1Timeline;
        public Timeline Callout1Timeline
        {
            get => _callout1Timeline;
            set => Set(ref _callout1Timeline, value);
        }

        private Timeline _callout2Timeline;
        public Timeline Callout2Timeline
        {
            get => _callout2Timeline;
            set => Set(ref _callout2Timeline, value);
        }

        public TimelineViewModel()
        {
            NotesTimeline = new Timeline(TrackNumber.Notes);

            VideoTimeline = new Timeline(TrackNumber.Video);

            AudioTimeline = new Timeline(TrackNumber.Audio);

            Callout1Timeline = new Timeline(TrackNumber.Callout1);

            Callout2Timeline = new Timeline(TrackNumber.Callout2);
        }


        internal void AddToTimeline(ref TimelineSettings timelineSettings, ref TimelineLayoutModel timelineLayoutModel, TimelineEvent timelineEvent)
        {
            timelineEvent.ScaleFactor = timelineSettings.ScaleFactor;

            //add rows to Audio Timeline
            if (timelineEvent.TrackNumber == TrackNumber.Audio)
            {
                AudioTimeline.TimelineEvents.Add(timelineEvent);
            }

            //add rows to Video Timeline
            if (timelineEvent.TrackNumber == TrackNumber.Video)
            {
                VideoTimeline.TimelineEvents.Add(timelineEvent);
            }

            //add rows to Callout1 Timeline
            if (timelineEvent.TrackNumber == TrackNumber.Callout1)
            {
                Callout1Timeline.TimelineEvents.Add(timelineEvent);
            }

            //add rows to Callout2 Timeline
            if (timelineEvent.TrackNumber == TrackNumber.Callout2)
            {

                if (timelineEvent.VideoEvent.videoevent_id < 0)
                {
                    var lastStartTime = Callout2Timeline.GetNextVideoEventStart();
                    timelineEvent.StartTime = lastStartTime;
                }

                Callout2Timeline.TimelineEvents.Add(timelineEvent);
            }

            //add rows to Notes Timeline
            if (timelineEvent.TrackNumber == TrackNumber.Notes)
            {
                NotesTimeline.TimelineEvents.Add(timelineEvent);
            }

            //sets the min-width everytime a new track is added so the events are displayed properly in TrackControl
            double trackEventLocation = timelineEvent.StartSecond + timelineEvent.EventDuration_Double;

            timelineLayoutModel.MinWidth = timelineLayoutModel.MinWidth < trackEventLocation ? trackEventLocation : timelineLayoutModel.MinWidth;

            SetTimelinesMinWidth(timelineLayoutModel.MinWidth);
        }

        /// <summary>
        /// Sets the miminum width for all timelines
        /// </summary>
        internal void SetTimelinesMinWidth(double minWidth)
        {
           
            VideoTimeline.TimelineMinWidth = minWidth;
            AudioTimeline.TimelineMinWidth = minWidth;
            Callout1Timeline.TimelineMinWidth = minWidth;
            Callout2Timeline.TimelineMinWidth = minWidth;
            NotesTimeline.TimelineMinWidth = minWidth;
        }

        public void ClearTimelines()
        {
            //clear the events of each timeline
            VideoTimeline.TimelineEvents.Clear();
            AudioTimeline.TimelineEvents.Clear();
            Callout1Timeline.TimelineEvents.Clear();
            Callout2Timeline.TimelineEvents.Clear();
            NotesTimeline.TimelineEvents.Clear();
        }


        public void RemoveFromTimeline(TimelineEvent timelineEvent)
        {
            if(timelineEvent.TrackNumber == TrackNumber.Video)
            {
                var eventToRemove = VideoTimeline.TimelineEvents.FirstOrDefault(e => e.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);
                if (eventToRemove != null)
                {
                    VideoTimeline.TimelineEvents.Remove(eventToRemove);
                }
            }
            else if (timelineEvent.TrackNumber == TrackNumber.Audio)
            {
                var eventToRemove = AudioTimeline.TimelineEvents.FirstOrDefault(e => e.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);
                if (eventToRemove != null)
                {
                    AudioTimeline.TimelineEvents.Remove(eventToRemove);
                }
            }
            else if (timelineEvent.TrackNumber == TrackNumber.Callout1)
            {
                var eventToRemove = Callout1Timeline.TimelineEvents.FirstOrDefault(e => e.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);
                if (eventToRemove != null)
                {
                    Callout1Timeline.TimelineEvents.Remove(eventToRemove);
                }
            }
            else if (timelineEvent.TrackNumber == TrackNumber.Callout2)
            {
                var eventToRemove = Callout2Timeline.TimelineEvents.FirstOrDefault(e => e.VideoEvent.videoevent_id == timelineEvent.VideoEvent.videoevent_id);
                if (eventToRemove != null)
                {
                    Callout2Timeline.TimelineEvents.Remove(eventToRemove);
                }
            }
            else if (timelineEvent.TrackNumber == TrackNumber.Notes)
            {
                var eventToRemove = NotesTimeline.TimelineEvents.FirstOrDefault(e => e.Note.notes_id == timelineEvent.Note.notes_id);
                if (eventToRemove != null)
                {
                    NotesTimeline.TimelineEvents.Remove(eventToRemove);
                }
            }
        }
    }
}
