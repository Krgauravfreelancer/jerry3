﻿using System;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using Authentication_UserControl;
using dbTransferUser_UserControl;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Net.Http;
using System.IO;

namespace AuthenitcationProject.Helpers
{
    public class ApiClientHelper : ViewModelBase
    {
        //private string token;
        private IAuthControl authCtrl; //authentication DLL
        private IDBTransferControl dbTransferCtrl;
        public string TokenNumber { get; set; }
        public string ErrorMessage { get; set; }

        const string APIKEY = "6Lwr_)@e{3GVSF58D#JiX4]5)%peA[";
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

        public ApiClientHelper()
        {
            ReadMACAddress();
            AccessKey = APIKEY;
            authCtrl = new AuthControl();
            authCtrl.InitConnection(); //Call this to initiate the connection. //Needed to call only once
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
            //MacAddress = MACADDRESS;
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
                InitializeOrResetDbTransferControl();
                var result = await dbTransferCtrl.Get(url);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task GetFile(string url, string fileToWriteTo)
        {
            try
            {
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
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public async Task<T> Create<T>(string url, FormUrlEncodedContent payload)
        {
            try
            {
                InitializeOrResetDbTransferControl();
                
                var result = await dbTransferCtrl.Create(url, payload);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> Create<T>(string url, MultipartFormDataContent payload)
        {
            try
            {
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.CreateWithFile(url, payload);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Update(url, payload);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

        public async Task<T> Update<T>(string url, MultipartFormDataContent payload)
        {
            try
            {
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.UpdateWithFile(url, payload);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
                InitializeOrResetDbTransferControl();

                var result = await dbTransferCtrl.Patch(url, payload);
                var data = JsonSerializer.Deserialize<T>(result);
                return data;
            }
            catch (DllNotFoundException dllex)
            {
                ErrorMessage = dllex.Message;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(NO_LOGIN_MESSAGE, "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
            return default;
        }

    }
}
