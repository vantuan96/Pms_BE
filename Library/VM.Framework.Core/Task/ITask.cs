using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Task interface
    /// </summary>
    public interface ITask
    {
        #region Functions

        /// <summary>
        /// Called to do the actual work of the task
        /// </summary>
        void DoWork();

        #endregion
    }
}
