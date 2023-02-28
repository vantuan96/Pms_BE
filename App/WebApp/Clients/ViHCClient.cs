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
    public class ViHCClient : HISClient
    {
        #region Revenue
        public static void SyncRevenue(DateTime from, DateTime to)
        {
            CustomLog.intervaljoblog.Info(string.Format("<ViHC revenue> Begin get from DB HISRevenues [F:{0} -> T:{1}]", from.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), to.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND)));
            List<HISRevenueModel> need_update_revenue = new List<HISRevenueModel>();
            var listEnity = GetHISRevenueHC_Calculate_NotYet(from, to);
            if (listEnity == null)
                return;
            foreach (var item in listEnity)
            {
                try
                {
                    var revenue = new HISRevenueModel
                    {
                        ChargeId = item.ChargeId,
                        ChargeMonth = int.Parse(item.ChargeUpdatedAt.Value.ToString(Constant.YEAR_MONTH_FORMAT)),
                        HISCode = item.HISCode,
                        HospitalId = item.HospitalId,
                        Service = item.Service,
                        ChargeDoctor = item.ChargeDoctor,
                        ChargeUpdatedDate = item.ChargeUpdatedAt.Value,
                        ChargeDoctorDepartmentCode = item.ChargeDoctorDepartmentCode,
                        OperationId = item.OperationId,
                        OperationDoctorDepartmentCode = item.OperationDoctorDepartmentCode,
                        OperationDoctor = item.OperationDoctor,
                        CustomerName = item.CustomerName,
                        CustomerPID = item.CustomerPID,
                        PackageCode = item.PackageCode,
                        AmountInPackage=item.AmountInPackage,
                        IsPackage = item.IsPackage,
                        VisitType = item.VisitType,
                        VisitCode = item.VisitCode,
                    };
                    // Bỏ qua chỉ định trùng lặp
                    if (need_update_revenue.FirstOrDefault(e => e.ChargeMonth == revenue.ChargeMonth && e.ChargeId == revenue.ChargeId) == null)
                        need_update_revenue.Add(revenue);
                }
                catch (Exception ex)
                {
                    CustomLog.errorlog.Error(string.Format("Charg Info: {0}. Err: {1}",JsonConvert.SerializeObject(item),ex.ToString()));
                    continue;
                }
            }
            CustomLog.intervaljoblog.Info(string.Format("<ViHC revenue> Begin CalculateRevenue [F:{0} -> T:{1}. Total records: {2}]", from.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), to.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND), need_update_revenue.Count()));
            foreach (var rev in need_update_revenue)
            {
                var cal_revenue = GetOrCreateCalculatedRevenue(rev);
                CalculateRevenue(cal_revenue);
            }
        }
        #endregion
    }
}