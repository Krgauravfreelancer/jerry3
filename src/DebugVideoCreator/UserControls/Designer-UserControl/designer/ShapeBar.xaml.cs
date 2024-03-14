using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesignerNp.controls
{
    /// <summary>
    /// Interaction logic for ShapeBar.xaml
    /// </summary>
    public partial class ShapeBar : UserControl
    {
        /// <summary>
        /// ShapeBar Constructor
        /// </summary>
        public ShapeBar()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat, "<Rectangle StrokeThickness='2' Fill='Transparent' Stroke='#0345bf' Width='50' Height='50'/>");
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat, "<Ellipse StrokeThickness='2' Fill='Transparent' Stroke='#0345bf' Width='50' Height='50'/>");
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void Line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat, "<Line StrokeThickness='3' Stroke='#0345bf' X1='0' Y1='0' X2='50' Y2='50'/>");
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void ArrowLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Arrow_MouseLeftButtonDown(sender, e);
        }

        private void ArrowHead_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Arrow_MouseLeftButtonDown(sender, e);
        }

        private void Arrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat,
                @"<Canvas Tag='Arrow'>
                    <Line Tag='Arrow' X1='0' Y1='0' X2='50' Y2='0' Stroke='#0345bf' StrokeThickness='20'/>
                    <Line Tag='Arrow' X1='50' Y1='0' X2='51' Y2='0' Stroke='#0345bf' StrokeThickness='50' StrokeEndLineCap='Triangle'/>
                </Canvas>"
                );
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat,
                @"<Path
                    Data='M 40,50 A 50,25 0 1 1 65,50 L 25, 75 Z'
                    Stroke='#0345bf' StrokeThickness='3'
                    Fill='Transparent'
                />"
                );
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataObject data = new DataObject();
            data.SetData(DataFormats.StringFormat,
                @"<TextBox Text='Text' FontSize='20' Background='Transparent' BorderThickness='0' 
                    Focusable='False' Cursor='Arrow' Foreground='#000000' AllowDrop='False'/>"
                );
            data.SetData("Object", this);

            // Initiate the drag-and-drop operation.
            DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
        }
    }
}
