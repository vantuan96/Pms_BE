using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Log file configuration object
    /// </summary>
    [ConfigAttribute(Name = "LogFile")]
    public class LogConfig : Config<LogConfig>
    {
        #region Properties

        /// <summary>
        /// Header text displayed in the log
        /// </summary>
        public virtual string Header
        {
            get
            {
                StringBuilder Message = new StringBuilder();
                Message.Append("----------Log-Beginning----------").Append(System.Environment.NewLine);
                Message.Append("Log created using Blammo.Net ver. 1.0").Append(System.Environment.NewLine);
                Message.Append("Blammo.Net created/maintained by James Craig, http://www.gutgames.com").Append(System.Environment.NewLine);
                Message.Append("Date Created: ").Append(DateTime.Now.ToString()).Append(System.Environment.NewLine);
                return Message.ToString();
            }

            set { }
        }

        /// <summary>
        /// Footer text displayed in the log
        /// </summary>
        public virtual string Footer
        {
            get
            {
                return "----------Log-Ending-------------" + System.Environment.NewLine;
            }

            set { }
        }

        /// <summary>
        /// The DateTime format used in logging messages (default is MMMM dd, yyyy HH:mm:ss)
        /// M = month
        /// d = day
        /// y = year
        /// H = hour
        /// m = minute
        /// s = second
        /// </summary>
        public virtual string MessageDateTimeFormat { get { return "MMMM dd, yyyy HH:mm:ss"; } set { } }

        /// <summary>
        /// The DateTime format used in creating files (default is MMddyyHHmmss)
        /// M = month
        /// d = day
        /// y = year
        /// H = hour
        /// m = minute
        /// s = second
        /// </summary>
        public virtual string FileDateTimeFormat { get { return "MMddyyHHmmss"; } set { } }

        /// <summary>
        /// Directory that log files are saved in
        /// </summary>
        public virtual string LogFileLocation { get { return @"C:\logs\"; } set { } }

        /// <summary>
        /// Number of files to rotate through with each logging set
        /// </summary>
        public virtual int NumberOfFiles { get { return 1; } set { } }

        /// <summary>
        /// Interval to switch between files. (0 means no switching)
        /// </summary>
        public virtual double Interval { get { return 0; } set { } }

        /// <summary>
        /// Defines the format for the message
        /// &lt;MessageType&gt; = Message Type
        /// &lt;Message&gt; = Message Text
        /// &lt;Current&gt; = Current Date/Time
        /// </summary>
        public virtual string MessageFormat { get { return "<MessageType> : <Current> : <Message>" + System.Environment.NewLine; } set { } }

        /// <summary>
        /// Defines the format used for the file
        /// &lt;Name&gt; = Log's name
        /// &lt;FileNumber&gt; = File number
        /// &lt;Current&gt; = Current Date/Time
        /// </summary>
        public virtual string FileFormat { get { return "<Name>_<Current>_<FileNumber>.log"; } }

        /// <summary>
        /// Determines if general messages are recorded
        /// </summary>
        public virtual bool GeneralEnabled { get { return true; } }

        /// <summary>
        /// Determines if debug messages are recorded
        /// </summary>
        public virtual bool DebugEnabled { get { return true; } }

        /// <summary>
        /// Determines if trace messages are recorded
        /// </summary>
        public virtual bool TraceEnabled { get { return true; } }

        /// <summary>
        /// Determines if info messages are recorded
        /// </summary>
        public virtual bool InfoEnabled { get { return true; } }

        /// <summary>
        /// Determines if warning messages are recorded
        /// </summary>
        public virtual bool WarnEnabled { get { return true; } }

        /// <summary>
        /// Determines if error messages are recorded
        /// </summary>
        public virtual bool ErrorEnabled { get { return true; } }

        #endregion
    }
}
