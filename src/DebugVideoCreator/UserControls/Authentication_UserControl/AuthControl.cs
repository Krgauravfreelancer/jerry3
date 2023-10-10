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
        private string _userName = string.Empty;
        private string _password = string.Empty;

        public AuthControl()
        { }

        public void InitConnection()
        {
            Authentication_UC_Helper.InitializeClient();
            ReadCredentialsFromRegistry();
        }

        public void InitConnection(string baseURI)
        {
            Authentication_UC_Helper.InitializeClient(baseURI);
            ReadCredentialsFromRegistry();
        }
        public HttpClient GetHttpClient()
        {
            HttpClient client = null;
            if(Authentication_UC_Helper.HttpApiClient.DefaultRequestHeaders.Contains("X-CBU-Access-Key"))
            {
                client = Authentication_UC_Helper.HttpApiClient;
            }
            return client;
        }


        public async Task<LoginResponseModel> Login(string macAddress, string accessKey)
        {
            var url = "employee/login";
            var parameters = new Dictionary<string, string> { { "username", this._userName }, { "password", this._password },{ "mac", macAddress }};
            var encodedContent = new FormUrlEncodedContent(parameters);
            SetAccessKey(accessKey);
            using (HttpResponseMessage response = await Authentication_UC_Helper.HttpApiClient.PostAsync(url, encodedContent))
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
            using (HttpResponseMessage response = await Authentication_UC_Helper.HttpApiClient.GetAsync(url))
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
            Authentication_UC_Helper.SetToken(token);
        }
        public void SetMACAddress(string macAddress)
        {
            Authentication_UC_Helper.SetMacAddress(macAddress);
        }
        public void SetAccessKey(string accessKey)
        {
            Authentication_UC_Helper.SetAccessKey(accessKey);
        }

        public string ReadApiKeyFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CommercialBase");
            try
            {
                if (key != null)
                {
                    var apiKey = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("apikey")));
                    key.Close();
                    if (string.IsNullOrEmpty(apiKey))
                        ExitApplication("Unable to read or decrypt API Key");
                    return apiKey;
                }
                else
                    ExitApplication("API Key not found in the registry");
            }
            catch (Exception ex)
            {
                ExitApplication(ex.Message);
            }
            finally
            {
                key.Close();
            }
            return string.Empty;
        }

        private void ReadCredentialsFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CommercialBase");
            try
            {
                if (key != null)
                {
                    this._userName = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("authuser")));
                    this._password = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("authpassword")));
                    key.Close(); 
                    if (string.IsNullOrEmpty(this._userName) || string.IsNullOrEmpty(this._password))
                        ExitApplication("Unable to read or decrypt credentials");
                    
                }
                else
                    ExitApplication("Credentials not found in the registry");
            }
            catch (Exception ex)
            {
                ExitApplication(ex.Message);
            }
            finally
            {
                key.Close();
            }
        }

       

        private void ExitApplication(string message)
        {
            MessageBox.Show(message, "Credentials Issue", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }
}
