using System;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ServerApiCall_UserControl
{
    public class DBTransferControl : IDBTransferControl
    {
        private HttpClient apiHttpClient;
        public DBTransferControl(HttpClient client)
        {
            apiHttpClient = client;
        }
        public void SetClient(HttpClient client)
        {
            apiHttpClient = client;
        }

        public async Task<string> Get(string url)
        {
            try
            {
                using (HttpResponseMessage response = await apiHttpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}"; }
            return null;
        }

        public async Task<byte[]> GetFileByteArray(string url)
        {
            try
            {
                using (HttpResponseMessage response = await apiHttpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var contentAsByteArray = await response.Content.ReadAsByteArrayAsync(); // get the actual content stream
                        return contentAsByteArray;
                    }
                }
            }
            catch { return null; }
            return null;
        }

        public async Task<string> Create(string url, FormUrlEncodedContent encodedPayload)
        {
            try
            {
                using (HttpResponseMessage response = await apiHttpClient.PostAsync(url, encodedPayload))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}"; }
            return null;
        }

        public async Task<string> CreateWithFile(string url, MultipartFormDataContent encodedPayload)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = encodedPayload
                };
                using (HttpResponseMessage response = await apiHttpClient.SendAsync(httpRequestMessage))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var inputResponseStream = await response.Content.ReadAsStreamAsync();
                        var outputStream = new MemoryStream();
                        byte[] inputBuffer = new byte[65535];
                        int readAmount;
                        while ((readAmount = inputResponseStream.Read(inputBuffer, 0, inputBuffer.Length)) > 0)
                            outputStream.Write(inputBuffer, 0, readAmount);
                        var result = Encoding.UTF8.GetString(outputStream.GetBuffer(), 0, (int)inputResponseStream.Length);
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}"; }
            return null;
        }

        public async Task<string> UpdateWithFile(string url, MultipartFormDataContent encodedPayload)
        {
            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = encodedPayload
                };
                using (HttpResponseMessage response = await apiHttpClient.SendAsync(httpRequestMessage))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var inputResponseStream = await response.Content.ReadAsStreamAsync();
                        var outputStream = new MemoryStream();
                        byte[] inputBuffer = new byte[65535];
                        int readAmount;
                        while ((readAmount = inputResponseStream.Read(inputBuffer, 0, inputBuffer.Length)) > 0)
                            outputStream.Write(inputBuffer, 0, readAmount);
                        var result = Encoding.UTF8.GetString(outputStream.GetBuffer(), 0, (int)inputResponseStream.Length);
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}"; }
            return null;
        }

        public async Task<string> Update(string url, FormUrlEncodedContent encodedPayload)
        {
            string content = "";
            try
            {
                using (HttpResponseMessage response = await apiHttpClient.PutAsync(url, encodedPayload))
                {
                    content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = content;
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}, response - {content}"; }
            return null;
        }

        public async Task<string> Patch(string url, FormUrlEncodedContent encodedPayload)
        {
            try
            {
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, url)
                {
                    Content = encodedPayload
                };
                using (HttpResponseMessage response = await apiHttpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                }
            }
            catch (Exception ex) { return $"Exception - {ex.Message}"; }
            return null;
        }

        public async Task<string> Delete(string url, FormUrlEncodedContent encodedPayload)
        {
            try
            {
                using (HttpResponseMessage response = await apiHttpClient.DeleteAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
            catch { return null; }
            return null;
        }
    }
}
