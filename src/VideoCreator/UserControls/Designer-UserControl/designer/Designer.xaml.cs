﻿using DesignerNp.Adorners;
using Sqllite_Library.Models;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Xceed.Wpf.Toolkit.Zoombox;

namespace DesignerNp.controls
{
    /// <summary>
    /// Interaction logic for Designer.xaml
    /// </summary>
    public partial class Designer : UserControl
    {
        private double width;
        private double height;

        // Zoom
        private Double zoomMax = 5;
        private Double zoomMin = 0.2;
        private Double zoomSpeed = 0.001;
        private Double zoom = 0.7;


        /// <summary>
        /// Zoom Event zoom_event
        /// </summary>
        /// 
        public event EventHandler<Double> zoom_event;

        /// <summary>
        /// PropertyWindow propertyWindow
        /// </summary>
        /// 

        public PropertyWindow propertyWindow { get; set; }

        /// <summary>
        /// ShapeBar shapeBar
        /// </summary>
        public ShapeBar shapeBar { get; set; }

        bool isPressed;
        Point startPosition;
        private UIElement uiElement;
        private TextBox textBox;
        private AdornerLayer adornerLayer;
        private DataTable dataTable;

        /// <summary>
        /// Creates new designer
        /// </summary>
        public Designer()
        {
            InitializeComponent();

            isPressed = false;
            uiElement = null;
            textBox = null;

            dataTable = null;


            container.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            
            //if (scrollbar != null)
            //{
            //    //Console.WriteLine($"I am here - {System.Windows.Window.ActualWidth.ToString()}, {System.Windows.Window.ActualHeightProperty}");
            //    //scrollbar.Width = System.Windows.Window.ActualWidthProperty - 280;
            //    //scrollbar.Height = System.Windows.Window.ActualWidthProperty - 90;

            //    //scrollbar.Width = 1640;
            //    //scrollbar.Height = 990;
            //}
            this.SizeChanged += UserControl_SizeChanged;
        }

        /// <summary>
        /// Handles drop functionality
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
            Point p = e.GetPosition(container);
            AddDesign(dataString, p.X, p.Y);
        }

        /// <summary>
        /// Returns a design in the datatable with the following columns 
        /// Column 0: <br/>
        /// - Name: id:<br/>
        /// - Type: int<br/>
        /// Column 1: <br/>
        /// - Name: xaml<br/>
        /// - Type: string<br/>
        /// </summary>
        /// <returns>The design in xaml string</returns>
        public DataTable GetDesign_()
        {

            if (null == dataTable)
            {
                dataTable = InitDataTable();
            }

            for (int index = 0; index < container.Children.Count; index++)
            {
                UIElement uIElement = container.Children[index];
                string xaml = XamlWriter.Save(uIElement);

                if (index < dataTable.Rows.Count)
                {
                    // Update the exesting 
                    dataTable.Rows[index]["xaml"] = xaml;
                }
                else
                {
                    // Create new
                    DataRow dataRow = dataTable.NewRow();
                    dataRow["id"] = -1;
                    dataRow["xaml"] = xaml;
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Returns a design in the datatable with the following columns 
        /// Column 0: <br/>
        /// - Name: id:<br/>
        /// - Type: int<br/>
        /// Column 1: <br/>
        /// - Name: xaml<br/>
        /// - Type: string<br/>
        /// </summary>
        /// <returns>The design in xaml string</returns>
        public DataTable GetDesign()
        {
            if (null == dataTable)
            {
                dataTable = InitDataTable();
            }
            // Create new
            bool isAdd = false;
            if (container.Children.Count > 0)
            {
                DataRow dataRow = default(DataRow);
                if (dataTable.Rows.Count == 1)
                {
                    dataRow = dataTable.Rows[0];
                    isAdd = false;
                }
                else if (dataTable.Rows.Count == 0)
                {
                    dataRow = dataTable.NewRow();
                    dataRow["id"] = -1;
                    isAdd = true;
                }
                var xaml = string.Empty;
                for (int index = 0; index < container.Children.Count; index++)
                {
                    UIElement uIElement = container.Children[index];
                    var xamlString = XamlWriter.Save(uIElement);
                    if (index < container.Children.Count - 1)
                        xaml += xamlString + Environment.NewLine;
                    else
                        xaml += xamlString;
                }
                dataRow["xaml"] = xaml;
                if (isAdd)
                    dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        /// <summary>
        /// Pasing dataTable = null clears the design
        /// </summary>
        /// <param name="dataTable">
        /// Datatable with following column <br/>
        /// Column 0: <br/>
        /// - Name: id:<br/>
        /// - Type: int<br/>
        /// Column 1: <br/>
        /// - Name: xaml<br/>
        /// - Type: string<br/>
        /// </param>
        public void LoadDesign(DataTable dataTable)
        {
            this.dataTable = dataTable;
            if (container.Children.Count > 0)
                container.Children.Clear();

            if (null != dataTable && dataTable.Rows?.Count > 0)
            {
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    AddElement((string)dataRow["xaml"]);
                }
            }
        }

        /// <summary>
        /// Pasing cbvdesign param to load the design
        /// </summary>
        /// <param name="design">
        /// </param>
        public void LoadDesignForEdit(CBVDesign design)
        {
            if (null != design)
            {
                XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
                xmlDoc.LoadXml($"<root>{design.design_xml}</root>");
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    string text = node.OuterXml; //or loop through its children as well
                    if (text.ToLower().StartsWith("<image"))
                        continue;
                    AddElement(text);
                }
            }
        }

        /// <summary>
        /// Returns a datatable with following columns <br/>
        /// Column 0: <br/>
        /// - Name: id:<br/>
        /// - Type: int<br/>
        /// Column 1: <br/>
        /// - Name: xaml<br/>
        /// - Type: string<br/>
        /// </summary>
        /// <returns></returns>
        public DataTable InitDataTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Clear();
            dataTable.Columns.Add("id", typeof(int));
            dataTable.Columns.Add("xaml", typeof(string));

            return dataTable;
        }

        /// <summary>
        /// Add new element to canvas
        /// </summary>
        private void AddDesign(string shape, double positionX, double positionY)
        {
            string canvasStart = @"<Canvas 
                                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string canvasEnd = "</Canvas>";
            string wrapped = canvasStart + shape + canvasEnd;

            StringReader stringReader = new StringReader(wrapped);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Canvas canvas = (Canvas)XamlReader.Load(xmlReader);
            UIElement uIElement = canvas.Children[0];
            canvas.Children.RemoveAt(0);

            Type type = uIElement.GetType();
            switch (type.Name)
            {
                case "Rectangle":
                case "Ellipse":
                case "Path":
                case "TextBox":
                    Canvas.SetTop(uIElement, positionY);
                    Canvas.SetLeft(uIElement, positionX);
                    break;
                case "Line":
                    Line line = (Line)uIElement;
                    line.X1 = positionX;
                    line.Y1 = positionY;
                    line.X2 = positionX + 50;
                    line.Y2 = positionY + 50;
                    break;
                case "Canvas":
                    Canvas arrowCanvas = (Canvas)uIElement;
                    if ((string)arrowCanvas.Tag == "Arrow")
                    {
                        Line arrowLine = (Line)arrowCanvas.Children[0];
                        arrowLine.X1 = positionX;
                        arrowLine.Y1 = positionY;
                        arrowLine.X2 = positionX + 50;
                        arrowLine.Y2 = positionY;

                        Line arrowHead = (Line)arrowCanvas.Children[1];
                        arrowHead.X1 = arrowLine.X2;
                        arrowHead.Y1 = arrowLine.Y2;
                        arrowHead.X2 = arrowHead.X1 + 1;
                        arrowHead.Y2 = arrowHead.Y1;
                    }
                    break;
            }

            container.Children.Add(uIElement);
        }
        /// <summary>
        /// Add new element to canvas
        /// </summary>
        private void AddElement(string xaml)
        {
            string canvasStart = @"<Canvas
                                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>";
            string canvasEnd = "</Canvas>";
            string wrapped = canvasStart + xaml + canvasEnd;

            StringReader stringReader = new StringReader(wrapped);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            Canvas canvas = (Canvas)XamlReader.Load(xmlReader);
            if (canvas.Children.Count == 1)
            {
                UIElement element = canvas.Children[0];
                canvas.Children.RemoveAt(0);
                container.Children.Add(element);
            }
            else
            {
                while (canvas.Children.Count > 0)
                {
                    UIElement element = canvas.Children[0];
                    canvas.Children.RemoveAt(0);
                    container.Children.Add(element);
                }
            }
        }

        /// <summary>
        /// Drag drop > left button is down
        /// </summary>
        private void container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is Canvas))
            {
                UIElement uiElement = (UIElement)e.OriginalSource;

                Type type = uiElement.GetType();
                if (type.Name == "Line")
                {
                    Line line = (Line)uiElement;
                    if ((string)line.Tag == "Arrow")
                    {
                        Canvas arrowCanvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                        Line arrowLine = (Line)arrowCanvas.Children[0];
                        arrowLine.Opacity = 0.5;
                        Line arrowHead = (Line)arrowCanvas.Children[1];
                        arrowHead.Opacity = 0.5;
                    }
                }



                uiElement.Opacity = 0.5;

                isPressed = true;
                startPosition = e.GetPosition(container);
                uiElement.CaptureMouse();
            }
        }

        /// <summary>
        /// Drag drop > left button is up
        /// </summary>
        private void container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            container.Focus();
            if (!(e.OriginalSource is Canvas))
            {
                setSelection(uiElement, textBox, false);

                uiElement = (UIElement)e.OriginalSource;

                Type uiType = uiElement.GetType();
                if (uiType.Name == "Line")
                {
                    Line line = (Line)uiElement;
                    if ((string)line.Tag == "Arrow")
                    {
                        Canvas arrowCanvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                        Line arrowLine = (Line)arrowCanvas.Children[0];
                        arrowLine.Opacity = 1;
                        Line arrowHead = (Line)arrowCanvas.Children[1];
                        arrowHead.Opacity = 1;
                    }
                }

                uiElement.Opacity = 1;
                uiElement.ReleaseMouseCapture();
                isPressed = false;



                Type type = e.Source.GetType();
                if (type.Name == "TextBox")
                {
                    textBox = (TextBox)e.Source;
                    setSelection(uiElement, textBox, true);
                    propertyWindow.textBox = textBox;
                    propertyWindow.uiElement = null;
                }
                else
                {
                    setSelection(uiElement, null, true);
                    propertyWindow.textBox = null;
                    propertyWindow.uiElement = uiElement;
                }
            }
            else
            {
                setSelection(uiElement, textBox, false);
                uiElement = null;
                textBox = null;
                propertyWindow.uiElement = null;
                propertyWindow.textBox = null;
            }
        }

        /// <summary>
        /// Drag drop > Selected item 
        /// </summary>
        private void setSelection(UIElement uiElement, TextBox textBox, bool isSelected)
        {
            if (null == uiElement)
            {
                return;
            }

            if (isSelected)
            {
                Type type = uiElement.GetType();
                switch (type.Name)
                {
                    case "Rectangle":
                    case "Ellipse":
                        adornerLayer.Add(new ResizeRotateAdorners(uiElement));
                        break;

                    case "Line":
                        Line element = (Line)uiElement;
                        if ((string)element.Tag == "Arrow")
                        {
                            Canvas canvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                            adornerLayer.Add(new ArrowAdorners(canvas.Children[0], canvas.Children[1]));
                        }
                        else
                        {
                            adornerLayer.Add(new LineAdorners(uiElement));
                        }
                        break;

                    case "Path":
                        adornerLayer.Add(new ChatBubbleAdorners(uiElement));
                        break;
                    case "TextBoxView":
                        adornerLayer.Add(new TextBoxAdorners(textBox));
                        break;
                }
            }
            else
            {
                Type type = uiElement.GetType();
                switch (type.Name)
                {
                    case "Rectangle":
                    case "Ellipse":
                        DoubleCollection doubles = new DoubleCollection();
                        propertyWindow.uiElement.SetValue(Shape.StrokeDashArrayProperty, doubles);

                        propertyWindow.uiElement.SetValue(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)));

                        break;
                    case "Line":
                        Line line = (Line)uiElement;
                        if ((string)line.Tag == "Arrow")
                        {
                            Canvas canvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                            // Remove Adorner
                            Adorner[] arrowAdornersArray = adornerLayer.GetAdorners(canvas.Children[0]);
                            if (arrowAdornersArray != null)
                            {
                                for (int i = 0; i < arrowAdornersArray.Length; i++)
                                {
                                    adornerLayer.Remove(arrowAdornersArray[i]);
                                }
                            }
                        }

                        break;
                    case "TextBoxView":
                        // Remove Adorner
                        Adorner[] textAdornersArray = adornerLayer.GetAdorners(textBox);
                        if (textAdornersArray != null)
                        {
                            for (int i = 0; i < textAdornersArray.Length; i++)
                            {
                                adornerLayer.Remove(textAdornersArray[i]);
                            }
                        }
                        break;
                }

                // Remove Adorner
                Adorner[] toRemoveArray = adornerLayer.GetAdorners(uiElement);
                if (toRemoveArray != null)
                {
                    for (int i = 0; i < toRemoveArray.Length; i++)
                    {
                        adornerLayer.Remove(toRemoveArray[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Drag drop > Mouse is moved
        /// </summary>
        private void container_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(e.OriginalSource is Canvas))
            {
                UIElement uiElement = (UIElement)e.OriginalSource;
                if (isPressed)
                {
                    Point position = e.GetPosition(container);
                    double daltaY = position.Y - startPosition.Y;
                    double daltaX = position.X - startPosition.X;

                    TextBox textBox = e.Source as TextBox;
                    if (null != textBox)
                    {
                        textBox.SetValue(Canvas.TopProperty, (double)textBox.GetValue(Canvas.TopProperty) + daltaY);
                        textBox.SetValue(Canvas.LeftProperty, (double)textBox.GetValue(Canvas.LeftProperty) + daltaX);
                    }
                    else
                    {
                        Type type = uiElement.GetType();
                        if (type.Name == "Line")
                        {
                            Line line = (Line)uiElement;
                            if ((string)line.Tag == "Arrow")
                            {
                                Canvas arrowCanvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                                Line arrowLine = (Line)arrowCanvas.Children[0];
                                arrowLine.X1 += daltaX;
                                arrowLine.Y1 += daltaY;
                                arrowLine.X2 += daltaX;
                                arrowLine.Y2 += daltaY;

                                Line arrowHead = (Line)arrowCanvas.Children[1];
                                arrowHead.X1 += daltaX;
                                arrowHead.Y1 += daltaY;
                                arrowHead.X2 += daltaX;
                                arrowHead.Y2 += daltaY;
                            }
                            else
                            {
                                line.X1 += daltaX;
                                line.Y1 += daltaY;
                                line.X2 += daltaX;
                                line.Y2 += daltaY;
                            }
                        }
                        else
                        {
                            uiElement.SetValue(Canvas.TopProperty, (double)uiElement.GetValue(Canvas.TopProperty) + daltaY);
                            uiElement.SetValue(Canvas.LeftProperty, (double)uiElement.GetValue(Canvas.LeftProperty) + daltaX);
                        }
                    }

                    startPosition = position;
                }
            }
        }


        

        // Zoom on Mouse wheel
        private void container_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoom += zoomSpeed * e.Delta; // Ajust zooming speed (e.Delta = Mouse spin value )
            if (zoom < zoomMin) { zoom = zoomMin; } // Limit Min Scale
            if (zoom > zoomMax) { zoom = zoomMax; } // Limit Max Scale

            Point mousePos = e.GetPosition(container);

            if (zoom > 1)
            {
                container.RenderTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y); // transform Canvas size from mouse position
            }
            else
            {
                container.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            }
            zoom_event(sender, zoom);
        }

        /// <summary>
        /// Called when user control is loaded
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            adornerLayer = AdornerLayer.GetAdornerLayer(container);
        }

        /// <summary>
        /// Called when key is up
        /// </summary>
        private void container_KeyUp(object sender, KeyEventArgs e)
        {
            int index = -1;

            if (null == uiElement)
            {
                return;
            }

            if (e.Key == Key.Delete)
            {
                Type type = uiElement.GetType();
                switch (type.Name)
                {
                    case "Rectangle":
                    case "Ellipse":
                    case "Path":
                        index = container.Children.IndexOf(uiElement);

                        container.Children.Remove(uiElement);
                        break;

                    case "Line":
                        Line element = (Line)uiElement;
                        if ((string)element.Tag == "Arrow")
                        {
                            Canvas canvas = VisualTreeHelper.GetParent(uiElement) as Canvas;
                            index = container.Children.IndexOf(uiElement);
                            container.Children.Remove(canvas);
                        }
                        else
                        {
                            index = container.Children.IndexOf(uiElement);
                            container.Children.Remove(uiElement);
                        }
                        break;

                    case "TextBoxView":
                        index = container.Children.IndexOf(uiElement);
                        container.Children.Remove(textBox);
                        break;

                }

                if (index < dataTable.Rows.Count)
                {
                    dataTable.Rows.RemoveAt(index);
                }
            }
        }

        private void container_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                propertyWindow.Visibility = Visibility.Visible;
                shapeBar.Visibility = Visibility.Visible;
            }
            else
            {
                propertyWindow.Visibility = Visibility.Hidden;
                shapeBar.Visibility = Visibility.Hidden;

                if (null != uiElement)
                {
                    Adorner[] toRemoveArray = adornerLayer.GetAdorners(uiElement);
                    if (toRemoveArray != null)
                    {
                        for (int i = 0; i < toRemoveArray.Length; i++)
                        {
                            adornerLayer.Remove(toRemoveArray[i]);
                        }
                    }
                }

                if (null != textBox)
                {
                    Adorner[] textAdornersArray = adornerLayer.GetAdorners(textBox);
                    if (textAdornersArray != null)
                    {
                        for (int i = 0; i < textAdornersArray.Length; i++)
                        {
                            adornerLayer.Remove(textAdornersArray[i]);
                        }
                    }
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var usedWidth = 40;
            var usedHeight = 100;

            if (width != e.NewSize.Width)
            {
                var NewWidth = e.NewSize.Width - usedWidth;
                grid.Width = NewWidth > 0 ? NewWidth : 0;
                width = e.NewSize.Width;
                Console.WriteLine($"1.sender - {sender}, Changed Size - {e?.NewSize}, Adjusted Size - {grid.Width},{grid.Height}");

            }

            if (Height != e.NewSize.Height)
            {
                var NewHeight = e.NewSize.Height - usedHeight;
                grid.Height = NewHeight > 0 ? NewHeight : 0;
                height = e.NewSize.Height;
                Console.WriteLine($"2.sender - {sender}, Changed Size - {e?.NewSize.Width},{e?.NewSize.Height}, Adjusted Size - {grid.Width},{grid.Height}");

            }
            this.SizeChanged -= UserControl_SizeChanged;
            //var NewWidth = e.NewSize.Width - usedWidth;

        }
    }
}

//TODO Textbox size
