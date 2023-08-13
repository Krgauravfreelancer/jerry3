using Sqllite_Library.Business;
using System.Windows;
using ScreenRecording_UserControl;
using System.Windows.Markup;

namespace ScreenRecorder_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var screenRecorderUserControl = new ScreenRecorderUserControl();
            var window = new Window
            {
                Title = "Screen Recorder",
                Content = screenRecorderUserControl,
                ResizeMode = ResizeMode.NoResize,
                Height = 200,
                Width = 600,
                RenderSize = screenRecorderUserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue && screenRecorderUserControl.datatable != null && screenRecorderUserControl.datatable.Rows.Count > 0)
            {
                if (screenRecorderUserControl.UserConsent || MessageBox.Show("Do you want save all recording??", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var insertedVideoSegmentId = DataManagerSqlLite.InsertRowsToVideoSegment(screenRecorderUserControl.datatable);
                    if (insertedVideoSegmentId > 0)
                    {
                        MessageBox.Show($"{screenRecorderUserControl.datatable.Rows.Count} rows added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }


            }
            Close();
        }
    }
}
