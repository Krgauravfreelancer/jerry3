using System;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using ServerApiCall_UserControl;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Drawing.Printing;

namespace VideoCreator.Auth
{
    public class VideoCreatorAuthHelper : ViewModelBase
    {
        //private string token;
        public IAuthControl authCtrl; //authentication DLL
        private IDBTransferControl dbTransferCtrl;
        public string TokenNumber { get; set; }
        public string ErrorMessage { get; set; }
        const string NO_LOGIN_MESSAGE = "Please login first !!!";

        #region == Properties ==

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set => Set(ref _isBusy, value);
        }

        private string _macAddress;
        public string MacAddress
        {
            get => _macAddress;
            set => Set(nameof(MacAddress), ref _macAddress, value);
        }

        private string _accessKey;
        public string AccessKey
        {
            get => _accessKey;
            set => Set(nameof(AccessKey), ref _accessKey, value);
        }

        #endregion

        public VideoCreatorAuthHelper()
        {
            ReadMACAddress();
            authCtrl = new AuthControl();
            AccessKey = authCtrl.ReadApiKeyFromRegistry();
            if (string.IsNullOrEmpty(AccessKey) || AccessKey.StartsWith("Error "))
            {
                MessageBox.Show(AccessKey, "Credentials Issue", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            var failureMessage = authCtrl.InitConnection();
            if (!string.IsNullOrEmpty(failureMessage))
            {
                MessageBox.Show(failureMessage, "Ini tConnection", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void ReadMACAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    var hex = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                    var macAddress = hex.Replace("-", ":");
                    if (!string.IsNullOrEmpty(macAddress))
                    {
                        MacAddress = macAddress;
                        break;
                    }
                }
            }
            //for testing purposes
            //this.MacAddress = "FC:B3:BC:A8:84:A5";
        }

        private void InitializeOrResetDbTransferControl()
        {
            IsBusy = true;
            authCtrl.SetMACAddress(this.MacAddress);
            dbTransferCtrl.SetClient(authCtrl.GetHttpClient());
        }


        public async Task ExecuteLoginAsync()
        {
            ErrorMessage = string.Empty;
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var loginRes = await authCtrl.Login(MacAddress?.Trim(), AccessKey?.Trim());
            if (!string.IsNullOrEmpty(loginRes.Token))
            {
                TokenNumber = loginRes.Token;
                dbTransferCtrl = new DBTransferControl(authCtrl.GetHttpClient());
            }
            else
                ErrorMessage = "No Token returned";
            IsBusy = false;
        }

        public async Task ExecuteLogoutAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            var result = await authCtrl.Logout();
            if (!string.IsNullOrEmpty(result?.Status))
            {
                ErrorMessage = result?.Status;
                TokenNumber = string.Empty;
            }
            IsBusy = false;
        }


        public async Task<T> Get<T>(string url)
        {
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.Get(url);
            var data = JsonConvert.DeserializeObject<T>(result);
            return data;

        }

        public async Task<byte[]> GetSecuredFileByteArray(string url)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var byteArray = await dbTransferCtrl.GetFileByteArray(url);
            IsBusy = false;
            return byteArray;
        }

        public async Task GetFile(string url, string fileToWriteTo)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var byteArray = await dbTransferCtrl.GetFileByteArray(url);
            var memoryStream = new MemoryStream(byteArray);
            Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.CreateNew);
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(streamToWriteTo);
            streamToWriteTo.Dispose();
            memoryStream.Dispose();
            IsBusy = false;

        }

        public async Task<T> Create<T>(string url, FormUrlEncodedContent payload)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.Create(url, payload);
            T data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;
        }

        public async Task<T> CreateWithMultipart<T>(string url, MultipartFormDataContent payload)
        {

            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.CreateWithFile(url, payload);
            var data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;

        }

        public async Task<T> Update<T>(string url, FormUrlEncodedContent payload)
        {

            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();

            var result = await dbTransferCtrl.Update(url, payload);
            var data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;

        }

        public async Task<T> UpdateWithMultipart<T>(string url, MultipartFormDataContent payload)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.UpdateWithFile(url, payload);
            var data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;
        }

        public async Task<T> Patch<T>(string url, FormUrlEncodedContent payload)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.Patch(url, payload);
            var data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;
        }

        public async Task<T> Delete<T>(string url, FormUrlEncodedContent payload)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            InitializeOrResetDbTransferControl();
            var result = await dbTransferCtrl.Delete(url, payload);
            T data = JsonConvert.DeserializeObject<T>(result);
            IsBusy = false;
            return data;
        }

    }
}
