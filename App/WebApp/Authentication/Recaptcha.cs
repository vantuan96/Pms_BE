using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace PMS.Authentication
{
    /// <summary>
    /// Recaptcha
    /// </summary>
    public class Recaptcha
    {
        /// <summary>
        /// IsReCaptchValid
        /// </summary>
        /// <param name="EncodedResponse"></param>
        /// <returns></returns>
        public static bool IsReCaptchValid(string EncodedResponse)
        {
            //return true;
            var result = false;
            try
            {
                string PrivateKey = System.Configuration.ConfigurationManager.AppSettings["ReCaptCha-Secret-Key"];
                var requestUri = string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse);
                var request = (HttpWebRequest)WebRequest.Create(requestUri);
                var proxy_addr = System.Configuration.ConfigurationManager.AppSettings["ProxySever"];
                if (!string.IsNullOrEmpty(proxy_addr))
                {
                    WebProxy wp = new WebProxy(proxy_addr);
                    request.Proxy = wp;
                }
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        JObject jResponse = JObject.Parse(stream.ReadToEnd());
                        var isSuccess = jResponse.Value<bool>("success");
                        result = (isSuccess) ? true : false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return result;
        }
    }
}