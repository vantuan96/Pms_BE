using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VM.Common
{
   public class ApiHelper
    {
        public static HttpClient client;
        static ApiHelper()
        {
            client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
        }
        public static async Task<HttpResponseMessage> HttpGet(string uri, string token = "")
        {
            var url = ConfigurationManager.AppSettings["API_ManageApp_URL"].ToString() + uri;

            try
            {
                if (!string.IsNullOrWhiteSpace(token))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await client.GetAsync(url);
            }
            catch (System.Exception ex)
            {
                var re = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.Message)
                };

                return await Task.FromResult(re);
            }
        }
    }
}
