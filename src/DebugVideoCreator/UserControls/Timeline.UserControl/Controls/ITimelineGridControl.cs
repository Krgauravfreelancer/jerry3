using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using Timeline.UserControls.Config;
using Timeline.UserControls.Models;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Contains definition of exported TimelineGridControl methods.
    /// </summary>
    public interface ITimelineGridControl
    {

        /// <summary>
        /// Builds the column structure of the timeline datatable.
        /// </summary>
        DataTable BuildTimelineDataTable();

        /// <summary>
        /// Clears both timeline view data and datatable.
        /// </summary>
        void ClearTimeline();

        /// <summary>
        /// Delete the selected event from the view only.
        /// </summary>
        void DeleteSelectedEvent();


        /// <summary>
        /// Returns the selected event when the user-right clicks on the timeline.
        /// </summary>
        TimelineEvent GetSelectedEvent();

        /// <summary>
        /// Returns a list of videoEvents where the trackbar is positioned within the videoEvent duration.
        /// </summary>
        List<TimelineEvent> GetTrackbarVideoEvents();

        /// <summary>
        /// Returns a list of newly added timeline events.
        /// </summary>
        List<TimelineEvent> GetAddedTimelineEvents();

        /// <summary>
        /// Returns a list of deleted event IDs.
        /// </summary>
        //List<int> GetDeletedTimelineEventsId();

        /// <summary>
        /// Returns a list of modified timeline events.
        /// </summary>
        List<TimelineEvent> GetModifiedTimelineEvents();

        /// <summary>
        /// Returns TRUE if the timeline has loaded data.
        /// </summary>
        bool HasData();

        /// <summary>
        /// Move the trackbar to the specified time
        /// </summary>
        void MoveTrackbarToTimeSpan(TimeSpan timeSpan, bool scrollToTrackbar);

        /// <summary>
        /// Sets collection of apps from database.
        /// </summary>
        void SetAppControl(List<TimelineApp> apps);

        /// <summary>
        /// Sets collection of designs from database.
        /// </summary>
        void SetDesigns(List<TimelineDesign> designs);

        void SetNotes(List<TimelineNote> notes);

        void SetVideoEvents(List<TimelineVideoEvent> videoEvents);  

        /// <summary>
        /// Sets collection of media objects from database.
        /// </summary>
        void SetMediaList(List<TimelineMedia> mediaList);

        /// <summary>
        /// Sets collection of screen objects from database.
        /// </summary>
        void SetScreenList(List<TimelineScreen> screenList);

        /// <summary>
        /// Sets the datatable containing the timeline events.
        /// </summary>
        //void SetTimelineDatatable(DataTable dataTable);


        bool ZoomIn();

        bool ZoomOut(); 
    }
}
