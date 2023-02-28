using DrFee.Contract.Models.ApigwModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VM.Common;

namespace DrFee.Clients
{
    public class EHosClient : HISClient
    {
        #region Service
        public static List<HISServiceModel> GetService(DateTime from, DateTime to)
        {
            string url_postfix = string.Format(
                "/eHos_Production/1.0.0/getServiceList?from={0}&to={1}",
                from.ToString(Constant.DATETIME_SQL),
                to.ToString(Constant.DATETIME_SQL)
            );
            var response = RequestAPI(url_postfix, "ServiceList", "Service");
            if (response != null)
                return BuildServiceResult(response);
            return new List<HISServiceModel>();
        }
        private static List<HISServiceModel> BuildServiceResult(JToken data)
        {
            List<HISServiceModel> result = new List<HISServiceModel>();
            foreach (JToken item in data)
            {
                result.Add(new HISServiceModel
                {
                    ServiceGroupCode = item["ServiceGroupCode"]?.ToString(),
                    ServiceGroupViName = item["ServiceGroupName"]?.ToString(),
                    ServiceGroupEnName = item["ServiceGroupNameE"]?.ToString(),
                    ServiceCode = item["ServiceCode"]?.ToString(),
                    ServiceViName = item["ServiceName"]?.ToString(),
                    ServiceEnName = item["ServiceNameE"]?.ToString(),
                    HISCode = Constant.HIS_CODE["OH"],
                });
            }
            return result;
        }
        #endregion

        #region Revenue
        internal static void SyncRevenue(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        internal static object GetDepartment()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}