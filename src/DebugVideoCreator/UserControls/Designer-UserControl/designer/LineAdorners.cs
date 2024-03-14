using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignerNp.controls
{
    internal class LineAdorners : Adorner
    {
        VisualCollection visuals;

        Thumb start;
        Thumb end;

        public LineAdorners(UIElement adornedElement) : base(adornedElement)
        {
            visuals = new VisualCollection(this);

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
        }

        private void ArrowEnd_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Line line = (Line)AdornedElement;

            if (null == line) return;

            double newX2 = line.X2 + e.HorizontalChange;
            double newY2 = line.Y2 + e.VerticalChange;

            if (newX2 > 0) line.X2 = newX2;

            if (newY2 > 0) line.Y2 = newY2;
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

            end.Arrange(new Rect(line.X2 - 5, line.Y2 - 5, 10, 10));

            return base.ArrangeOverride(finalSize);
        }
    }
}
