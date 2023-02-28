#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Log object that outputs to a file
    /// </summary>
    public class FileLog : Log
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Log name</param>
        public FileLog(string Name)
            : base(Name)
        {
        }

        #endregion

        #region Protected Overridden Functions

        protected override void DisplayHeader(string Message, string TempFileFormat)
        {
           FileManager.SaveFile(Message, TempFileFormat);
        }

        protected override void DisplayFooter(string Message, string TempFileName)
        {
           FileManager.SaveFile(Message, TempFileName, true);
        }

        #endregion

        #region ILog Members

        public override void LogMessage(string Message, MessageType Type, params object[] args)
        {
            bool Display = false;
            if (Type == MessageType.General && LogManager.Configuration.GeneralEnabled)
                Display = true;
            else if (Type == MessageType.Debug && LogManager.Configuration.DebugEnabled)
                Display = true;
            else if (Type == MessageType.Error && LogManager.Configuration.ErrorEnabled)
                Display = true;
            else if (Type == MessageType.Info && LogManager.Configuration.InfoEnabled)
                Display = true;
            else if (Type == MessageType.Trace && LogManager.Configuration.TraceEnabled)
                Display = true;
            else if (Type == MessageType.Warn && LogManager.Configuration.WarnEnabled)
                Display = true;
            if (Display)
            {
                if (args.Length > 0)
                    Message = string.Format(Message, args);
                Message = GetMessage(LogManager.Configuration.MessageFormat, Type, Message);
              FileManager.SaveFile(Message, CurrentFileName, true);
            }
        }

        #endregion
    }
}
