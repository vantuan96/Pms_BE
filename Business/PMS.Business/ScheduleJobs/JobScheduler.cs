using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.ScheduleJobs
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            //Đồng bộ Bệnh Viện
            #region Đồng bộ Bệnh Viện
            IJobDetail sync_oh_Hospital_job = JobBuilder.Create<SyncHospitalOHJob>().Build();
            ITrigger sync_oh_Hospital_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHHospital_CS)
                //.WithCronSchedule("0 50 11 ? * *")
                .Build();
            scheduler.ScheduleJob(sync_oh_Hospital_job, sync_oh_Hospital_trigger);
            CustomLog.Instant.IntervalJobLog("SyncHospital job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Đồng bộ Bệnh Viện

            //Đồng bộ phòng ban
            #region Đồng bộ phòng ban
            IJobDetail sync_oh_department_job = JobBuilder.Create<SyncOHDepartmentJob>().Build();
            ITrigger sync_oh_department_trigger = TriggerBuilder.Create()
                //.WithCronSchedule(" 0 0 0 ? * * *")
                .WithCronSchedule(ConfigHelper.CF_SyncOHDepartment_CS)
                .Build();
            scheduler.ScheduleJob(sync_oh_department_job, sync_oh_department_trigger);
            CustomLog.Instant.IntervalJobLog("SyncOHDepartment job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Đồng bộ phòng ban

            //Đồng bộ dịch vụ OH
            #region Đồng bộ dịch vụ OH
            IJobDetail sync_oh_service_job = JobBuilder.Create<SyncOHServiceJob>().Build();
            ITrigger sync_oh_service_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHService_CS)
                //.WithCronSchedule("0 50 11 ? * *")
                .Build();
            scheduler.ScheduleJob(sync_oh_service_job, sync_oh_service_trigger);
            CustomLog.Instant.IntervalJobLog("SyncOHService job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Đồng bộ dịch vụ OH

            //Auto cập nhật trạng thái gói dịch vụ của khách hàng
            #region Tự động cập nhật trạng thái gói dịch vụ
            IJobDetail sync_auto_update_patientinpackageStatus_job = JobBuilder.Create<AutoUpdatePatientInPackageStatusJob>().Build();
            ITrigger sync_auto_update_patientinpackageStatus_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_AutoUpdatePatientInPackageStatus_CS)
                //.WithCronSchedule("0 50 11 ? * *")
                .Build();
            scheduler.ScheduleJob(sync_auto_update_patientinpackageStatus_job, sync_auto_update_patientinpackageStatus_trigger);
            CustomLog.Instant.IntervalJobLog("AutoUpdatePatientInPackageStatus job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Tự động cập nhật trạng thái gói dịch vụ
        }
    }
}
