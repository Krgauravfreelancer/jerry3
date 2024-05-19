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
using Sqllite_Library.Models;

namespace ManageMedia_UserControl.Controls
{
    public class TrackVideoEventItem : Border
    {
        internal Media Media { get; set; }
        Color HighlightedColor = Colors.Gray;
        internal Color Color = Colors.Gray;
        internal event EventHandler<Media> MediaSelectedEvent;
        ContextMenu myContextMenu;
        MenuItem DeleteEventBtn, CloneEventAtTrackbarLocationBtn, CloneEventAtTimelineEndBtn, AddImageUsingLibraryAfterSelectedEventBtn, EditEventBtn;
        MenuItem FocusEventBtn;
        MenuItem SetDurationBtn;

        TimeLine timeLine;

        Border OverTimeBorder;
        bool IsManageMedia;
        public bool IsSelected;
        bool IsReadOnly;

        internal TrackVideoEventItem(Media media, Color color, EnumMedia MediaType, TimeLine timeline, double width, double height, bool _IsReadOnly, bool _IsManageMedia)
        {
            this.Unloaded += TrackVideoEventItem_Unloaded;
            Media = media;
            Color = color;
            timeLine = timeline;
            IsManageMedia = _IsManageMedia;
            IsReadOnly = _IsReadOnly;
            if (height < 2)
            { 
                height = 2;
            }

            SetupContextMenuEvent(timeline);

            Canvas canvas = new Canvas() { ClipToBounds = true, SnapsToDevicePixels = true };
            OverTimeBorder = new Border() { Background = Brushes.White, Opacity = 0.3, Height = height - 2 };

            Image Icon = new Image() { Height = 17,  SnapsToDevicePixels = true };
            RenderOptions.SetBitmapScalingMode(Icon, BitmapScalingMode.HighQuality);
            Icon.Margin = new Thickness(2);
            Icon.HorizontalAlignment = HorizontalAlignment.Left;
            Icon.Opacity = 0.5;

            if (media.screenType.ToString() != "")
            {
                int Brightness = Color.R + Color.G + Color.B;
                if (Brightness > 250)
                {
                    TextBlock textBlock = new TextBlock() { SnapsToDevicePixels = true };
                    textBlock.Text = FirstCharToUpper(media.screenType.ToString());
                    textBlock.Opacity = 0.7;
                    textBlock.Foreground = Brushes.Black;
                    textBlock.Margin = new Thickness(25, 2, 2, 2);
                    canvas.Children.Add(textBlock);
                }
                else
                {
                    TextBlock textBlock = new TextBlock() {SnapsToDevicePixels = true };
                    textBlock.Text = FirstCharToUpper(media.screenType.ToString());
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

                    switch (MediaType)
                    {
                        case EnumMedia.IMAGE:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Image-Small-Dark.png"));

                            break;
                        case EnumMedia.VIDEO:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Video-Small-Dark.png"));

                            break;
                        case EnumMedia.AUDIO:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small-Dark.png"));

                            break;
                        case EnumMedia.FORM:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));

                            break;
                    }
                }
                else
                {
                    switch (MediaType)
                    {
                        case EnumMedia.IMAGE:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Image-Small.png"));

                            break;
                        case EnumMedia.VIDEO:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Video-Small.png"));

                            break;
                        case EnumMedia.AUDIO:
                            Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small.png"));

                            break;
                        case EnumMedia.FORM:
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
                this.MouseLeftButtonDown += TrackVideoEventItem_MouseLeftButtonDown;
            }
            CalculateOriginalTime(width);
        }

        private void SetupContextMenuEvent(Controls.TimeLine timeline)
        {
            if (IsManageMedia && IsReadOnly == false)
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
            else if (!IsManageMedia)
            {
                myContextMenu = new ContextMenu();

                EditEventBtn = new MenuItem()
                {
                    Header = "Edit event",
                };
                EditEventBtn.Click += timeline.EditEventBtnClicked;

                DeleteEventBtn = new MenuItem()
                {
                    Header = "Delete Event",
                };
                DeleteEventBtn.Click += timeline.DeleteEventForTimeline;

                CloneEventAtTrackbarLocationBtn = new MenuItem()
                {
                    Header = "Clone Event at trackbar location",
                };
                CloneEventAtTrackbarLocationBtn.Click += timeline.CloneEventAtTrackbar;

                CloneEventAtTimelineEndBtn = new MenuItem()
                {
                    Header = "Clone Event at End of Timeline",
                };
                CloneEventAtTimelineEndBtn.Click += timeline.CloneEventAtTimelineEnd;

                AddImageUsingLibraryAfterSelectedEventBtn = new MenuItem()
                {
                    Header = "Add Image using Library After Selected Event",
                };
                AddImageUsingLibraryAfterSelectedEventBtn.Click += timeline.AddImageUsingLibraryAfterSelectedEvent;

                myContextMenu.Items.Add(EditEventBtn);
                myContextMenu.Items.Add(DeleteEventBtn);
                myContextMenu.Items.Add(CloneEventAtTrackbarLocationBtn);
                myContextMenu.Items.Add(CloneEventAtTimelineEndBtn);
                myContextMenu.Items.Add(AddImageUsingLibraryAfterSelectedEventBtn);

                HighlightedColor = ScaleColorByValue(Color, 0.2);

                myContextMenu.Closed += MyContextMenu_Closed;

                this.ContextMenu = myContextMenu;
            }
        }


        private void TrackVideoEventItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MediaSelectedEvent(sender, Media);
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

            
            if (myContextMenu != null && timeLine != null)
            {
                if (IsManageMedia)
                {
                    myContextMenu.Closed -= MyContextMenu_Closed;
                    if (timeLine != null)
                    {
                        DeleteEventBtn.Click -= timeLine.DeleteEventBtn_Click;
                        FocusEventBtn.Click -= timeLine.FocusEventBtn_Click;
                    }
                }
                else
                {
                    myContextMenu.Closed -= MyContextMenu_Closed;
                    if (timeLine != null)
                    {
                        EditEventBtn.Click -= timeLine.EditEventBtnClicked;
                        DeleteEventBtn.Click -= timeLine.DeleteEventBtn_Click;
                        CloneEventAtTimelineEndBtn.Click -= timeLine.CloneEventAtTimelineEnd;
                        CloneEventAtTrackbarLocationBtn.Click -= timeLine.CloneEventAtTrackbar;
                        AddImageUsingLibraryAfterSelectedEventBtn.Click -= timeLine.AddImageUsingLibraryAfterSelectedEvent;
                    }
                }
            }

            if (OverTimeBorder != null)
                OverTimeBorder = null;

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
            AnimateOut();
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
