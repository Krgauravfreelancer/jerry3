using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignerNp.Adorners
{
    internal class ArrowAdorners : Adorner
    {
        VisualCollection visuals;

        Line headElement;
        Thumb start;
        Thumb end;


        public ArrowAdorners(UIElement adornedElement, UIElement headElement) : base(adornedElement)
        {
            visuals = new VisualCollection(this);

            this.headElement = (Line)headElement;

            start = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            start.DragDelta += ArrowStart_DragDelta;
            visuals.Add(start);

            end = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            end.DragDelta += ArrowEnd_DragDelta; ;
            visuals.Add(end);
        }

        private void ArrowStart_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Line line = (Line)AdornedElement;

            if (null == line) return;

            double newX1 = line.X1 + e.HorizontalChange;
            double newY1 = line.Y1 + e.VerticalChange;

            if (newX1 > 0) line.X1 = newX1;
            if (newY1 > 0) line.Y1 = newY1;

            moveHead(line);
        }

        private void ArrowEnd_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Line line = (Line)AdornedElement;

            if (null == line) return;

            double newX2 = line.X2 + e.HorizontalChange;
            double newY2 = line.Y2 + e.VerticalChange;

            if (newX2 > 0) line.X2 = newX2;
            if (newY2 > 0) line.Y2 = newY2;

            moveHead(line);
        }

        private void moveHead(Line line)
        {
            // Calculate lenght of the line using pythagorean theorem
            double length = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2));

            // Find 1 'Unit' of lenght on a line in each direction 
            double dx = (line.X2 - line.X1) / length;
            double dy = (line.Y2 - line.Y1) / length;

            // End of the 'line' is the start of the 'head'
            headElement.X1 = line.X2;
            headElement.Y1 = line.Y2;

            // Make 'head' as a line of 1 unit to give the arrow a direction.
            headElement.X2 = headElement.X1 + dx;
            headElement.Y2 = headElement.Y1 + dy;
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Size ArrangeOverride(Size finalSize)
        {
            Line line = (Line)AdornedElement;
            start.Arrange(new Rect(line.X1 - 5, line.Y1 - 5, 10, 10));


            // Calculate lenght of the line using pythagorean theorem
            double length = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2));

            // Find 1 'Unit' of lenght on a line in each direction 
            double dx = (line.X2 - line.X1) / length;
            double dy = (line.Y2 - line.Y1) / length;
            double X3 = headElement.X1 + 25 * dx;
            double Y3 = headElement.Y1 + 25 * dy;


            end.Arrange(new Rect(X3 - 5, Y3 - 5, 10, 10));

            return base.ArrangeOverride(finalSize);
        }
    }
}
