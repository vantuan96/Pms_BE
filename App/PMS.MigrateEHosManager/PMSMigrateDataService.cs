using PMS.Business.ScheduleJobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.MigrateEHosManager
{
    public partial class PMSMigrateDataService : ServiceBase
    {
        public PMSMigrateDataService()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            try
            {
                Console.Title = ConfigHelper.AppName;
                //Using Quartz to create job
                JobMigrateScheduler.Start();
                CustomLog.intervaljoblog.Info("MigrateEHosManager was started");

            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("MigrateEHosManager start Error: {0}", ex));
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            try
            {
                CustomLog.intervaljoblog.Info("MigrateEHosManager was stoped");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("MigrateEHosManager stop Error: {0}", ex));
            }
        }
    }
}
