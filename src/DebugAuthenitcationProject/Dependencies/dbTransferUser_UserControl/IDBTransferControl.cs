using System.Net.Http;
using System.Threading.Tasks;

namespace dbTransferUser_UserControl
{
    public interface IDBTransferControl
    {
        void SetClient(HttpClient client);
        Task<string> Get(string url);
        Task<byte[]> GetFileByteArray(string url);
        Task<string> Create(string url, FormUrlEncodedContent encodedPayload);
        Task<string> CreateWithFile(string url, MultipartFormDataContent encodedPayload);
        Task<string> Update(string url, FormUrlEncodedContent encodedPayload);
        Task<string> UpdateWithFile(string url, MultipartFormDataContent encodedPayload);
        Task<string> Patch(string url, FormUrlEncodedContent encodedPayload);



    }
}
