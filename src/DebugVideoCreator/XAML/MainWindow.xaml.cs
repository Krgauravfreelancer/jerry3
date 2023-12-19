using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using Sqllite_Library.Business;
using VideoCreator.Helpers;
using System.Threading.Tasks;
using VideoCreator.Auth;
using System.Windows.Media;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using dbTransferUser_UserControl.ResponseObjects.Background;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sqllite_Library.Models;
using System.IO;
using Authentication_UserControl.Helpers;
using System.Windows.Input;
using VideoCreator.PaginatedListView;

namespace VideoCreator.XAML
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly AuthAPIViewModel authApiViewModel;
        private List<ProjectModel> pendingProjects;
        public MainWindow()
        {
            InitializeComponent();
            authApiViewModel = new AuthAPIViewModel();
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Login();
                await SyncApp();
                await SyncMedia();
                await SyncScreens();
                await SyncCompany();
                await SyncBackground();

                rbWIP.IsChecked = true;
                InitialiseAndRefreshScreen();
                //await PopulateProjects();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            SetWelcomeMessage();

        }

        
        private void InitialiseAndRefreshScreen()
        {
            lblLoading.Visibility = Visibility.Hidden;
            stackRadioButtons.Visibility = Visibility.Visible;
            rbWIP_Click(null, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async Task SyncApp()
        {
            var data = await authApiViewModel.GetAllApp();
            if (data == null) return;
            SyncDbHelper.SyncApp(data);
        }

        private async Task SyncMedia()
        {
            var data = await authApiViewModel.GetAllMedia();
            if (data == null) return;
            SyncDbHelper.SyncMedia(data);
        }

        private async Task SyncScreens()
        {
            var data = await authApiViewModel.GetAllScreens();
            if (data == null) return;
            SyncDbHelper.SyncScreen(data);
        }

        private async Task SyncCompany()
        {
            var data = await authApiViewModel.GetAllCompany();
            if (data == null) return;
            SyncDbHelper.SyncCompany(data);
        }

        private async Task SyncBackground()
        {
            var data = await authApiViewModel.GetAllBackground();
            if (data == null) return;
            var result = new List<BackgroundModel>();
            result.Add(data);
            SyncDbHelper.SyncBackground(result, authApiViewModel);
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
            var userId = authApiViewModel.GetLoggedInUser();
            if (string.IsNullOrEmpty(userId))
                this.Title = "Video Creator, Not Logged in - Username not present in registry !!!";
            else
            {
                this.Title = $"Video Creator, Logged in as - {userId}";
            }
        }

        private bool PreValidations()
        {
            if (datagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select one recrod from Grid", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if ((bool)rbPending.IsChecked)
                MessageBox.Show("Please accept project to start working on it", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            else if ((bool)rbArchived.IsChecked)
                MessageBox.Show("project is archived, no work possible", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            else if ((bool)rbWIP.IsChecked)
                return true;
            else
                MessageBox.Show("Please select a project from WIP to continue", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        private void BtnManageTimeline_Click(object sender, RoutedEventArgs e)
        {
            int selectedProjectId;
            Int64 selectedServerProjectId;
            if (PreValidations())
            {
                selectedProjectId = ((CBVWIPOrArchivedProjectList)datagrid.SelectedItem)?.project_id ?? 0;
                selectedServerProjectId = ((CBVWIPOrArchivedProjectList)datagrid.SelectedItem)?.project_serverid ?? 0;
            }
            else return;

            //selectedProjectId = ((ProjectModelUI)datagrid.SelectedItem)?.project_id ?? 0;

            var manageTimeline_UserControl = new ManageTimeline_UserControl(selectedProjectId, selectedServerProjectId, authApiViewModel);

            var window = new Window
            {
                //Title = "Manage Timeline",
                //Content = manageTimeline_UserControl,
                //WindowState = WindowState.Maximized,
                //ResizeMode = ResizeMode.CanResize,
                //WindowStartupLocation = WindowStartupLocation.CenterScreen
                Title = "Manage Timeline",
                Content = manageTimeline_UserControl,
                WindowState = WindowState.Maximized,
            };

            manageTimeline_UserControl.closeTheEditWindow += (object sender_2, EventArgs e_2) =>
            {
                window.Close();
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
            InitialiseAndRefreshScreen();
        }

        private async void BtnAcceptProject_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Are you sure you want to accept the project?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var selectedProject = (ProjectModelUI)datagrid.SelectedItem;
                if (selectedProject != null)
                {
                    var result = await authApiViewModel.AcceptOrRejectProject(selectedProject.project_id, true);
                    if (result != null)
                    {
                        var projectToInsertLocally = pendingProjects.Find(x => x.project_id == selectedProject.project_id);
                        SyncDbHelper.SyncProject_Insert(projectToInsertLocally);
                        rbPending_Click(sender, e);
                        MessageBox.Show("Project Accepted successfully, you will see it in WIP tab", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
            }
        }
        private async void BtnRejectProject_Click(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("Are you sure you want to Reject the project?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var selectedProject = (ProjectModelUI)datagrid.SelectedItem;
                if (selectedProject != null)
                {
                    var result = await authApiViewModel.AcceptOrRejectProject(selectedProject.project_id, false);
                    if (result != null)
                    {
                        rbPending_Click(sender, e);
                        MessageBox.Show("Project Rejected successfully.", "Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        
        private void BtnShowPaginatedListView_Click(object sender, RoutedEventArgs e)
        {
            var f = new frmLsvPage();
            f.ShowDialog();
            //MessageBox.Show("Coming Soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
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
            foreach (var item in data)
            {
                dataResult += item.ToString();
            }
            MessageBox.Show(dataResult, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion == Events ==
        private void datagrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((bool)rbPending.IsChecked)
            {
                btnAcceptProject.IsEnabled = true;
                btnRejectProject.IsEnabled = true;
            }
            else
            {
                btnAcceptProject.IsEnabled = false;
                btnRejectProject.IsEnabled = false;
            }
        }
        private List<ProjectModelUI> RemoveUnnecessaryFields(List<ProjectModel> data)
        {
            var result = JsonConvert.DeserializeObject<List<ProjectModelUI>>(JsonConvert.SerializeObject(data));
            return result;
        }

        private async void rbPending_Click(object sender, RoutedEventArgs e)
        {
            pendingProjects = await authApiViewModel.GetProjectsData(null, ProjectStatusEnum.Pending);
            datagrid.ItemsSource = RemoveUnnecessaryFields(pendingProjects);
            datagrid.Visibility = Visibility.Visible;

            pendingStack.Visibility = Visibility.Visible;
            manageStack.Visibility = Visibility.Hidden;

        }
        private void rbWIP_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetWIPOrArchivedProjectList(false, true);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;

            pendingStack.Visibility = Visibility.Hidden;
            manageStack.Visibility = Visibility.Visible;
        }
        private void rbArchived_Click(object sender, RoutedEventArgs e)
        {
            var data = DataManagerSqlLite.GetWIPOrArchivedProjectList(true, false);
            datagrid.ItemsSource = data;
            datagrid.Visibility = Visibility.Visible;

            pendingStack.Visibility = Visibility.Hidden;
            manageStack.Visibility = Visibility.Hidden;
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