using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;

namespace DesignerNp.controls
{
    /// <summary>
    /// Interaction logic for PropertyWindow.xaml
    /// </summary>
    public partial class PropertyWindow : UserControl
    {
        private UIElement _uiElement;
        private TextBox _textBox;
        private bool IsPinned;
        private bool isPressed;
        private Point startPosition;
        private double userWidth; // width set by user

        public PropertyWindow()
        {
            InitializeComponent();

            IsPinned = false;
            isPressed = false;
            startPosition = Mouse.GetPosition(Application.Current.MainWindow);

            colorPicker.IsEnabled = false;
            cbHidden.IsEnabled = false;
            txtPositionX.IsEnabled = false;
            txtPositionY.IsEnabled = false;
            txtSizeX.IsEnabled = false;
            txtSizeY.IsEnabled = false;
            txtRotation.IsEnabled = false;
            cbMultiline.IsEnabled = false;
            txtText.IsEnabled = false;
            txtFontSize.IsEnabled = false;
            txtStartX.IsEnabled = false;
            txtStartY.IsEnabled = false;
            txtEndX.IsEnabled = false;
            txtEndY.IsEnabled = false;

            userWidth = 200;
            this.Width = 30;
            this.HorizontalAlignment = HorizontalAlignment.Right;

            Application.Current.MainWindow.MouseMove += MainWindow_MouseMove;
        }

        public TextBox textBox
        {
            get { return _textBox; }
            set 
            { 
                _textBox = value;
                updateProperties();
            }
        }
        public UIElement uiElement
        {
            get { return _uiElement; }
            set
            {
                _uiElement = value;
                updateProperties();
            }
        }

        private void updateProperties()
        {
            if (null == _uiElement && null == _textBox)
            {
                colorPicker.IsEnabled = false;
                colorPicker.SelectedColor = null;

                cbHidden.IsEnabled = false;
                cbHidden.IsChecked = false;

                txtPositionX.IsEnabled = false;
                txtPositionX.Text = "";

                txtPositionY.IsEnabled = false;
                txtPositionY.Text = "";

                txtSizeX.IsEnabled = false;
                txtSizeX.Text = "";

                txtSizeY.IsEnabled = false;
                txtSizeY.Text = "";

                txtRotation.IsEnabled = false;
                txtRotation.Text = "";

                cbMultiline.IsEnabled = false;
                cbMultiline.IsChecked = false;

                txtText.IsEnabled = false;
                txtText.Text = "";

                txtFontSize.IsEnabled = false;
                txtFontSize.Text = "";

                txtStartX.IsEnabled = false;
                txtStartX.Text = "";

                txtStartY.IsEnabled = false;
                txtStartY.Text = "";

                txtEndX.IsEnabled = false;
                txtEndX.Text = "";

                txtEndY.IsEnabled = false;
                txtEndY.Text = "";


            }
            else if (null == _uiElement && null != _textBox)
            {
                txtText.IsEnabled = true;
                txtText.Text = _textBox.Text;

                txtFontSize.IsEnabled = true;
                txtFontSize.Text = _textBox.FontSize.ToString();

                colorPicker.IsEnabled = true;
                Brush brush = _textBox.Foreground;
                SolidColorBrush solidColorBrush = (SolidColorBrush)brush;
                colorPicker.SelectedColor = solidColorBrush.Color;

                // Position X
                txtPositionX.IsEnabled = true;
                double left = (double)_textBox.GetValue(Canvas.LeftProperty);
                txtPositionX.Text = left.ToString();

                // Position Y
                txtPositionY.IsEnabled = true;
                double top = (double)_textBox.GetValue(Canvas.TopProperty);
                txtPositionY.Text = top.ToString();

                // Rotation 
                txtRotation.IsEnabled = true;
                RotateTransform rotateTransform = _textBox.RenderTransform as RotateTransform;
                if (rotateTransform == null)
                {
                    rotateTransform = new RotateTransform(0);
                    _textBox.RenderTransform = rotateTransform;
                }

                txtRotation.Text = rotateTransform.Angle.ToString();

                // Multi Line
                cbMultiline.IsEnabled = true;
                cbMultiline.IsChecked = _textBox.AcceptsReturn;
            }
            else
            {
                Type type = _uiElement.GetType();

                // Color
                colorPicker.IsEnabled = true;
                Brush brush = (Brush)_uiElement.GetValue(Shape.StrokeProperty);
                if (brush != null)
                {
                    SolidColorBrush solidColorBrush = (SolidColorBrush)brush;
                    colorPicker.SelectedColor = solidColorBrush.Color;
                }

                //Type type = _uiElement.GetType();
                switch (type.Name)
                {
                    case "Rectangle":
                    case "Ellipse":
                    case "Path":
                        // Position X
                        txtPositionX.IsEnabled = true;
                        double left = (double)_uiElement.GetValue(Canvas.LeftProperty);
                        txtPositionX.Text = left.ToString();

                        // Position Y
                        txtPositionY.IsEnabled = true;
                        double top = (double)_uiElement.GetValue(Canvas.TopProperty);
                        txtPositionY.Text = top.ToString();

                        // Rotation 
                        txtRotation.IsEnabled = true;
                        RotateTransform rotateTransform = _uiElement.RenderTransform as RotateTransform;
                        if (rotateTransform == null)
                        {
                            rotateTransform = new RotateTransform(0);
                            _uiElement.RenderTransform = rotateTransform;
                        }

                        txtRotation.Text = rotateTransform.Angle.ToString();

                        if ("Path" == type.Name)
                        {
                            Path bubble = (Path)_uiElement;

                            StreamGeometry pathGeometry = bubble.Data as StreamGeometry;
                            string data = pathGeometry.ToString();

                            char[] seperators = new char[] { 'M', 'A', 'L', 'z', ' ', ',' };
                            string[] toks = data.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

                            double sizeX, sizeY;

                            double.TryParse(toks[2], out sizeX);
                            double.TryParse(toks[3], out sizeY);

                            // width
                            txtSizeX.IsEnabled = true;
                            txtSizeX.Text = sizeX.ToString();

                            // Height
                            txtSizeY.IsEnabled = true;
                            txtSizeY.Text = sizeY.ToString();
                        }
                        else
                        {
                            // width
                            txtSizeX.IsEnabled = true;
                            double width = (double)_uiElement.GetValue(WidthProperty);
                            txtSizeX.Text = width.ToString();

                            // Height
                            txtSizeY.IsEnabled = true;
                            double height = (double)_uiElement.GetValue(HeightProperty);
                            txtSizeY.Text = height.ToString();
                        }

                        break;

                    case "Line":
                        txtStartX.IsEnabled = true;
                        txtStartY.IsEnabled = true;
                        txtEndX.IsEnabled = true;
                        txtEndY.IsEnabled = true;

                        Line element = (Line)_uiElement;
                        if ((string)element.Tag == "Arrow")
                        {
                            Canvas canvas = VisualTreeHelper.GetParent(element) as Canvas;
                            Line line = (Line)canvas.Children[0];
                            Line head = (Line)canvas.Children[1];

                            txtStartX.Text = line.X1.ToString();
                            txtStartY.Text = line.Y1.ToString();

                            calculateDxDy(line, out double dx, out double dy);

                            txtEndX.Text = (head.X1 + 25 * dx).ToString();
                            txtEndY.Text = (head.Y1 + 25 * dy).ToString();
                        }
                        else
                        {
                            txtStartX.Text = element.X1.ToString();
                            txtStartY.Text = element.Y1.ToString();
                            txtEndX.Text = element.X2.ToString();
                            txtEndY.Text = element.Y2.ToString();
                        }
                        break;
                }
            }
        }

        private void txtPositionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;

            double positionX;
            if (!double.TryParse(txtPositionX.Text, out positionX)) return;

            if ( null != _textBox )
            {
                _textBox.SetValue(Canvas.LeftProperty, positionX);
            }
            else if (null != _uiElement)
            {
                _uiElement.SetValue(Canvas.LeftProperty, positionX);
            }

        }

        private void txtPositionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;

            double positionY;
            if (!double.TryParse(txtPositionY.Text, out positionY)) return;
            if (null != _textBox)
            {
                _textBox.SetValue(Canvas.TopProperty, positionY);
            }
            else if (null != _uiElement)
            {
                _uiElement.SetValue(Canvas.TopProperty, positionY);
            }
        }

        private void txtSizeX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double sizeX;
            if (!double.TryParse(txtSizeX.Text, out sizeX)) return;

            double sizeY;
            if (!double.TryParse(txtSizeY.Text, out sizeY)) return;

            Type type = _uiElement.GetType();
            if ("Path" == type.Name)
            {
                resizeChatBubble((Path)_uiElement, sizeX, sizeY);
            }
            else
            {
                _uiElement.SetValue(WidthProperty, sizeX);
            }
        }

        private void txtSizeY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double sizeX;
            if (!double.TryParse(txtSizeX.Text, out sizeX)) return;

            double sizeY;
            if (!double.TryParse(txtSizeY.Text, out sizeY)) return;

            Type type = _uiElement.GetType();
            if ("Path" == type.Name)
            {
                resizeChatBubble((Path)_uiElement, sizeX, sizeY);
            }
            else
            {


                _uiElement.SetValue(HeightProperty, sizeY);
            }
        }

        private void txtRotation_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;

            double rotation;
            if (!double.TryParse(txtRotation.Text, out rotation)) return;

            if (null != _textBox)
            {
                RotateTransform rotateTransform = _textBox.RenderTransform as RotateTransform;

                if (rotateTransform == null)
                {
                    rotateTransform = new RotateTransform(0);
                    _textBox.RenderTransform = rotateTransform;
                }

                rotateTransform.Angle = rotation;
            }
            else if (null != _uiElement)
            {
                RotateTransform rotateTransform = _uiElement.RenderTransform as RotateTransform;

                if (rotateTransform == null)
                {
                    rotateTransform = new RotateTransform(0);
                    _uiElement.RenderTransform = rotateTransform;
                }

                rotateTransform.Angle = rotation;
            }
        }

        private void txtText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _textBox) return;

            _textBox.Text = txtText.Text;
        }

        private void txtFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _textBox) return;
            double fontSize;
            if (double.TryParse(txtFontSize.Text, out fontSize) && fontSize > 0)
            {
                _textBox.FontSize = fontSize;
            }
        }

        private void txtStartX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double startX;
            if (!double.TryParse(txtStartX.Text, out startX)) return;

            Line element = (Line)_uiElement;
            if ((string)element.Tag == "Arrow")
            {
                // Calculate the point like adorners
                Canvas canvas = (Canvas)VisualTreeHelper.GetParent(element);
                Line line = (Line)canvas.Children[0];
                Line head = (Line)canvas.Children[1];
                line.X1 = startX;
                moveHead(line, head);
            }
            else
            {
                element.X1 = startX;
            }
        }

        private void txtStartY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double startY;
            if (!double.TryParse(txtStartY.Text, out startY)) return;

            Line element = (Line)_uiElement;
            if ((string)element.Tag == "Arrow")
            {
                Canvas canvas = (Canvas)VisualTreeHelper.GetParent(element);
                Line line = (Line)canvas.Children[0];
                Line head = (Line)canvas.Children[1];
                line.Y1 = startY;
                moveHead(line, head);
                // TODO Calculate the point like adorners
            }
            else
            {
                element.Y1 = startY;
            }
        }

        private void txtEndX_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double endX;
            if (!double.TryParse(txtEndX.Text, out endX)) return;

            Line element = (Line)_uiElement;
            if ((string)element.Tag == "Arrow")
            {
                Canvas canvas = (Canvas)VisualTreeHelper.GetParent(element);
                Line line = (Line)canvas.Children[0];
                Line head = (Line)canvas.Children[1];

                calculateDxDy(line, out double dx, out double dy);
                line.X2 = endX - 25 * dx;
                moveHead(line, head);
            }
            else
            {
                element.X2 = endX;
            }
        }

        private void txtEndY_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == _uiElement) return;

            double endY;
            if (!double.TryParse(txtEndY.Text, out endY)) return;

            Line element = (Line)_uiElement;
            if ((string)element.Tag == "Arrow")
            {
                Canvas canvas = (Canvas)VisualTreeHelper.GetParent(element);
                Line line = (Line)canvas.Children[0];
                Line head = (Line)canvas.Children[1];

                calculateDxDy(line, out double dx, out double dy);
                line.Y2 = endY - 25 * dy;
                moveHead(line, head);
            }
            else
            {
                element.Y2 = endY;
            }
        }

        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            //https://stackoverflow.com/questions/6808739/how-to-convert-color-code-into-media-brush
            
            if (null == _uiElement && null == _textBox) return;

            Color color = (Color)e.NewValue;

            var converter = new BrushConverter();
            Brush brush = (Brush)converter.ConvertFromString(color.ToString());

            if ( null != _textBox )
            {
                _textBox.Foreground = brush;
            }
            else if (null != _uiElement)
            {
                Type type = _uiElement.GetType();
                switch (type.Name)
                {
                    case "Rectangle":
                    case "Ellipse":
                    case "Path":
                        _uiElement.SetValue(Shape.StrokeProperty, brush);
                        break;

                    case "Line":
                        Line element = (Line)_uiElement;
                        if ((string)element.Tag == "Arrow")
                        {
                            Canvas canvas = (Canvas)VisualTreeHelper.GetParent(element);
                            Line line = (Line)canvas.Children[0];
                            Line head = (Line)canvas.Children[1];
                            line.Stroke = brush;
                            head.Stroke = brush;
                        }
                        else
                        {
                            _uiElement.SetValue(Shape.StrokeProperty, brush);
                        }
                        break;
                }
            }
        }

        private void moveHead(Line line, Line head)
        {
            calculateDxDy(line, out double dx, out double dy);

            // End of the 'line' is the start of the 'head'
            head.X1 = line.X2;
            head.Y1 = line.Y2;

            // Make 'head' as a line of 1 unit to give the arrow a direction.
            head.X2 = head.X1 + dx;
            head.Y2 = head.Y1 + dy;
        }

        private void calculateDxDy(Line line, out double dx, out double dy)
        {
            // Calculate lenght of the line using pythagorean theorem
            double length = Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2));

            // Find 1 'Unit' of lenght on a line in each direction 
            dx = (line.X2 - line.X1) / length;
            dy = (line.Y2 - line.Y1) / length;
        }


        private void resizeChatBubble(Path bubble, double newSizeX, double newSizeY)
        {
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

            double horizontalChange = newSizeX - sizeX;
            double verticalChange = newSizeY - sizeY;

            moveX += horizontalChange;
            moveY += verticalChange;
            sizeX += horizontalChange;
            sizeY += verticalChange;
            endX += horizontalChange;
            endY += verticalChange;
            lineX += horizontalChange;
            lineY += verticalChange;

            string newData = $"M{moveX},{moveY} A{sizeX},{sizeY} {toks[4]} {toks[5]} {toks[6]} {endX},{endY} L{lineX},{lineY} z";
            bubble.Data = Geometry.Parse(newData);

        }

        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            txt.SelectAll();
        }

        private void cbMultiline_Checked(object sender, RoutedEventArgs e)
        {
            if (null == _textBox) return;

            _textBox.AcceptsReturn = true;
            txtText.AcceptsReturn = true;
        }

        private void cbMultiline_Unchecked(object sender, RoutedEventArgs e)
        {
            if (null == _textBox) return;

            _textBox.AcceptsReturn = false;
            txtText.AcceptsReturn = false;
        }

        private void btnOpenProperties_Click(object sender, RoutedEventArgs e)
        {
            openSelf();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            closeSelf();
        }

    

        private void openSelf()
        {
            this.Width = userWidth;
            this.HorizontalAlignment = HorizontalAlignment.Right;
            btnOpenProperties.Visibility = Visibility.Hidden;

            var bc = new BrushConverter();
            verticalLine.Background = (Brush)bc.ConvertFrom("#0345bf");
            verticalLine.Width = 3;
            verticalLine.Cursor = Cursors.SizeWE;
        }

        private void closeSelf()
        {
            this.Width = 30;
            btnOpenProperties.Visibility = Visibility.Visible;

            var bc = new BrushConverter();
            verticalLine.Background = (Brush)bc.ConvertFrom("#e9edf2");
            verticalLine.Width = 20;
            verticalLine.Cursor = Cursors.Arrow;

            isPressed = false;
        }


        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (false == IsPinned)
            {
                openSelf();
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if ( false == IsPinned)
            { 
                closeSelf();
            }
        }

        private void btnPin_Checked(object sender, RoutedEventArgs e)
        {
            IsPinned = true;
            ToggleButton toggle = sender as ToggleButton;
        }

        private void btnPin_Unchecked(object sender, RoutedEventArgs e)
        {
            IsPinned=false;

            ToggleButton toggle = sender as ToggleButton;
        }

        private void verticalLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPressed = true;
            startPosition = Mouse.GetPosition(Application.Current.MainWindow);
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if(isPressed)
            {
                Point currentPosition = Mouse.GetPosition(Application.Current.MainWindow);
                
                double newWidth = this.Width + startPosition.X - currentPosition.X;
                if (newWidth > 0)
                {
                    this.Width = newWidth;
                    userWidth = newWidth;
                }
                startPosition = Mouse.GetPosition(Application.Current.MainWindow);
            }
        }

        private void verticalLine_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPressed = false;
        }

        private void leftAlign_Click(object sender, RoutedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;
            if(null != _uiElement)
                _uiElement.SetValue(Canvas.LeftProperty, 10.0);
            if (null != _textBox)
                _textBox.SetValue(Canvas.LeftProperty, 10.0);
            return;
        }

        private void rightAlign_Click(object sender, RoutedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;
            if (null != _uiElement)
            {
                var x = Convert.ToDouble(1920 - _uiElement.RenderSize.Width - 300);
                _uiElement.SetValue(Canvas.LeftProperty, x);
            }
            if (null != _textBox)
            {
                var x = Convert.ToDouble(1920 - _textBox.RenderSize.Width - 300);
                _textBox.SetValue(Canvas.LeftProperty, x);
            }
            return;
        }

        private void centerAlign_Click(object sender, RoutedEventArgs e)
        {
            if (null == _uiElement && null == _textBox) return;
            if (null != _uiElement)
            {
                var x = Convert.ToDouble(1920 - _uiElement.RenderSize.Width - 10)/2;
                _uiElement.SetValue(Canvas.LeftProperty, x);
            }
            if (null != _textBox)
            {
                var x = Convert.ToDouble(1920 - _textBox.RenderSize.Width - 10)/2;
                _textBox.SetValue(Canvas.LeftProperty, x);
            }
            return;
        }
    }
}
