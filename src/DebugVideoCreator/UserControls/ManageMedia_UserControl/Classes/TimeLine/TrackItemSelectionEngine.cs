using ManageMedia_UserControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ManageMedia_UserControl.Classes.TimeLine
{
    internal class TrackItemSelectionEngine
    {
        bool _MouseCaptured = false;
        Point _SelectionStartPoint = new Point(0, 0);
        Rectangle _SelectionRectangle = null;
        double _TimeScaleHeight = 10;

        internal void SelectNotesInSelectionArea(List<NoteItemControl> NoteItemControlList, TimeSpan selectionStart, TimeSpan selectionEnd)
        {
            //UnSelect All Notes
            for (int i = 0; i < NoteItemControlList.Count; i++)
            {
                NoteItemControl note = NoteItemControlList[i];
                note.NoteItemUnSelected();
            }

            //Select Notes In Section
            for (int i = 0; i < NoteItemControlList.Count; i++)
            {
                NoteItemControl note = NoteItemControlList[i];
                if (note.VideEventMinLimit + note.TextItem.Start > selectionStart && note.VideEventMinLimit + note.TextItem.Start + note.TextItem.Duration < selectionEnd)
                {
                    note.NoteItemSelected();
                }
            }
        }

        public Point MouseDown(Canvas MainCanvas)
        {
            //if (MainCanvas.IsMouseDirectlyOver)
            //{
                _SelectionStartPoint = Mouse.GetPosition(MainCanvas);
                _MouseCaptured = true;
                MainCanvas.CaptureMouse();

                _SelectionRectangle = new Rectangle()
                {
                    Stroke = Brushes.DodgerBlue,
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(100, 30, 160, 255)),
                };
            //}
            return _SelectionStartPoint;
        }

        internal void SetTrackbar(Point trackbarPosition, Canvas MainCanvas)
        {
            _SelectionStartPoint = trackbarPosition;
            _MouseCaptured = true;
            //MainCanvas.CaptureMouse();
            _SelectionRectangle = new Rectangle()
            {
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(100, 30, 160, 255)),
            };

        }

        internal (TimeSpan SelectionStart, TimeSpan SelectionEnd, bool WasMouseCaptured) MouseUp(Canvas MainCanvas, TimeSpan ViewPortStart, TimeSpan ViewPortDuration)
        {
            if (_MouseCaptured == true)
            {
                _MouseCaptured = false;
                MainCanvas.ReleaseMouseCapture();
                MainCanvas.Children.Remove(_SelectionRectangle);
                _SelectionRectangle = null;

                //Calculate Selection TimeSpans
                Point Location = Mouse.GetPosition(MainCanvas);
                double SelectionWidth = Location.X - _SelectionStartPoint.X;

                double StartLocation = 0;
                double EndLocation = 0;
                if (SelectionWidth > 0)
                {
                    StartLocation = _SelectionStartPoint.X;
                    EndLocation = StartLocation + SelectionWidth;
                }
                else
                {
                    StartLocation = _SelectionStartPoint.X + SelectionWidth;
                    EndLocation = StartLocation + (SelectionWidth * -1);
                }

                TimeSpan SelectionStart = TimeLineHelpers.GetTimeSpanByLocation(MainCanvas, ViewPortStart, ViewPortDuration, StartLocation);
                TimeSpan SelectionEnd = TimeLineHelpers.GetTimeSpanByLocation(MainCanvas, ViewPortStart, ViewPortDuration, EndLocation);

                return (SelectionStart, SelectionEnd, true);
            }

            return (TimeSpan.Zero, TimeSpan.Zero, false);
        }

        internal void MouseMoved(Canvas MainCanvas)
        {
            if (_MouseCaptured == true)
            {
                MainCanvas.Children.Remove(_SelectionRectangle);
            }

            if (_MouseCaptured == true)
            {

                if (_SelectionRectangle != null)
                {
                    double HeaderHeight = _TimeScaleHeight + 20;
                    double RemainingHeight = MainCanvas.ActualHeight - HeaderHeight;

                    double NoteTrackHeight = RemainingHeight / 5 * 1.5;

                    Point Location = Mouse.GetPosition(MainCanvas);
                    double SelectionWidth = Location.X - _SelectionStartPoint.X;
                    if (SelectionWidth > 0)
                    {
                        _SelectionRectangle.Margin = new Thickness(_SelectionStartPoint.X, HeaderHeight - 5, 0, 0);
                        _SelectionRectangle.Width = SelectionWidth;
                    }
                    else
                    {
                        _SelectionRectangle.Margin = new Thickness(_SelectionStartPoint.X + SelectionWidth, HeaderHeight - 5, 0, 0);
                        _SelectionRectangle.Width = SelectionWidth * -1;
                    }
                    _SelectionRectangle.Height = NoteTrackHeight + 10;
                    MainCanvas.Children.Add(_SelectionRectangle);
                }
            }
        }

        
    }
}
