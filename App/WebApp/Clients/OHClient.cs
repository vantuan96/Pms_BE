using DataAccess.Models;
using DrFee.Contract.Models.ApigwModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VM.Common;

namespace DrFee.Clients
{
    public class OHClient : HISClient
    {
        #region Service
        public static List<HISServiceModel> GetService(DateTime from, DateTime to)
        {
            string url_postfix = string.Format(
                //"/OH_Production/1.0.0/getServiceList?from={0}&to={1}",
                "/DimsVinmecCom/1.0.0/getServiceListOH?from={0}&to={1}",
                from.ToString(Constant.DATETIME_SQL),
                to.ToString(Constant.DATETIME_SQL)
            );
            var response = RequestAPI(url_postfix, "ServiceList", "Service");
            if (response != null)
                return response.Select(e => new HISServiceModel {
                    ServiceGroupCode = e["ServiceGroupCode"]?.ToString(),
                    ServiceGroupViName = e["ServiceGroupName"]?.ToString(),
                    ServiceGroupEnName = e["ServiceGroupNameE"]?.ToString(),
                    ServiceCode = e["ServiceCode"]?.ToString(),
                    ServiceViName = e["ServiceName"]?.ToString(),
                    ServiceEnName = e["ServiceNameE"]?.ToString(),
                    HISCode = Constant.HIS_CODE["OH"],
                }).ToList();
            return new List<HISServiceModel>();
        }
        #endregion

        #region Revenue
        public static void SyncRevenue(DateTime from, DateTime to)
        {
            CustomLog.intervaljoblog.Info(string.Format("<OH revenue> Begin get from HIS [F:{0} -> T:{1}]", from.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), to.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND)));
            string url_postfix = string.Format(
                //"/OH_Production/1.0.0/getDoanhThu?from={0}&to={1}", from.ToString(Constant.DATETIME_SQL), to.ToString(Constant.DATETIME_SQL)
                "/DimsVinmecCom/1.0.0/getDoanhThuOH?from={0}&to={1}", from.ToString(Constant.DATETIME_SQL), to.ToString(Constant.DATETIME_SQL)
            );
            var response = RequestAPI(url_postfix, "Entries", "Entry");
            if (response == null)
                return;
            CustomLog.intervaljoblog.Info(string.Format("<OH revenue> End get from HIS [F:{0} -> T:{1}. Total records: {2}]", from.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), to.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), response.Count()));
            List<HISRevenueModel> need_update_revenue = new List<HISRevenueModel>();
            foreach (JToken item in response)
            {
                try
                {
                    //if (item["VisitCode"].ToString() != "46984")
                    //{
                    //    continue;
                    //}
                    var isSkip = response.Any(x => x["ParentChargeId"]?.ToString() == item["ChargeId"].ToString());
                    isSkip = false;
                    if (!isSkip)
                    {
                        var revenue = HandleHISRevenue(Constant.HIS_CODE["OH"], item);
                        // Bỏ qua chỉ định trùng lặp
                        if (need_update_revenue.FirstOrDefault(e => e.ChargeMonth == revenue.ChargeMonth && e.ChargeId == revenue.ChargeId) == null)
                            need_update_revenue.Add(revenue);
                    }
                    else
                    {
                        //Dịch vụ là gói hoặc có dịch vụ CON nên bỏ qua
                        CustomLog.intervaljoblog.Info(string.Format("<OH revenue> Skip service when sync [Pid:{0} -> VisitCode:{1} -> ServiceCode: {2}]", item["PID"], item["VisitCode"], item["ItemCode"]));
                    }
                }
                catch (Exception ex)
                {
                    CustomLog.errorlog.Error(string.Format("Charg Info: {0}. Err: {1}",JsonConvert.SerializeObject(item),ex.ToString()));
                    continue;
                }
            }
            CustomLog.intervaljoblog.Info(string.Format("<OH revenue> Begin CalculateRevenue [F:{0} -> T:{1}. Total records: {2}]", from.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), to.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), need_update_revenue.Count()));
            foreach (var rev in need_update_revenue)
            {
                var cal_revenue = GetOrCreateCalculatedRevenue(rev);
                CalculateRevenue(cal_revenue);
            }
        }

        #endregion

        #region Department
        public static List<HISDepartmentModel> GetDepartment()
        {
            //string url_postfix = "/OH_Production/1.0.0/getDepartments";
            string url_postfix = "/DimsVinmecCom/1.0.0/getDepartmentsOH";
            var response = RequestAPI(url_postfix, "Departments", "Department");
            if (response != null)
                return response.Select(e=> new HISDepartmentModel {
                    ViName = e["DepartmentName"]?.ToString(),
                    EnName = e["DepartmentName"]?.ToString(),
                    Code = e["DepartmentCode"]?.ToString(),
                    HospitalCode = e["HospitalCode"]?.ToString(),
                }).ToList();
            return new List<HISDepartmentModel>();
        }
        #endregion

        #region CDHA
        public static string GetXRayOperationDoctor(string visit_code, string service_code)
        {
            //string url_postfix = $"/OH_Production/1.0.0/getBsThucHienCDHA_DF?VisitCode={visit_code}&ItemCode={service_code}";
            string url_postfix = $"/DimsVinmecCom/1.0.0/getBsThucHienCDHAOH?VisitCode={visit_code}&ItemCode={service_code}";
            var response = RequestAPI(url_postfix, "DSBacsi", "Bacsi");
            if (response != null)
                return BuildXRayOperationDoctorResult(response);
            return null;
        }

        private static string BuildXRayOperationDoctorResult(JToken data)
        {
            foreach (JToken item in data)
                return item["AccountAD"]?.ToString();
            return null;
        }
        #endregion
    }
}