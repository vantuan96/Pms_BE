#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Base class for logs
    /// </summary>
    public class Log : ILog, IDisposable
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Log name</param>
        public Log(string Name)
        {
            this.FileName = Path.Combine(LogManager.Configuration.LogFileLocation, Name);
            CurrentFileNumber = 1;
            CurrentDate = DateTime.Now;
            for (int x = 1; x <= LogManager.Configuration.NumberOfFiles; ++x)
            {
                Start(x);
            }
            CurrentFileName = GetFileName(CurrentFileNumber, LogManager.Configuration.FileFormat);
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Adds header information to the 
        /// </summary>
        /// <param name="FileNumberUsing"></param>
        protected virtual void Start(int FileNumberUsing)
        {
            string Message = LogManager.Configuration.Header;
            string TempFileFormat = LogManager.Configuration.FileFormat;
            TempFileFormat = GetFileName(FileNumberUsing, TempFileFormat);
            DisplayHeader(Message, TempFileFormat);
        }

        /// <summary>
        /// Actually displays the header information (must be overridden)
        /// </summary>
        /// <param name="Message">Header message</param>
        /// <param name="TempFileFormat">Name of the file to use (may not be needed)</param>
        protected virtual void DisplayHeader(string Message, string TempFileFormat)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the file name in use
        /// </summary>
        /// <param name="FileNumberUsing">Current File Number</param>
        /// <param name="TempFileFormat">File format in use</param>
        /// <returns>The formatted file name to use</returns>
        protected virtual string GetFileName(int FileNumberUsing, string TempFileFormat)
        {
            TempFileFormat = Current.Replace(TempFileFormat, CurrentDate.ToString(LogManager.Configuration.FileDateTimeFormat));
            TempFileFormat = Name.Replace(TempFileFormat, FileName);
            TempFileFormat = FileNumber.Replace(TempFileFormat, FileNumberUsing.ToString());
            return TempFileFormat;
        }

        /// <summary>
        /// Gets the formatted message
        /// </summary>
        /// <param name="MessageFormat">Message format to use</param>
        /// <param name="TempMessageType">Message type</param>
        /// <param name="MessageString">Actual message string</param>
        /// <returns>The formatted message</returns>
        protected virtual string GetMessage(string MessageFormat, MessageType TempMessageType, string MessageString)
        {
            MessageFormat = Current.Replace(MessageFormat, DateTime.Now.ToString(LogManager.Configuration.MessageDateTimeFormat));
            MessageFormat = MessageType.Replace(MessageFormat, TempMessageType.ToString());
            MessageFormat = Message.Replace(MessageFormat, MessageString);
            return MessageFormat;
        }

        /// <summary>
        /// Ends the log
        /// </summary>
        /// <param name="Message">Message to display</param>
        /// <param name="FileNumber">File number</param>
        protected virtual void End(string Message, int FileNumber)
        {
            string TempFileName = GetFileName(FileNumber, LogManager.Configuration.FileFormat);
            DisplayFooter(Message, TempFileName);
        }

        /// <summary>
        /// Actually displays the footer information (must be overridden)
        /// </summary>
        /// <param name="Message">Header message</param>
        /// <param name="TempFileName">Name of the file to use (may not be needed)</param>
        protected virtual void DisplayFooter(string Message, string TempFileName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// File name
        /// </summary>
        protected string FileName { get; set; }

        /// <summary>
        /// Current file number
        /// </summary>
        protected int CurrentFileNumber { get; set; }

        /// <summary>
        /// The full current file name
        /// </summary>
        protected string CurrentFileName { get; set; }

        /// <summary>
        /// Gets the current date/time (used in formatting)
        /// </summary>
        protected DateTime CurrentDate { get; set; }

        #endregion

        #region Private Variables

        private Regex Current = new Regex(@"(?<Current><Current>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex Name = new Regex(@"(?<Name><Name>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex FileNumber = new Regex(@"(?<FileNumber><FileNumber>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex MessageType = new Regex(@"(?<MessageType><MessageType>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private Regex Message = new Regex(@"(?<Message><Message>)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #endregion

        #region Interface Functions

        public virtual void Dispose()
        {
            string Message = LogManager.Configuration.Footer;
            for (int x = 1; x <= LogManager.Configuration.NumberOfFiles; ++x)
            {
                End(Message, x);
            }
        }

        public virtual void LogMessage(string Message, MessageType Type, params object[] args)
        {
            throw new NotImplementedException();
        }

        public virtual void TimeElapsed()
        {
            CurrentFileNumber = CurrentFileNumber % LogManager.Configuration.NumberOfFiles;
            if (CurrentFileNumber == 0)
            {
                for (int x = 1; x <= LogManager.Configuration.NumberOfFiles; ++x)
                {
                    End(LogManager.Configuration.Footer, x);
                }
                CurrentDate = DateTime.Now;
                ++CurrentFileNumber;
                for (int x = 1; x <= LogManager.Configuration.NumberOfFiles; ++x)
                {
                    Start(x);
                }
                CurrentFileName = GetFileName(CurrentFileNumber, LogManager.Configuration.FileFormat);
            }
            else
            {
                ++CurrentFileNumber;
                Start(CurrentFileNumber);
                CurrentFileName = GetFileName(CurrentFileNumber, LogManager.Configuration.FileFormat);
            }
        }

        #endregion
    }
}
