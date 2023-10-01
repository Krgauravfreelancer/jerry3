using FullScreenPlayer_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DebugVideoCreator.XAML
{
    /// <summary>
    /// Interaction logic for FullScreen_TimeLine_UserControl.xaml
    /// </summary>
    public partial class FullScreen_TimeLine_UserControl : UserControl
    {
        List<TimeLineItem> TimeLineItems = null;

        LinearGradientBrush Blue = new LinearGradientBrush(Color.FromArgb(200, 0, 200, 150), Color.FromArgb(0, 0, 200, 150), new Point(0, 0), new Point(1, 1));
        LinearGradientBrush Gray = new LinearGradientBrush(Color.FromArgb(200, 20, 20, 20), Color.FromArgb(0, 20, 20, 20), new Point(0, 0), new Point(1, 1));
        LinearGradientBrush DarkBlue = new LinearGradientBrush(Color.FromArgb(200, Colors.Blue.R, 60, Colors.DarkBlue.B), Color.FromArgb(0, Colors.Blue.R, 60, Colors.Blue.B), new Point(0, 0), new Point(1, 1));
        LinearGradientBrush Orange = new LinearGradientBrush(Color.FromArgb(200, Colors.Orange.R, Colors.Orange.G, Colors.Orange.B), Color.FromArgb(0, Colors.Orange.R, Colors.Orange.G, Colors.Orange.B), new Point(0, 0), new Point(1, 1));


        double TotalWidth = 0;
        double TotalTime = 0;
        public FullScreen_TimeLine_UserControl()
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
            MediaStack.Children.Clear();

            if (timeLineItems.Count > 0)
            {
                TotalWidth = this.ActualWidth;
                TotalTime = timeLineItems[timeLineItems.Count - 1].EndTime.TotalSeconds;

                foreach (TimeLineItem item in timeLineItems)
                {
                    double Width = item.Duration.TotalSeconds / TotalTime * TotalWidth;
                    Label newLabel = CreateLabel("Media" + item.Index, Width, item.Type);
                    MediaStack.Children.Add(newLabel);
                }
            }
            else
            {
                Set_Elapsed(TimeSpan.Zero);
            }

        }

        private Label CreateLabel(string Text, double Width, TimeLineType Type)
        {
            LinearGradientBrush Background;
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

            if (Width < 2)
            {
                Width = 2;
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

            if (Double.IsNaN(Offset) == true)
            {
                Offset = 0;
            }

            ElapsedTxt.Text = Elapsed.ToString("mm':'ss");
            Indicator.Margin = new Thickness(Offset, 0, 0, 0);
        }

        public void Completed()
        {
            Indicator.Margin = new Thickness(TotalWidth - 2, 0, 0, 0);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition((Canvas)sender);
            double value = pos.X / ((Canvas)sender).ActualWidth;

            //Outputs 0.0 to 1.0

            TimeLineClicked(value);
        }

        public event EventHandler<SeekEventArgs> TimeLine_Clicked;

        private void TimeLineClicked(double position)
        {
            if (TimeLine_Clicked != null)
            {
                if (TimeLineItems != null)
                {
                    if (TimeLineItems.Count > 0)
                    {
                        TimeLineItem lastItem = TimeLineItems.LastOrDefault();

                        long Ticks = (long)(((double)lastItem.StartTime.Ticks + (double)lastItem.Duration.Ticks) * position);

                        TimeLine_Clicked(this, new SeekEventArgs(TimeSpan.FromTicks(Ticks)));
                    }
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MediaStack.Children.Clear();
            Cursor1.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor1.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            Cursor1.Margin = new Thickness(e.GetPosition(this).X, 0, 0, 0);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor1.Visibility = Visibility.Hidden;
        }
    }

    public class SeekEventArgs : EventArgs
    {
        public SeekEventArgs(TimeSpan position)
        {
            this.Position = position;
        }

        public TimeSpan Position { get; private set; }
    }
}
