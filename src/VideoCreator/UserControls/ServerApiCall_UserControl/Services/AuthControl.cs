//using Renci.SshNet;
//using Renci.SshNet.Sftp;
using Microsoft.Win32;
using ServerApiCall_UserControl.DTO;
using ServerApiCall_UserControl.Helpers;
using ServerApiCall_UserControl.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl.Services
{
    public class AuthControl : IAuthControl
    {
        private string UserName = string.Empty;
        private string Password = string.Empty;

        public AuthControl()
        { }

        public string InitConnection(string baseURI)
        {
            Auth_ServerAPICall_Helper.InitializeClient(baseURI);
            var failureMessage = ReadCredentialsFromRegistry();
            return failureMessage;

        }
        public HttpClient GetHttpClient()
        {
            HttpClient client = null;
            if (Auth_ServerAPICall_Helper.HttpApiClient.DefaultRequestHeaders.Contains("X-CBU-Access-Key"))
            {
                client = Auth_ServerAPICall_Helper.HttpApiClient;
            }
            return client;
        }

        public string GetLoggedInUser()
        {
            return this.UserName;
        }

        public async Task<LoginResponseModel> Login(string macAddress, string accessKey)
        {
            var url = "employee/login";
            var parameters = new Dictionary<string, string> { { "username", this.UserName }, { "password", this.Password }, { "mac", macAddress } };
            var encodedContent = new FormUrlEncodedContent(parameters);
            SetAccessKey(accessKey);
            using (HttpResponseMessage response = await Auth_ServerAPICall_Helper.HttpApiClient.PostAsync(url, encodedContent))
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
            using (HttpResponseMessage response = await Auth_ServerAPICall_Helper.HttpApiClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    var jsonResponse = JsonSerializer.Deserialize<LogoutResponseModel>(result);
                    return jsonResponse;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }

            }
        }

        public void SetToken(string token)
        {
            Auth_ServerAPICall_Helper.SetToken(token);
        }
        public void SetMACAddress(string macAddress)
        {
            Auth_ServerAPICall_Helper.SetMacAddress(macAddress);
        }
        public void SetAccessKey(string accessKey)
        {
            Auth_ServerAPICall_Helper.SetAccessKey(accessKey);
        }

        public string ReadApiKeyFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CommercialBase");
            if (key != null)
            {
                var api_key = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("reg5")));
                key.Close();
                if (string.IsNullOrEmpty(api_key))
                    return "Error - Unable to read or decrypt API Key";
                return api_key;
            }
            else
                return "Error - API Key not found in the registry";
        }

        private string ReadCredentialsFromRegistry()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\CommercialBase");
            if (key != null)
            {
                this.UserName = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("reg2")));
                this.Password = EncryptionHelper.DecryptString(EncryptionHelper.SecuredKey, Convert.ToString(key.GetValue("reg1")));
                // Comment below to use user creds else manager overwrite code
                //this.UserName = "manager_kumar";
                //this.Password = "manag3rKumarP@ssw0rd";


                key.Close();
                if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
                    return "Error - Unable to read or decrypt credentials";
            }
            else
                return "Error - Credentials not found in the registry";
            return string.Empty;
        }
    }
}
