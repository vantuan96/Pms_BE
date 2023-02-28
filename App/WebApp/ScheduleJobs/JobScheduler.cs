using DrFee.Utils;
using Quartz;
using Quartz.Impl;

namespace DrFee.ScheduleJobs
{
    public class JobScheduler
    {
        static IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
        static IJobDetail sync_oh_service_job = JobBuilder.Create<SyncOHServiceJob>().Build();
        public static void Start()
        {
            scheduler.Start();

            //IJobDetail sync_oh_revenue_job = JobBuilder.Create<SyncOHRevenueJob>().Build();
            //ITrigger sync_oh_revenue_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_SyncOHRevenue_CS)
            //    //.WithCronSchedule("0 40 15 ? * *")
            //    .Build();
            //scheduler.ScheduleJob(sync_oh_revenue_job, sync_oh_revenue_trigger);

            //IJobDetail sync_ehos_revenue_job = JobBuilder.Create<SyncEHosRevenueJob>().Build();
            //ITrigger sync_ehos_revenue_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule("0 15 0/1 1/1 * ? *")
            //    .Build();
            //scheduler.ScheduleJob(sync_ehos_revenue_job, sync_ehos_revenue_trigger);

            //Đồng bộ dịch vụ OH
            //IJobDetail sync_oh_service_job = JobBuilder.Create<SyncOHServiceJob>().Build();
            ITrigger sync_oh_service_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_SyncOHService_CS)
                //.WithCronSchedule("0 50 11 ? * *")
                .Build();
            scheduler.ScheduleJob(sync_oh_service_job, sync_oh_service_trigger);

            //IJobDetail sync_ehos_service_job = JobBuilder.Create<SyncEHosServiceJob>().Build();
            //ITrigger sync_ehos_service_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule("0 45 0/1 1/1 * ? *")
            //    .Build();
            //scheduler.ScheduleJob(sync_ehos_service_job, sync_ehos_service_trigger);

            //IJobDetail sync_oh_department_job = JobBuilder.Create<SyncOHDepartmentJob>().Build();
            //ITrigger sync_oh_department_trigger = TriggerBuilder.Create()
            //    //.WithCronSchedule(" 0 0 0 ? * * *")
            //    .WithCronSchedule(ConfigHelper.CF_SyncOHDepartment_CS)
            //    .Build();
            //scheduler.ScheduleJob(sync_oh_department_job, sync_oh_department_trigger);

            //IJobDetail sync_ehos_department_job = JobBuilder.Create<SyncOHServiceJob>().Build();
            //ITrigger sync_ehos_department_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(" 0 0 0 ? * * *")
            //    .Build();
            //scheduler.ScheduleJob(sync_ehos_department_job, sync_ehos_department_trigger);

            //IJobDetail sync_vihc_revenue_job = JobBuilder.Create<SyncViHCRevenueJob>().Build();
            //ITrigger sync_vihc_revenue_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_SyncViHCRevenue_CS)
            //    .Build();
            //scheduler.ScheduleJob(sync_vihc_revenue_job, sync_vihc_revenue_trigger);
        }
    }
}