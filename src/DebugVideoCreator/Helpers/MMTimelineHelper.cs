
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using MMClass = ManageMedia_UserControl.Classes;
using MMControl = ManageMedia_UserControl.Controls;
using SRModel = ScreenRecorder_UserControl.Models;

namespace VideoCreator.Helpers
{
    public static class MMTimelineHelper
    {
        private static List<MMClass.Media> _PlayList = new List<MMClass.Media>();
        private static List<MMClass.Media> _OriginalPlayList = new List<MMClass.Media>();
        private static (TimeSpan ViewportStart, TimeSpan ViewportDuration, TimeSpan MainCursorTime) Viewport;
        private static void LoadEvents(List<MMClass.Media> MediaList, MMControl.TimeLine TimelineGridCtrl2)
        {
            List<MMClass.Media> SortedList = MediaList.OrderBy(o => o.StartTime).ToList();
            MediaList.Clear();
            _PlayList = SortedList;
            _OriginalPlayList.Clear();

            //ItemsAreSaved = true;
            //UpdateSaveBtnStatus();

            //Duplicate PlayList with out media to compare when saving
            for (int i = 0; i < SortedList.Count; i++)
            {
                MMClass.Media media = SortedList[i];

                List<SRModel.TextItem> DuplicatedTextList = new List<SRModel.TextItem>();

                for (int j = 0; j < media.RecordedTextList.Count; j++)
                {
                    SRModel.TextItem OriginalItem = media.RecordedTextList[j];
                    SRModel.TextItem NewItem = new SRModel.TextItem()
                    {
                        NoteID = OriginalItem.NoteID,
                        Start = OriginalItem.Start,
                        Duration = OriginalItem.Duration,
                        WordCount = OriginalItem.WordCount,
                        Text = OriginalItem.Text,
                        AddedToMedia = OriginalItem.AddedToMedia
                    };

                    DuplicatedTextList.Add(NewItem);
                }

                _OriginalPlayList.Add(new MMClass.Media()
                {
                    ProjectId = media.ProjectId,
                    TrackId = media.TrackId,
                    VideoEventID = media.VideoEventID,
                    StartTime = media.StartTime,
                    Duration = media.Duration,
                    OriginalDuration = media.OriginalDuration,
                    mediaType = media.mediaType,
                    RecordedTextList = DuplicatedTextList,
                    Color = media.Color
                });
            }

            TimelineGridCtrl2.SetManageMedia(false);
            TimelineGridCtrl2.LoadMedia(SortedList);
            if (Viewport.ViewportStart != TimeSpan.Zero || Viewport.ViewportDuration != TimeSpan.Zero)
            {
                TimelineGridCtrl2.SetViewport(Viewport.ViewportStart, Viewport.ViewportDuration, Viewport.MainCursorTime);
            }
            TimelineGridCtrl2.ShowTrackbar();
        }

        public static void Init(SelectedProjectEvent selectedProjectEvent, MMControl.TimeLine TimelineGridCtrl2)
        {
            #region Video Events
            var VideoEventData = DataManagerSqlLite.GetVideoEvents(selectedProjectEvent.projdetId, true, true);
            List<MMClass.Media> mediaList = new List<MMClass.Media>();
            foreach (var item in VideoEventData)
            {
                List<CBVDesign> Designs = item.design_data;
                //obj.fk_design_background, obj.fk_design_screen

                var Screens = DataManagerSqlLite.GetScreens();
                var media = new MMClass.Media();
                Color background = Colors.DarkRed;
                if (Designs.Count > 0 && Screens.Count >= 7)
                {
                    background = (Color)ColorConverter.ConvertFromString(Screens[Designs[0].fk_design_screen - 1].screen_hexcolor);
                    media.ImageType = Screens[Designs[0].fk_design_screen - 1].screen_name;
                }
                else if (Screens.Count >= 7)
                {
                    background = (Color)ColorConverter.ConvertFromString(Screens[4].screen_hexcolor);
                }

                if (item.videosegment_data != null && item.videosegment_data.Count > 0)
                {
                    media.mediaData = item.videosegment_data[0].videosegment_media;
                }

                if (item.fk_videoevent_media == 1 || item.fk_videoevent_media == 4)
                {
                    media.mediaType = MMClass.MediaType.Image;
                }
                else if (item.fk_videoevent_media == 2)
                {
                    media.mediaType = MMClass.MediaType.Video;

                    if (Screens.Count >= 7)
                    {
                        background = (Color)ColorConverter.ConvertFromString(Screens[4].screen_hexcolor);
                    }
                }
                else if (item.fk_videoevent_media == 3)
                {
                    media.mediaType = MMClass.MediaType.Audio;
                }
                else if (item.fk_videoevent_media == 4)
                {
                    media.mediaType = MMClass.MediaType.Form;
                }

                media.Color = background;

                media.VideoEventID = item.videoevent_id;
                media.ProjectId = selectedProjectEvent.projectId;
                media.TrackId = item.videoevent_track;
                if (item.notes_data != null)
                {
                    List<ScreenRecorder_UserControl.Models.TextItem> textItems = new List<ScreenRecorder_UserControl.Models.TextItem>();
                    foreach (var note in item.notes_data)
                    {
                        ScreenRecorder_UserControl.Models.TextItem textItem = new ScreenRecorder_UserControl.Models.TextItem();
                        textItem.NoteID = note.notes_id;
                        textItem.Text = note.notes_line;
                        textItem.Start = TimeSpan.Parse(note.notes_start);
                        textItem.Duration = TimeSpan.Parse(note.notes_duration);

                        textItems.Add(textItem);
                    }
                    media.RecordedTextList = textItems;
                }
                media.Duration = TimeSpan.Parse(item.videoevent_duration);
                media.OriginalDuration = TimeSpan.Parse(item.videoevent_origduration);
                media.StartTime = TimeSpan.Parse(item.videoevent_start);
                mediaList.Add(media);
            }

            //_ManageMedia.SetProjectInfo(selectedProjectEvent.projectId);

            LoadEvents(mediaList, TimelineGridCtrl2);

            #endregion
        }

        
    }
}
