using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Timeline.UserControls.Controls
{
    /// <summary>
    /// Interaction logic for TrackbarLineControl.xaml
    /// </summary>
    public partial class TrackbarLineControl : UserControl
    {
        public TrackbarLineControl()
        {
            InitializeComponent();
        }

        public void SetHeight(double height)
        {
            TrackbarLine.Y1= height;
        }
    }
}
