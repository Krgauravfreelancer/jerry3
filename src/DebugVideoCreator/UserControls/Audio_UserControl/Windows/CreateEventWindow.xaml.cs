using Microsoft.Win32;
using Sqllite_Library.Business;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Audio_UserControl.Windows
{
    public partial class CreateEventWindow : Window
    {
        int selectedProjectId = -1;
        public CreateEventWindow(int projectID)
        {
            InitializeComponent();
            selectedProjectId = projectID;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PathTxt.Text) || PathTxt.Text.StartsWith("..."))
                return;

            //AudioUserControl Mwindow = ((AudioUserControl)Application.Current.MainWindow);

            try
            {

                var dataTable = new DataTable();
                dataTable.Columns.Add("videoevent_id", typeof(int));
                dataTable.Columns.Add("fk_videoevent_project", typeof(int));
                dataTable.Columns.Add("fk_videoevent_media", typeof(int));
                dataTable.Columns.Add("videoevent_track", typeof(int));
                dataTable.Columns.Add("videoevent_start", typeof(string));
                dataTable.Columns.Add("videoevent_duration", typeof(int));
                dataTable.Columns.Add("videoevent_createdate", typeof(string));
                dataTable.Columns.Add("videoevent_modifydate", typeof(string));
                //optional column
                dataTable.Columns.Add("media", typeof(byte[])); // Media Column
                dataTable.Columns.Add("fk_videoevent_screen", typeof(int));//temp column for screen

                // Since this table has Referential Integrity, so lets push one by one
                dataTable.Rows.Clear();
                var row = dataTable.NewRow();
                row["videoevent_id"] = -1;
                row["fk_videoevent_project"] = selectedProjectId;
                row["videoevent_track"] = 1;
                //row["videoevent_start"] = $"00:{SMinTxt.Text}:{SSecTxt.Text}";
                //row["videoevent_duration"] = (Convert.ToInt32(DMinTxt.Text) * 60) + Convert.ToInt32(DSecTxt.Text);
                row["videoevent_start"] = $"00:00:00";
                row["videoevent_duration"] = 10;
                row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                row["fk_videoevent_media"] = 3; // For Audio
                
                //Fill Media in case image, video or audio is selected
                row["fk_videoevent_screen"] = -1; // Not needed for this case
                var path = PathTxt.Text;
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    byte[] mediaByte = StreamToByteArray(fileStream, 0);
                    row["media"] = mediaByte;
                }
                
                dataTable.Rows.Add(row);
                var insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);
                MessageBox.Show($"VideoEvent Table populated to Database for selected Audio with Id - {insertedId[0]}");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();   
            //((MainWindow)Application.Current.MainWindow).Create_Canceled();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(((TextBox)sender).Text + e.Text);
        }



        public static bool IsValid(string str)
        {
            int i;
            return int.TryParse(str, out i) && i >= 0 && i <= 60;
        }



        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).Text == "")
            {

                ((TextBox)sender).Text = "00";

            }
            else
            {

                ((TextBox)sender).Text = Convert.ToInt32(((TextBox)sender).Text).ToString("D2");



            }
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (AudBtn.IsChecked == true)
            {
                openFileDialog.Filter = "Audio files(*.MP3;*.WAV)|*.MP3;*.WAV";
            }

            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                PathTxt.Text = openFileDialog.FileName;
            }
        }

        private void Radio_Click(object sender, RoutedEventArgs e)
        {
            PathTxt.Text = "...";


            if (AudBtn.IsChecked == true)
            {
                //DurationStack.IsEnabled = false;
            }

        }
    }
}
