using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notes_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotesViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
           
            viewModel =  new NotesViewModel(this.notesReadCtrl);
            DataContext = viewModel;
            InitializeDatabase();
            //this.notesReadCtrl.DataContext = viewModel;
        }

        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                MessageBox.Show(message + ", populating tables");

                SyncApp();
                SyncMedia();
                SyncScreen();

                //if (DataManagerSqlLite.GetProjectsCount() == 0)
                //    PopulateProjectTable();                                                                        //MessageBox.Show(message + ", syncing lookup tables !!");
                if (DataManagerSqlLite.GetVideoEventsCount() == 0)
                    PopulateVideoEventTable();

                if (DataManagerSqlLite.GetNotes().Count == 0)
                {
                    try
                    {
                        var dtNotes = BuildNotesDataTable();
                        dtNotes = AddNoteRow(dtNotes, 1, "Launch Visual Studio, and open the New Project dialog box");
                        dtNotes = AddNoteRow(dtNotes, 2, "In the Window category, select the \"WPF User control Library\"");
                        dtNotes = AddNoteRow(dtNotes, 3, "Name the new project");
                        dtNotes = AddNoteRow(dtNotes, 4, "Click OK to create the project");
                        dtNotes = AddNoteRow(dtNotes, 5, "In Solution Explorer, rename UserControl1 to an appropriate name");
                        dtNotes = AddNoteRow(dtNotes, 6, "Add System.Windows.forms");
                        var insertedId = DataManagerSqlLite.InsertRowsToNotes(dtNotes);
                        if (insertedId > 0)
                        {
                            MessageBox.Show("Notes Row Added to Database");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
        private void PopulateProjectTable()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("id", typeof(int));
                dataTable.Columns.Add("project_name", typeof(string));
                dataTable.Columns.Add("project_version", typeof(int));
                dataTable.Columns.Add("project_comments", typeof(string));
                dataTable.Columns.Add("project_createdate", typeof(string));
                dataTable.Columns.Add("project_modifydate", typeof(string));

                for (var i = 1; i <= 5; i++)
                {
                    var row = dataTable.NewRow();
                    row["id"] = 1;
                    row["project_name"] = $"Sample Project - {i}";
                    row["project_version"] = i;
                    row["project_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["project_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToProject(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show("Projects populated to Database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void PopulateVideoEventTable()
        {
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
                for (var i = 0; i < 16; i++)
                {
                    // Since this table has Referential Integrity, so lets push one by one
                    dataTable.Rows.Clear();
                    var row = dataTable.NewRow();
                    row["videoevent_id"] = -1;
                    row["fk_videoevent_project"] = (i % 5) + 1;
                    row["videoevent_track"] = 1;
                    row["videoevent_start"] = "00:15:00";
                    row["videoevent_duration"] = 10;
                    row["videoevent_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    row["videoevent_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    var mediaId = (i % 4) + 1;
                    row["fk_videoevent_media"] = mediaId;

                    if (mediaId == 4) //select screen if media is form
                    {
                        row["fk_videoevent_screen"] = 1; // Hardcoded 1 for now
                        row["media"] = null;
                    }
                    else
                    {
                        //Fill Media in case image, video or audio is selected
                        row["fk_videoevent_screen"] = -1; // Not needed for this case
                        var currentDirectory = Directory.GetCurrentDirectory();
                        if (mediaId == 1)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Image1.jpg";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if (mediaId == 2)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Screencast1.mp4";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                        else if (mediaId == 3)
                        {
                            var path = $"{currentDirectory}\\MediaFiles\\Audio1.mp3";
                            using (var fileStream = new FileStream(path, FileMode.Open))
                            {
                                byte[] mp3Byte = StreamToByteArray(fileStream, 0);
                                row["media"] = mp3Byte;
                            }
                        }
                    }
                    dataTable.Rows.Add(row);
                    var insertedId = DataManagerSqlLite.InsertRowsToVideoEvent(dataTable);
                }
                MessageBox.Show("VideoEvent Table populated to Database");
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

        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(bool));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = true;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = false;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = false;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = false;
                datatable.Rows.Add(row4);
                var insertedIds = DataManagerSqlLite.SyncApp(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SyncMedia()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("media_id", typeof(int));
                datatable.Columns.Add("media_name", typeof(string));
                datatable.Columns.Add("media_color", typeof(string));

                var row = datatable.NewRow();
                row["media_id"] = -1;
                row["media_name"] = "image";
                row["media_color"] = "Tomato";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["media_id"] = -1;
                row2["media_name"] = "video";
                row2["media_color"] = "Thistle";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["media_id"] = -1;
                row3["media_name"] = "audio";
                row3["media_color"] = "Yellow";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["media_id"] = -1;
                row4["media_name"] = "form";
                row4["media_color"] = "LightSalmon";
                datatable.Rows.Add(row4);

                var insertedIds = DataManagerSqlLite.SyncMedia(datatable);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void SyncScreen()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("screen_id", typeof(int));
                datatable.Columns.Add("screen_name", typeof(string));
                datatable.Columns.Add("screen_color", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "LightSalmon";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["screen_id"] = -1;
                row2["screen_name"] = "prerequisites";
                row2["screen_color"] = "Azure";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["screen_id"] = -1;
                row3["screen_name"] = "screen cast";
                row3["screen_color"] = "Beige";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["screen_id"] = -1;
                row4["screen_name"] = "conclusion";
                row4["screen_color"] = "Aqua";
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["screen_id"] = -1;
                row5["screen_name"] = "next";
                row5["screen_color"] = "LightSteelBlue";
                datatable.Rows.Add(row5);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable BuildNotesDataTable()
        {
            var dtNotes = new DataTable();
            dtNotes.Columns.Add("notes_id", typeof(int));
            dtNotes.Columns.Add("fk_notes_videoevent", typeof(int));
            dtNotes.Columns.Add("notes_line", typeof(string));
            dtNotes.Columns.Add("notes_wordcount", typeof(int));
            dtNotes.Columns.Add("notes_createdate", typeof(string));
            dtNotes.Columns.Add("notes_modifydate", typeof(string));

            return dtNotes;
        }

        public DataTable AddNoteRow(DataTable dt, int notesId, string notesLine)
        {
            //DataRow dRow = null;

            var dRow = dt.NewRow();

            dRow["notes_id"] = notesId;
            dRow["notes_line"] = notesLine;
            dRow["notes_createdate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dRow["notes_modifydate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            dRow["fk_notes_videoevent"] = 2;
            dRow["notes_wordcount"] = notesLine.Split(' ').Length;
            dt.Rows.Add(dRow);

            return dt;
        }
        private void btnManageNotes_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = viewModel.GetCurrentData();
            ManageNotes frm =  new ManageNotes(this,dt);
            frm.ShowDialog();
        }

        public void UpdateNotes(DataTable dt)
        {
            viewModel.RefreshNotes(dt);        
        }

        
    }
}
