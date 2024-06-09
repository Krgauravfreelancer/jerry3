using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace DesignerNp.Adorners
{
    internal class TextBoxAdorners : Adorner
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

        public TextBoxAdorners(TextBox adornedElement) : base(adornedElement)
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
            double width = AdornedElement.RenderSize.Width;
            double height = AdornedElement.RenderSize.Height;

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
            return;
            /*
            uIElement = AdornedElement;
            container = VisualTreeHelper.GetParent(uIElement) as Canvas;
            if (null == uIElement || null == container)
            {
                return;
            }

            TextBox textBox = (TextBox)AdornedElement;

            // Initial Condition
            if ("MatrixTransform" == textBox.LayoutTransform.GetType().Name)
            {
                textBox.LayoutTransform = new ScaleTransform(1, 1);
            }

            ScaleTransform scaleTransform = (ScaleTransform)textBox.LayoutTransform;

            double oldWidth = textBox.RenderSize.Width;
            double oldHeight = textBox.RenderSize.Height;

            double newWwidth = oldWidth + e.HorizontalChange / scaleTransform.ScaleX;
            double newhHeight = oldHeight + e.VerticalChange / scaleTransform.ScaleY;

            if (newWwidth > 0 && newhHeight > 0)
            {
                scaleTransform.ScaleX = newWwidth * scaleTransform.ScaleX / oldWidth;
                scaleTransform.ScaleY = newhHeight * scaleTransform.ScaleY / oldHeight;
                textBox.LayoutTransform = scaleTransform;
            }
            */
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
            TextBox textBox = (TextBox)AdornedElement;
            double scaleX = 1;
            double scaleY = 1;
            if ("ScaleTransform" == textBox.LayoutTransform.GetType().Name)
            {
                ScaleTransform scaleTransform = (ScaleTransform)textBox.LayoutTransform;
                scaleX = scaleTransform.ScaleX;
                scaleY = scaleTransform.ScaleY;
            }

            rotate.Arrange(new Rect(AdornedElement.RenderSize.Width / 2 - (5 / scaleX), -5 / scaleY, 10, 10));

            resize.Arrange(new Rect(AdornedElement.RenderSize.Width - 5, AdornedElement.RenderSize.Height - 5, 10, 10));

            return base.ArrangeOverride(finalSize);
        }


        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            TextBox textBox = (TextBox)AdornedElement;

            if ("ScaleTransform" == textBox.LayoutTransform.GetType().Name)
            {
                ScaleTransform scaleTransform = (ScaleTransform)textBox.LayoutTransform;

                double newScaleX = 1.0 / scaleTransform.ScaleX;
                double newScaleY = 1.0 / scaleTransform.ScaleY;

                if (newScaleX > 0 && newScaleY > 0)
                {
                    rotate.RenderTransform = new ScaleTransform(1.0 / scaleTransform.ScaleX, 1.0 / scaleTransform.ScaleY);
                    resize.RenderTransform = new ScaleTransform(1.0 / scaleTransform.ScaleX, 1.0 / scaleTransform.ScaleY);
                }
            }

            return base.GetDesiredTransform(transform);
        }

    }
}
