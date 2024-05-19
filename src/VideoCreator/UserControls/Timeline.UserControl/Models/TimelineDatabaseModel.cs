using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Timeline.UserControls.Base;
using Timeline.UserControls.Models.Datatables;

namespace Timeline.UserControls.Models
{
    /// <summary>
    /// This model needs to store data directly from the database, ensuring that any modifications made are promptly reflected. 
    /// Updates or changes made elsewhere do not automatically synchronize with this model.
    /// </summary>
    internal class TimelineDatabaseModel : NotifyPropertyChanged
    {
        private List<TimelineVideoEvent> _timelineVideoEvents;
        public List<TimelineVideoEvent> TimelineVideoEvents
        {
            get => _timelineVideoEvents;
            set => Set(ref _timelineVideoEvents, value);
        }

        private List<TimelineNote> _timelineNotes;
        public List<TimelineNote> TimelineNotes
        {
            get => _timelineNotes;
            set => Set(ref _timelineNotes, value);
        } 

        public List<TimelineApp> TimelineApps { get; set; } = new List<TimelineApp>();
        public List<TimelineDesign> TimelineDesigns { get; set; } = new List<TimelineDesign>();
        public List<TimelineMedia> TimelineMediaList { get; set; } = new List<TimelineMedia>();
        public List<TimelineScreen> ScreenList { get; set; } = new List<TimelineScreen>();

        internal TimelineAppAccess AppAccess { get; set; } = new TimelineAppAccess();

        public TimelineDatabaseModel() {
            TimelineNotes = new List<TimelineNote>();
            TimelineVideoEvents = new List<TimelineVideoEvent>();
        }

        public List<TimelineDesign> GetDesignsForVideoEvent(int videoevent_id)
        {
            return TimelineDesigns.Where(p => p.fk_design_videoevent == videoevent_id).ToList();
        }

        public TimelineMedia GetMedia(int mediaId)
        {
            return TimelineMediaList.Where(p => p.media_id == mediaId).FirstOrDefault();
        }

        public TimelineScreen GetScreen(int screenId)
        {
            return ScreenList.Where(p => p.screen_id == screenId).FirstOrDefault();
        }

        public void Clear()
        {
            TimelineNotes.Clear();
            TimelineVideoEvents.Clear();
        }

        public List<int> GetVideoEventsIds()
        {
            return TimelineVideoEvents.Select(p => p.videoevent_id).ToList();
        }

        public List<int> GetNotesIds()
        {
            return TimelineNotes.Select(p => p.notes_id).ToList();
        }
    }
}
