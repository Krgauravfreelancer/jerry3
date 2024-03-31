using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ManageMedia_UserControl.Models
{
    public class PlannedTextGroup
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public List<PlannedText> PlannedTexts { get; set; }
        public List<PlannedImage> Images = new List<PlannedImage>();

        public List<string> SuggestedText = new List<string>();
    }
    public class PlannedText
    {
        public int PlannedTextID { get; set;}
        public string Text { get; set; }
        public int SortKey { get; set; }
    }

    public class PlannedImage
    {
        public int PlannedTextID { get; set; }
        public byte[] Image { get; set; }
        public byte[] ThumbNail { get; set; }
        public int SortKey { get; set; }
    }
}
