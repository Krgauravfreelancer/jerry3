using dbTransferUser_UserControl.ResponseObjects.Projects;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FileIO = System.IO;

namespace AuthenitcationProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private APIViewModel viewModel;
        const string AUTHCONTROLDLL = "Authentication_UserControl.dll";
        const string DBTRANSFERCONTROLDLL = "dbTransferUser_UserControl.dll";
        public MainWindow()
        {
            InitializeComponent();

            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
           
            string dllStr = String.Empty;
            string assemblyPath = FileIO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(assemblyPath + @"\"+ AUTHCONTROLDLL))
            {                   
                dllStr = AUTHCONTROLDLL;
            }
            else if (!File.Exists(assemblyPath + @"\" + DBTRANSFERCONTROLDLL))
            {
                dllStr = DBTRANSFERCONTROLDLL;
            }

            if(dllStr != string.Empty) 
            {
                //DLL not found
                MessageBox.Show($"The {dllStr} was not found in the executing assembly. This application shall exit", "DLL Not Found",
                                                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            

            viewModel = new APIViewModel();
            DataContext = viewModel;
        }

        
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(viewModel.TokenNumber))
            {
                if (viewModel.TokenNumber == "logged out")
                {
                    txtBlockLoginStatus.Text = "Logged Out!";
                    txtBlockLoginStatus.Foreground = Brushes.Red;
                }

                txtBlockLoginStatus.Text = "Logged In!";
                txtBlockLoginStatus.Foreground = Brushes.Green;
            }
        }

        private async void btnCreateProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            Random rand =  new Random();
            int number = rand.Next(0, 1000);
            string name = "Project-"+number.ToString();
            await viewModel.CreateProject(name,"2","1","1","this is a test","2");
        }

        private async void btnUpdateProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            Random rand = new Random();
            int number = rand.Next(0, 1000);
            string name = "Project-" + number.ToString();

            if(lstProjects.SelectedItem!=null)
            {
                var proj = (ProjectModel)lstProjects.SelectedItem;
                var projectId = proj.ProjectId;
                await viewModel.UpdateProject(projectId,name, "2", "1", "1", "test update", "2");

                //await viewModel.UpdateProject(projectId,"1", "test update");

            }
            else
            {
                MessageBox.Show("Login first or choose an item from the project list to update", "Select a project", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }             
        }

        private async void btnUpdateShortProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            Random rand = new Random();
            int number = rand.Next(0, 1000);
            string name = "Project-" + number.ToString();

            if (lstProjects.SelectedItem != null)
            {
                var proj = (ProjectModel)lstProjects.SelectedItem;
                var projectId = proj.ProjectId;

                await viewModel.UpdateProject(projectId, "1", "test update");

            }
            else
            {
                MessageBox.Show("Login first or choose an item from the project list to update", "Select a project",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnGetProjectCount_Click(object sender, RoutedEventArgs e)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string dateString = "2023-06-14 09:42:07";
            DateTime dt = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", provider);
            await viewModel.GetProjectCount(dt);
        }

        private async void btnProjectOwnerShip_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.GetOwnershipOfProjects("2,3,4,50,51,52,58");
        }

        private async void btnIsAssigned_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.IsAssignedToAnyProject();
        }

        private async void btnGetMedia_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.GetAllMedia();
        }

        private async void btnGetScreens_Click(object sender, RoutedEventArgs e)
        {
            await viewModel.GetAllScreens();
        }









        //private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    using (var stream = Assembly.GetExecutingAssembly().get.GetManifestResourceStream("AuthenitcationProject.SFTP_UserControl.dll"))
        //    {
        //        byte[] assemblyData = new byte[stream.Length];
        //        stream.Read(assemblyData,0, assemblyData.Length);
        //        return Assembly.Load(assemblyData);
        //    }
        //}
    }
}
