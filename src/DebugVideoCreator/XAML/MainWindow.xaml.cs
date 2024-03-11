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
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Background;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sqllite_Library.Models;
using System.IO;
using ServerApiCall_UserControl.Helpers;
using System.Windows.Input;
using VideoCreator.MediaLibraryData;
using NLog;
using NLog.Config;
using System.Windows.Controls;
using System.Windows.Data;
using NAudio.Wave;
using System.Linq;
using VideoCreator.Models;
using ServerApiCall_UserControl.DTO.AutofillModels;
using Sqllite_Library.Helpers;

namespace VideoCreator.XAML
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly AuthAPIViewModel authApiViewModel;
       
        private List<ProjectList> availableProjects;
        private List<CBVProjectForJoin> downloadedProjects;
        private List<ProjectListUI> availableProjectsDataSource;
        private ProjectListUI selectedItem;
        Window manageTimelineWindow;

        public MainWindow()
        {
            LogManagerHelper.WriteVerboseLog("Starting the windows application");
            InitializeComponent();
            authApiViewModel = new AuthAPIViewModel();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            LogManagerHelper.WriteVerboseLog("Initiating Application shutdown");
            if (manageTimelineWindow != null)
            {
                manageTimelineWindow.Close();
                manageTimelineWindow = null;
            }
            Application.Current.Shutdown();
            PathHelper.ClearTempDirectories();
            LogManagerHelper.WriteVerboseLog("Application shutdown completed");
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            // AutoFill();
            await Login();
            await SyncApp();
            await SyncMedia();
            await SyncCompany();
            await SyncBackground();
            await SyncScreens();
            await SyncPlanningHead();
            await InitialiseAndRefreshScreen(true);

            LoaderHelper.HideLoader(this, loader);
        }

        private string GetHeaderName(string name)
        {
            var output = "";
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var input in name.Split('_'))
                {
                    output += input.First().ToString().ToUpper() + input.Substring(1) + ' ';
                }
            }
            return output;
        }


        private async Task InitialiseAndRefreshScreen(bool isFirstCall = false)
        {
            downloadedProjects = DataManagerSqlLite.GetDownloadedProjectList();
            availableProjects = await authApiViewModel.GetAvailableProjectsData();
            availableProjectsDataSource = RemoveUnnecessaryFields(availableProjects);
            if (isFirstCall)
            {
                var item = availableProjectsDataSource != null ? availableProjectsDataSource[0] : null;

                if (item != null)
                {
                    foreach (PropertyInfo prop in item.GetType().GetProperties())
                    {
                        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        if (type == typeof(System.Collections.Generic.List<string>))
                        {
                            datagrid.Columns.Add(new DataGridComboBoxColumn() { Header = GetHeaderName(prop.Name) });
                        }
                        else
                        {
                            datagrid.Columns.Add(new DataGridTextColumn() { Header = GetHeaderName(prop.Name), Binding = new Binding(prop.Name) });
                        }
                    }
                }
            }
            var selected = selectedItem; //make a local copy;
            datagrid.ItemsSource = availableProjectsDataSource;
            if(selected != null)
                datagrid.SelectedItem = availableProjectsDataSource?.Find(x=>x.project_id == selected.project_id && x.projdet_version == selected.projdet_version); 
            datagrid.Visibility = Visibility.Visible;
            manageStack.Visibility = Visibility.Visible;
        }

        private List<ProjectListUI> RemoveUnnecessaryFields(List<ProjectList> projects)
        {
            if (projects == null)
                return null;
            foreach(var item in projects)
            {
                var proj = downloadedProjects.Find(x => x.project_serverid == item.project_id && x.project_version == item.projdet_version);
                var isExist = proj != null;
                if (isExist)
                {
                    item.projstatus_name = "AVAILABLE";
                    item.project_localId = proj.project_id;
                }
                else
                {
                    item.project_localId = -1;
                }
            }
            var result = JsonConvert.DeserializeObject<List<ProjectListUI>>(JsonConvert.SerializeObject(projects));
            return result;
        }


        


        #region == API and Auth Events ==
        private async Task Login()
        {
            await authApiViewModel.ExecuteLoginAsync();
            ResetTokenOrError();
            SetWelcomeMessage(false);
        }
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            await authApiViewModel.ExecuteLogoutAsync();
            ResetTokenOrError();
            SetWelcomeMessage(true);
        }

        private void ResetTokenOrError()
        {
            if (!string.IsNullOrEmpty(authApiViewModel.GetError()))
            {
                txtError.Text = authApiViewModel.GetError();
                txtError.Foreground = Brushes.Red;
                btnLogin.Visibility = Visibility.Visible;
            }
            else
            {
                txtError.Text = "";
                txtError.Foreground = Brushes.Green;
                btnLogin.Visibility = Visibility.Hidden;
            }

            if (!string.IsNullOrEmpty(authApiViewModel.GetToken()))
            {
                btnLogout.IsEnabled = true;
            }
            else
            {
                btnLogout.IsEnabled = false;
            }
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

        private async Task SyncPlanningHead()
        {
            var data = await authApiViewModel.GetAllPlanningHead();
            if (data == null) return;
            SyncDbHelper.SyncPlanningHead(data);
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

        #endregion

        #region == Events ==

        private void SetWelcomeMessage(bool isLogOut = false)
        {
            if(isLogOut) { this.Title = "Video Creator"; return;  }
            var userId = authApiViewModel.GetLoggedInUser();
            if (string.IsNullOrEmpty(userId))
                this.Title = "Video Creator";
            else
            {
                this.Title = $"Video Creator, Logged in as - {userId}";
            }
        }

        private bool PreValidations()
        {
            if (selectedItem == null)
            {
                MessageBox.Show("Please select one recrod from Grid", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            else if(selectedItem.projstatus_name != "AVAILABLE")
            {
                MessageBox.Show("You need to download the project first to start working on it.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private async void BtnManageTimeline_Click(object sender, RoutedEventArgs e)
        {
            if (PreValidations() == false)
                return;

            if (manageTimelineWindow != null)
            {
                MessageBox.Show("Another Window is already opened", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CBVProject cbvProject = DataManagerSqlLite.GetProjectById(selectedItem.project_localId, true);
            CBVProjdet cbvProjDet = cbvProject.projdet_data?.Find(x => x.projdet_version == selectedItem.projdet_version);
            
            var selectedProjectEvent = new SelectedProjectEvent
            {
                projectId = selectedItem?.project_localId ?? -1,
                serverProjectId = selectedItem?.project_id ?? -1,
                projdetId = cbvProjDet?.projdet_id ?? -1,
                serverProjdetId = cbvProjDet?.projdet_serverid ?? -1,
            };
            string obj = JsonConvert.SerializeObject(selectedProjectEvent);
            var readonlyFlag = selectedItem.projstatus_name == "AVAILABLE" && selectedItem.current_version == false;
            var manageTimeline_UserControl = new ManageTimeline_UserControl(selectedProjectEvent, authApiViewModel, readonlyFlag);

            manageTimelineWindow = new Window
            {
                Title = "Manage Timeline",
                Content = manageTimeline_UserControl,
                WindowState = WindowState.Maximized,
            };
            manageTimelineWindow.Closed += (object s, EventArgs er) =>
            {
                manageTimeline_UserControl.Dispose();
                manageTimelineWindow = null;
            };
            manageTimelineWindow.Show();
            await InitialiseAndRefreshScreen();
        }

        private async void BtnDownloadLocally_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemFull = await authApiViewModel.GetProjectById(selectedItem.project_id);

            if (selectedItemFull != null)
            {
                LoaderHelper.ShowLoader(this, loader);
                var projectLocalId = SyncDbHelper.UpsertProject(selectedItemFull, selectedItem.projdet_version);

                //Lets download planning and insert it as well
                var plannings = await authApiViewModel.GetPlanningsByProjectId(selectedItem.project_id);
                await SyncDbHelper.UpsertPlanning(plannings, projectLocalId, selectedItemFull, authApiViewModel);

                await InitialiseAndRefreshScreen();

                LoaderHelper.HideLoader(this, loader);
            }
            else
                MessageBox.Show("Something went wrong, please try again later", "Download Error", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private void BtnViewPlanning_Click(object sender, RoutedEventArgs e)
        {
            if (PreValidations() == false)
                return;

            var planning = DataManagerSqlLite.GetPlanning(selectedItem.project_localId);
            MessageBox.Show($"{planning?.Count} rows found in planning for projectId = {selectedItem.project_id}", "Planning Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            await InitialiseAndRefreshScreen();
            LoaderHelper.HideLoader(this, loader);
        }

        


        //private void BtnInsertLocAudio_Click(object sender, RoutedEventArgs e)
        //{
        //    InitialiseDbHelper.PopulateLOCAudioTable();
        //}

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
            selectedItem = datagrid.SelectedItem != null ? (ProjectListUI)datagrid.SelectedItem : null;
            btnManageTimelineButton.IsEnabled = selectedItem != null && selectedItem.projstatus_name == "AVAILABLE";
            btnDownloadProject.IsEnabled = selectedItem != null && selectedItem.projstatus_name != "AVAILABLE";
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