#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Log interface
    /// </summary>
    public interface ILog : IDisposable
    {
        #region Functions

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="Message">Message text</param>
        /// <param name="Type">Message type</param>
        /// <param name="args">Any additional arguments that will be used in formatting the message</param>
        void LogMessage(string Message, MessageType Type, params object[] args);

        /// <summary>
        /// Called by the manager when time has elapsed and a new file is needed.
        /// </summary>
        void TimeElapsed();

        #endregion
    }
}
