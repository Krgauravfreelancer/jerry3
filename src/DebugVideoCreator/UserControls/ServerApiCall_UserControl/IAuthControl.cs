//using Renci.SshNet.Sftp;
using ServerApiCall_UserControl.DTO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl
{
    public interface IAuthControl
    {
        string InitConnection(string baseURI);
        Task<LoginResponseModel> Login(string macAddress, string accessKey);
        Task<LogoutResponseModel> Logout();
        void SetToken(string token);
        void SetMACAddress(string macAddress);
        void SetAccessKey(string accessKey);
        string ReadApiKeyFromRegistry();

        string GetLoggedInUser();

        HttpClient GetHttpClient();
        //Task<ProjectModel> ListProjects();
        //string GetUserName();
        //string GetPassword();
    }
}
