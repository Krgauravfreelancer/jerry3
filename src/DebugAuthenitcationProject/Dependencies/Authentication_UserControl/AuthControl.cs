//using Renci.SshNet;
//using Renci.SshNet.Sftp;
using Authentication_UserControl.Helpers;
using Authentication_UserControl.ResponseObjects;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Authentication_UserControl
{
    public class AuthControl : IAuthControl
    {
        //private SftpClient _client;
        //private ConnectionInfo connectionInfo;
        private string _userName = string.Empty;
        private string _password = string.Empty;

        public void InitConnection()
        {
            APIAuthClientHelper.InitializeClient();
            ReadRegistry();
        }

        public void InitConnection(string baseURI)
        {
            APIAuthClientHelper.InitializeClient(baseURI);
            ReadRegistry();
        }
        public HttpClient GetHttpClient()
        {
            HttpClient client = null;
            if(APIAuthClientHelper.HttpApiClient.DefaultRequestHeaders.Contains("X-CBU-Access-Key"))
            {
                client = APIAuthClientHelper.HttpApiClient;
            }
            return client;
        }


        public async Task<LoginResponseModel> Login(string macAddress, string accessKey)
        {
            var url = "employee/login";
            var parameters = new Dictionary<string, string> { { "username", this._userName }, { "password", this._password },{ "mac", macAddress }};
            var encodedContent = new FormUrlEncodedContent(parameters);
            SetAccessKey(accessKey);
            using (HttpResponseMessage response = await APIAuthClientHelper.HttpApiClient.PostAsync(url, encodedContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    var loginMod = response.Content.ReadAsStringAsync().Result;                    
                    var user = JsonSerializer.Deserialize<LoginResponseModel>(loginMod);

                    //var result = user.Status;
                    if (!string.IsNullOrEmpty(user.Token))
                    {
                        SetToken(user.Token);
                        //SetMACAddress(macAddress);
                    }
                    else
                    {
                        switch (user.Code)
                        {
                            case 8872:
                                throw new Exception("Wrong Username or Password");
                            case 4432:
                                throw new Exception("Employee/User not enabled");
                            case 6543:
                                throw new Exception("Invalid MAC Address");
                            case 4091:
                                throw new Exception("Username, password or MAC Address was invalid");
                            case 0:
                                break;
                        }
                    }
                    
                    return user;
                }
                else
                {   
                    throw new Exception(response.ReasonPhrase);
                }                

            }
        }
        public async Task<LogoutResponseModel> Logout()
        {
             var url = "employee/logout";

            //var parameters = new Dictionary<string, string> { { "username", this._userName }, { "password", this._password }, { "mac", macAddress } };
            //var encodedContent = new FormUrlEncodedContent(parameters);

            //SetAccessKey(accessKey);
            //SetToken(token);
            //SetMACAddress(macAddress);
            using (HttpResponseMessage response = await APIAuthClientHelper.HttpApiClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var projectDet = response.Content.ReadAsStringAsync().Result;
                    var project = JsonSerializer.Deserialize<LogoutResponseModel>(projectDet);
                    return project;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }

            }
        }
        public void SetToken(string token) 
        {
            APIAuthClientHelper.SetToken(token);
        }
        public void SetMACAddress(string macAddress)
        {
            APIAuthClientHelper.SetMacAddress(macAddress);
        }
        public void SetAccessKey(string accessKey)
        {
            APIAuthClientHelper.SetAccessKey(accessKey);
        }

        //private void ReadMACAddress()
        //{
        //    string macAddress = "";

        //    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        //    {
        //        if (nic.OperationalStatus == OperationalStatus.Up)
        //        {
        //            var hex = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
        //            macAddress = hex.Replace("-", ":");
        //            if (!string.IsNullOrEmpty(macAddress))
        //            {
        //                MacAddress = macAddress;
        //                break;
        //            }
        //        }
        //    }

        //    //for testing purposes
        //    //MacAddress = MACADDRESS;
        //}

        private void ReadRegistry()
        {
            //opening the subkey  
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\CommercialBase");

            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                this._userName = key.GetValue("authuser")?.ToString();
                this._password = key.GetValue("authpassword")?.ToString();

                if (string.IsNullOrEmpty(this._userName) || string.IsNullOrEmpty(this._password))
                {
                    MessageBox.Show("The user credentials do not match with the registry", "Mismatch of credentials", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                key.Close();
            }
            else //if the key does not exist display relevant message
            {
                this._userName = string.Empty;
                this._password = string.Empty;
                MessageBox.Show("The user credentials couldn't be found in the registry", "Registry not found", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
