#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Logging Manager
    /// </summary>
    public class LogManager:Singleton<LogManager>,IDisposable
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected LogManager()
            : base()
        {
            Logs = new Dictionary<string, ILog>();
            LogManager.Configuration = ConfigManager.Instance.GetConfigFile<LogConfig>("LogFile");
            if (LogManager.Configuration.Interval > 0)
            {
                FileTimer = new Timer(Configuration.Interval);
                FileTimer.Elapsed += new ElapsedEventHandler(FileTimer_Elapsed);
                FileTimer.Start();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when the timer is up and we must switch to a new log file
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event args object</param>
        void FileTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (string Key in Logs.Keys)
            {
                Logs[Key].TimeElapsed();
            }
        }

        #endregion

        #region Public Functions
        public ILog GetLog()
        {
            return GetLog("Default");
        }
        /// <summary>
        /// Gets a specified log
        /// </summary>
        /// <param name="Name">The name of the log file</param>
        /// <returns>The log file specified</returns>
        public ILog GetLog(string Name)
        {
            if (!Logs.ContainsKey(Name))
                Logs.Add(Name, new FileLog(Name));
            return Logs[Name];
        }


        public void AddLog(ILog Log)
        {
            AddLog(Log);
        }
        /// <summary>
        /// Adds a log object or replaces one already in use
        /// </summary>
        /// <param name="Log">Log file to add</param>
        /// <param name="Name">The name of the log file</param>
        public void AddLog(ILog Log, string Name)
        {
            if (Logs.ContainsKey(Name))
                Logs[Name] = Log;
            else
                Logs.Add(Name, Log);
        }

        #endregion

        #region Private and Internal Properties

        private Timer FileTimer { get; set; }

        private static Dictionary<string, ILog> Logs { get; set; }
        internal static LogConfig Configuration { get; set; }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (FileTimer != null)
            {
                FileTimer.Stop();
                FileTimer.Dispose();
                FileTimer = null;
            }
            if(Logs!=null)
            {
                foreach (string Key in Logs.Keys)
                {
                    Logs[Key].Dispose();
                }
            }
        }

        #endregion
    }
}
