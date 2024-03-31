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
    internal class TrackVideoEventItem : Border
    {
        internal Media Media { get; set; }
        Color HighlightedColor = Colors.Gray;
        internal Color Color = Colors.Gray;

        ContextMenu myContextMenu;
        MenuItem DeleteEventBtn;
        MenuItem FocusEventBtn;
        MenuItem SetDurationBtn;

        TimeLine timeLine;

        Border OverTimeBorder;

        internal TrackVideoEventItem(Media media, Color color, MediaType ImageType, TimeLine timeline, double width, double height, bool IsReadOnly)
        {
            this.Unloaded += TrackVideoEventItem_Unloaded;
            Media = media;
            Color = color;
            timeLine = timeline;

            if (height < 2)
            { 
                height = 2;
            }


            if (IsReadOnly == false)
            {
                myContextMenu = new ContextMenu();

                DeleteEventBtn = new MenuItem()
                {
                    Header = "Delete Event",
                };
                DeleteEventBtn.Click += timeline.DeleteEventBtn_Click;

                FocusEventBtn = new MenuItem()
                {
                    Header = "Enter Selected Mode",
                };
                FocusEventBtn.Click += timeline.FocusEventBtn_Click;

                SetDurationBtn = new MenuItem()
                {
                    Header = "Set Duration",
                };
                SetDurationBtn.Click += timeline.SetDurationBtn_Click;

                myContextMenu.Items.Add(SetDurationBtn);
                myContextMenu.Items.Add(DeleteEventBtn);
                myContextMenu.Items.Add(FocusEventBtn);

                HighlightedColor = ScaleColorByValue(Color, 0.2);

                myContextMenu.Closed += MyContextMenu_Closed;

                this.ContextMenu = myContextMenu;
            }
            Canvas canvas = new Canvas() { ClipToBounds = true, SnapsToDevicePixels = true };
            OverTimeBorder = new Border() { Background = Brushes.White, Opacity = 0.3, Height = height - 2 };


            Image Icon = new Image() { Height = 17,  SnapsToDevicePixels = true };
            RenderOptions.SetBitmapScalingMode(Icon, BitmapScalingMode.HighQuality);
            Icon.Margin = new Thickness(2);
            Icon.HorizontalAlignment = HorizontalAlignment.Left;
            Icon.Opacity = 0.5;

            if (media.ImageType != null && media.ImageType != "")
            {
                int Brightness = Color.R + Color.G + Color.B;
                if (Brightness > 250)
                {
                    TextBlock textBlock = new TextBlock() { SnapsToDevicePixels = true };
                    textBlock.Text = FirstCharToUpper(media.ImageType);
                    textBlock.Opacity = 0.7;
                    textBlock.Foreground = Brushes.Black;
                    textBlock.Margin = new Thickness(25, 2, 2, 2);
                    canvas.Children.Add(textBlock);


                   
                }
                else
                {
                    TextBlock textBlock = new TextBlock() {SnapsToDevicePixels = true };
                    textBlock.Text = FirstCharToUpper(media.ImageType);
                    textBlock.Opacity = 0.8;
                    textBlock.Foreground = Brushes.White;
                    textBlock.Margin = new Thickness(25, 2, 2, 2);
                    canvas.Children.Add(textBlock);
                    Icon.Opacity = 0.8;

                   
                }
            }

            {
                int Brightness = Color.R + Color.G + Color.B;
                if (Brightness > 250)
                {

                    switch (ImageType)
                    {
                        case MediaType.Image:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Image-Small-Dark.png"));

                            break;
                        case MediaType.Video:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Video-Small-Dark.png"));

                            break;
                        case MediaType.Audio:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small-Dark.png"));

                            break;
                        case MediaType.Form:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));

                            break;
                    }
                }
                else
                {
                    switch (ImageType)
                    {
                        case MediaType.Image:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Image-Small.png"));

                            break;
                        case MediaType.Video:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Video-Small.png"));

                            break;
                        case MediaType.Audio:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small.png"));

                            break;
                        case MediaType.Form:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small.png"));

                            break;
                    }
                }
            }
            



            canvas.Children.Add(Icon);
            canvas.Children.Add(OverTimeBorder);

            this.Child = canvas;

            if (IsReadOnly == false)
            {
                this.MouseEnter += TrackVideoEventItem_MouseEnter;
                this.MouseLeave += TrackVideoEventItem_MouseLeave;
            }

            CalculateOriginalTime(width);
        }

        private string FirstCharToUpper(string input)
        {
            if (input.Length > 0)
            {
                return input[0].ToString().ToUpper() + input.Substring(1);
            }
            else
            {
                return input;
            }
        }

        internal void CalculateOriginalTime(double width)
        {
            if (Media.OriginalDuration != TimeSpan.Zero)
            {
                double timeFraction = Media.OriginalDuration.TotalSeconds / Media.Duration.TotalSeconds;
                OverTimeBorder.Width = width - (width * timeFraction);
                OverTimeBorder.Margin = new Thickness(width - OverTimeBorder.Width, 0, 0, 0);
            }
        }

        private void TrackVideoEventItem_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseEnter -= TrackVideoEventItem_MouseEnter;
            this.MouseLeave -= TrackVideoEventItem_MouseLeave;

            if (myContextMenu != null)
            {
                myContextMenu.Closed -= MyContextMenu_Closed;
                DeleteEventBtn.Click -= timeLine.DeleteEventBtn_Click;
                FocusEventBtn.Click -= timeLine.FocusEventBtn_Click;
            }

            if (OverTimeBorder != null)
            {
                OverTimeBorder = null;
            }

            Media = null;
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

            return Color.FromArgb(Color.A, (byte)Red, (byte)Green, (byte)Blue);
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
