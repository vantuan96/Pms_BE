using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Configuration base class for tasks
    /// </summary>
    public class TaskConfiguration : Config<TaskConfiguration>
    {
        #region Properties

        /// <summary>
        /// Frequency the task is run
        /// </summary>
        public virtual RunTime Frequency { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        public virtual DateTime Start { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        public virtual DateTime End { get; set; }

        /// <summary>
        /// Next run time
        /// </summary>
        public virtual DateTime NextRunTime { get; set; }

        #endregion
    }
}
