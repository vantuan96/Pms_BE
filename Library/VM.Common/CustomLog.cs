using System;

namespace VM.Common
{
    public class CustomLog
    {
        public static readonly NLog.Logger accesslog = NLog.LogManager.GetLogger("accesslogger");
        public static readonly NLog.Logger errorlog = NLog.LogManager.GetLogger("errorlogger");
        public static readonly NLog.Logger apigwlog = NLog.LogManager.GetLogger("apigwlogger");
        public static readonly NLog.Logger intervaljoblog = NLog.LogManager.GetLogger("intervaljoblogger");
        public static readonly NLog.Logger performancejoblog = NLog.LogManager.GetLogger("performancejoblogger");
        public static readonly NLog.Logger requestlog = NLog.LogManager.GetLogger("requestlogger");
        private static CustomLog _instant { get; set; }
        public static CustomLog Instant
        {
            get
            {
                if (_instant == null)
                    _instant = new CustomLog();
                return _instant;
            }
            set
            {
                _instant = value;
            }
        }
        public void IntervalJobLog(string sMsg, string logType, bool printConsole=false)
        {
            if(logType== Constant.Log_Type_Info)
            {
                if (printConsole)
                    Console.WriteLine(sMsg);
                intervaljoblog.Info(sMsg);
            }else if(logType == Constant.Log_Type_Debug)
            {
                if (printConsole)
                    Console.WriteLine(sMsg);
                intervaljoblog.Debug(sMsg);
            }else if(logType == Constant.Log_Type_Error)
            {
                if (printConsole)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(sMsg);
                    Console.ResetColor();
                }
                intervaljoblog.Error(sMsg);
            }
        }
        public void ErrorLog(string sMsg, string logType, bool printConsole = false)
        {
            if (logType == Constant.Log_Type_Info)
            {
                if (printConsole)
                    Console.WriteLine(sMsg);
                errorlog.Info(sMsg);
            }
            else if (logType == Constant.Log_Type_Debug)
            {
                if (printConsole)
                    Console.WriteLine(sMsg);
                errorlog.Debug(sMsg);
            }
            else if (logType == Constant.Log_Type_Error)
            {
                if (printConsole)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(sMsg);
                    Console.ResetColor();
                }
                errorlog.Error(sMsg);
            }
        }
    }
}