using ManageMedia_UserControl.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace ManageMedia_UserControl.Controls
{
    internal class MissingVideoEventItem : Border
    {
        internal (Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration) Item { get; set; }
        internal Media MediaAfter { get; set; }
        Color HighlightedColor = Colors.Gray;
        internal Color Color = Colors.Gray;
        internal Color BorderColor = Colors.Gray;

        ContextMenu myContextMenu;
        MenuItem FillWithNextBtn;
        MenuItem FillWithPreviousBtn;

        TimeLine timeLine;

        internal MissingVideoEventItem((Media BeforeMedia, Media AfterMedia, TimeSpan Start, TimeSpan Duration) item, TimeLine timeline, bool IsReadOnly)
        {
            this.Unloaded += TrackVideoEventItem_Unloaded;
            Item = item;

            Color = Color.FromArgb(20, 0, 130, 140);
            BorderColor = Color.FromArgb(100, 0, 130, 140);
            timeLine = timeline;


            if (IsReadOnly == false)
            {
                myContextMenu = new ContextMenu();



                if (item.BeforeMedia != null)
                {
                    FillWithPreviousBtn = new MenuItem()
                    {
                        Header = "Fill With Previous",
                    };

                    FillWithPreviousBtn.Click += timeline.FillWithPrevious;

                    myContextMenu.Items.Add(FillWithPreviousBtn);
                }

                if (item.AfterMedia != null)
                {
                    FillWithNextBtn = new MenuItem()
                    {
                        Header = "Fill With Next",
                    };
                    FillWithNextBtn.Click += timeline.FillWithNext;

                    myContextMenu.Items.Add(FillWithNextBtn);
                }

                HighlightedColor = ScaleColorByValue(Color, 0.2);

                myContextMenu.Closed += MyContextMenu_Closed;

                this.ContextMenu = myContextMenu;
            }


            Border canvas = new Border() { 
                Background = new SolidColorBrush(Color),
                BorderBrush = new SolidColorBrush(BorderColor),
                BorderThickness = new Thickness(1),
            };

            this.Child = canvas;

            if (IsReadOnly == false)
            {
                this.MouseEnter += TrackVideoEventItem_MouseEnter;
                this.MouseLeave += TrackVideoEventItem_MouseLeave;
            }
        }

        private void TrackVideoEventItem_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseEnter -= TrackVideoEventItem_MouseEnter;
            this.MouseLeave -= TrackVideoEventItem_MouseLeave;

            if (myContextMenu != null)
            {
                myContextMenu.Closed -= MyContextMenu_Closed;
                if (FillWithNextBtn != null)
                {
                    FillWithNextBtn.Click -= timeLine.FillWithNext;
                }
                if (FillWithPreviousBtn != null)
                {
                    FillWithPreviousBtn.Click += timeLine.FillWithPrevious;
                }
            }


            Item = (null, null, TimeSpan.Zero, TimeSpan.Zero);
            timeLine = null;
        }

        private void MyContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            AnimateOut();
        }

        private Color ScaleColorByValue(Color color, double value)
        {
            if (value > 1)
            {
                value = 1;
            }

            if (value < 0)
            {
                value = 0;
            }

            double Red = color.R;
            double Green = color.G;
            double Blue = color.B;

            double AddToRed = (255 - Red) * value;
            double AddToGreen = (255 - Green) * value;
            double AddToBlue = (255 - Blue) * value;

            Red = Red + AddToRed;
            Green = Green + AddToGreen;
            Blue = Blue + AddToBlue;

            return Color.FromArgb(200, (byte)Red, (byte)Green, (byte)Blue);
        }

        private void TrackVideoEventItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.ContextMenu.IsOpen == false)
            {
                AnimateOut();
            }
        }

        private void TrackVideoEventItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AnimateIn();
        }

        private void AnimateIn()
        {
            ColorAnimation colorChangeAnimation = new ColorAnimation();
            colorChangeAnimation.From = (Color)((SolidColorBrush)this.Background).Color;
            colorChangeAnimation.To = HighlightedColor;
            colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
            Storyboard BackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation, this);
            Storyboard.SetTargetProperty(colorChangeAnimation, colorTargetPath);
            BackgroundChangeStory.Children.Add(colorChangeAnimation);
            BackgroundChangeStory.Begin();
        }

        private void AnimateOut()
        {
            ColorAnimation colorChangeAnimation = new ColorAnimation();
            colorChangeAnimation.From = (Color)((SolidColorBrush)this.Background).Color;
            colorChangeAnimation.To = Color;
            colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
            Storyboard BackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation, this);
            Storyboard.SetTargetProperty(colorChangeAnimation, colorTargetPath);
            BackgroundChangeStory.Children.Add(colorChangeAnimation);
            BackgroundChangeStory.Begin();
        }
    }
}
