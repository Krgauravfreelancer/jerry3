using dbTransferUser_UserControl.ResponseObjects.Projects;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AuthenitcationProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly APIViewModel apiViewModel;
        private bool _IsReady = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckIfDLLExists();
            apiViewModel = new APIViewModel();
            DataContext = apiViewModel;
            txtAccessKey.Text = apiViewModel.GetAccessKey();
            _ = Login();
        }

        public bool IsReady
        {
            get { return _IsReady; }
            set
            {
                _IsReady = value;
            }
        }

        private void CheckIfDLLExists()
        {
            string AUTHCONTROLDLL = "Authentication_UserControl.dll";
            string DBTRANSFERCONTROLDLL = "dbTransferUser_UserControl.dll";
            string dllStr = string.Empty;
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(assemblyPath + @"\" + AUTHCONTROLDLL))
            {
                dllStr = AUTHCONTROLDLL;
            }
            else if (!File.Exists(assemblyPath + @"\" + DBTRANSFERCONTROLDLL))
            {
                dllStr = DBTRANSFERCONTROLDLL;
            }

            if (dllStr != string.Empty)
            {
                MessageBox.Show($"The {dllStr} was not found in the executing assembly. This application shall exit", "DLL Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void ResetTokenOrError()
        {
            if (!string.IsNullOrEmpty(apiViewModel.GetError()))
            {
                txtBlockError.Text = apiViewModel.GetError();
                txtBlockError.Foreground = Brushes.Red;
            }
            else
            {
                txtBlockError.Text = "No Error !!!";
                txtBlockError.Foreground = Brushes.Green;
            }

            if (!string.IsNullOrEmpty(apiViewModel.GetToken()))
            {
                txtTokenNumber.Text = apiViewModel.GetToken();
                txtBlockLoginStatus.Text = "Logged In!";
                txtBlockLoginStatus.Foreground = Brushes.Green;
            }
            else
            {
                txtBlockLoginStatus.Text = "Not Logged In!";
                txtTokenNumber.Text = "";
                txtBlockLoginStatus.Foreground = Brushes.Red;
            }
            lstProjects.ItemsSource = apiViewModel.AllProjects;
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResetTokenOrError();
            txtTokenNumber.Text = string.Empty;
        }

        #region == Authenticate Event Click ==

        private async Task Login()
        {
            await apiViewModel.ExecuteLoginAsync();
            ResetTokenOrError();
        }
        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ExecuteLogoutAsync();
            ResetTokenOrError();
        }

        #endregion

        #region == Project Click Events ==

        private async void BtnListProjects_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ExecuteLoadProjectsAsync();
            ResetTokenOrError();
        }

        private async void BtnCreateProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            var rand = new Random();
            int number = rand.Next(0, 1000);
            string name = "Project - " + number.ToString();
            await apiViewModel.CreateProject(name, 2, 1, 1, "this is a test comment");
            ResetTokenOrError();
        }

        private async void BtnUpdateProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            Random rand = new Random();
            int number = rand.Next(0, 1000);
            string name = "Project - " + number.ToString() + " updated";

            if (lstProjects.SelectedItem != null)
            {
                var proj = (ProjectModel)lstProjects.SelectedItem;
                await apiViewModel.UpdateProject(proj.project_id, name, 2, 1, 1, "this is a test comment - Updated");
            }
            else
            {
                MessageBox.Show("Login first or choose an item from the project list to update", "Select a project", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            ResetTokenOrError();
        }

        private async void BtnPatchProject_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            Random rand = new Random();
            int number = rand.Next(0, 1000);
            string name = "Project - " + number.ToString() + " patched";

            if (lstProjects.SelectedItem != null)
            {
                var proj = (ProjectModel)lstProjects.SelectedItem;
                var projectId = proj.project_id;
                await apiViewModel.PatchProject(projectId, 1, "this is a test comment - Patched");
            }
            else
            {
                MessageBox.Show("Login first or choose an item from the project list to update", "Select a project", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            ResetTokenOrError();
        }

        private async void BtnGetProjectCount_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetProjectCount();
            ResetTokenOrError();
        }

        private async void BtnProjectOwnerShip_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetOwnershipOfProjects("2,3,4,50,51,52,58");
            ResetTokenOrError();
        }

        private async void BtnIsAssigned_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.IsAssignedToAnyProject();
            ResetTokenOrError();
        }

        #endregion

        #region == Master Table ==
        private async void BtnGetApp_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetAllApp();
            ResetTokenOrError();
        }

        private async void BtnGetMedia_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetAllMedia();
            ResetTokenOrError();
        }

        private async void BtnGetScreens_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetAllScreens();
            ResetTokenOrError();
        }

        #endregion

        #region == Company Click Events ==
        private async void BtnListCompany_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllCompany();
            ResetTokenOrError();
        }
        private async void BtnCreateCompany_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateCompany("company name new");
            ResetTokenOrError();
        }

        private async void BtnUpdateCompany_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateCompany(3, "company name new - updated");
            ResetTokenOrError();
        }
        
        #endregion

        #region == Background Click Events ==
        private async void BtnListBackground_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllBackground();
            ResetTokenOrError();
        }
        //private async void BtnGetBackgroundById_Click(object sender, RoutedEventArgs e)
        //{
        //    common_Click(sender, e);
        //    //await apiViewModel.ListBackgroundById(1);
        //    //ResetTokenOrError();

        //}
        private async void BtnCreateBackground_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateBackground(1, 1, "Image1.jpg");
            ResetTokenOrError();
        }

        private async void BtnUpdateBackground_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateBackground(1, 1, 1);
            ResetTokenOrError();
        }

        private async void BtnDownloadBinary_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.DownloadBackground(1, "jpg");
            ResetTokenOrError();
        }

        #endregion

        #region == Video Events Click Events ==
        private async void BtnListVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllVideoEvent();
            ResetTokenOrError();
        }
        private async void BtnGetVideoEventById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListVideoEventById(2, 3);
            ResetTokenOrError();
        }
        private async void BtnCreateVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateVideoEvent(2, 1, 2, "00:01:10", 100);
            ResetTokenOrError();
        }
        
        private async void BtnUpdateVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateVideoEvent(2, 7, 1, 2, "00:02:20", 200);
            ResetTokenOrError();
        }

        private async void BtnPatchVideoEvent_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.PatchVideoEvent(2, 7, 1, "00:03:30", 300);
            ResetTokenOrError();
        }
        #endregion

        #region == Audio click events ==

        private async void BtnListAudio_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllAudio(2);
            ResetTokenOrError();
        }
        private async void BtnGetAudioById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetAudioById(2, 1);
            ResetTokenOrError();
        }
        private async void BtnCreateAudio_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateAudio(6, "Audio1.mp3");
            ResetTokenOrError();
        }
        private async void BtnUpdateAudio_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateAudio(2, 1, "Audio1.mp3");
            ResetTokenOrError();
        }
        private async void BtnGetAudioBinary_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.DownloadAudioBinary(1, "mp3");
            ResetTokenOrError();
        }
        private void BtnUploadAudioBinary_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Not Needed !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region == Notes Click Events ==
        private async void BtnListNotes_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllNotes(2);
            ResetTokenOrError();
        }
        private async void BtnGetNotesById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetNotesById(2, 1);
            ResetTokenOrError();
        }
        private async void BtnCreateNotes_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateNotes(2,"Testing Notes", 2, 1);
            ResetTokenOrError();
        }

        private async void BtnUpdateNotes_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateNotes(2, 2, "Testing Notes Updated", 3, 2);
            ResetTokenOrError();
        }

        private async void BtnPatchNotes_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.PatchNotes(2, 2, "Testing Notes Patched");
            ResetTokenOrError();
        }
        #endregion

        #region == Video Segments Click Events ==
        private async void BtnListVideoSegment_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllVideoSegments(2);
            ResetTokenOrError();
        }
        private async void BtnVideoSegmentById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetVideoSegmentById(2, 1);
            ResetTokenOrError();
        }
        private async void BtnCreateVideoSegmentImage_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateVideoSegmentBinary(2, "Image1.jpg");
            ResetTokenOrError();
        }

        private async void BtnCreateVideoSegmentVideo_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateVideoSegmentBinary(2, "Screencast1.mp4");
            ResetTokenOrError();
        }

        private async void BtnUpdateVideoSegmentBinary_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateVideoSegmentBinary(2, 1, "Screencast1.mp4");
            ResetTokenOrError();
        }

        private async void BtnGetBinaryVideoSegment_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.DownloadBinaryVideoSegment(1, "mp4");
            ResetTokenOrError();
        }

        private async void BtnPatchVideoSegment_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.PatchVideoSegment(2,1);
            ResetTokenOrError();
        }

            

        #endregion

        #region == Design Click Events ==
        private async void BtnListDesign_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListAllDesigns(2);
            ResetTokenOrError();
        }
        private async void BtnGetDesignById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.ListDesignById(2, 1);
            ResetTokenOrError();
        }
        private async void BtnCreateDesign_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateDesign(2, 1, "<xml>some xml content here</xml>");
            ResetTokenOrError();
        }

        private async void BtnUpdateDesign_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateDesign(2, 1, 2, 2, "<xml>some xml content here - Updated</xml>");
            ResetTokenOrError();
        }

        private async void BtnPatchDesign_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.PatchDesign(2, 1, 2, "<xml>some xml content here - Patched</xml>");
            ResetTokenOrError();
        }
        #endregion 

        #region == MP4 Click Events ==
        
        private async void BtnGetFinalMp4ById_Click(object sender, RoutedEventArgs e)
        {
            await apiViewModel.GetFinalMp4ById(1, 1);
            ResetTokenOrError();
        }
        private async void BtnCreateMp4_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.CreateFinalMp4(1, 1, "test Final Mp4 comments", "Screencast1.mp4");
            ResetTokenOrError();
        }
    

        private async void BtnUpdateFinalMp4_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.UpdateFinalMp4(1, 2, 2, "test Final Mp4 comments", "Screencast1.mp4");
            ResetTokenOrError();
        }

        private async void BtnDownloadFinalMp4_Click(object sender, RoutedEventArgs e)
        {
            //Test code
            await apiViewModel.DownloadFinalMp4(1, "mp4");
            ResetTokenOrError();
        }
        #endregion 

        private void common_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Coming soon !!!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await apiViewModel.ExecuteLogoutAsync();
        }
    }
}
