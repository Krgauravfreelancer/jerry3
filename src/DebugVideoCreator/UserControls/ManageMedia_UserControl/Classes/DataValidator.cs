using ScreenRecorder_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageMedia_UserControl.Classes
{
    internal class DataValidator
    {
        internal Media DoesItContain(Media MediaItem, List<Media> MediaList)
        {
            for (int i = 0; i < MediaList.Count; i++)
            { 
                Media OldMedia = MediaList[i];
                if (OldMedia.VideoEventID == MediaItem.VideoEventID)
                {
                    return OldMedia;
                }
            }

            return null;
        }

        internal List<Media> CreateListWithItemsRemoved(List<Media> MediaList, List<Media> RemoveList1)
        {
            List<Media> NewList = MediaList.ToList();
            if (RemoveList1 != null)
            {
                for (int i = 0; i < RemoveList1.Count; i++ )
                {
                    Media media = RemoveList1[i];
                    NewList.Remove(media);
                }
            }

            return NewList;
        }

        internal Media FindMediaByVideoEventID(List<Media> MediaList, int VideoEventID)
        {
            for (int i = 0; i < MediaList.Count; i++)
            {
                Media media = MediaList[i];
                if (media.VideoEventID == VideoEventID)
                { 
                    return media;
                }
            }

            return null;
        }

        internal bool IsMediaDifferent(Media MediaItem1, Media MediaItem2)
        {
            if (MediaItem1.StartTime != MediaItem2.StartTime)
            {
                return true;
            }

            if (MediaItem1.Duration != MediaItem2.Duration)
            {
                return true;
            }

            return false;
        }

        internal TextItem FindNoteByNoteID(List<TextItem> TextList, int NoteID)
        {
            for (int i = 0; i < TextList.Count; i++)
            {
                TextItem text = TextList[i];
                if (text.NoteID == NoteID)
                {
                    return text;
                }
            }

            return null;
        }

        internal bool IsNotesDifferent(TextItem TextItem1, TextItem TextItem2)
        {
            if (TextItem1.Start != TextItem2.Start)
            {
                return true;
            }

            if (TextItem1.Duration != TextItem2.Duration)
            {
                return true;
            }

            if (TextItem1.Text != TextItem2.Text)
            {
                return true;
            }

            return false;
        }
    }
}
