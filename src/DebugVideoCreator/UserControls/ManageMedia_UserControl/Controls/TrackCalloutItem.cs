﻿using ManageMedia_UserControl.Classes;
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

namespace ManageMedia_UserControl.Controls
{
    internal class TrackCalloutItem : Border
    {
        internal Media Media { get; set; }
        Color HighlightedColor = Colors.Gray;
        internal Color Color = Colors.Gray;
        internal event EventHandler<Media> MediaSelectedEvent;
        ContextMenu myContextMenu;
        MenuItem DeleteEventBtn, CloneEventAtTrackbarLocationBtn, CloneEventAtTimelineEndBtn, AddImageUsingLibraryAfterSelectedEventBtn;
        MenuItem FocusEventBtn;
        MenuItem SetDurationBtn;

        TimeLine timeLine;

        Border OverTimeBorder;
        bool _IsManageMedia;
        public bool IsSelected;
        internal TrackCalloutItem(Media media, Color color, MediaType ImageType, TimeLine timeline, double width, double height, bool IsManageMedia, Canvas MainCanvas)
        {
            this.Unloaded += TrackCalloutItem_Unloaded;
            Media = media;
            Color = color;
            timeLine = timeline;
            _IsManageMedia = IsManageMedia;
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
            var border = GetBorder(height, width, color, MainCanvas);
            canvas.Children.Add (border);
            this.Child = canvas;
            this.MouseEnter += TrackCalloutItem_MouseEnter;
            this.MouseLeave += TrackCalloutItem_MouseLeave;
            this.MouseLeftButtonDown += TrackCalloutItem_MouseLeftButtonDown;

        }

        private Border GetBorder(double TrackHeight, double width, Color FillColor, Canvas MainCanvas)
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
            rightBorderItem.MouseEnter += RightBorderItem_MouseEnter;
            AddEventHandlers(rightBorderItem, MainCanvas);

            return rightBorderItem;
        }

        private void RightBorderItem_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;
            var border = sender as Border;
            border.Cursor = Cursors.SizeWE;
        }
        bool drag = false;
        double previousDiff = 0.0;
        bool isFirstClick = false;
        Point BorderPosition;
        private void AddEventHandlers(Border rightBorderItem, Canvas MainCanvas)
        {
            rightBorderItem.PreviewMouseLeftButtonDown += (object s, MouseButtonEventArgs e) =>
            {
                e.Handled = true;
                BorderPosition = e.GetPosition(MainCanvas);
                rightBorderItem.CaptureMouse();
                drag = true;
                isFirstClick = true;
            };

            rightBorderItem.PreviewMouseMove += (object s, MouseEventArgs e) =>
            {
                e.Handled = true;
                if (!drag) return;
                var border = s as Border;
                Console.WriteLine($"Inside Extension");

                // get the position of the mouse relative to the Canvas
                var mousePos = e.GetPosition(MainCanvas);

                // center the trackbarLine on the mouse
                double mouseX = mousePos.X;
                if (mouseX >= 0)
                {
                    if (isFirstClick == false)
                    {
                        var diffNew = mouseX - BorderPosition.X;
                        Console.WriteLine($"Inside Extension isFirstClick -> {mouseX}, {BorderPosition.X}, {diffNew}, {this.Width}, {isFirstClick}");
                        if (diffNew != previousDiff)
                        {
                            if (this.Width + diffNew - previousDiff > 0)
                            {
                                this.Width = this.Width + diffNew - previousDiff;
                                rightBorderItem.Margin = new Thickness(this.Width - 8, 0, 0, 0);
                            }
                            previousDiff = diffNew;
                        }
                    }
                    else
                    {
                        isFirstClick = false;
                    }
                  


                    //OnTrackbarMouseMoved(EventArgs.Empty, mouseX);
                }
                
            };

            rightBorderItem.PreviewMouseLeftButtonUp += (object sender, MouseButtonEventArgs e) =>
            {
                e.Handled = true;
                drag = false;
                rightBorderItem.ReleaseMouseCapture();
            };
        }


        private void SetupContextMenu(TimeLine timeline)
        {
            if (!_IsManageMedia)
            {
                myContextMenu = new ContextMenu();

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


                myContextMenu.Items.Add(DeleteEventBtn);
                myContextMenu.Items.Add(CloneEventAtTrackbarLocationBtn);
                myContextMenu.Items.Add(CloneEventAtTimelineEndBtn);
                myContextMenu.Items.Add(AddImageUsingLibraryAfterSelectedEventBtn);

                HighlightedColor = ScaleColorByValue(Color, 0.2);

                myContextMenu.Closed += MyContextMenu_Closed;

                this.ContextMenu = myContextMenu;
            }
        }

        private void TrackCalloutItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Console.WriteLine($"Clicked On", Media);
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

        private void TrackCalloutItem_Unloaded(object sender, RoutedEventArgs e)
        {
            this.MouseEnter -= TrackCalloutItem_MouseEnter;
            this.MouseLeave -= TrackCalloutItem_MouseLeave;

            
            if (myContextMenu != null)
            {
                if (!_IsManageMedia)
                {
                    myContextMenu.Closed -= MyContextMenu_Closed;
                    DeleteEventBtn.Click -= timeLine.DeleteEventBtn_Click;
                    CloneEventAtTimelineEndBtn.Click -= timeLine.CloneEventAtTimelineEnd;
                    CloneEventAtTrackbarLocationBtn.Click -= timeLine.CloneEventAtTrackbar;
                    AddImageUsingLibraryAfterSelectedEventBtn.Click -= timeLine.AddImageUsingLibraryAfterSelectedEvent;
                }
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

        private void TrackCalloutItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_IsManageMedia || this.ContextMenu?.IsOpen == false)
            {
                AnimateOut();
            }
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
