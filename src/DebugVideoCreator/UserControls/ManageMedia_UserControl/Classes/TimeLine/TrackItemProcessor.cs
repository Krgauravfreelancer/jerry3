using ManageMedia_UserControl.Controls;
using NAudio.Mixer;
using ScreenRecorder_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace ManageMedia_UserControl.Classes.TimeLine
{
    internal static class TrackItemProcessor
    {
        internal static void RecalculateNoteLimits(List<NoteItemControl> noteItemControlList)
        {
            List<NoteItemControl> _AllNoteItemControls = noteItemControlList;
            _AllNoteItemControls = Sort(_AllNoteItemControls).ToList();

            for (int i = 0; i < _AllNoteItemControls.Count; i++)
            {
                NoteItemControl CurrentControl = _AllNoteItemControls[i];
                if (i > 0)
                {
                    NoteItemControl PrevControl = _AllNoteItemControls[i - 1];
                    PrevControl.NextItemLimit = (CurrentControl.VideEventMinLimit + CurrentControl.TextItem.Start) - PrevControl.VideEventMinLimit;
                    CurrentControl.PreviousItemLimit = PrevControl.VideEventMinLimit + PrevControl.TextItem.Start + PrevControl.TextItem.Duration;
                }
                else
                {
                    CurrentControl.PreviousItemLimit = TimeSpan.Zero;
                }

                if (i + 1 < _AllNoteItemControls.Count)
                {
                    NoteItemControl NextControl = _AllNoteItemControls[i + 1];
                    NextControl.PreviousItemLimit = CurrentControl.VideEventMinLimit + CurrentControl.TextItem.Start + CurrentControl.TextItem.Duration;
                    CurrentControl.NextItemLimit = (NextControl.VideEventMinLimit + NextControl.TextItem.Start) - CurrentControl.VideEventMinLimit;
                }
                else
                {
                    CurrentControl.NextItemLimit = CurrentControl.VideEventMaxLimit;
                }
            }
        }

        internal static NoteItemControl[] Sort(List<NoteItemControl> Items)
        {
            NoteItemControl[] array = Items.ToArray();
            int n = array.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int index = i;
                NoteItemControl min = array[i];
                for (int j = i + 1; j < n; j++)
                {
                    if (array[j].TextItem.Start.TotalSeconds + array[j].VideEventMinLimit.TotalSeconds < array[index].TextItem.Start.TotalSeconds + array[index].VideEventMinLimit.TotalSeconds)
                    {
                        index = j;
                        min = array[j];
                    }
                }
                NoteItemControl t = array[index];
                array[index] = array[i];
                array[i] = t;
            }

            return array;
        }

        static internal List<(TextItem Item, int VideoEventID)> DeletedSelectedNotes(List<NoteItemControl> noteItemControlList)
        {

            List<NoteItemControl> ItemsToRemove = new List<NoteItemControl>();
            for (int i = 0; i < noteItemControlList.Count; i++)
            {
                NoteItemControl note = noteItemControlList[i];
                if (note.IsSelected == true)
                {
                    ItemsToRemove.Add(note);
                }
            }

            List<(TextItem Item, int VideoEventID)> DeletedNotes = new List<(TextItem Item, int VideoEventID)>();
            for (int i = 0; i < ItemsToRemove.Count; i++)
            {
                NoteItemControl note = ItemsToRemove[i];
                DeletedNotes.Add((note.TextItem, note.VideoEventID));
                noteItemControlList.Remove(ItemsToRemove[i]);
            }

            return DeletedNotes;
        }

        static internal void NoteSelected(NoteItemControl SelectedControl, Controls.TimeLine timeline)
        {
            List<NoteItemControl> noteItemControlList = timeline.TimeLineDrawEngine.GetNoteItemControls();
            for (int i = 0; i < noteItemControlList.Count; i++)
            {
                NoteItemControl note = noteItemControlList[i];
                if (SelectedControl != note)
                {
                    note.NoteItemUnSelected();
                }
            }
        }

        static internal List<(TextItem TextItem, int VideoEventID)> GetTextItems(List<NoteItemControl> noteItemControlList)
        {
            List<(TextItem textItem, int VideoEventID)> NoteItems = new List<(TextItem textItem, int VideoEventID)>();
            for (int i = 0; i < noteItemControlList.Count; i++)
            {
                NoteItemControl noteItemControl = noteItemControlList[i];
                if (noteItemControl.VideoEventID != -1)
                {
                    NoteItems.Add((noteItemControl.TextItem, noteItemControl.VideoEventID));
                }
            }
            return NoteItems;
        }

        static internal NoteItemControl CreateNoteAtTimeSpan(TextItem textItem, TimeSpan TimeSpanAtPoint, Media EventFound, bool PositionAtHalf, Controls.TimeLine timeline, bool IsReadOnly)
        {

            if (textItem.Duration < TimeSpan.FromSeconds(1))
            {
                //Must be at least 1 second long
                textItem.Duration = TimeSpan.FromSeconds(1);
            }

            //Create Note Item
            if (PositionAtHalf == true)
            {
                textItem.Start = TimeSpanAtPoint - EventFound.StartTime - TimeSpan.FromSeconds(textItem.Duration.TotalSeconds / 2);
            }
            else
            {
                textItem.Start = TimeSpanAtPoint - EventFound.StartTime;
            }

            NoteItemControl noteItemControl = CreateNoteControl(EventFound, textItem, timeline, IsReadOnly);


            //if (textItem.Start + textItem.Duration > noteItemControl.GetMaximum())
            //{
            //    textItem.Duration = noteItemControl.GetMaximum() - textItem.Start;
            //}
            if (textItem.Start < TimeSpan.Zero)
            {
                textItem.Start = TimeSpan.Zero;
            }

            noteItemControl.ToolTip = textItem.Text;

            return noteItemControl;
        }

        static internal NoteItemControl CreateNoteControl(Media media, TextItem textItem, Controls.TimeLine timeline, bool IsReadOnly)
        {
            NoteItemControl noteItemControl = new NoteItemControl(textItem, media.VideoEventID, media.Duration, media.StartTime, IsReadOnly);

            timeline.TimeLineDrawEngine.AddNoteItemControls(noteItemControl);

            return noteItemControl;
        }

        internal static TimeSpan GetNextNoteStartTime(List<NoteItemControl> noteItemControlList, TimeSpan TotalDuration, TimeSpan time)
        {
            List<NoteItemControl> _AllNoteItemControls = noteItemControlList.ToList();
            _AllNoteItemControls = Sort(_AllNoteItemControls).ToList();

            foreach (NoteItemControl element in _AllNoteItemControls)
            {
                if (element.GetStartTime() > time)
                {
                    return element.GetStartTime();
                }
            }

            return TotalDuration;
        }

        internal static TimeSpan GetTotalTime(List<Media> Playlist)
        {
            TimeSpan _TotalDuration = TimeSpan.Zero;

            for (int i = 0; i < Playlist.Count; i++)
            {
                Media item = Playlist[i];
                if (item.StartTime + item.Duration > _TotalDuration)
                {
                    _TotalDuration = item.StartTime + item.Duration;
                }
            }

            return _TotalDuration;
        }

        internal static List<Media> ShiftVideoEvents(List<Media> VideoEvents, TimeSpan Duration, bool ShouldReduce)
        {
            List<Media> EventsFound = new List<Media>();

            if (ShouldReduce == false)
            {
                //Should add to VideoEvents
                for (int i = 0; i < VideoEvents.Count; i++)
                {
                    Media media = VideoEvents[i];
                    media.StartTime = media.StartTime + Duration;
                }
            }
            else
            {
                //Should Reduce from VideoEvents
                for (int i = 0; i < VideoEvents.Count; i++)
                {
                    Media media = VideoEvents[i];
                    media.StartTime = media.StartTime - Duration;
                }
            }

            return EventsFound;
        }

        internal static List<Media> FindMediaAfterTimeSpan(List<Media> Playlist, TimeSpan time)
        {
            List<Media> EventsFound = new List<Media>();
            for (int i = 0; i < Playlist.Count; i++)
            {
                Media media = Playlist[i];
                if (media.StartTime >= time)
                {
                    EventsFound.Add(media);
                }
            }

            return EventsFound;
        }

        internal static (Media media, TimeSpan TimeAtPoint) FindMediaAtPoint(Canvas MainCanvas, Canvas LegendCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration, List<Media> Playlist, Point location, bool PositionAtHalf)
        {
            //Get Media Item
            TimeSpan TimeSpanAtPoint = TimeSpan.Zero;

            if (location.X < LegendCanvas.ActualWidth)
            {
                //It was dropped on the legend
                TimeSpanAtPoint = ViewPortStart;
            }
            else
            {
                double PointOnCanvas = location.X - LegendCanvas.ActualWidth;

                //Convert Point To TimeSpan
                TimeSpanAtPoint = TimeSpan.FromSeconds(ViewPortStart.TotalSeconds + (ViewPortDuration.TotalSeconds * (PointOnCanvas / MainCanvas.ActualWidth)));
            }

            Media EventFound = null;
            for (int i = 0; i < Playlist.Count; i++)
            {
                Media media = Playlist[i];
                if (TimeSpanAtPoint > media.StartTime && TimeSpanAtPoint < media.StartTime + media.Duration && (media.mediaType == MediaType.Image || media.mediaType == MediaType.Video) && media.TrackId == 2)
                {
                    EventFound = media;
                    break;
                }
            }

            return (EventFound, TimeSpanAtPoint);
        }

        internal static Media FindMediaAtTimeSpan(List<Media> Playlist, TextItem textItem, TimeSpan TimeSpanAtPoint, bool PositionAtHalf)
        {
            //Get Media Item
            Media EventFound = null;
            for (int i = 0; i < Playlist.Count; i++)
            {
                Media media = Playlist[i];
                if (TimeSpanAtPoint > media.StartTime && TimeSpanAtPoint < media.StartTime + media.Duration && (media.mediaType == MediaType.Image || media.mediaType == MediaType.Video) && media.TrackId == 2)
                {
                    EventFound = media;
                    break;
                }
            }

            return EventFound;
        }

        internal static List<(Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration)> FindMissingVideoEventTimeSpans(List<Media> Playlist)
        {
            List<(Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration)> MissingTimeSpans = new List<(Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration)>();

            TimeSpan PreviousEndTime = TimeSpan.Zero;
            Media PreviousItem = null;
            for (int i = 0; i < Playlist.Count; i++)
            {
                Media media = Playlist[i];
                if (media.TrackId == 2)
                {

                    TimeSpan CalculatedDuration = media.StartTime - PreviousEndTime;

                    if (CalculatedDuration > TimeSpan.Zero)
                    {
                        MissingTimeSpans.Add((PreviousItem, media, PreviousEndTime, CalculatedDuration));
                    }

                    PreviousEndTime = media.StartTime + media.Duration;
                    PreviousItem = media;
                }
            }

            return MissingTimeSpans;
        }

        internal static TextItem GetLastTextItem(Media media)
        {
            if (media != null)
            {
                TextItem LastTextItem = null;
                for (int i = 0; i < media.RecordedTextList.Count; i++)
                {
                    TextItem textItem = media.RecordedTextList[i];
                    if (LastTextItem != null)
                    {
                        if (textItem.Start > LastTextItem.Start)
                        {
                            LastTextItem = textItem;
                        }
                    }
                    else
                    {
                        LastTextItem = textItem;
                    }
                }
                return LastTextItem;
            }
            else
            {
                return null;
            }
        }
    }
}
