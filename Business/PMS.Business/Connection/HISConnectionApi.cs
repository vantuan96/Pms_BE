using DataAccess.Models;
using DataAccess.Repository;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.Connection
{
    public class HISConnectionApi
    {
        public static IUnitOfWork unitOfWork = new EfUnitOfWork();
        #region Request
        protected static JToken RequestAPI(string url_postfix, string json_collection, string json_item, out bool isThrowEx, int mnTimeout = 3)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["HIS_API_SERVER_TOKEN"]);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                string url = string.Format("{0}{1}", ConfigurationManager.AppSettings["HIS_API_SERVER_URL"], url_postfix);
                string raw_data = string.Empty;
                try
                {
                    client.Timeout = TimeSpan.FromMinutes(mnTimeout);
                    var response = client.GetAsync(url);
                    raw_data = response.Result.Content.ReadAsStringAsync().Result;
                    if (response.Result.StatusCode != HttpStatusCode.OK)
                        HandleError(url, raw_data);
                    else
                        HandleSuccess(url);

                    JObject json_data = JObject.Parse(raw_data);
                    var log_response = json_data.ToString();
                    CustomLog.apigwlog.Info(new
                    {
                        URI = url,
                        Response = log_response,
                    });
                    try
                    {
                        if (!string.IsNullOrEmpty(json_collection))
                        {
                            JToken customer_data = json_data[json_collection][json_item];
                            isThrowEx = false;
                            return customer_data;
                        }
                        else
                        {
                            JToken customer_data = json_data[json_item];
                            isThrowEx = false;
                            return customer_data;
                        }
                    }
                    catch
                    {
                        isThrowEx = true;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    var log_response = string.Format("{0}\n{1}", ex.ToString(), raw_data);
                    HandleError(url, log_response);
                    CustomLog.apigwlog.Info(new
                    {
                        URI = url,
                        Response = log_response,
                    });
                    isThrowEx = true;
                    return null;
                }
            }
        }
        private static void HandleSuccess(string url)
        {
            try
            {
                var scope = url.Split('?')[0];
                var noti = GetNotification(scope);
                if (noti == null || noti.Status == 2) return;

                if (noti.Status == 0)
                {
                    unitOfWork.SystemNotificationRepository.HardDelete(noti);
                    unitOfWork.Commit();
                    return;
                }

                noti.Status = 2;
                unitOfWork.SystemNotificationRepository.Update(noti);
                unitOfWork.Commit();
            }
            catch { }
        }
        private static void HandleError(string url, string error)
        {
            try
            {
                var scope = url.Split('?')[0];
                var noti = GetNotification(scope);
                if (noti == null)
                {
                    noti = new SystemNotification
                    {
                        Service = Constant.SERVICE_APIGW,
                        Scope = scope,
                        Subject = url,
                        Content = error,
                    };
                    unitOfWork.SystemNotificationRepository.Add(noti);
                    unitOfWork.Commit();
                }
                else if (noti.Status == 2)
                {
                    noti.Status = 0;
                    noti.Content = error;
                    unitOfWork.SystemNotificationRepository.Update(noti);
                    unitOfWork.Commit();
                }
            }
            catch { }
        }
        private static SystemNotification GetNotification(string scope)
        {
            return unitOfWork.SystemNotificationRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.Service) &&
                e.Service == Constant.SERVICE_APIGW &&
                !string.IsNullOrEmpty(e.Scope) &&
                e.Scope == scope
            );
        }
        #endregion
    }
}
