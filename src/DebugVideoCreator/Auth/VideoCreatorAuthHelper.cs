using System;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using Authentication_UserControl;
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
            authCtrl.InitConnection();
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
            try
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
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ExecuteLogoutAsync()
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                var result = await authCtrl.Logout();
                if (!string.IsNullOrEmpty(result?.Status))
                {
                    ErrorMessage = result?.Status;
                    TokenNumber = string.Empty;
                }
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }


        public async Task<T> Get<T>(string url)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();
                var result = await dbTransferCtrl.Get(url);
                var data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<byte[]> GetSecuredFileByteArray(string url)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();
                var byteArray = await dbTransferCtrl.GetFileByteArray(url);
                return byteArray;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return null;
        }

        public async Task GetFile(string url, string fileToWriteTo)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();
                var byteArray = await dbTransferCtrl.GetFileByteArray(url);
                var memoryStream = new MemoryStream(byteArray);
                Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.CreateNew);
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(streamToWriteTo);
                streamToWriteTo.Dispose();
                memoryStream.Dispose();
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<T> Create<T>(string url, FormUrlEncodedContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Create(url, payload);
                T data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> CreateWithMultipart<T>(string url, MultipartFormDataContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();
                var result = await dbTransferCtrl.CreateWithFile(url, payload);
                var data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> Update<T>(string url, FormUrlEncodedContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Update(url, payload);
                var data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> UpdateWithMultipart<T>(string url, MultipartFormDataContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.UpdateWithFile(url, payload);
                var data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> Patch<T>(string url, FormUrlEncodedContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Patch(url, payload);
                var data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> Delete<T>(string url, FormUrlEncodedContent payload)
        {
            try
            {
                ErrorMessage = string.Empty;
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Delete(url, payload);
                T data = JsonConvert.DeserializeObject<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                ErrorMessage = NO_LOGIN_MESSAGE;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

    }
}
