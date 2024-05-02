﻿using Newtonsoft.Json;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Projects;
using Sqllite_Library.Business;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using VideoCreator.Auth;
using VideoCreator.Helpers;
using VideoCreator.Models;

namespace VideoCreator.XAML
{
    public partial class MainWindow : Window, IDisposable
    {
        private readonly AuthAPIViewModel authApiViewModel;

        private List<ProjectList> availableProjects;
        private List<CBVProjectForJoin> downloadedProjects;
        private List<ProjectListUI> availableProjectsDataSource;
        private ProjectListUI selectedItem;
        private SelectedProjectEvent selectedProjectEvent;
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
            await Login();
            await SyncMasterTablesAndRefresh();
           
            LoaderHelper.HideLoader(this, loader);
        }

        #region == Sync Section ==

        private async Task SyncMasterTablesAndRefresh()
        {
            LogManagerHelper.WriteVerboseLog("SyncMasterTables Started");
            var sqlCon = SyncDbHelper.InitializeDatabaseAndGetConnection();
            using (var transaction = sqlCon.BeginTransaction())
            {
                try
                {
                    await SyncApp(sqlCon);
                    await SyncMedia(sqlCon);
                    await SyncCompany(sqlCon);
                    await SyncBackground(sqlCon);
                    await SyncScreens(sqlCon);
                    await InitialiseAndRefreshScreen(sqlCon);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
                sqlCon?.Close();
            }
            LogManagerHelper.WriteVerboseLog("SyncMasterTables Completed");
        }

        private async Task SyncApp(SQLiteConnection sqlCon)
        {
            var data = await authApiViewModel.GetAllApp();
            if (data == null) return;
            SyncDbHelper.SyncAppForTransaction(data, sqlCon);
        }

        private async Task SyncMedia(SQLiteConnection sqlCon)
        {
            var data = await authApiViewModel.GetAllMedia();
            if (data == null) return;
            SyncDbHelper.SyncMediaForTransaction(data, sqlCon);
        }

        private async Task SyncScreens(SQLiteConnection sqlCon)
        {
            var data = await authApiViewModel.GetAllScreens();
            if (data == null) return;
            SyncDbHelper.SyncScreenForTransaction(data, sqlCon);
        }

        private async Task SyncCompany(SQLiteConnection sqlCon)
        {
            var data = await authApiViewModel.GetAllCompany();
            if (data == null) return;
            SyncDbHelper.SyncCompanyForTransaction(data, sqlCon);
        }

        private async Task SyncBackground(SQLiteConnection sqlCon)
        {
            var data = await authApiViewModel.GetAllBackground();
            if (data == null) return;
            var result = new List<BackgroundModel>();
            result.Add(data);
            SyncDbHelper.SyncBackgroundForTransaction(result, authApiViewModel, sqlCon);
        }

        #endregion

        private string GetHeaderName(string name)
        {
            if (name.ToLower() == "project_id".ToLower())
                return "ID";
            else if (name.ToLower() == "project_videotitle".ToLower())
                return "TITLES";
            else if (name.ToLower() == "projstatus_name".ToLower())
                return "STATUS";
            else if (name.ToLower() == "project_downloaded".ToLower())
                return "DOWNLOADED";
            else if (name.ToLower() == "current_version".ToLower())
                return "CURRENT VERSION";
            else if (name.ToLower() == "projdet_version".ToLower())
                return "VERSION";
            else if (name.ToLower() == "project_localId".ToLower())
                return "LOCAL ID";
            else
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
        }

        private int GetWidth(string name)
        {
            if (name.ToLower() == "project_id".ToLower())
                return 75;
            else if (name.ToLower() == "project_videotitle".ToLower())
                return 500;
            else if (name.ToLower() == "projstatus_name".ToLower())
                return 150;
            else if (name.ToLower() == "project_downloaded".ToLower())
                return 150;
            else if (name.ToLower() == "current_version".ToLower())
                return 150;
            else if (name.ToLower() == "projdet_version".ToLower())
                return 100;
            return 50;
        }

        private Style GetStyle(string name)
        {
            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter(HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(BackgroundProperty, (SolidColorBrush)new BrushConverter().ConvertFrom("#3381CC")));
            style.Setters.Add(new Setter(ForegroundProperty, new SolidColorBrush(Colors.White)));
            style.Setters.Add(new Setter(BorderBrushProperty, new SolidColorBrush(Colors.White)));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(0.5)));
            style.Setters.Add(new Setter(PaddingProperty, new Thickness(5)));
            style.Setters.Add(new Setter(PaddingProperty, new Thickness(5)));
            style.Setters.Add(new Setter(FontSizeProperty, (double)13));
            style.Setters.Add(new Setter(FontWeightProperty, FontWeight.FromOpenTypeWeight(700)));
            return style;
        }

        private async Task InitialiseAndRefreshScreen(SQLiteConnection sqlCon)
        {
            downloadedProjects = DataManagerSqlLite.GetDownloadedProjectListForTransaction(sqlCon);
            availableProjects = await authApiViewModel.GetAvailableProjectsData();
            availableProjectsDataSource = RemoveUnnecessaryFields(availableProjects);
            var item = availableProjectsDataSource != null ? availableProjectsDataSource[0] : null;
            if (item != null)
            {
                foreach (PropertyInfo prop in item.GetType().GetProperties())
                {
                    var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (prop.Name == "project_localId")
                    { }
                    else
                    {
                        if (type == typeof(System.Collections.Generic.List<string>))
                        {
                            datagrid.Columns.Add(new DataGridComboBoxColumn() { Header = GetHeaderName(prop.Name) });
                        }
                        else
                        {
                            datagrid.Columns.Add(new DataGridTextColumn()
                            {
                                Header = GetHeaderName(prop.Name),
                                Binding = new Binding(prop.Name),
                                Width = GetWidth(prop.Name),
                                HeaderStyle = GetStyle(prop.Name)
                            });
                        }
                    }
                }
            }
            var selected = selectedItem; //make a local copy;
            datagrid.ItemsSource = availableProjectsDataSource;
            if (selected != null)
                datagrid.SelectedItem = availableProjectsDataSource?.Find(x => x.project_id == selected.project_id && x.projdet_version == selected.projdet_version);
            datagrid.Visibility = Visibility.Visible;
            manageApplicationStack.Visibility = Visibility.Visible;
        }

        private async Task InitialiseAndRefreshScreenForSubsequentCall()
        {
            downloadedProjects = DataManagerSqlLite.GetDownloadedProjectList();
            availableProjects = await authApiViewModel.GetAvailableProjectsData();
            availableProjectsDataSource = RemoveUnnecessaryFields(availableProjects);
           
            var selected = selectedItem; //make a local copy;
            datagrid.ItemsSource = availableProjectsDataSource;
            if (selected != null)
                datagrid.SelectedItem = availableProjectsDataSource?.Find(x => x.project_id == selected.project_id && x.projdet_version == selected.projdet_version);
            datagrid.Visibility = Visibility.Visible;
            manageApplicationStack.Visibility = Visibility.Visible;
        }

        private void GridContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGridCell cell;
            DataGridRow row;

            var dep = FindVisualParentAsDataGridSubComponent((DependencyObject)e.OriginalSource);
            if (dep == null)
                return;

            FindCellAndRow(dep, out cell, out row);
            if (dep is DataGridColumnHeader || dep is DataGridRow)
            {
                e.Handled = true;
                return;
            }

            var selectedRow = row.Item as ProjectListUI;
            if (selectedRow != null)
            {
                ManageProjectMenu.IsEnabled = selectedRow.project_downloaded == "YES";
                SubmitProjectMenu.IsEnabled = selectedRow.project_downloaded == "YES";
                DownloadProjectMenu.IsEnabled = selectedRow.project_downloaded != "YES";
            }

        }

        private static void FindCellAndRow(DependencyObject originalSource, out DataGridCell cell, out DataGridRow row)
        {
            cell = originalSource as DataGridCell;
            if (cell == null)
            {
                row = null;
                return;
            }

            // Walk the visual tree to find the cell's parent row.
            while ((originalSource != null) && !(originalSource is DataGridRow))
            {
                if (originalSource is Visual || originalSource is Visual3D)
                {
                    originalSource = VisualTreeHelper.GetParent(originalSource);
                }
                else
                {
                    // If we're in Logical Land then we must walk up the logical tree 
                    // until we find a Visual/Visual3D to get us back to Visual Land.
                    // See: http://www.codeproject.com/Articles/21495/Understanding-the-Visual-Tree-and-Logical-Tree-in
                    originalSource = LogicalTreeHelper.GetParent(originalSource);
                }
            }

            row = originalSource as DataGridRow;
        }
        
        private static DependencyObject FindVisualParentAsDataGridSubComponent(DependencyObject originalSource)
        {
            // iteratively traverse the visual tree
            while ((originalSource != null) && !(originalSource is DataGridCell)
                && !(originalSource is DataGridColumnHeader) && !(originalSource is DataGridRow))
            {
                if (originalSource is Visual || originalSource is Visual3D)
                {
                    originalSource = VisualTreeHelper.GetParent(originalSource);
                }
                else
                {
                    // If we're in Logical Land then we must walk 
                    // up the logical tree until we find a 
                    // Visual/Visual3D to get us back to Visual Land.
                    originalSource = LogicalTreeHelper.GetParent(originalSource);
                }
            }

            return originalSource;
        }

        private List<ProjectListUI> RemoveUnnecessaryFields(List<ProjectList> projects)
        {
            if (projects == null)
                return null;
            foreach (var item in projects)
            {
                var proj = downloadedProjects.Find(x => x.project_serverid == item.project_id && x.project_version == item.projdet_version);
                var isExist = proj != null;
                if (isExist)
                {
                    item.project_downloaded = "YES";
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



        #endregion

        #region == Events ==

        private void SetWelcomeMessage(bool isLogOut = false)
        {
            if (isLogOut) { this.Title = "Video Creator"; return; }
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
            else if (selectedItem.project_downloaded != "YES")
            {
                MessageBox.Show("You need to download the project first to start working on it.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            else if (selectedItem.projstatus_name?.ToLower() == "review")
            {
                MessageBox.Show("Project is already submitted.", "READ ONLY", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        
        private async Task PlanningUpdatedFlow(SQLiteConnection sqlCon)
        {
            var isPlanningUpdated = await authApiViewModel.IsPlanningUpdated(selectedItem.project_id);
            if (isPlanningUpdated)
            {
                var selectedItemFull = await authApiViewModel.GetProjectById(selectedItem.project_id);
                if (selectedItemFull != null)
                {
                    MessageBox.Show("Planning Data changed after last fetch, so refreshing planning. This should not take long!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoaderHelper.ShowLoader(this, loader);
                    //Delete Planning data locally
                    DataManagerSqlLite.HardDeletePlanningsByProjectIdForTransaction(selectedProjectEvent.projectId, sqlCon);

                    var serverVideoEventData = await authApiViewModel.GetAllVideoEventsbyProjdetId(selectedProjectEvent);
                    int i = 1;
                    foreach (var item in serverVideoEventData.Data)
                    {
                        LoaderHelper.ShowLoader(this, loader, $"Removing Exisiting {i++}/{serverVideoEventData.Data?.Count} events");
                        var success = await authApiViewModel.HardDeleteVideoEvent(selectedProjectEvent.serverProjectId, selectedProjectEvent.serverProjdetId, item.videoevent_id);
                        DataManagerSqlLite.HardDeleteVideoEventsByServerIdForTransaction(item.videoevent_id, cascadeDelete: true, sqlCon);

                    }
                    //Lets download planning and insert it as well
                    var plannings = await authApiViewModel.GetPlanningsByProjectId(selectedItem.project_id);
                    LoaderHelper.ShowLoader(this, loader, $"Saving new {plannings?.Count} Records");
                    await SyncDbHelper.UpsertPlanningForTransaction(plannings, selectedItem.project_localId, selectedItemFull, authApiViewModel, sqlCon);
                    // await InitialiseAndRefreshScreen();
                    await authApiViewModel.UpdatedPlanningLastUpdated(selectedItem.project_id);

                    LoaderHelper.HideLoader(this, loader);
                }
            }

        }

        private async Task CreatePlanningAutomatically(SQLiteConnection sqlCon)
        {
            //Check if planning events exists or not
            var serverVideoEventData = await authApiViewModel.GetAllVideoEventsbyProjdetId(selectedProjectEvent);
            if (serverVideoEventData?.Data != null)
            {
                var isPlanningEventExist = serverVideoEventData?.Data?.Where(x => x.videoevent_planning > 0 && x.videoevent_isdeleted == false).Any();
                if (isPlanningEventExist.HasValue && isPlanningEventExist.Value == true)
                {
                    // Do nothing
                }
                else
                {
                    // Add Planning Events automatically
                    MessageBox.Show("Adding planning events, This should not take long!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoaderHelper.ShowLoader(this, loader);
                    var trackbarTime = DataManagerSqlLite.GetNextStartForTransaction((int)EnumMedia.VIDEO, selectedProjectEvent.projdetId, sqlCon);
                    var payload = new PlanningEvent
                    {
                        Type = EnumScreen.All,
                        TimeAtTheMoment = trackbarTime
                    };

                    var backgroundImagePath = PlanningHandlerHelper.CheckIfBackgroundPresent(sqlCon);
                    if (backgroundImagePath == null)
                        MessageBox.Show($"No Background found, plannings cannot be added.", "Information", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        await PlanningHandlerHelper.ProcessForTransaction(payload, selectedProjectEvent, authApiViewModel, null, loader, sqlCon, backgroundImagePath);
                    }
                    LoaderHelper.HideLoader(this, loader);
                }
            }
        }

        private async void ManageProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            if (PreValidations() == false)
                return;

            if (manageTimelineWindow != null)
            {
                MessageBox.Show("Another Window is already opened", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selectedProjectEvent == null)
                return;

            //string obj = JsonConvert.SerializeObject(selectedProjectEvent);
            var readonlyFlag = selectedItem.project_downloaded == "YES" && selectedItem.current_version == false;
            if (!readonlyFlag)
            {
                var sqlCon = SyncDbHelper.InitializeDatabaseAndGetConnection();
                using (var transaction = sqlCon.BeginTransaction())
                {
                    try
                    {
                        await PlanningUpdatedFlow(sqlCon);
                        await CreatePlanningAutomatically(sqlCon);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    transaction.Commit();
                    sqlCon?.Close();
                }
            }

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
            await InitialiseAndRefreshScreenForSubsequentCall();
        }

        private async void DownloadProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemFull = await authApiViewModel.GetProjectById(selectedItem.project_id);
            if (selectedItemFull != null)
            {
                var sqlCon = SyncDbHelper.InitializeDatabaseAndGetConnection();
                using (var transaction = sqlCon.BeginTransaction())
                {
                    try
                    {
                        LoaderHelper.ShowLoader(this, loader);
                        var projectLocalId = SyncDbHelper.UpsertProjectForTransaction(selectedItemFull, selectedItem.projdet_version, sqlCon);

                        //Lets download planning and insert it as well
                        LoaderHelper.ShowLoader(this, loader, "Downloading Planning data");
                        var plannings = await authApiViewModel.GetPlanningsByProjectId(selectedItem.project_id);
                        await SyncDbHelper.UpsertPlanningForTransaction(plannings, projectLocalId, selectedItemFull, authApiViewModel, sqlCon);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    transaction.Commit();
                    sqlCon?.Close();
                    await InitialiseAndRefreshScreenForSubsequentCall();
                    await authApiViewModel.UpdatedPlanningLastUpdated(selectedItem.project_id);
                    LoaderHelper.HideLoader(this, loader);
                }
            }
            else
                MessageBox.Show("Something went wrong, please try again later", "Download Error", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        private async void SubmitProjectMenu_Click(object sender, RoutedEventArgs e)
        {
            var submitProjectMessage = await authApiViewModel.SubmitProject(selectedProjectEvent);
            //DataManagerSqlLite.SetProjectDetailSubmitted(selectedProjectEvent.projdetId);
            MessageBox.Show($"{submitProjectMessage}", "Submit Project", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoaderHelper.ShowLoader(this, loader);
            await InitialiseAndRefreshScreenForSubsequentCall();
            LoaderHelper.HideLoader(this, loader);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            if (selectedItem != null)
            {
                CBVProject cbvProject = DataManagerSqlLite.GetProjectById(selectedItem.project_localId, true);
                CBVProjdet cbvProjDet = cbvProject?.projdet_data?.Find(x => x.projdet_version == selectedItem.projdet_version);

                selectedProjectEvent = new SelectedProjectEvent
                {
                    projectId = selectedItem?.project_localId ?? -1,
                    serverProjectId = selectedItem?.project_id ?? -1,
                    projdetId = cbvProjDet?.projdet_id ?? -1,
                    serverProjdetId = cbvProjDet?.projdet_serverid ?? -1,
                };
            }
            else
                selectedProjectEvent = null;
            //btnManageTimelineButton.IsEnabled = selectedItem != null && selectedItem.project_downloaded == "YES";
            //btnDownloadProject.IsEnabled = selectedItem != null && selectedItem.project_downloaded != "YES";
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