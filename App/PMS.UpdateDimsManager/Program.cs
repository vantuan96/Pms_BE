using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PMS.UpdateDimsManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            #if DEBUG
            UpdateDimsService sv = new UpdateDimsService();
            sv.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
            #else
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new UpdateDimsService()
                };
                ServiceBase.Run(ServicesToRun);
            #endif
        }
    }
}
