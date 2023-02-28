using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Data.Queue
{
    public class Globals
    {
        public static bool IsAutoUpdateServiceProcessing = false;
        public static bool IsAutoCalculatePriceProcessing = false;
        public static bool IsAutoUpdateStatusProcessing = false;
        public static bool IsGettingDimsHisChargeProcessing = false;
        public static bool IsGettingPatientInPackage4UpdateUsingProcessing = false;
        #region Revenue his
        public static Queue HIS_REVENUE_QUEUE = new Queue();
        #endregion
        public static Options AgentConfigs = new Options();

        public static void SaveAllQueue(string Path)
        {
            if (!System.IO.Directory.Exists(Path))
                System.IO.Directory.CreateDirectory(Path);
            SaveQueue(Globals.HIS_REVENUE_QUEUE, string.Format("{0}\\HIS_REVENUE_QUEUE.dat", Path));
        }

        public static void LoadAllQueue(string Path)
        {
            Globals.HIS_REVENUE_QUEUE = LoadQueue(string.Format("{0}\\HIS_REVENUE_QUEUE.dat", Path));
        }

        private static void SaveQueue(Queue q, string fileName)
        {
            //Save data                                
            if (q.ToArrayList().Count > 0)
            {
                q.SaveQueue(fileName);
            }
        }

        private static Queue LoadQueue(string fileName)
        {
            Queue q = new Queue();
            if (System.IO.File.Exists(fileName))
            {
                q.LoadQueue(fileName);
                System.IO.File.Delete(fileName);
            }
            return q;
        }

        public static string FormatPath(string path)
        {
            string pathRet;
            if (path.StartsWith("."))
            {
                pathRet = string.Format("{0}\\{1}", System.IO.Directory.GetCurrentDirectory(), path.Substring(1));
            }
            else
            {
                pathRet = path;
            }
            return pathRet;
        }
        public static bool CheckSSH(int iPort)
        {
            bool isAvailable = false;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", iPort);
                    isAvailable = true;
                }
                catch (Exception)
                {
                    //Console.WriteLine("Port closed");
                }
            }
            return isAvailable;
        }
    }
}
