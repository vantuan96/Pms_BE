using System;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Configuration for the task scheduler
    /// </summary>
    [ConfigAttribute(Name = "TaskScheduler")]
    public class Configuration : Config<Configuration>
    {       
        #region Properties

        /// <summary>
        /// Number of threads to use
        /// </summary>
        public virtual int NumberOfThreads { get {                       
            return 4; } }


        protected override string ConfigFileLocation
        {
            get
            {
                return @"TaskManager.config";
            }
        }

        #endregion
    }
}
