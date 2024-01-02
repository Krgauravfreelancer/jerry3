using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignerNp.controls
{
    internal class ResizeRotateAdorners : Adorner
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

        Thumb resize;

        public ResizeRotateAdorners(UIElement adornedElement) : base(adornedElement)
        {
            visuals = new VisualCollection(this);

            rotate = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
            rotate.DragDelta += Rotate_DragDelta;
            rotate.DragStarted += Rotate_DragStarted;
            visuals.Add(rotate);

            resize = new Thumb() { Background = Brushes.LightGray, Height = 10, Width = 10 };
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


        private void Resize_DragDelta(object sender, DragDeltaEventArgs e)
        {
            uIElement = AdornedElement;
            container = VisualTreeHelper.GetParent(uIElement) as Canvas;
            if (null == uIElement || null == container)
            {
                return;
            }

            double newhHeight = (double)uIElement.GetValue(HeightProperty) + e.VerticalChange;
            double newWwidth = (double)uIElement.GetValue(WidthProperty) + e.HorizontalChange;


            if (newhHeight > 0)
            {
                uIElement.SetValue(HeightProperty, newhHeight);
            }

            if (newWwidth > 0)
            {
                uIElement.SetValue(WidthProperty, newWwidth);
            }
        }

        private void ArrowEnd_DragDelta(object sender, DragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ArrowStart_DragDelta(object sender, DragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Size ArrangeOverride(Size finalSize)
        {
            rotate.Arrange(new Rect(AdornedElement.DesiredSize.Width / 2 - 5, -30, 10, 10));

            resize.Arrange(new Rect(AdornedElement.DesiredSize.Width - 5, AdornedElement.DesiredSize.Height - 5, 10, 10));

            return base.ArrangeOverride(finalSize);
        }
    }
}
