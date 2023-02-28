using PMS.Business.ScheduleJobs;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VM.Common;
using VM.Data.Queue;

namespace PMS.AutoUpdatePiPackageUsing
{
    public partial class UpdateUsingService : ServiceBase
    {
        static string TEMP_DIR_PATH = "";
        private static PatientInPackageUpdateUsingJob[] _patientInPackageUpdateUsingJobs = null;
        public UpdateUsingService()
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
                StartProgram();
                CustomLog.intervaljoblog.Info("UpdateUsingService was started");

            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("UpdateUsingService start Error: {0}", ex));
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            try
            {
                StopProgram();
                CustomLog.intervaljoblog.Info("UpdateUsingService was stoped");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("UpdateUsingService stop Error: {0}", ex));
            }
        }

        #region Function start job
        static void StartProgram()
        {
            Console.Clear();
            TEMP_DIR_PATH = System.IO.Directory.GetCurrentDirectory() + "\\Temp";
            //DisplayMessage("Loading config data ...\r\n");
            //Load data here
            AppConfiguration cons = null;
            //CPCatalog cps = null;
            var cnFile = ConfigurationManager.AppSettings["CF_DATA_PATH"];
            var code = ConfigurationManager.AppSettings["CF_CODE"];
            try
            {
                cons = AppConfigurationSerializer.ReadFile(cnFile);
                //cps = CPCatalogSerializer.ReadFile(cpFile);
            }
            catch (Exception ex)
            {
                //DisplayMessage("Error when load config data: \r\n" + ex.Message + "\r\n");
                //DisplayMessage("Press any key to return.");
                Console.ReadLine();
                return;
            }


            VMConfiguration cn = null;

            if (cons == null)
            {
                //DisplayMessage("No config data loaded\r\n");
                //DisplayMessage("Press any key to return.");
                Console.ReadLine();
                return;
            }
            else
            {
                foreach (VMConfiguration c in cons.VMConfigurations)
                {
                    if (c.CODE.ToUpper() == code.ToUpper())
                    {
                        cn = c;
                        break;
                    }
                }
            }

            if (cn == null)
            {
                CustomLog.Instant.IntervalJobLog("No configuration data with code: " + code + ".\r\n Please check value in App.config or Connection Manager tool.\r\n", Constant.Log_Type_Info, printConsole: true);
                CustomLog.Instant.IntervalJobLog("Press any key to return.", Constant.Log_Type_Info, printConsole: true);
                Console.ReadLine();
                return;
            }


            Console.Title = cn.NAME;
            CustomLog.Instant.IntervalJobLog("Press any key to start PMS | UPDATE USING STAT PROGRAM", Constant.Log_Type_Info, printConsole: true);
            Console.ReadLine();

            if (System.IO.Directory.GetFileSystemEntries(TEMP_DIR_PATH).Length != 0)
            {
                CustomLog.Instant.IntervalJobLog("Restore queue saved before (N to inogre)?", Constant.Log_Type_Info, printConsole: true);
                if (Console.ReadKey().Key != ConsoleKey.N)
                {
                    Console.WriteLine();
                    InitGlobalData(cn);
                }
            }
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();
            IJobDetail getPatientInPackage4UpdateUsing_job = JobBuilder.Create<GetPatientInPackageForUpdateUsingJob>().Build();
            ITrigger getPatientInPackage4UpdateUsing_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_AutoUpdatePatientInPackageUsing_CS)
                .Build();
            scheduler.ScheduleJob(getPatientInPackage4UpdateUsing_job, getPatientInPackage4UpdateUsing_trigger);
            CustomLog.Instant.IntervalJobLog("Get Patient In Package for update Using Service job was created", Constant.Log_Type_Info, printConsole: true);


            CustomLog.Instant.IntervalJobLog("Start Update Patient In Package Using Service", Constant.Log_Type_Info, printConsole: true);
            StartUpdatePatient4UsingService(cn);

            CustomLog.Instant.IntervalJobLog("Program started successfully", Constant.Log_Type_Info, printConsole: true);
        }
        private static void StartUpdatePatient4UsingService(VMConfiguration cn)
        {
            try
            {
                _patientInPackageUpdateUsingJobs = new PatientInPackageUpdateUsingJob[cn.THREAD.UPDATEPATIENTINPACKAGEUSING_THREADS];
                for (int i = 0; i < _patientInPackageUpdateUsingJobs.Length; i++)
                {
                    _patientInPackageUpdateUsingJobs[i] = new PatientInPackageUpdateUsingJob { Cn = cn };
                    string sPName = "Update Dims HisRevenue (" + (i + 1).ToString(CultureInfo.InvariantCulture) + ") started: " + Guid.NewGuid().ToString();
                    _patientInPackageUpdateUsingJobs[i].Start(sPName);
                }
            }
            catch (Exception pException)
            {
                CustomLog.Instant.ErrorLog("Update Dims HisRevenue starting error: " + pException.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Press any key to return.", Constant.Log_Type_Error, printConsole: true);
                Console.ReadLine();
                return;
            }
        }
        #endregion
        static void InitGlobalData(VMConfiguration cn)
        {
            if (!System.IO.Directory.Exists(TEMP_DIR_PATH))
            {
                CustomLog.Instant.IntervalJobLog("Temp dir not exist for re-loading", Constant.Log_Type_Info, printConsole: true);
                return;
            }
            try
            {
                Globals.LoadAllQueue(TEMP_DIR_PATH);
                CustomLog.Instant.IntervalJobLog("All queue loaded", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.Instant.ErrorLog(ex.ToString(), Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Init global data error: " + ex.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Press any key to return.", Constant.Log_Type_Error, printConsole: true);
                Console.ReadLine();
                return;
            }
        }
        static void StopProgram()
        {
            try
            {
                CustomLog.Instant.IntervalJobLog("Program stopping!", Constant.Log_Type_Info, printConsole: true);
                try
                {
                    Globals.SaveAllQueue(TEMP_DIR_PATH);
                    CustomLog.Instant.IntervalJobLog("All queue saved", Constant.Log_Type_Info, printConsole: true);
                }
                catch (Exception ex)
                {
                    CustomLog.Instant.ErrorLog("Save queue error:" + ex.ToString(), Constant.Log_Type_Error, printConsole: true);
                }
                if (_patientInPackageUpdateUsingJobs != null)
                {
                    for (int i = 0; i < _patientInPackageUpdateUsingJobs.Length; i++)
                    {
                        _patientInPackageUpdateUsingJobs[i].Stop();
                        _patientInPackageUpdateUsingJobs[i] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.Instant.ErrorLog(ex.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Program stopped by error!", Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog(ex.ToString(), Constant.Log_Type_Error, printConsole: true);
            }
        }
    }
}