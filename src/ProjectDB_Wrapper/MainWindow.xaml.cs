using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;

/// <summary>
/// ***************************************************************************************************************************************
/// **************************************************** DEVELOPER REMARKS ****************************************************************
/// ***************************************************************************************************************************************
/// ---------------------------------------------------- METHODS --------------------------------------------------------------------------
///     InitializeDatabase()
///         This method is called when control is loaded And It does the following work.  
///         Check if DB exists. If Not Create the Database using two parametes
///         Params encryptFlag - create DB is encryption mode, canCreateRegistryIfNotExists - Flag to create registry in case does not exist
///         Synchronise the APP, MEDIA AND SCREEN Tables
///         Populate some rows to Project table, controlled by PopulateProjectFlag in case not needed
///         Populate some rows to VideoEvent table, controlled by PopulateVideoEventFlag in case not needed
///     
///     DataManagerSqlLite.SyncApp(datatable)
///         Synchronise the APP Table with the latest data
///         datatable columns - app_id and app_name
///     
///     DataManagerSqlLite.SyncMedia(datatable)
///         Synchronise the MEDIA Table with the latest data
///         datatable columns - media_id, media_name and media_color
///         
///     DataManagerSqlLite.SyncScreen(datatable)
///         Synchronise the SCREEN Table with the latest data
///         datatable columns - screen_id, screen_name and screen_color
///         
///     DataManagerSqlLite.GetVideoEvents(selectedprojectId, dependentTableFlag)
///         Fetch video events for the project Id provided. If dependentTableFlag is true, also fetched dependent/associated tables
///         
///     DataManagerSqlLite.GetProjects(archivedFlag)
///         Fetch all projects present in the database where archived flag is false / true
///      
/// **************************************************** DEVELOPER REMARKS END ************************************************************
/// </summary>
/// 
namespace ProjectDB_Wrapper
{
    public partial class MainWindow : Window
    {
        private bool IsSetUp = false;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsSetUp)
            {
                InitializeDatabase();
                IsSetUp = true;
            }
        }

        #region == Helper Functions ==
        private void InitializeDatabase()
        {
            try
            {
                var message = DataManagerSqlLite.CreateDatabaseIfNotExist(false, true); // Lets keep the flag false for now
                MessageBox.Show(message + ", syncing lookup tables !!");
                SyncApp();
                SyncMedia();
                SyncScreen();
                MessageBox.Show("lookup tables synced successfully !!");
                RefreshOrLoadProjectComboBoxes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        #endregion

        #region == Sync Functions ==
        private void SyncApp()
        {
            try
            {
                var datatable = new DataTable();

                datatable.Columns.Add("app_id", typeof(int));
                datatable.Columns.Add("app_name", typeof(string));

                var row = datatable.NewRow();
                row["app_id"] = -1;
                row["app_name"] = "draft";
                datatable.Rows.Add(row);

                var row2 = datatable.NewRow();
                row2["app_id"] = -1;
                row2["app_name"] = "write";
                datatable.Rows.Add(row2);

                var row3 = datatable.NewRow();
                row3["app_id"] = -1;
                row3["app_name"] = "talk";
                datatable.Rows.Add(row3);

                var row4 = datatable.NewRow();
                row4["app_id"] = -1;
                row4["app_name"] = "admin";
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

        #endregion


        #region == Button Clicks ==
        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message);
        }

        private void BtnInsertDownload_Click(object sender, RoutedEventArgs e)
        {
            PopulateLastDownloadTable();
        }

        private void BtnInsertUploadClick(object sender, RoutedEventArgs e)
        {
            PopulateLastUploadTable();
        }

        private void BtnGetDownload_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetLastDownload();
            if (data?.Count > 0)
                MessageBox.Show($@"Fetched {data?.Count} rows for last download table", "Last Download");
            else
                MessageBox.Show("No Data", "Last Download");

        }

        private void BtnGetUploadClick(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetLastUpload();
            if (data?.Count > 0)
                MessageBox.Show($@"Fetched {data?.Count} rows for last upload table", "Last Upload");
            else
                MessageBox.Show("No Data", "Last Upload");
        }

        #endregion == Button Clicks ==


        #region == Other functions ==
        private void CmbProject_SelectionChanged(object sender, EventArgs e)
        {
            var selectedproject = (CBVProject)cmbProject.SelectedItem;
            var selectedprojectId = selectedproject?.project_id ?? 0;
            lblSelectedProject.Content = $@"Selected  Project Id - {selectedprojectId}, createdate - {selectedproject.project_createdate} and modidied - {selectedproject.project_modifydate}" ;
        }

        private void RefreshOrLoadProjectComboBoxes()
        {
            var projectData = DataManagerSqlLite.GetProjects(false);
            RefreshComboBoxes<CBVProject>(cmbProject, projectData, "project_name");
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

        private void PopulateLastDownloadTable()
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("lastdownload_id", typeof(int));
                dataTable.Columns.Add("lastdownload_date", typeof(string));

                for (var i = 1; i <= 10; i++)
                {
                    var row = dataTable.NewRow();
                    row["lastdownload_id"] = i;
                    row["lastdownload_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToLastDownload(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show($@"LastDownload Table populated to Database for 10 rows", "Last Download");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void PopulateLastUploadTable()
        {
            try
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("lastupload_id", typeof(int));
                dataTable.Columns.Add("lastupload_date", typeof(string));

                for (var i = 1; i <= 10; i++)
                {
                    var row = dataTable.NewRow();
                    row["lastupload_id"] = i;
                    row["lastupload_date"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    dataTable.Rows.Add(row);
                }

                var insertedId = DataManagerSqlLite.InsertRowsToLastUpload(dataTable);
                if (insertedId > -1)
                {
                    MessageBox.Show($@"LastUpload Table populated to Database for 10 rows", "Last Upload");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
