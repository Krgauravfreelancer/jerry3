using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows;
using Sqllite_Library.Business;
using Sqllite_Library.Models;
using DebugVideoCreator.Helpers;

namespace DebugVideoCreator
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
            try
            {
                if (!IsSetUp)
                {
                    InitialiseDbHelper.InitializeDatabase();
                    IsSetUp = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            LoadProjectDataGrid();
        }

        #region == Events ==

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetWelcomeMessage()
        {
            var encryptedUserId = TestRegisteryHelper.GetUsername();
            if (string.IsNullOrEmpty(encryptedUserId) )
                this.Title = "Video Creator, Not Logged in - Username not present in registry !!!";
            else
            {
                var userId = TestEncryptionHelper.DecryptString(TestEncryptionHelper.SecuredKey, encryptedUserId);
                this.Title = $"Video Creator, Logged in as - {userId}";
            }
        }
        
        private void BtnManageTimeline_Click(object sender, RoutedEventArgs e)
        {
            if(datagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one recrod from Grid", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var selectedProject = (CBVProject)datagrid.SelectedItem;
            var manageTimeline_UserControl = new ManageTimeline_UserControl(selectedProject.project_id);

            var window = new Window
            {
                Title = "Manage Timeline",
                Content = manageTimeline_UserControl,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                RenderSize = manageTimeline_UserControl.RenderSize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var result = window.ShowDialog();
            if (result.HasValue)
            {
                datagrid.SelectedItem = null;
            }
            
        }
        
        private void BtnManageAudio_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnManageVideo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void BtnDownloadProject_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion == Events ==

        private void LoadProjectDataGrid()
        {
            var data = DataManagerSqlLite.GetProjects(true, true);
            //var dt = ToDataTable(data);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
            SetWelcomeMessage();
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        private void rbPending_Click(object sender, RoutedEventArgs e)
        {

        }
        private void rbWIP_Click(object sender, RoutedEventArgs e)
        {

        }
        private void rbArchived_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}