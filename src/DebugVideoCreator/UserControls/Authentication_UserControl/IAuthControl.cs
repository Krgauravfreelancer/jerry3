//using Renci.SshNet.Sftp;
using Authentication_UserControl.ResponseObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Authentication_UserControl
{
    public interface IAuthControl
    {
        
        void InitConnection();

        void InitConnection(string baseURI);
        Task<LoginResponseModel> Login(string macAddress, string accessKey);
        Task<LogoutResponseModel> Logout();
        void SetToken(string token);
        void SetMACAddress(string macAddress);        
        void SetAccessKey(string accessKey);
        string ReadApiKeyFromRegistry();

        HttpClient GetHttpClient();
        //Task<ProjectModel> ListProjects();
        //string GetUserName();
        //string GetPassword();
    }
}
