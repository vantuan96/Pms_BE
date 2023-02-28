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
    public class PatientInPackageUpdateUsingJob : ProcessingThread
    {
        public VMConfiguration Cn = null;
        public override void Process()
        {
            var start_time = DateTime.Now;
            TimeSpan tp;
            var entity = (PatientInPackageUpdateUsingModel)PatientInPackageUpdateUsingQueue.Receiver();
            if (entity == null) return;
            #region Debug Time spend
            //tp = DateTime.Now - start_time;
            //CustomLog.intervaljoblog.Info(string.Format("Spend time step PatientInPackageUpdateUsingQueue.Receiver: {0} (ms)", tp.TotalMilliseconds.ToString()));
            #endregion .Debug Time spend
            //Tinh thoi gian
            try
            {
                bool bResend = false;
                if (entity.count_fail == 0)
                {
                    var entityReturn = new PatientInPackageRepo().RefreshInformationPatientInPackage(entity.PatientInPackageId.Value);
                    tp = DateTime.Now - start_time;
                    if (entityReturn!=null)
                    {
                        CustomLog.Instant.IntervalJobLog("Auto Update using service successed in " + tp.TotalMilliseconds.ToString() + " (ms)\r\n"
                                     + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                     + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
                                     + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                     Constant.Log_Type_Info, printConsole: true);
                    }
                    else
                    {
                        CustomLog.Instant.IntervalJobLog("Auto Update using service Fail in " + tp.TotalMilliseconds.ToString() + " (ms)\r\n"
                                     + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                     + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
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
                    if (entity.process_fail <= Cn.THREAD.UPDATEPATIENTINPACKAGEUSING_ATTEMPS)
                    {
                        //Day lai trong queue de cập nhật lại
                        PatientInPackageUpdateUsingQueue.Send(entity);
                        //Co loi nen nghi lau hon mot chut moi chay tiep
                        Sleep(100);
                    }
                    else
                    {
                        CustomLog.Instant.IntervalJobLog("Auto Update using service Max attempt " + Environment.NewLine
                                            + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                            + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
                                            + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                             Constant.Log_Type_Info, printConsole: true);
                    }
                }
            }
            catch (Exception ex)
            {
                entity.process_fail += 1;

                CustomLog.Instant.ErrorLog("Auto Update using service error : "
                                        + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                        + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
                                        + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine)
                                        + "    Message: " + ex.Message + "",
                    Constant.Log_Type_Error, printConsole: false);

                if (entity.process_fail <= Cn.THREAD.UPDATEPATIENTINPACKAGEUSING_ATTEMPS)
                {
                    //Day lai trong queue de cập nhật lại
                    PatientInPackageUpdateUsingQueue.Send(entity);
                    //Co loi nen nghi lau hon mot chut moi chay tiep
                    Sleep(100);
                }
                else
                {
                    CustomLog.Instant.IntervalJobLog("Auto Update using service Max attempt " + Environment.NewLine
                                        + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                        + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
                                        + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                         Constant.Log_Type_Info, printConsole: true);
                }
            }
            //Process speed here
            if (Cn.THREAD.UPDATEPATIENTINPACKAGEUSING_SPEED > 0)
                Sleep(Cn.THREAD.UPDATEPATIENTINPACKAGEUSING_SPEED);
        }
    }
}
