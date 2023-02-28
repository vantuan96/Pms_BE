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
    public class UpdateDimsHisRevenueJob : ProcessingThread
    {
        public VMConfiguration Cn = null;
        public override void Process()
        {
            var start_time = DateTime.Now;
            TimeSpan tp;
            var entity = (HisChargeRevenueModel)HisChargeQueue.Receiver();
            if (entity == null) return;
            //if(entity.NextTimeForProcess!=null && entity.NextTimeForProcess>DateTime.Now)
            //{
            //    HisChargeQueue.Send(entity);
            //    return;
            //}
            #region Debug Time spend
            //tp = DateTime.Now - start_time;
            //CustomLog.intervaljoblog.Info(string.Format("Spend time step User4SyncQueue.Receiver: {0} (ms)", tp.TotalMilliseconds.ToString()));
            #endregion .Debug Time spend
            //Tinh thoi gian
            try
            {
                bool bResend = false;
                if (entity.count_fail == 0)
                {
                    var statusUpdate = OHConnectionAPI.UpdateDimsHisRevenue(entity);
                    tp = DateTime.Now - start_time;
                    if (statusUpdate)
                    {
                        CustomLog.Instant.IntervalJobLog("Dims HisRevenue Update successed in " + tp.TotalMilliseconds.ToString() + " (ms)\r\n"
                                     + string.Format("    ChargeId: {0}{1}", entity.ChargeId, Environment.NewLine)
                                     + string.Format("    InPackageType: {0}{1}", entity.InPackageType, Environment.NewLine)
                                     + string.Format("    GroupPackageCode: {0}{1}", entity.GroupPackageCode, Environment.NewLine)
                                     + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                     Constant.Log_Type_Info, printConsole: true);
                        //Cập nhật vào DB là đã thực hiện. Khống set thời gian xử lý tiếp
                        entity.NextTimeForProcess = null;
                        PatientInPackageRepo.UpdateHisChargeNextTimeForProcess(entity);
                    }
                    else
                    {
                        CustomLog.Instant.IntervalJobLog("Dims HisRevenue Update Fail in " + tp.TotalMilliseconds.ToString() + " (ms)\r\n"
                                     + string.Format("    ChargeId: {0}{1}", entity.ChargeId, Environment.NewLine)
                                     + string.Format("    InPackageType: {0}{1}", entity.InPackageType, Environment.NewLine)
                                     + string.Format("    GroupPackageCode: {0}{1}", entity.GroupPackageCode, Environment.NewLine)
                                     + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                     Constant.Log_Type_Info, printConsole: true);
                        entity.count_fail += 1;//dem so lan gui ko thanh cong
                        entity.process_fail += 1;
                        entity.last_fail = DateTime.Now;//ghi nhan thoi diem gui ko thanh cong cuoi cung
                        bResend = true;
                    }
                }
                else
                {
                    TimeSpan t = DateTime.Now - entity.last_fail;

                    //sau 30 giây thi đẩy lại xử lý
                    if (t.TotalSeconds >= 30)
                    {
                        entity.count_fail = 0;
                    }
                    else
                    {
                        //sau 15 giây thi bat dau giam so lan gui ko thanh cong
                        if (t.TotalSeconds >= 15) entity.count_fail -= 1;
                    }

                    bResend = true;
                }
                if (bResend)
                {
                    if (entity.process_fail <= Cn.THREAD.UPDATEDIMSREVENUE_ATTEMPS)
                    {
                        //Day lai trong queue de cập nhật lại
                        entity.NextTimeForProcess = DateTime.Now.AddMinutes(5);
                        HisChargeQueue.Send(entity);
                        //Co loi nen nghi lau hon mot chut moi chay tiep
                        Sleep(100);
                    }
                    else
                    {
                        CustomLog.Instant.IntervalJobLog("Dims HisRevenue Update Max attempt " + Environment.NewLine
                                            + string.Format("    ChargeId: {0}{1}", entity.ChargeId, Environment.NewLine)
                                            + string.Format("    InPackageType: {0}{1}", entity.InPackageType, Environment.NewLine)
                                            + string.Format("    GroupPackageCode: {0}{1}", entity.GroupPackageCode, Environment.NewLine)
                                            + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                             Constant.Log_Type_Info, printConsole: true);
                        //Cập nhật vào DB là đã thực hiện. Can cap nhat de xu ly lai sau
                        entity.NextTimeForProcess = DateTime.Now.AddMinutes(ConfigHelper.CF_ExMinutesToNextProcess);
                        PatientInPackageRepo.UpdateHisChargeNextTimeForProcess(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                entity.process_fail += 1;

                CustomLog.Instant.ErrorLog("Dims HisRevenue Update error : "
                            + string.Format("    ChargeId: {0}{1}", entity.ChargeId, Environment.NewLine)
                            + string.Format("    InPackageType: {0}{1}", entity.InPackageType, Environment.NewLine)
                            + string.Format("    GroupPackageCode: {0}{1}", entity.GroupPackageCode, Environment.NewLine)
                            + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine)
                            + "    Message: " + ex.Message + "",
                    Constant.Log_Type_Error, printConsole: false);

                if (entity.process_fail <= Cn.THREAD.UPDATEDIMSREVENUE_ATTEMPS)
                {
                    //Day lai trong queue de cập nhật lại
                    HisChargeQueue.Send(entity);
                    //Co loi nen nghi lau hon mot chut moi chay tiep
                    Sleep(100);
                }
                else
                {
                    CustomLog.Instant.IntervalJobLog("Dims HisRevenue Update Max attempt " + Environment.NewLine
                                        + string.Format("    ChargeId: {0}{1}", entity.ChargeId, Environment.NewLine)
                                        + string.Format("    InPackageType: {0}{1}", entity.InPackageType, Environment.NewLine)
                                        + string.Format("    GroupPackageCode: {0}{1}", entity.GroupPackageCode, Environment.NewLine)
                                        + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                         Constant.Log_Type_Info, printConsole: true);
                    //Cập nhật vào DB là đã thực hiện. Can cap nhat de xu ly lai sau
                    entity.NextTimeForProcess = DateTime.Now.AddMinutes(ConfigHelper.CF_ExMinutesToNextProcess);
                    PatientInPackageRepo.UpdateHisChargeNextTimeForProcess(entity);
                }
            }
            //Process speed here
            if (Cn.THREAD.UPDATEDIMSREVENUE_SPEED > 0)
                Sleep(Cn.THREAD.UPDATEDIMSREVENUE_SPEED);
        }
    }
}
