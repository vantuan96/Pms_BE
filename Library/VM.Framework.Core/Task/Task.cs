#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Task base class
    /// </summary>
    public abstract class Task : ITask
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public Task()
        {
        }

        #endregion

        #region Abstract Functions

        public abstract void DoWork();

        #endregion

        #region Internal Functions

        internal void Setup(string ClassName)
        {
            try
            {
                Config = ConfigManager.Instance.GetConfigFile<TaskConfiguration>(ClassName);
                NextRunTime = Config.NextRunTime;
                if (Config.Frequency != RunTime.Once)
                {
                    while (NextRunTime < DateTime.Now || NextRunTime < Config.Start)
                    {
                        UpdateTime(false);
                    }
                }
                else
                {
                    if (NextRunTime < DateTime.Now)
                        NextRunTime = DateTime.Now;
                    if (NextRunTime < Config.Start)
                        NextRunTime = Config.Start;
                }
                if (NextRunTime > Config.End)
                    NextRunTime = DateTime.MaxValue;
            }
            catch { throw; }
        }

        internal void UpdateTime(bool Save)
        {
            try
            {
                if (Config.Frequency == RunTime.Hourly)
                {
                    NextRunTime = NextRunTime.AddHours(1.0d);
                }
                else if (Config.Frequency == RunTime.Daily)
                {
                    NextRunTime = NextRunTime.AddDays(1.0d);
                }
                else if (Config.Frequency == RunTime.Monthly)
                {
                    NextRunTime = NextRunTime.AddMonths(1);
                }
                else if (Config.Frequency == RunTime.Yearly)
                {
                    NextRunTime = NextRunTime.AddYears(1);
                }
                else if (Config.Frequency == RunTime.Weekly)
                {
                    NextRunTime = NextRunTime.AddDays(7.0d);
                }
                else if (Config.Frequency == RunTime.Once)
                {
                    NextRunTime = DateTime.MaxValue;
                }
                if (Save)
                {
                    Config.NextRunTime = NextRunTime;
                    Config.Save();
                }
            }
            catch { throw; }
        }

        #endregion

        #region Properties

        protected TaskConfiguration Config { get; set; }
        internal DateTime NextRunTime { get; set; }

        #endregion
    }
}
