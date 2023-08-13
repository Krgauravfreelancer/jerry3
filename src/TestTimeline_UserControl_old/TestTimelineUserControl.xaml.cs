using System.Windows;
using System.Windows.Controls;

namespace TestTimeline_UserControl
{
    public partial class TestTimelineUserControl : UserControl
    {
        //private List<CBVProject> _projectData;
        private int project_id;
        public event RoutedEventHandler BtnInsertVideoEventDataClickEvent;
        //private string FilterMedia = string.Empty;
        //private string DefaultExt = string.Empty;
        //private CBVMedia SelectedMedia;

        public TestTimelineUserControl()
        {
            InitializeComponent();
        }
        

        #region == Public Functions ==
        //public void SetSelectedProjectIdText(string text)
        //{
        //    lblSelectedProjectId.Content = text;
        //}

        public void SetSelectedProjectId(int _project_id)
        {
            project_id = _project_id;
            //RefreshOrLoadComboBoxes(EnumEntity.ALL);
            //cmbProjectForVideoEvent.SelectedItem = _projectData.Find(x=>x.project_id == _project_id);
        }

        #endregion
        /*
        #region == Events ==

        private void TxtStart_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            ((TextBox)sender).Text = s;
        }

        private void TxtDuration_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            ((TextBox)sender).Text = s;
        }

        private void cmbMedia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedMedia = (CBVMedia)cmbMedia.SelectedItem;
            btnInsert.IsEnabled = false;
            btnBrowse.IsEnabled = false;
            txtMediaPath.Text = string.Empty;
            if (SelectedMedia?.media_name == "image")
            {
                btnBrowse.IsEnabled = true;
                FilterMedia = "All Image Files|*.jpeg;*.png;*.jpg;*.gif";
                DefaultExt = ".jpeg";
            }
            else if (SelectedMedia?.media_name == "video")
            {
                btnBrowse.IsEnabled = true;
                FilterMedia = "All Video Files|*.mp4";
                DefaultExt = ".mp4";
            }
            else if (SelectedMedia?.media_name == "audio")
            {
                btnBrowse.IsEnabled = true;
                FilterMedia = "All Audio Files|*.mp3";
                DefaultExt = ".mp3";
            }
            else if (SelectedMedia?.media_name == "form")
            {
                btnBrowse.IsEnabled = false;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            btnInsert.IsEnabled = false;
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = DefaultExt,
                Filter = FilterMedia
            };

            var result = dlg.ShowDialog();
            if (result == true)
            {
                btnInsert.IsEnabled = true;
                string filename = dlg.FileName;
                txtMediaPath.Text = filename;
            }
        }

        private void BtnInsertVideoEventData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var datatable = new DataTable();
                datatable.Columns.Add("videoevent_id", typeof(int));
                datatable.Columns.Add("fk_videoevent_project", typeof(int));
                datatable.Columns.Add("fk_videoevent_media", typeof(int));
                datatable.Columns.Add("videoevent_track", typeof(int));
                datatable.Columns.Add("videoevent_start", typeof(string));
                datatable.Columns.Add("videoevent_duration", typeof(int));
                datatable.Columns.Add("videoevent_createdate", typeof(string));
                datatable.Columns.Add("videoevent_modifydate", typeof(string));
                //optional column
                datatable.Columns.Add("media", typeof(byte[])); // Media Column
                datatable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen

                var row = datatable.NewRow();

                row["videoevent_id"] = -1;
                row["fk_videoevent_project"] = ((CBVProject)cmbProjectForVideoEvent.SelectedItem)?.project_id ?? 0;
                row["videoevent_track"] = txtTrack.Text;
                var date = Convert.ToDateTime(timespancontrol.Text).ToString("HH:mm:ss");
                row["videoevent_start"] = date;
                row["videoevent_duration"] = txtDuration.Text;
                row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                var selectedMedia = (CBVMedia)cmbMedia.SelectedItem;
                row["fk_videoevent_media"] = selectedMedia != null ? selectedMedia.media_id : 0;
                if (SelectedMedia?.media_name == "image" || SelectedMedia?.media_name == "video" || SelectedMedia?.media_name == "audio")
                {
                    row["fk_videoevent_screen"] = -1; // Not needed for this case
                    var path = $"{txtMediaPath.Text}";
                    using (var fileStream = new FileStream(path, FileMode.Open))
                    {
                        byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                        row["media"] = mp3Byte;
                    }
                }

                datatable.Rows.Add(row);
                var insertedIds = DataManagerSqlLite.InsertRowsToVideoEvent(datatable, true);
                if (insertedIds.Count > 0)
                {
                    MessageBox.Show("VideoEvent Row Added to Database", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Reset all controls                
                    cmbMedia.SelectedItem = null;
                    txtTrack.Text = string.Empty;
                    txtDuration.Text = string.Empty;
                    timespancontrol.Text = "00:00:00";
                    txtMediaPath.Text = string.Empty;
                    btnBrowse.IsEnabled = false;
                    btnInsert.IsEnabled = false;

                    // Send event to parent to refresh other controls
                    BtnInsertVideoEventDataClickEvent.Invoke(sender, e);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        

        #endregion == Events ==

        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
            {
                _projectData = DataManagerSqlLite.GetProjects(true, false);
                RefreshComboBoxes<CBVProject>(cmbProjectForVideoEvent, _projectData, "project_name");
            }

            if (entity == EnumEntity.ALL || entity == EnumEntity.MEDIA)
            {
                var data = DataManagerSqlLite.GetMedia();
                RefreshComboBoxes<CBVMedia>(cmbMedia, data, "media_name");
            }


        }

        private void RefreshComboBoxes<T>(ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }

        }

        private static byte[] StreamToByteArray(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }
            byte[] buffer = new byte[initialLength];
            int read = 0;
            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;
                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }
                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }
        */
    }
}