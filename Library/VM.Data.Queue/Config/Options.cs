using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    [Serializable]
    public class Options
    {
        #region Database

        public string DatabaseDateFormat = "yyyyMMddHHmmss";
        public string CPDataFile = AppDomain.CurrentDomain.BaseDirectory + "CPs/CPCatalog.xml";
        public string OperatorDataFile = AppDomain.CurrentDomain.BaseDirectory + "Operators/OperatorCatalog.xml";
        public string UserDataFile = AppDomain.CurrentDomain.BaseDirectory + "Users/UserProfiles.xml";
        public string ConnectionDataFile = AppDomain.CurrentDomain.BaseDirectory + "Connections/ConnectionCatalog.xml";

        #endregion

        #region Gateway

        #endregion

        #region Log

        public int MaxLogFileLength = 100000;
        public int MaxLogFileLineNumber = 10;
        public bool LogCheckByLineNumber = true;
        public string LogFileSuffixPattern = "yyyy_MM_dd_HH_mm_ss";
        public string LogFileName = "AgentLog.log";
        public string LogFileDir = "D:/Agent/Log/";

        #endregion

        #region Message

        //public int MOLogFileMaxLength = 100000;
        //public int MOLogFileLineNumber = 10;
        //public bool MOLogCheckByLineNumber = true;
        //public string MOLogFileSuffixPattern = "yyyy_MM_dd_HH_mm_ss";
        //public string MOLogFileName = "MOLog.log";
        //public string MOLogFileDir = "D:/Agent/Inbox/";

        //public int MOReceiverThreadNumber = 1;
        //public int MOReceiverProcessSpeed = 5;//ms


        //public int MTLogFileMaxLength = 100000;
        //public int MTLogFileLineNumber = 10;
        //public bool MTLogCheckByLineNumber = true;
        //public string MTLogFileSuffixPattern = "yyyy_MM_dd_HH_mm_ss";
        //public string MTLogFileName = "MTLog.log";
        //public string MTLogFileDir = "D:/Agent/Sent/";

        //public int BADLogFileMaxLength = 100000;
        //public int BADLogFileLineNumber = 10;
        //public bool BADLogCheckByLineNumber = true;
        //public string BADLogFileSuffixPattern = "yyyy_MM_dd_HH_mm_ss";
        //public string BADLogFileName = "BADLog.log";
        //public string BADLogFileDir = "D:/Agent/Bad/";

        #endregion        

        #region Methods

        public bool Exists()
        {
            string filepath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Settings.config");
            return File.Exists(filepath);
        }

        public static Options Load()
        {
            Options _settings = new Options();
            try
            {
                string filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Settings.config");
                XmlSerializer ser = new XmlSerializer(typeof(Options));
                TextReader reader = new StreamReader(filename);
                _settings = (Options)ser.Deserialize(reader);
                reader.Close();
                return _settings;
            }
            catch
            {                
                return _settings;
            }
        }

        public void Save()
        {
            string filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Settings.config");
            XmlSerializer ser = new XmlSerializer(typeof(Options));
            TextWriter writer = new StreamWriter(filename);
            ser.Serialize(writer, this);
            writer.Close();
        }
        #endregion
    }
}
