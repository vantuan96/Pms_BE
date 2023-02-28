using DataAccess.Repository;
using PMS.Business.MongoDB;
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.Enum;
using Quartz;
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
    public class GetPatientInPackageForUpdateUsingJob : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsGettingPatientInPackage4UpdateUsingProcessing)
                return;
            Globals.IsGettingPatientInPackage4UpdateUsingProcessing = true;
            CustomLog.intervaljoblog.Info($"<Get Patient In Package For Update Using> Start!");
            try
            {
                var results = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => !x.IsDeleted && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED || x.Status == (int)PatientInPackageEnum.EXPIRED || x.Status == (int)PatientInPackageEnum.CLOSED) && x.PatientInformation.PID== "618010072");
                CustomLog.intervaljoblog.Info(string.Format("<Get Patient In Package For Update Using> Total item: {0}", results?.Count()));
                if (results?.Count() <= 0)
                {
                    Globals.IsGettingPatientInPackage4UpdateUsingProcessing = false;
                    return;
                }
                foreach (var item in results)
                {
                    var entity = new PatientInPackageUpdateUsingModel()
                    {
                        PID= item.PatientInformation.PID,
                        PatientInPackageId=item.Id,
                        PackageCode= item.PackagePriceSite.PackagePrice.Package.Code
                    };
                    CustomLog.Instant.IntervalJobLog("Get Patient In Package receiver in\r\n"
                                     + string.Format("    PID: {0}{1}", entity.PID, Environment.NewLine)
                                     + string.Format("    PatientInPackageId: {0}{1}", entity.PatientInPackageId, Environment.NewLine)
                                     + string.Format("    PackageCode: {0}{1}", entity.PackageCode, Environment.NewLine),
                                     Constant.Log_Type_Info, printConsole: true);
                    PatientInPackageUpdateUsingQueue.Send(entity);
                }
                CustomLog.intervaljoblog.Info($"<Get Patient In Package For Update Using> Success!");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Get Patient In Package For Update Using> Error: {0}", ex));
            }
            Globals.IsGettingPatientInPackage4UpdateUsingProcessing = false;
        }
    }
}
