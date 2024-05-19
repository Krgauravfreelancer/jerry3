using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignerNp.controls
{
    internal class ChatBubbleAdorners : Adorner
    {
        VisualCollection visuals;

        Thumb rotate;
        private double initialAngle;
        private RotateTransform rotateTransform;
        private Vector startVector;
        private Point centerPoint;
        //private ContentControl designerItem;
        private UIElement uIElement;
        private Canvas container;

        Thumb bubblePoint;
        Thumb resize;

        public ChatBubbleAdorners(UIElement adornedElement) : base(adornedElement)
        {
            visuals = new VisualCollection(this);

            rotate = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            rotate.DragDelta += Rotate_DragDelta;
            rotate.DragStarted += Rotate_DragStarted;
            visuals.Add(rotate);

            bubblePoint = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            bubblePoint.DragDelta += BubblePoint_DragDelta;
            visuals.Add(bubblePoint);

            resize = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            resize.DragStarted += Resize_DragStarted;
            resize.DragDelta += Resize_DragDelta;
            visuals.Add(resize);
        }


        private void Rotate_DragStarted(object sender, DragStartedEventArgs e)
        {
            uIElement = AdornedElement;
            if (uIElement == null)
            {
                return;
            }

            uIElement.RenderTransformOrigin = new Point(0.5, 0.5);
            double width = AdornedElement.DesiredSize.Width;
            double height = AdornedElement.DesiredSize.Height;

            container = VisualTreeHelper.GetParent(uIElement) as Canvas;

            if (container != null)
            {
                centerPoint = uIElement.TranslatePoint(
                    new Point(width * uIElement.RenderTransformOrigin.X / 2,
                              height * uIElement.RenderTransformOrigin.Y / 2),
                              container);

                Point startPoint = Mouse.GetPosition(container);
                startVector = Point.Subtract(startPoint, centerPoint);

                rotateTransform = uIElement.RenderTransform as RotateTransform;
                if (rotateTransform == null)
                {
                    uIElement.RenderTransform = new RotateTransform(0);
                    initialAngle = 0;
                }
                else
                {
                    initialAngle = rotateTransform.Angle;
                }
            }

        }

        private void Rotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (uIElement != null && container != null)
            {
                Point currentPoint = Mouse.GetPosition(container);
                Vector deltaVector = Point.Subtract(currentPoint, centerPoint);

                double angle = Vector.AngleBetween(startVector, deltaVector);

                RotateTransform rotateTransform = uIElement.RenderTransform as RotateTransform;
                rotateTransform.Angle = this.initialAngle + Math.Round(angle, 0);
                uIElement.InvalidateMeasure();
            }
        }

        private void BubblePoint_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Path bubble = (Path)AdornedElement;

            StreamGeometry pathGeometry = bubble.Data as StreamGeometry;
            string data = pathGeometry.ToString();

            char[] seperators = new char[] { 'M', 'A', 'L', 'z' };
            string[] toks = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            string[] pointTok = toks[2].Split(',');

            double X, Y;
            double.TryParse(pointTok[0], out X);
            double.TryParse(pointTok[1], out Y);

            X += e.HorizontalChange;
            Y += e.VerticalChange;

            string newData = $"M{toks[0]} A{toks[1]} L{X},{Y} z";
            bubble.Data = Geometry.Parse(newData);
        }

        private void Resize_DragStarted(object sender, DragStartedEventArgs e)
        {

        }

        private void Resize_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Path bubble = (Path)AdornedElement;

            StreamGeometry pathGeometry = bubble.Data as StreamGeometry;
            string data = pathGeometry.ToString();

            char[] seperators = new char[] { 'M', 'A', 'L', 'z', ' ', ',' };
            string[] toks = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            double moveX, moveY; // Also start point
            double sizeX, sizeY;
            double endX, endY;
            double lineX, lineY;

            double.TryParse(toks[0], out moveX);
            double.TryParse(toks[1], out moveY);
            double.TryParse(toks[2], out sizeX);
            double.TryParse(toks[3], out sizeY);
            double.TryParse(toks[7], out endX);
            double.TryParse(toks[8], out endY);
            double.TryParse(toks[9], out lineX);
            double.TryParse(toks[10], out lineY);

            moveX += e.HorizontalChange;
            moveY += e.VerticalChange;
            sizeX += e.HorizontalChange;
            sizeY += e.VerticalChange;
            endX += e.HorizontalChange;
            endY += e.VerticalChange;
            lineX += e.HorizontalChange;
            lineY += e.VerticalChange;

            string newData = $"M{moveX},{moveY} A{sizeX},{sizeY} {toks[4]} {toks[5]} {toks[6]} {endX},{endY} L{lineX},{lineY} z";
            bubble.Data = Geometry.Parse(newData);
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Size ArrangeOverride(Size finalSize)
        {
            Path bubble = (Path)AdornedElement;

            StreamGeometry pathGeometry = bubble.Data as StreamGeometry;
            string data = pathGeometry.ToString();

            char[] seperators = new char[] { 'M', 'A', 'L', 'z' };
            string[] toks = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            string[] pointTok = toks[2].Split(',');

            double X, Y;
            double.TryParse(pointTok[0], out X);
            double.TryParse(pointTok[1], out Y);

            bubblePoint.Arrange(new Rect(X - 5, Y - 5, 10, 10));


            // Based on arc end point 
            char[] arcSeperators = new char[] { ' ', ',' };
            string[] arcParts = toks[1].Split(arcSeperators, StringSplitOptions.RemoveEmptyEntries);

            double.TryParse(arcParts[5], out X);
            double.TryParse(arcParts[6], out Y);

            resize.Arrange(new Rect(X + 5, Y + 5, 10, 10));

            rotate.Arrange(new Rect(AdornedElement.DesiredSize.Width / 2 - 5, -30, 10, 10));



            return base.ArrangeOverride(finalSize);
        }
    }
}
