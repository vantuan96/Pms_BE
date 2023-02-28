#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Outputs messages to XML
    /// </summary>
    public class ConsoleLog : Log
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the log</param>
        public ConsoleLog(string Name)
            : base(Name)
        {
        }

        #endregion

        #region Overridden Functions

        protected override void DisplayHeader(string Message, string TempFileFormat)
        {
            Console.Write(Message);
        }

        protected override void DisplayFooter(string Message, string TempFileName)
        {
            Console.Write(Message);
        }

        public override void LogMessage(string Message, MessageType Type, params object[] args)
        {
            if (args.Length > 0)
                Message = string.Format(Message, args);
            Message = GetMessage(LogManager.Configuration.MessageFormat, Type, Message);
            Console.Write(Message);
        }

        public override void TimeElapsed()
        {

        }

        #endregion
    }
}
