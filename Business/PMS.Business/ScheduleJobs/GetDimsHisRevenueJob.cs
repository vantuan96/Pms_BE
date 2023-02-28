using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Business.MongoDB;
using PMS.Business.Provider;
using PMS.Contract.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;
using VM.Data.Queue;
using VM.ThreadingEx.Worker;

namespace PMS.Business.ScheduleJobs
{
    public class GetDimsHisRevenueJob : ProcessingThread
    {
        public VMConfiguration Cn = null;
        public override void Process()
        {
            if (Globals.IsGettingDimsHisChargeProcessing)
                return;
            Globals.IsGettingDimsHisChargeProcessing = true;
            var start_time = DateTime.Now;
            TimeSpan tp;
            if (HisChargeQueue.Count() > Cn.THREAD.GET_REVENUE_HOWITEMS2GET)
            {
                //Vẫn còn đủ dữ liệu để xử lý. Chưa cần get thêm
                //Sleep(Cn.THREAD.GET_REVENUE_SPEED);
                Globals.IsGettingDimsHisChargeProcessing = false;
                return;
            }
            List<HisChargeRevenueModel> listEntity = null;
            int iTypeConfig = Constant.SYSTEM_CONFIG["GET_HISCHARGE_4UPDATEDIMS"];

            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                try
                {
                    var config = unitOfWork.SystemConfigRepository.Find(x => x.TypeConfig == iTypeConfig).FirstOrDefault();
                    DateTime from = (DateTime)config?.LastUpdatedOHService;
                    DateTime to = config?.EndDate != null ? config.EndDate.Value : DateTime.Now;
                    DateTime end4NextRun = to <= DateTime.Now ? to : DateTime.Now;
                    using (HisRevenueRepo reposity = new HisRevenueRepo())
                    {
                        CustomLog.Instant.IntervalJobLog(string.Format("<GetDimsHisRevenueJob> Begin get from HisCharge [F:{0} -> T:{1}]", from.ToString(Constant.DATE_TIME_FORMAT), to.ToString(Constant.DATE_TIME_FORMAT)), Constant.Log_Type_Info, printConsole: true);
                        listEntity = reposity.GetHISCharge4UpdateDims(from.ToString(Constant.DATETIME_SQL), to.ToString(Constant.DATETIME_SQL));
                    }
                    if (listEntity != null && listEntity.Count > 0)
                    {
                        foreach (HisChargeRevenueModel item in listEntity)
                        {
                            CustomLog.Instant.IntervalJobLog("Dims HisRevenue receiver to update\r\n"
                                     + string.Format("    ChargeId: {0}{1}", item.ChargeId, Environment.NewLine)
                                     + string.Format("    InPackageType: {0}{1}", item.InPackageType, Environment.NewLine)
                                     + string.Format("    PackageCode: {0}{1}", item.PackageCode, Environment.NewLine),
                                     Constant.Log_Type_Info, printConsole: true);
                            HisChargeQueue.Send(item);
                        }
                    }
                    config.LastUpdatedOHService = end4NextRun;
                    config.EndDate = to <= DateTime.Now ? to.AddMinutes(config.ExMinute) : DateTime.Now.AddMinutes(config.ExMinute);
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    CustomLog.errorlog.Info(string.Format("<GetDimsHisRevenueJob> Error: {0}", ex));
                }
            }
            
            //Process speed here
            if (Cn.THREAD.GETHISCHARGE4UPDATEDIMS_SPEED > 0)
                Sleep(Cn.THREAD.GETHISCHARGE4UPDATEDIMS_SPEED);

            Globals.IsGettingDimsHisChargeProcessing = false;
        }
    }
}
