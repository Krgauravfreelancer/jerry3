using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using Sqllite_Library.Business;
using DebugVideoCreator.Helpers;
using System.Threading.Tasks;
using DebugVideoCreator.Auth;
using System.Windows.Media;
using dbTransferUser_UserControl.ResponseObjects.Projects;

namespace DebugVideoCreator.XAML
{
    public partial class MainWindow : Window, IDisposable
    {
        private bool IsSetUp = false;
        private readonly AuthAPIViewModel authApiViewModel;

        public MainWindow()
        {
            InitializeComponent();
            authApiViewModel = new AuthAPIViewModel();
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (!IsSetUp)
                //{
                //    InitialiseDbHelper.InitializeDatabase();
                //    IsSetUp = true;
                //}
                //rbPending.IsChecked = true;
                //rbPending_Click();
                await Login();
                await SyncApp();
                await SyncMedia();
                await SyncScreens();
                await PopulateProjects();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            SetWelcomeMessage();

        }

        private async Task SyncApp()
        {
            var data = await authApiViewModel.GetAllApp();
            if (data == null) return;
            SyncDbHelper.SyncApp(data);
            MessageBox.Show("App Data Synchronised", "Apps Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task SyncMedia()
        {
            var data = await authApiViewModel.GetAllMedia();
            if (data == null) return;
            SyncDbHelper.SyncMedia(data);
            MessageBox.Show("Media Data Synchronised", "Media Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task SyncScreens()
        {
            var data = await authApiViewModel.GetAllScreens();
            if (data == null) return;
            SyncDbHelper.SyncScreen(data);
            MessageBox.Show("Screen Data Synchronised", "Screen Data", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private async Task PopulateProjects()
        {
            var data = await authApiViewModel.GetProjectsData();
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }

        #region == Auth Events ==
        private async Task Login()
        {
            await authApiViewModel.ExecuteLoginAsync();
            ResetTokenOrError();
        }
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            await authApiViewModel.ExecuteLogoutAsync();
            ResetTokenOrError();
        }

        private void ResetTokenOrError()
        {
            if (!string.IsNullOrEmpty(authApiViewModel.GetError()))
            {
                txtError.Text = authApiViewModel.GetError();
                txtError.Foreground = Brushes.Red;
                btnLogin.IsEnabled = true;
            }
            else
            {
                txtError.Text = "";
                txtError.Foreground = Brushes.Green;
                btnLogin.IsEnabled = false;
            }

            if (!string.IsNullOrEmpty(authApiViewModel.GetToken()))
            {
                txtToken.Text = "Logged In with Token - " + authApiViewModel.GetToken();
                txtToken.Foreground = Brushes.Green;
                btnLogout.IsEnabled = true;
            }
            else
            {
                txtToken.Text = "";
                txtToken.Foreground = Brushes.Red;
                btnLogout.IsEnabled = false;
            }
        }
        #endregion

        #region == Events ==

        private void BtnCleanRegistry_Click(object sender, RoutedEventArgs e)
        {
            var message = DataManagerSqlLite.ClearRegistryAndDeleteDB(); // Lets clean for testing purpose
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SetWelcomeMessage()
        {
            var encryptedUserId = TestRegisteryHelper.GetUsername();
            if (string.IsNullOrEmpty(encryptedUserId))
                this.Title = "Video Creator, Not Logged in - Username not present in registry !!!";
            else
            {
                var userId = TestEncryptionHelper.DecryptString(TestEncryptionHelper.SecuredKey, encryptedUserId);
                this.Title = $"Video Creator, Logged in as - {userId}";
            }
        }

        private void BtnManageTimeline_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one recrod from Grid", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int selectedProjectId;
            //if ((bool)rbPending.IsChecked)
            //    selectedProjectId = ((CBVPendingProjectList)datagrid.SelectedItem)?.project_id ?? 0;
            //else
            //    selectedProjectId = ((CBVWIPOrArchivedProjectList)datagrid.SelectedItem)?.project_id ?? 0;

            selectedProjectId = ((ProjectModel)datagrid.SelectedItem)?.project_id ?? 0;

            var manageTimeline_UserControl = new ManageTimeline_UserControl(selectedProjectId);
            
            var window = new Window
            {
                Title = "Manage Timeline",
                Content = manageTimeline_UserControl,
                WindowState = WindowState.Maximized,
                ResizeMode = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
                //Title = "Manage Timeline",
                //Content = manageTimeline_UserControl,
                //SizeToContent = SizeToContent.WidthAndHeight,
                //ResizeMode = ResizeMode.NoResize,
                //RenderSize = manageTimeline_UserControl.RenderSize,
                //WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            try
            {
                var result = window.ShowDialog();
                if (result.HasValue)
                {
                    datagrid.SelectedItem = null;
                    manageTimeline_UserControl.Dispose();
                }
            }
            catch (Exception)
            { }
            
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

        private void BtnInsertLocAudio_Click(object sender, RoutedEventArgs e)
        {
            InitialiseDbHelper.PopulateLOCAudioTable();
        }

        private void BtnGetLOCAudio_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetLocAudio();
            var dataResult = $"id \t notesid \t media.Length \r\n";
            foreach ( var item in data)
            {
                dataResult += item.ToString();
            }
            MessageBox.Show(dataResult, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        #endregion == Events ==

        private async void rbPending_Click(object sender, RoutedEventArgs e)
        {
            var data = await authApiViewModel.GetProjectsData(null, ProjectStatusEnum.Pending);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }
        private async void rbWIP_Click(object sender, RoutedEventArgs e)
        {
            var data = await authApiViewModel.GetProjectsData(null, ProjectStatusEnum.WIP);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
        }
        private async void rbArchived_Click(object sender, RoutedEventArgs e)
        {
            var data = await authApiViewModel.GetProjectsData(null, ProjectStatusEnum.Archived);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;
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

        public void Dispose()
        {
            Console.WriteLine("The dispose() function has been called and the resources have been released!");
        }
    }
}