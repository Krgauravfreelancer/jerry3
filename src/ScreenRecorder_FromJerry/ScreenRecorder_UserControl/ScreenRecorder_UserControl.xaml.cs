using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using SharpAvi.Output;

namespace ScreenRecording_UserControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ScreenRecorderUserControl : System.Windows.Controls.UserControl
    {
        private AviWriter writer;
        private IAviVideoStream videoStream;
        private Stopwatch stopwatch;
        private Rectangle screenBounds;
        private Popup popup;
        private bool isResizing = false;
        private System.Windows.Point resizeStartPoint;
        private double initialWidth;
        private double initialHeight;
        private double intialMarginLeft;
        private double intialMarginTop;
        bool isDownSize;
        private bool canResize;
        string outputPath = string.Empty;


        // Public Properties        
        public bool UserConsent = false;
        public DataTable datatable { get; private set; }


        #region == Contructor and Helper ==

        public ScreenRecorderUserControl()
        {
            InitializeComponent();
            Screens.ItemsSource = Screen.AllScreens.Select(x => x.DeviceName);
            Screens.SelectedIndex = 0;
            InitializeTable();
        }

        private void InitializeTable()
        {
            datatable = new DataTable();
            datatable.Columns.Add("videosegment_id", typeof(int));
            datatable.Columns.Add("fk_videosegment_videoevent", typeof(int));
            datatable.Columns.Add("videosegment_media", typeof(byte[]));
            datatable.Columns.Add("videosegment_createdate", typeof(string));
            datatable.Columns.Add("videosegment_modifydate", typeof(string));
        }


        private void ConvertFileToBlobAndAddtoDataTable(string filepath, string blobtype)
        {
            Console.WriteLine("Adding to datatable ");
            if (!string.IsNullOrEmpty(filepath) && File.Exists(filepath))
            {
                var blob = File.ReadAllBytes(filepath);
                if (Directory.Exists(Path.GetDirectoryName(filepath)))
                {
                    // Directory.Delete(Path.GetDirectoryName(outputPath), true);
                    SaveToDataTable(blob);
                    Console.WriteLine($"Added to datatable, Rowcount - {datatable.Rows.Count}");
                    lblRecordingInfo.Content = $@"Total Recordings - {datatable.Rows.Count}";
                }
            }
        }

        private void SaveToDataTable(byte[] blob)
        {
            var newRow = datatable.NewRow();
            newRow["videosegment_id"] = -1;
            newRow["fk_videosegment_videoevent"] = 1;
            newRow["videosegment_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            newRow["videosegment_media"] = blob;

            datatable.Rows.Add(newRow);
        }

        #endregion

        #region == Events  ==


        private void SaveToDatabase_Click(object sender, RoutedEventArgs e)
        {
            UserConsent = true;
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        private void UserControl_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            Console.WriteLine("Closing !!!! ");
            UserConsent = false;
        }

        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var filepath = Path.Combine(appDirectory, "ScreenRecordings\\ScreenShots\\ScreenShot_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".png");
                var directory = Path.GetDirectoryName(filepath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Get the bounds of the screen
                Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

                // Create a bitmap with the screen size
                using (Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
                {
                    // Create a Graphics object from the bitmap
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        // Capture the screen into the bitmap
                        graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, bitmap.Size);
                    }

                    // Save the bitmap to the specified file path
                    bitmap.Save(filepath, ImageFormat.Png);
                    ConvertFileToBlobAndAddtoDataTable(filepath, "ScreenShot");
                }
                if (popup != null)
                {
                    popup.IsOpen = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to capture screenshot: " + ex.Message, "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Screens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            screenBounds = Screen.AllScreens.FirstOrDefault(x => x.DeviceName == (string)Screens.SelectedItem).Bounds;
        }

        private void DrawFrame_Click(object sender, RoutedEventArgs e)
        {
            // Display an overlay window to allow the user to draw the frame
            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.ShowDialog();

            if (overlayWindow.DialogResult.HasValue && overlayWindow.DialogResult.Value)
            {
                screenBounds = overlayWindow.CaptureRectangle;
                var rectangle = overlayWindow.DrawnFrame;

                // Subscribe to the necessary mouse events
                rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                rectangle.MouseMove += Rectangle_MouseMove;
                rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
                rectangle.Fill = System.Windows.Media.Brushes.Transparent;

                // Set the properties of the Popup
                popup = new Popup
                {
                    AllowsTransparency = true,
                    Placement = PlacementMode.Absolute,

                    // Set the content of the Popup to the created Rectangle
                    Child = rectangle,

                    // Open the Popup
                    IsOpen = true
                };

                if (screenBounds.X <= Screen.PrimaryScreen.Bounds.Width)
                {
                    PresentationSource source = PresentationSource.FromVisual(rectangle);
                    if (source != null)
                    {
                        Matrix matrix = source.CompositionTarget.TransformToDevice;
                        screenBounds.X = (int)(screenBounds.X * matrix.M11);
                        screenBounds.Y = (int)(screenBounds.Y * matrix.M22);
                    }
                }

                popup.HorizontalOffset = screenBounds.X;
                popup.VerticalOffset = screenBounds.Y;
                popup.Width = screenBounds.Width;
                popup.Height = screenBounds.Height;

                //popup.PlacementRectangle = new Rect(screenBounds.X, screenBounds.Y, screenBounds.Width, screenBounds.Height);
            }
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RecordButton.Content.ToString() == "Record")
                {
                    // Start recording
                    RecordButton.Content = "Stop";

                    ScreenShot.IsEnabled = false;
                    Screens.IsEnabled = false;
                    DrawFrame.IsEnabled = false;

                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    outputPath = Path.Combine(appDirectory, "ScreenRecordings\\Recordings\\Recording_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".avi");
                    if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    }

                    writer = new AviWriter(outputPath)
                    {
                        FramesPerSecond = 30,
                        EmitIndex1 = true
                    };

                    videoStream = writer.AddVideoStream();
                    videoStream.Width = screenBounds.Width;
                    videoStream.Height = screenBounds.Height;

                    stopwatch = Stopwatch.StartNew();

                    CompositionTarget.Rendering += CompositionTarget_Rendering;
                }
                else
                {
                    // Stop recording
                    RecordButton.Content = "Record";

                    CompositionTarget.Rendering -= CompositionTarget_Rendering;

                    writer.Close();
                    writer = null;
                    ConvertFileToBlobAndAddtoDataTable(outputPath, "Recording");
                    if (popup != null)
                    {
                        popup.IsOpen = false;
                    }
                    ScreenShot.IsEnabled = true;
                    Screens.IsEnabled = true;
                    DrawFrame.IsEnabled = true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to capture Recording: " + ex.Message, "Error Occured!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == draw and video events ==

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            using (var bitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(screenBounds.Location, System.Drawing.Point.Empty, screenBounds.Size);
                }
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipY); // Rotate 180 degrees and flip horizontally
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var bytes = new byte[data.Stride * data.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                videoStream.WriteFrame(true, bytes, 0, bytes.Length);

                bitmap.UnlockBits(data);
            }
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape && popup != null)
            {
                popup.IsOpen = false;
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (popup != null)
            {
                popup.IsOpen = false;
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (canResize)
            {
                isResizing = true;
                System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)sender;
                resizeStartPoint = e.GetPosition(rectangle);
                initialWidth = popup.Width;
                initialHeight = popup.Height;
                intialMarginLeft = popup.HorizontalOffset;
                intialMarginTop = popup.VerticalOffset;
                rectangle.CaptureMouse();
            }
        }

        private void Rectangle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)sender;
            System.Windows.Point mousePosition = e.GetPosition(rectangle);
            ChangeMouseCursor(mousePosition, rectangle);
            if (isResizing)
            {
                System.Windows.Point currentPoint = e.GetPosition(rectangle);

                double deltaX = currentPoint.X - resizeStartPoint.X;
                double deltaY = currentPoint.Y - resizeStartPoint.Y;
                double width, height;
                if (isDownSize)
                {
                    width = initialWidth - deltaX;
                    height = initialHeight - deltaY;
                }
                else
                {
                    width = initialWidth + deltaX;
                    height = initialHeight + deltaY;
                }

                // Update the size of the rectangle
                rectangle.Width = width;
                rectangle.Height = height;
                if (isDownSize)
                {
                    screenBounds.X = (int)(intialMarginLeft + deltaX);
                    screenBounds.Y = (int)(intialMarginTop + deltaY);
                }
                screenBounds.Width = (int)width;
                screenBounds.Height = (int)height;

                popup.HorizontalOffset = screenBounds.X;
                popup.VerticalOffset = screenBounds.Y;
                popup.Width = screenBounds.Width;
                popup.Height = screenBounds.Height;
            }
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            System.Windows.Shapes.Rectangle rectangle = (System.Windows.Shapes.Rectangle)sender;
            rectangle.ReleaseMouseCapture();
        }

        private void ChangeMouseCursor(System.Windows.Point mousePosition, System.Windows.Shapes.Rectangle rectangle)
        {
            // Calculate the distance from the mouse position to each side of the rectangle
            double leftDistance = mousePosition.X;
            double rightDistance = rectangle.Width - mousePosition.X;
            double topDistance = mousePosition.Y;
            double bottomDistance = rectangle.Height - mousePosition.Y;

            // Check if the mouse is near any of the rectangle's borders
            double threshold = 5; // Define a threshold to determine proximity to the border
            if (leftDistance < threshold || rightDistance < threshold || topDistance < threshold || bottomDistance < threshold)
            {
                canResize = true;
                // Change the cursor based on the position relative to the rectangle's borders
                if (leftDistance < threshold || rightDistance < threshold)
                    rectangle.Cursor = System.Windows.Input.Cursors.SizeWE; // Horizontal resize cursor
                else
                    rectangle.Cursor = System.Windows.Input.Cursors.SizeNS; // Vertical resize cursor

                if (leftDistance < threshold || topDistance < threshold)
                {
                    isDownSize = true;
                }
                else if (rightDistance < threshold || bottomDistance < threshold)
                {
                    isDownSize = false;
                }
            }
            else
            {
                canResize = false;
                // Reset the cursor if it is not near any of the borders
                rectangle.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        #endregion

        
    }
}
