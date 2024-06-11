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
using System.Windows.Input;
using System.Diagnostics;
using System.Net;
using ManageMedia_UserControl.Classes.TimeLine.DrawEngine;
using System.Windows.Documents;
using Sqllite_Library.Models;

namespace ManageMedia_UserControl.Controls
{
    public class TrackCalloutItem : Border
    {
        internal Media MediaCallout { get; set; }
        Color HighlightedColor = Colors.Gray;
        internal Color Color = Colors.Gray;
        internal event EventHandler<Media> MediaSelectedEvent;
        ContextMenu myContextMenu;
        MenuItem DeleteEventBtn, CloneEventAtTrackbarLocationBtn, CloneEventAtTimelineEndBtn, EditEventBtn;
        //MenuItem AddImageUsingLibraryAfterSelectedEventBtn;
        TimeLine timeLine;
        List<TrackCalloutItem> TrackCalloutItems;
        Border OverTimeBorder;
        bool IsManageMedia;
        TrackCalloutItem NextElement;
        public bool IsSelected;
        internal TrackCalloutItem(Media media, Color color, EnumMedia MediaType, TimeLine timeline, double width, double height, bool _IsManageMedia, Canvas MainCanvas, List<TrackCalloutItem> _TrackCalloutItems)
        {
            this.Unloaded += TrackCalloutItem_Unloaded;
            MediaCallout = media;
            Color = color;
            timeLine = timeline;
            TrackCalloutItems = _TrackCalloutItems;
            IsManageMedia = _IsManageMedia;
            if (height < 2)
            { 
                height = 2;
            }

            SetupContextMenu(timeline);
            Canvas canvas = new Canvas() { ClipToBounds = true, SnapsToDevicePixels = true };

            Image Icon = new Image() { Height = 17, SnapsToDevicePixels = true };
            RenderOptions.SetBitmapScalingMode(Icon, BitmapScalingMode.HighQuality);

            Icon.Margin = new Thickness(2);
            Icon.HorizontalAlignment = HorizontalAlignment.Left;
            Icon.Opacity = 0.5;

            int red = (int)((double)media.Color.R / 1.5);
            int green = (int)((double)media.Color.G / 1.5);
            int blue = (int)((double)media.Color.B / 1.5);
            int Brightness = red + green + blue;

            if (media.TrackId == 3)
            {
                //Callout 1
                if (Brightness > 250)
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));
                }
                else
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small.png"));
                }
            }
            else if (media.TrackId == 4)
            {
                //Callout 2
                if (Brightness > 250)
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small-Dark.png"));
                }
                else
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Form-Small.png"));
                }
            }
            else if (media.TrackId == 1)
            {
                //Audio
                if (Brightness > 250)
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small-Dark.png"));
                }
                else
                {
                    Icon.Source = new BitmapImage(new Uri("pack://application:,,,/ManageMedia_UserControl;component/Icons/video-events/Audio-Small.png"));
                }
            }
            canvas.Children.Add(Icon);
            var border = GetBorder(height, width, color, MainCanvas, IsManageMedia);
            var tt = new ToolTip();
            tt.Content = $"ID - {MediaCallout.VideoEventID}";
            border.ToolTip = tt;
            canvas.Children.Add (border);
            this.Child = canvas;
            this.MouseEnter += TrackCalloutItem_MouseEnter;
            this.MouseLeave += TrackCalloutItem_MouseLeave;
            this.MouseLeftButtonDown += TrackCalloutItem_MouseLeftButtonDown;

        }

        private Border GetBorder(double TrackHeight, double width, Color FillColor, Canvas MainCanvas, bool IsManageMedia)
        {
            var rightBorderItem = new Border();

            rightBorderItem.Width = 5;
            rightBorderItem.Height = TrackHeight - 2;
            rightBorderItem.BorderBrush = new SolidColorBrush(FillColor);
            rightBorderItem.BorderThickness = new Thickness(2, 0, 2, 0);
            rightBorderItem.HorizontalAlignment = HorizontalAlignment.Right;
            rightBorderItem.Background = new SolidColorBrush(FillColor);
            //rightBorderItem.IsHitTestVisible = false;
            if (width > 8)
            {
                rightBorderItem.Margin = new Thickness(width - 8, 0, 0, 0);
            }
            AddEventHandlers(rightBorderItem, MainCanvas);

            return rightBorderItem;
        }

        bool drag = false;
        double previousDiff = 0.0;
        bool isFirstClick = false;
        Point BorderPosition;
        
        private void AddEventHandlers(Border rightBorderItem, Canvas MainCanvas)
        {
            rightBorderItem.MouseEnter += RightBorderItem_MouseEnter;

            rightBorderItem.PreviewMouseLeftButtonDown += (object s, MouseButtonEventArgs e) =>
            {
                e.Handled = true;
                rightBorderItem.CaptureMouse();
                BorderPosition = e.GetPosition(MainCanvas);
                drag = true;
                isFirstClick = true;
                previousDiff = 0.0;
                NextElement = null;
            };

            rightBorderItem.PreviewMouseMove += (object s, MouseEventArgs e) =>
            {
                e.Handled = true;
                if (!drag) return;

                var mousePos = e.GetPosition(MainCanvas);

                double mouseX = mousePos.X;
                if (mouseX >= 0)
                {
                    if (isFirstClick == false)
                    {
                        var diffNew = mouseX - BorderPosition.X;
                        
                        
                        if (diffNew != previousDiff)
                        {
                            var canMove = CanMoveAhead(MediaCallout, timeLine, mouseX);
                            if (canMove)
                            {
                                if (this.Width + diffNew - previousDiff > 0)
                                {
                                    this.Width = this.Width + diffNew - previousDiff;
                                    rightBorderItem.Margin = new Thickness(this.Width - 8, 0, 0, 0);
                                }
                                previousDiff = diffNew;
                                MediaCallout.Duration = timeLine.GetTimeSpanByLocation(mouseX) - MediaCallout.StartTime;
                                timeLine.CalloutSizeChanged_Event(MediaCallout);
                            }
                        }
                    }
                    else
                    {
                        isFirstClick = false;
                    }
                }
            };

            rightBorderItem.PreviewMouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                e.Handled = true;
                drag = false;
                rightBorderItem.ReleaseMouseCapture();
                NextElement = null;
            };
        }

        private bool CanMoveAhead(Media calloutEvent, Controls.TimeLine timeline, double mouseX)
        {
            var canMove = false;
            if (NextElement == null)
                NextElement = TrackCalloutItems
                                        .FindAll(x => x.MediaCallout.TrackId == MediaCallout.TrackId)?
                                        .OrderBy(x => x.MediaCallout.StartTime)?
                                        .Where(x => x.MediaCallout.StartTime > MediaCallout.StartTime)?
                                        .FirstOrDefault();
            if (NextElement != null)
            {
                var newEndTime = timeline.GetTimeSpanByLocation(mouseX);
                if (newEndTime <= NextElement.MediaCallout.StartTime)
                    canMove = true;
                else if (newEndTime > NextElement.MediaCallout.StartTime)
                    canMove = false;
            }
            else
                canMove = true;
            return canMove;
        }

        private void RightBorderItem_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            var border = sender as Border;
            border.Cursor = Cursors.SizeWE;
        }

        private void SetupContextMenu(TimeLine timeline)
        {
            if (!IsManageMedia)
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

                //AddImageUsingLibraryAfterSelectedEventBtn = new MenuItem()
                //{
                //    Header = "Add Image using Library After Selected Event",
                //};
                //AddImageUsingLibraryAfterSelectedEventBtn.Click += timeline.AddImageUsingLibraryAfterSelectedEvent;

                myContextMenu.Items.Add(EditEventBtn);
                myContextMenu.Items.Add(DeleteEventBtn);
                myContextMenu.Items.Add(CloneEventAtTrackbarLocationBtn);
                myContextMenu.Items.Add(CloneEventAtTimelineEndBtn);
                //myContextMenu.Items.Add(AddImageUsingLibraryAfterSelectedEventBtn);

                HighlightedColor = ScaleColorByValue(Color, 0.2);

                myContextMenu.Closed += MyContextMenu_Closed;

                this.ContextMenu = myContextMenu;
            }
        }

        private void TrackCalloutItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MediaSelectedEvent(sender, MediaCallout);
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
            if (MediaCallout.OriginalDuration != TimeSpan.Zero)
            {
                double timeFraction = MediaCallout.OriginalDuration.TotalSeconds / MediaCallout.Duration.TotalSeconds;
                OverTimeBorder.Width = width - (width * timeFraction);
                OverTimeBorder.Margin = new Thickness(width - OverTimeBorder.Width, 0, 0, 0);
            }
        }

        private void TrackCalloutItem_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseEnter -= TrackCalloutItem_MouseEnter;
            this.MouseLeave -= TrackCalloutItem_MouseLeave;

            
            if (myContextMenu != null)
            {
                if (!IsManageMedia)
                {
                    myContextMenu.Closed -= MyContextMenu_Closed;
                    DeleteEventBtn.Click -= timeLine.DeleteEventBtn_Click;
                    CloneEventAtTimelineEndBtn.Click -= timeLine.CloneEventAtTimelineEnd;
                    CloneEventAtTrackbarLocationBtn.Click -= timeLine.CloneEventAtTrackbar;
                    //AddImageUsingLibraryAfterSelectedEventBtn.Click -= timeLine.AddImageUsingLibraryAfterSelectedEvent;
                }
            }

            if (OverTimeBorder != null)
            {
                OverTimeBorder = null;
            }

            MediaCallout = null;
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

        private void TrackCalloutItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AnimateOut();
        }

        private void TrackCalloutItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
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
