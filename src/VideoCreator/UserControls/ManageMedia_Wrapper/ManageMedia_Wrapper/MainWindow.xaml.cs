using ManageMedia_Wrapper.Helpers;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace ManageMedia_Wrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool _IsSetUp = false;
        bool _showMessageBoxes = false;
        ManageMediaWindowManager _Manager;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_IsSetUp)
            {
                InitializeDatabase();
                _IsSetUp = true;
            }

        }

        #region Loading Database
        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                if (_showMessageBoxes)
                {
                    MessageBox.Show(message + ", syncing lookup tables !!");
                }
                SyncApp();
                SyncMedia();
                if (_showMessageBoxes)
                {
                    MessageBox.Show("lookup tables synced successfully !!");
                }
                RefreshOrLoadComboBoxes(EnumEntity.ALL);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }
        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));
                datatable.Columns.Add("app_active", typeof(int));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                row["app_active"] = 1;
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                row2["app_active"] = 0;
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                row3["app_active"] = 0;
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
                row4["app_active"] = 0;
                datatable.Rows.Add(row4);

                var row5 = datatable.NewRow();
                row5["app_id"] = -1;
                row5["app_name"] = "superadmin";
                row5["app_active"] = 0;
                datatable.Rows.Add(row5);

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
                datatable.Columns.Add("screen_hexcolor", typeof(string));

                var row = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "title";
                row["screen_color"] = "Blue";
                row["screen_hexcolor"] = "#0000FF";
                datatable.Rows.Add(row);

                var row1 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "intro";
                row["screen_color"] = "Amethyst";
                row["screen_hexcolor"] = "#9966CC";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "requirements";
                row["screen_color"] = "Copper";
                row["screen_hexcolor"] = "#B87333";
                datatable.Rows.Add(row);

                var row3 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "objectives";
                row["screen_color"] = "Cyan";
                row["screen_hexcolor"] = "#00FFFF";
                datatable.Rows.Add(row);

                var row4 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "custom";
                row["screen_color"] = "green";
                row["screen_hexcolor"] = "#7FFF00";
                datatable.Rows.Add(row);

                var row5 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "conclusion";
                row["screen_color"] = "Gold";
                row["screen_hexcolor"] = "#FFD700";
                datatable.Rows.Add(row);

                var row6 = datatable.NewRow();
                row["screen_id"] = -1;
                row["screen_name"] = "next";
                row["screen_color"] = "Electric blue";
                row["screen_hexcolor"] = "#4B0082";
                datatable.Rows.Add(row);

                var insertedIds = DataManagerSqlLite.SyncScreen(datatable);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region ListBox and ComboBox


        private void RefreshOrLoadComboBoxes(EnumEntity entity = EnumEntity.ALL)
        {
            if (entity == EnumEntity.ALL || entity == EnumEntity.PROJECT)
            {
                var data = DataManagerSqlLite.GetDownloadedProjectList();
                RefreshComboBoxes<CBVProjectForJoin>(ProjectCmbBox, data, "project_videotitle");
                if (ProjectCmbBox.Items.Count > 0)
                {
                    ProjectCmbBox.SelectedIndex = 0;
                }
            }
        }

        private void RefreshComboBoxes<T>(System.Windows.Controls.ComboBox combo, List<T> source, string columnNameToShow)
        {
            combo.SelectedItem = null;
            combo.DisplayMemberPath = columnNameToShow;
            combo.Items.Clear();
            foreach (var item in source)
            {
                combo.Items.Add(item);
            }
        }

        private void ProjectCmbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Manager != null)
            {
                if (ProjectCmbBox.SelectedItem != null)
                {
                    int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;

                    if (Selected_ID != -1)
                    {
                        _Manager.SetProjectID(Selected_ID);
                    }
                }
            }
        }
        #endregion

        private void LaunchGeneratedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_Manager == null)
            {
                if (ProjectCmbBox.SelectedItem != null)
                {
                    int Selected_ID = ((CBVProjectForJoin)ProjectCmbBox.SelectedItem).project_id;
                    if (_Manager == null)
                    {
                        _Manager = new ManageMediaWindowManager();
                    }

                    Window _GeneratedManageMediaWindow = _Manager.CreateWindow(Selected_ID);
                    _GeneratedManageMediaWindow.Closed += _GeneratedManageMediaWindow_Closed; ;
                    _GeneratedManageMediaWindow.Show();
                    this.Visibility = Visibility.Hidden;
                }
            }
        }

        private void _GeneratedManageMediaWindow_Closed(object sender, EventArgs e)
        {
            this.Visibility = Visibility.Visible;
            _Manager.ReleaseReferences();
            _Manager = null;
        }
    }

}
