using FullScreenPlayer_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FSP_UserControl.Controls
{
    /// <summary>
    /// Interaction logic for TimeLine_UserControl.xaml
    /// </summary>
    public partial class TimeLine_UserControl : UserControl
    {
        List<TimeLineItem> TimeLineItems = null;

        SolidColorBrush Blue = new SolidColorBrush(Color.FromArgb(100, Colors.DodgerBlue.R, Colors.DodgerBlue.G, Colors.DodgerBlue.B));
        SolidColorBrush Gray = new SolidColorBrush(Color.FromArgb(100, 50, 50, 50));
        SolidColorBrush DarkBlue = new SolidColorBrush(Color.FromArgb(100, Colors.Blue.R, Colors.Blue.G, Colors.Blue.B));
        SolidColorBrush Orange = new SolidColorBrush(Color.FromArgb(100, Colors.Orange.R, Colors.Orange.G, Colors.Orange.B));

        double TotalWidth = 0;
        double TotalTime = 0;
        public TimeLine_UserControl()
        {
            InitializeComponent();
        }

        public void SetTimeline(List<TimeLineItem> timeLineItems)
        {
            TimeLineItems = timeLineItems;
            BuildTimeLine(timeLineItems);
        }

        private void BuildTimeLine(List<TimeLineItem> timeLineItems)
        {
            if (timeLineItems.Count > 0)
            {
                TotalWidth = this.ActualWidth;
                TotalTime = timeLineItems[timeLineItems.Count - 1].EndTime.TotalSeconds;

                MediaStack.Children.Clear();

                foreach (TimeLineItem item in timeLineItems)
                {
                    double Width = item.Duration.TotalSeconds / TotalTime * TotalWidth;
                    Label newLabel = CreateLabel("Media" + item.Index, Width, item.Type);
                    MediaStack.Children.Add(newLabel);
                }
            }
        }

        private Label CreateLabel(string Text, double Width, TimeLineType Type)
        {
            SolidColorBrush Background;
            string Ext = "";

            switch (Type)
            {
                case TimeLineType.Video:
                    Background = Blue;
                    Ext = " - Video";
                    break;
                case TimeLineType.Image:
                    Background = DarkBlue;
                    Ext = " - Image";
                    break;
                case TimeLineType.Audio:
                    Background = Orange;
                    Ext = " - Audio";
                    break;
                default:
                    Background = Gray;
                    Text = "";
                    Ext = "Empty";
                    break;

            }

            Label label = new Label()
            {
                Content = Text + Ext,
                Background = Background,
                Width = Width - 2,
                Margin = new Thickness(1, 1, 1, 1),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
            };

            return label;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MainStack.Width = this.ActualWidth;
            MainStack.Height = this.ActualHeight;

            if (TimeLineItems != null)
            {
                BuildTimeLine(TimeLineItems);
            }
        }

        public void Set_Elapsed(TimeSpan Elapsed)
        {
            double Offset = Elapsed.TotalSeconds / TotalTime * TotalWidth;
            ElapsedTxt.Text = Elapsed.ToString("mm':'ss");
            Cursor.Margin = new Thickness(Offset, 0, 0, 0);
        }
    }
}
