using ScreenRecording_UserControl;
using Sqllite_Library.Business;
using System.Windows;

namespace TimelineDB_Wrapper
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ContextMenuAddVideoEventDataClickEvent(object sender, RoutedEventArgs e)
        {
            var screenRecorderUserControl = new ScreenRecorderUserControl(1);
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
                    var insertedVideoEventId = DataManagerSqlLite.InsertRowsToVideoEvent(screenRecorderUserControl.datatable);
                    if (insertedVideoEventId.Count > 0)
                    {
                        MessageBox.Show($"{screenRecorderUserControl.datatable.Rows.Count} video event record added to database successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"No data added to database ", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


    }
}