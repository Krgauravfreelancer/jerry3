using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ServerApiCall_UserControl.Helpers
{
    public class Auth_ServerAPICall_Helper
    {
        private readonly static string baseAddress = "https://sftp.commercial-base.com/";
        public static HttpClient HttpApiClient { get; set; }
        
        public static void InitializeClient(string baseURI = null)
        {
            if (string.IsNullOrEmpty(baseURI))
                baseURI = baseAddress;
            HttpApiClient = new HttpClient
            {
                BaseAddress = new Uri(baseURI)
            };
            HttpApiClient.DefaultRequestHeaders.Accept.Clear();
            HttpApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static void SetToken(string token)
        {
            if (!string.IsNullOrEmpty(token))
                HttpApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        //public static void SetBearerToken(string token)
        //{
        //    ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //}

        public static void SetMacAddress(string macAddress)
        {
            if (!HttpApiClient.DefaultRequestHeaders.Contains("X-MAC-Address"))
                HttpApiClient.DefaultRequestHeaders.Add("X-MAC-Address", macAddress);
        }

        public static void SetAccessKey(string accessKey)
        {
            if (!HttpApiClient.DefaultRequestHeaders.Contains("X-CBU-Access-Key"))
                HttpApiClient.DefaultRequestHeaders.Add("X-CBU-Access-Key", accessKey);
        }


    }
}
