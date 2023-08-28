using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ScreenRecording_UserControl.Models
{
    public class Dpi
    {
        static Dpi()
        {
            using (var src = new HwndSource(new HwndSourceParameters()))
            {
                if (src.CompositionTarget != null)
                {
                    var matrix = src.CompositionTarget.TransformToDevice;

                    X = (float)matrix.M11;
                    Y = (float)matrix.M22;
                }
            }
        }

        public static float X { get; }

        public static float Y { get; }
    }
}
