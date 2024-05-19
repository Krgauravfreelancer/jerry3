
using ScreenRecorder_UserControl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ManageMedia_UserControl.Controls
{
    /// <summary>
    /// Interaction logic for NoteItemControl.xaml
    /// </summary>
    public partial class NoteItemControl : UserControl
    {
        string Text;

        public TextItem TextItem { get; set; }
        public int VideoEventID { get; set; }
        internal TimeSpan VideEventMaxLimit;
        internal TimeSpan VideEventMinLimit;
        internal TimeSpan PreviousItemLimit;
        internal TimeSpan NextItemLimit;
        TimeLine _TimeLine = null;
        DispatcherTimer DragSelectedTimmer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
        internal bool IsSelected { get; private set; } = false ;
        bool IsDragging = false;



        public NoteItemControl(bool IsReadOnly = false)
        {
            InitializeComponent();
            if (IsReadOnly == false)
            {
                DragSelectedTimmer.Tick += DragSelectedTimmer_Tick;
            }
            else
            {
                ControlHandle.ContextMenu = null;

                RightHandle.MouseDown -= RightHandle_MouseDown;
                RightHandle.MouseMove -= RightHandle_MouseMove;
                RightHandle.MouseUp -= RightHandle_MouseUp;

                LeftHandle.MouseDown -= LeftHandle_MouseDown;
                LeftHandle.MouseMove -= LeftHandle_MouseMove;
                LeftHandle.MouseUp -= LeftHandle_MouseUp;

                ControlHandle.MouseUp -= ControlHandle_MouseUp;
                ControlHandle.MouseMove -= ControlHandle_MouseMove;
                ControlHandle.MouseDown -= ControlHandle_MouseDown;

                TextTxt.IsEnabled = false;
            }
        }

        public TimeSpan GetStartTime()
        {
            return VideEventMinLimit + TextItem.Start;
        }

        public TimeSpan GetEndTime()
        {
            return VideEventMinLimit + TextItem.Start + TextItem.Duration;
        }
        public TimeSpan GetDuration()
        {
            return TextItem.Duration;
        }


        public NoteItemControl(TextItem textItem, int videoEventID, TimeSpan Max, TimeSpan Min, bool IsReadOnly = false)
        {
            InitializeComponent();
            VideoEventID = videoEventID;
            TextItem = textItem;
            Text = textItem.Text;
            VideEventMaxLimit = Max;
            VideEventMinLimit = Min;
            PreviousItemLimit = Min + textItem.Start - TimeSpan.FromSeconds(0.5);
            NextItemLimit = textItem.Start + textItem.Duration + TimeSpan.FromSeconds(0.5);


            if (IsReadOnly == false)
            {
                DragSelectedTimmer.Tick += DragSelectedTimmer_Tick;
            }
            else
            {
                ControlHandle.ContextMenu = null;

                RightHandle.MouseDown -= RightHandle_MouseDown;
                RightHandle.MouseMove -= RightHandle_MouseMove;
                RightHandle.MouseUp -= RightHandle_MouseUp;

                LeftHandle.MouseDown -= LeftHandle_MouseDown;
                LeftHandle.MouseMove -= LeftHandle_MouseMove;
                LeftHandle.MouseUp -= LeftHandle_MouseUp;

                ControlHandle.MouseUp -= ControlHandle_MouseUp;
                ControlHandle.MouseMove -= ControlHandle_MouseMove;
                ControlHandle.MouseDown -= ControlHandle_MouseDown;

                TextTxt.IsEnabled = false;
            }

            UpdateText();
        }

        internal void ClearResources()
        {
            ControlHandle.ContextMenu = null;

            RightHandle.MouseDown -= RightHandle_MouseDown;
            RightHandle.MouseMove -= RightHandle_MouseMove;
            RightHandle.MouseUp -= RightHandle_MouseUp;

            LeftHandle.MouseDown -= LeftHandle_MouseDown;
            LeftHandle.MouseMove -= LeftHandle_MouseMove;
            LeftHandle.MouseUp -= LeftHandle_MouseUp;

            ControlHandle.MouseUp -= ControlHandle_MouseUp;
            ControlHandle.MouseMove -= ControlHandle_MouseMove;
            ControlHandle.MouseDown -= ControlHandle_MouseDown;

            TextItem = null;

            ControlHandle.MouseEnter -= ControlHandle_MouseEnter;
            ControlHandle.MouseLeave -= ControlHandle_MouseLeave;
        }

        internal void SetTimeLine(TimeLine timeLine)
        {
            _TimeLine = timeLine;
        }

        private void UpdateText()
        {
            TextTxt.TextChanged -= TextTxt_TextChanged;
            TextTxt.Text = Text;
            TextTxt.TextChanged += TextTxt_TextChanged;
        }

        bool TextChanged = false;
        private void TextTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextItem.Text = TextTxt.Text;
            TextChanged = true;
        }

        private void TextTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextChanged == true)
            {
                NoteChangedEvent();
                TextChanged = false;
            }
        }

        bool CapturedMouse = false;
        Point Offset = new Point(0, 0);



        #region Move Control


        public TimeSpan GetMinimum()
        {
            if (VideEventMinLimit > PreviousItemLimit)
            {
                return VideEventMinLimit;
            }
            else
            {
                return PreviousItemLimit;
            }
        }
        public TimeSpan GetMaximum()
        {
            if (VideEventMaxLimit < NextItemLimit)
            {
                return VideEventMaxLimit;
            }
            else
            {
                return NextItemLimit;
            }
        }

        private void DragSelectedTimmer_Tick(object sender, EventArgs e)
        {
            if (CapturedMouse == true)
            {
                IsDragging = true;
            }

            DragSelectedTimmer.Stop();
        }

        internal void NoteItemSelected()
        {
            IsSelected = true;
            Keyboard.ClearFocus();
            AnimateIn();
        }

        internal void NoteItemUnSelected()
        {
            if (IsSelected == true)
            {
                IsSelected = false;
                Keyboard.ClearFocus();
                AnimateOut();
            }
        }

        private void ControlHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == false)
            {
                IsSelected = true;
                IsDragging = false;
                CapturedMouse = true;
                Offset = Mouse.GetPosition(this);
                ControlHandle.CaptureMouse();
                DragSelectedTimmer.Start();
            }
        }

        private void ControlHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == true)
            {
                CapturedMouse = false;
                ControlHandle.ReleaseMouseCapture();
                if (_TimeLine != null)
                {
                    TextItem.Start = _TimeLine.GetTimeSpanByLocation(this.Margin.Left) - VideEventMinLimit;
                }
                if (IsDragging == true)
                {
                    IsDragging = false;
                    NoteChangedEvent();
                }

                NoteItemSelected();
                NoteSelectedEvent();

            }

            DragSelectedTimmer.Stop();
        }

        private void ControlHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (CapturedMouse == true)
            {
                Canvas Parent = this.Parent as Canvas;
                Point NewMouseLocation = Mouse.GetPosition(this);
                if (IsDragging == false)
                {
                    if (Math.Abs(NewMouseLocation.X - Offset.X) > 5)
                    {
                        DragSelectedTimmer.Stop();
                        IsDragging = true;
                    }

                    if (Math.Abs(NewMouseLocation.Y - Offset.Y) > 5)
                    {
                        DragSelectedTimmer.Stop();
                        IsDragging = true;
                    }
                }

                if (IsDragging == true)
                {
                    double top = this.Margin.Top;
                    Point MouseLocation = Mouse.GetPosition(Parent);


                    double NewLocation = MouseLocation.X - Offset.X;
                    if (_TimeLine != null)
                    {
                        TimeSpan MinLimit = GetMinimum();
                        TimeSpan MaxLimit = GetMaximum();

                        TimeSpan StartLocationTime = _TimeLine.GetTimeSpanByLocation(NewLocation);
                        TimeSpan EndLocationTime = _TimeLine.GetTimeSpanByLocation(NewLocation + this.ActualWidth);
                        if (StartLocationTime > MinLimit && EndLocationTime < VideEventMinLimit + MaxLimit)
                        {
                            this.Margin = new Thickness(NewLocation, top, 0, 0);
                        }
                        else
                        {
                            if (StartLocationTime < MinLimit)
                            {
                                double left = _TimeLine.GetLocationByTimeSpan(MinLimit);
                                this.Margin = new Thickness(left, top, 0, 0);
                            }

                            if (EndLocationTime > VideEventMinLimit + MaxLimit)
                            {
                                double left = _TimeLine.GetLocationByTimeSpan(VideEventMinLimit + MaxLimit - TextItem.Duration);
                                this.Margin = new Thickness(left, top, 0, 0);
                            }
                        }
                    }
                    else
                    {
                        this.Margin = new Thickness(NewLocation, top, 0, 0);
                    }

                }
            }
        }
        #endregion

        #region Scale LeftHandle
        private void LeftHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == false)
            {
                CapturedMouse = true;
                Offset = Mouse.GetPosition(this);
                LeftHandle.CaptureMouse();
            }
        }

        private void LeftHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == true)
            {
                CapturedMouse = false;
                LeftHandle.ReleaseMouseCapture();
                if (_TimeLine != null)
                {
                    TextItem.Start = _TimeLine.GetTimeSpanByLocation(this.Margin.Left) - VideEventMinLimit;
                    TextItem.Duration = _TimeLine.GetTimeSpanByLocation(this.Margin.Left + this.Width) - TextItem.Start - VideEventMinLimit;
                }
                NoteChangedEvent();
            }

        }

        private void LeftHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (CapturedMouse == true)
            {
                double top = this.Margin.Top;
                Canvas Parent = this.Parent as Canvas;
                Point MouseLocation = Mouse.GetPosition(Parent);
                double Left = this.Margin.Left;

                double NewLocation = MouseLocation.X - Offset.X;
                if (_TimeLine != null)
                {
                    TimeSpan StartLocationTime = _TimeLine.GetTimeSpanByLocation(NewLocation);

                    TimeSpan MinLimit = GetMinimum();
                    TimeSpan MaxLimit = GetMaximum();

                    if (StartLocationTime > MinLimit )
                    {

                        double width = this.Width + (Left - this.Margin.Left);
                        if (NewLocation < this.Margin.Left + this.Width - 10)
                        {
                            this.Margin = new Thickness(NewLocation, top, 0, 0);
                            this.Width = this.Width + (Left - this.Margin.Left);

                        }
                    }
                    else
                    {
                        if (StartLocationTime < MinLimit)
                        {
                            double left = _TimeLine.GetLocationByTimeSpan(MinLimit);
                            this.Margin = new Thickness(left, top, 0, 0);
                        }
                    }
                }
                else
                {
                    this.Margin = new Thickness(NewLocation, top, 0, 0);
                    double width = this.Width + (Left - this.Margin.Left);
                    if (width > 5)
                    {
                        this.Width = this.Width + (Left - this.Margin.Left);
                    }
                }
            }
        }

        #endregion

        #region Scale RightHandle
        private void RightHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == false)
            {
                CapturedMouse = true;
                Offset = new Point(this.Width - Mouse.GetPosition(this).X, 0);
                RightHandle.CaptureMouse();
            }
        }

        private void RightHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CapturedMouse == true)
            {
                CapturedMouse = false;
                RightHandle.ReleaseMouseCapture();
                if (_TimeLine != null)
                {
                    TextItem.Duration = _TimeLine.GetTimeSpanByLocation(this.Margin.Left + this.Width) - TextItem.Start - VideEventMinLimit;
                }
                NoteChangedEvent();
            }
        }

        private void RightHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (CapturedMouse == true)
            {
                double top = this.Margin.Top;
                Canvas Parent = this.Parent as Canvas;
                Point MouseLocation = Mouse.GetPosition(Parent);


                double NewWidth = MouseLocation.X - this.Margin.Left + Offset.X;

                if (NewWidth < 10)
                {
                    NewWidth = 10;
                }

                if (_TimeLine != null)
                {
                    TimeSpan MaxLimit = GetMaximum();

                    TimeSpan EndLocationTime = _TimeLine.GetTimeSpanByLocation(this.Margin.Left + NewWidth);
                    if (EndLocationTime < VideEventMinLimit + MaxLimit)
                    {
                        this.Width = NewWidth;
                    }
                    else
                    {
                        if (EndLocationTime > VideEventMinLimit + MaxLimit)
                        {
                            double width = _TimeLine.GetLocationByTimeSpan(VideEventMinLimit + MaxLimit) - this.Margin.Left;
                            this.Width = width;
                        }
                    }
                }
                else
                {
                    this.Width = NewWidth;
                }
            }
        }
        #endregion
        private void ControlHandle_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsSelected == false)
            {
                AnimateIn();
            }
        }

        private void ControlHandle_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsSelected == false)
            {
                AnimateOut();
            }
        }

        private void AnimateIn()
        {
            ColorAnimation colorChangeAnimation = new ColorAnimation();
            colorChangeAnimation.From = (Color)((SolidColorBrush)ControlHandle.Background).Color;
            colorChangeAnimation.To = (Color)ColorConverter.ConvertFromString("#FF14B8CE");
            colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
            Storyboard BackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation, ControlHandle);
            Storyboard.SetTargetProperty(colorChangeAnimation, colorTargetPath);
            BackgroundChangeStory.Children.Add(colorChangeAnimation);
            BackgroundChangeStory.Begin();
        }

        private void AnimateOut()
        {
            ColorAnimation colorChangeAnimation = new ColorAnimation();
            colorChangeAnimation.From = (Color)((SolidColorBrush)ControlHandle.Background).Color;
            colorChangeAnimation.To = (Color)ColorConverter.ConvertFromString("#FF0C90A2");
            colorChangeAnimation.Duration = TimeSpan.FromMilliseconds(200);

            PropertyPath colorTargetPath = new PropertyPath("(Border.Background).(SolidColorBrush.Color)");
            Storyboard BackgroundChangeStory = new Storyboard();
            Storyboard.SetTarget(colorChangeAnimation, ControlHandle);
            Storyboard.SetTargetProperty(colorChangeAnimation, colorTargetPath);
            BackgroundChangeStory.Children.Add(colorChangeAnimation);
            BackgroundChangeStory.Begin();
        }



        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            NoteDeleteClickedEvent();
        }

        private void RecalculateBtn_Click(object sender, RoutedEventArgs e)
        {
            RecalculateClickedEvent();
        }

        public event EventHandler NoteChanged;
        public void NoteChangedEvent()
        {
            if (NoteChanged != null)
            {
                NoteChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler NoteSelected;
        public void NoteSelectedEvent()
        {
            if (NoteSelected != null)
            {
                NoteSelected(this, EventArgs.Empty);
            }
        }

        public event EventHandler NoteUnSelected;
        public void NoteUnSelectedEvent()
        {
            if (NoteUnSelected != null)
            {
                NoteUnSelected(this, EventArgs.Empty);
            }
        }

        public event EventHandler NoteDeleteClicked;
        public void NoteDeleteClickedEvent()
        {
            if (NoteDeleteClicked != null)
            {
                NoteDeleteClicked(this, EventArgs.Empty);
            }
        }

        public event EventHandler RecalculateClicked;
        public void RecalculateClickedEvent()
        {
            if (RecalculateClicked != null)
            {
                RecalculateClicked(this, EventArgs.Empty);
            }
        }


    }
}
