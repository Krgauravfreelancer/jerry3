using ScreenRecording_UserControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Controls;
using Rectangle = System.Drawing.Rectangle;

namespace ScreenRecording_UserControl
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public Rectangle CaptureRectangle { get; private set; }
        public Point EndPoint { get; private set; }
        public System.Windows.Shapes.Rectangle DrawnFrame {get;set;}

        private Point startPoint;
        private bool isDrawing;

        public OverlayWindow()
        {
            InitializeComponent();

            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            isDrawing = false;
            CanvasFrame.Background = Brushes.LightGray;
            CanvasFrame.HorizontalAlignment = HorizontalAlignment.Stretch; 
            CanvasFrame.VerticalAlignment = VerticalAlignment.Stretch;
            CanvasFrame.Opacity = 0.2;

            // Attach event handlers programmatically
            CanvasFrame.MouseDown += Canvas_MouseDown;
            CanvasFrame.MouseMove += Canvas_MouseMove;
            CanvasFrame.MouseUp += Canvas_MouseUp;
            CanvasFrame.Focusable= true;
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                DialogResult = true; 
                CanvasFrame.Children.Clear();
                Close();
            }
            e.Handled = true;
            UIElement el = (UIElement)sender;
            el.ReleaseMouseCapture();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                var currentPosition = e.GetPosition(CanvasFrame);
                DrawFrame(startPoint, currentPosition);
                EndPoint = currentPosition;
            }
            e.Handled = true;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            startPoint = e.GetPosition(CanvasFrame);
            e.Handled = true;
            UIElement el = (UIElement)sender;
            el.CaptureMouse();
        }
        private void DrawFrame(Point startPoint, Point endPoint)
        {
            // Clear previous drawings
            CanvasFrame.Children.Clear();

            // Calculate the rectangle dimensions
            double x = Math.Min(startPoint.X, endPoint.X);
            double y = Math.Min(startPoint.Y, endPoint.Y);
            double width = Math.Abs(startPoint.X - endPoint.X);
            double height = Math.Abs(startPoint.Y - endPoint.Y);

            // Draw the frame rectangle
            var frameRect = new System.Windows.Shapes.Rectangle()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0)),
                Width = width,
                Height = height,
            };
            CanvasFrame.Children.Add(frameRect);
            Canvas.SetLeft(frameRect,x);
            Canvas.SetTop(frameRect,y);
            DrawnFrame = frameRect;

            // Update the capture rectangle variable
            CaptureRectangle = new Rectangle((int)x, (int)y, (int)width, (int)height);
            CanvasFrame.Focus();
        }

        void CloseClick(object Sender, RoutedEventArgs E)
        {
            isDrawing = false;
            DialogResult = false;
            Close();
        }
    }
}
