using GalaSoft.MvvmLight;
using Authentication_UserControl;
using dbTransferUser_UserControl;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Collections.ObjectModel;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using System.Xml.Linq;
using System.Globalization;

namespace AuthenitcationProject
{
    internal class APIViewModel : ViewModelBase
    {
        //private string token;
        private IAuthControl authCtrl; //authentication DLL
        private IDBTransferControl dbTransferCtrl;

        //string MACADDRESS = "00:00:00:00:00:05";
        const string APIKEY = "6Lwr_)@e{3GVSF58D#JiX4]5)%peA[";

        const string dateString = "2023-06-14 09:42:07";
        DateTime dtime = DateTime.ParseExact(dateString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(ref _isBusy, value);
        }

        #region UI Variables
        private string _tokenNumber;        
        private string _macAddress;
        private string _accessKey;
        private ObservableCollection<ProjectModel> _allProjects;
        private string _projectString;
        public IAsyncCommand Login { get; private set; }
        public IAsyncCommand Logout { get; private set; }
        public IAsyncCommand LoadProjects { get; private set; }

        
        public string TokenNumber
        {
            get => _tokenNumber;
            set => Set(nameof(TokenNumber),ref _tokenNumber, value);
        }
       
        public string MacAddress { get => _macAddress; set => Set(nameof(MacAddress), ref _macAddress, value); }
        public string AccessKey { get => _accessKey; set => Set(nameof(AccessKey), ref _accessKey, value); }
        public string ProjectString { get => _projectString; set => Set(nameof(ProjectString), ref _projectString, value); }
        public ObservableCollection<ProjectModel> AllProjects { get => _allProjects; 
                                                    set => Set(nameof(AllProjects),ref _allProjects, value);

                                                        }
        #endregion

        public APIViewModel()
        {
            InitValues();
            Login = new AsyncCommand(ExecuteLoginAsync, CanExecuteLogin);
            Logout = new AsyncCommand(ExecuteLogoutAsync, CanExecuteLogout);

            LoadProjects = new AsyncCommand(ExecuteLoadProjectsAsync, CanExecuteLoadProjects);

        }

        private void InitValues()
        {
            //ReadRegistry();
            //UserName = "mafaz";
            //Password = "mafaz@password1";
            ReadMACAddress();
            //MacAddress = MACADDRESS;
            AccessKey = APIKEY;
        }

        private void ReadMACAddress()
        {
            string macAddress = "";

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    var hex = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                    macAddress = hex.Replace("-", ":");
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        MacAddress = macAddress;
                        break;
                    }
                }
            }

            //for testing purposes
            //MacAddress = MACADDRESS;
        }

       

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;                
                //ProjectString = string.Empty;

                authCtrl = new AuthControl();
                authCtrl.InitConnection(); //Call this to initiate the connection.
                                           //Needed to call only once
                var loginRes = await authCtrl.Login(MacAddress?.Trim(),AccessKey?.Trim());                
                
                if(!string.IsNullOrEmpty(loginRes.Token))
                {
                    TokenNumber = loginRes.Token;
                    dbTransferCtrl = new DBTransferControl(authCtrl.GetHttpClient());
                }
                else
                    TokenNumber = "No Token returned";
            }
            catch(DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch(Exception ex)
            {
                TokenNumber = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecuteLogoutAsync()
        {
            try
            {
                IsBusy = true;
                //TokenNumber = string.Empty;
                ProjectString = string.Empty;

                //authCtrl = new AuthControl();
                //authCtrl.InitConnection(); //Call this to initiate the connection.
                                           //Needed to call only once
                var logoutRes = await authCtrl.Logout();

                if (!string.IsNullOrEmpty(logoutRes.Status))
                {
                    TokenNumber = logoutRes.Status;
                    //dbTransferCtrl = new DBTransferControl(authCtrl.GetHttpClient());
                }
                //else
                //    TokenNumber = "No Token returned";
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (Exception ex)
            {
                TokenNumber = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteLoadProjectsAsync()
        {
            try
            {
                IsBusy = true;
                //ProjectString = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                //For Test Purposes  - Can be removed later
#if DEBUG
                //authCtrl.SetToken(TokenNumber?.Trim());
                //dbTransferCtrl.SetClient(authCtrl.GetHttpClient()); //Update reference
#endif
                

                var loadProjects = await dbTransferCtrl.GetProjects(dtime); //Call the method

                //var projects = loadProjects.Data.ForEach(data => data)

                AllProjects = new ObservableCollection<ProjectModel>(loadProjects);

                
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanExecuteLogin()
        {
            return !IsBusy;
        }
        private bool CanExecuteLogout()
        {
            return !IsBusy;
        }
        private bool CanExecuteLoadProjects()
        {
            return !IsBusy;
        }

        internal async Task CreateProject(string formname,
                                            string formsection,
                                            string formprojstatus,
                                            string formversion,
                                            string formcomments,
                                            string formcurrentassigned)
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;
               
                var result = await dbTransferCtrl.CreateProject(formname,formsection,formprojstatus,
                                                                formversion,formcomments,formcurrentassigned);
                if(result.Status == "success")
                {
                    MessageBox.Show(result.Message,"Project Creation Success",
                                   MessageBoxButton.OK,MessageBoxImage.Information);
                    
                    var loadProjects = await dbTransferCtrl.GetProjects(dtime); //Call the method
                    AllProjects = new ObservableCollection<ProjectModel>(loadProjects);
                }
                else 
                {
                    MessageBox.Show(result.Message, "Project Creation Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task UpdateProject(int projectId,string formname,
                                                string formsection,
                                                string formprojstatus,
                                                string formversion,
                                                string formcomments,
                                                string formcurrentassigned)
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.UpdateProject(projectId,formname, formsection, formprojstatus,
                                                                formversion, formcomments, formcurrentassigned);
                if (result.Status == "success")
                {
                    MessageBox.Show(result.Message, "Project Update Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    var loadProjects = await dbTransferCtrl.GetProjects(dtime); //Call the method
                    AllProjects = new ObservableCollection<ProjectModel>(loadProjects);
                }
                else
                {
                    MessageBox.Show(result.Message, "Project Update Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }


        }

        internal async Task UpdateProject(int projectId, string formversion,
                                               string formcomments)
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.UpdateProject(projectId,formversion, formcomments);
                if (result.Status == "success")
                {
                    MessageBox.Show(result.Message, "Project Update Success",
                                   MessageBoxButton.OK, MessageBoxImage.Information);

                    var loadProjects = await dbTransferCtrl.GetProjects(dtime); //Call the method
                    AllProjects = new ObservableCollection<ProjectModel>(loadProjects);
                }
                else
                {
                    MessageBox.Show(result.Message, "Project Update Error",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }


        }

        internal async Task GetProjectCount(DateTime dateTime)
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.GetProjectCount(dateTime);
                MessageBox.Show($"Available number of projects: {result}", "Project Count", 
                                MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task GetOwnershipOfProjects(string projectIds)
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.GetOwnershipOfProjects(projectIds);
                //MessageBox.Show($"Available number of projects: {result}", "Project Count",
                //                MessageBoxButton.OK, MessageBoxImage.Information);
               StringBuilder builder = new StringBuilder();
               foreach(var keyPair in result) 
                {
                    builder.Append(keyPair.Key + ":" + keyPair.Value+"\n");
                }
                MessageBox.Show(builder.ToString(), "Projects owned");
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task IsAssignedToAnyProject()
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.IsAssignedToProjects();
                //MessageBox.Show($"Available number of projects: {result}", "Project Count",
                //                MessageBoxButton.OK, MessageBoxImage.Information);

                MessageBox.Show("Assigned to any projects: " + result.ToString(),
                    "Projects Assigned");
                  
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task GetAllMedia()
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.GetAllMedia();
                //MessageBox.Show($"Available number of projects: {result}", "Project Count",
                //                MessageBoxButton.OK, MessageBoxImage.Information);
                StringBuilder builder = new StringBuilder();
                foreach (var keyPair in result)
                {
                    builder.Append(keyPair.MediaName + ":" + keyPair.MediaColor + "\n");
                }
                MessageBox.Show(builder.ToString(), "All Media");
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        internal async Task GetAllScreens()
        {
            try
            {
                IsBusy = true;
                TokenNumber = string.Empty;

                authCtrl.SetMACAddress(this.MacAddress);
                var client = authCtrl.GetHttpClient();
                dbTransferCtrl.SetClient(client);

                var result = await dbTransferCtrl.GetAllScreens();
                //MessageBox.Show($"Available number of projects: {result}", "Project Count",
                //                MessageBoxButton.OK, MessageBoxImage.Information);
                StringBuilder builder = new StringBuilder();
                foreach (var keyPair in result)
                {
                    builder.Append(keyPair.ScreenName + ":" + keyPair.ScreenColor + "\n");
                }
                MessageBox.Show(builder.ToString(), "All Screens");
            }
            catch (DllNotFoundException dllex)
            {
                TokenNumber = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Please login first", "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
