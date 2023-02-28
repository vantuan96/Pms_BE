namespace DrFee.Utils
{
    public class CustomLog
    {
        public static readonly NLog.Logger accesslog = NLog.LogManager.GetLogger("accesslogger");
        public static readonly NLog.Logger errorlog = NLog.LogManager.GetLogger("errorlogger");
        public static readonly NLog.Logger apigwlog = NLog.LogManager.GetLogger("apigwlogger");
        public static readonly NLog.Logger intervaljoblog = NLog.LogManager.GetLogger("intervaljoblogger");
    }
}