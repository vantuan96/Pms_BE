using System;
using System.Collections;
using System.Xml.Serialization;


namespace VM.Data.Queue
{
    public enum DATABASE_TYPE
    {
        None,
        Text,
        XML, 
        MSSQL, 
        Oracle, 
        MSAccess
    }

    public class Database
    {
        private DATABASE_TYPE _type = DATABASE_TYPE.Text;        
        [XmlElement("TYPE")]
        public DATABASE_TYPE TYPE
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _date_format = "yyyyMMdd24HHmmss";
        [XmlElement("DATE_FORMAT")]
        public string DATE_FORMAT
        {
            get { return _date_format; }
            set { _date_format = value; }
        }       

        #region Flat Text File

        private string _inbox_path = AppDomain.CurrentDomain.BaseDirectory + "Inbox\\";
        [XmlElement("INBOX_PATH")]
        public string INBOX_PATH
        {
            get { return _inbox_path; }
            set { _inbox_path = value; }
        }
        private string _sent_path = AppDomain.CurrentDomain.BaseDirectory + "Sent\\";
        [XmlElement("SENT_PATH")]
        public string SENT_PATH
        {
            get { return _sent_path; }
            set { _sent_path = value; }
        }

        private string _htrconsumerresponse_path = AppDomain.CurrentDomain.BaseDirectory + "htrconsumerresponse\\";
        [XmlElement("HTRCONSUMERRESPONSE_PATH")]
        public string HTRCONSUMERRESPONSE_PATH
        {
            get { return _htrconsumerresponse_path; }
            set { _htrconsumerresponse_path = value; }
        }

        private string _bad_path = AppDomain.CurrentDomain.BaseDirectory + "Bad\\";
        [XmlElement("BAD_PATH")]
        public string BAD_PATH
        {
            get { return _bad_path; }
            set { _bad_path = value; }
        }


        private string _inbox_file = "MO.log";
        [XmlElement("INBOX_FILE")]
        public string INBOX_FILE
        {
            get { return _inbox_file; }
            set { _inbox_file = value; }
        }

        private string _sent_file = "MT.log";
        [XmlElement("SENT_FILE")]
        public string SENT_FILE
        {
            get { return _sent_file; }
            set { _sent_file = value; }
        }

        private string _htrconsumerresponse_file = "htrconsumerreponse.log";
        [XmlElement("HTRCONSUMERRESPONSE_FILE")]
        public string HTRCONSUMERRESPONSE_FILE
        {
            get { return _htrconsumerresponse_file; }
            set { _htrconsumerresponse_file = value; }
        }

        private string _bad_file = "BAD.log";
        [XmlElement("BAD_FILE")]
        public string BAD_FILE
        {
            get { return _bad_file; }
            set { _bad_file = value; }
        }


        private string _prefix_pattern = "yyyy_MM_dd_24HH_mm_ss";
        [XmlElement("PREFIX_PATTERN")]
        public string PREFIX_PATTERN
        {
            get { return _prefix_pattern; }
            set { _prefix_pattern = value; }
        }



        private bool _checkByLineNumber = true;
        [XmlElement("CHECK_BY_LINE_NUMBER")]
        public bool CHECK_BY_LINE_NUMBER
        {
            get { return _checkByLineNumber; }
            set { _checkByLineNumber = value; }
        }


        private int _fileLineNumber = 100;
        [XmlElement("FILE_LINE_NUMBER")]
        public int FILE_LINE_NUMBER
        {
            get { return _fileLineNumber; }
            set { _fileLineNumber = value; }
        }


        private int _fileSize = 100000;
        [XmlElement("FILE_SIZE")]
        public int FILE_SIZE
        {
            get { return _fileSize; }
            set { _fileSize = value; }
        }


        private string _listFirstName = "";
        [XmlElement("LISTFIRSTNAME")]
        public string LISTFIRSTNAME
        {
            get { return _listFirstName; }
            set { _listFirstName = value; }
        }
        private string _listLastName = "";
        [XmlElement("LISTLASTNAME")]
        public string LISTLASTNAME
        {
            get { return _listLastName; }
            set { _listLastName = value; }
        }

        private string _listFirstName_Vn = "";
        [XmlElement("LISTFIRSTNAME_VN")]
        public string LISTFIRSTNAME_VN
        {
            get { return _listFirstName_Vn; }
            set { _listFirstName_Vn = value; }
        }
        private string _listLastName_Vn = "";
        [XmlElement("LISTLASTNAME_VN")]
        public string LISTLASTNAME_VN
        {
            get { return _listLastName_Vn; }
            set { _listLastName_Vn = value; }
        }

        private string _listFirstName_Kr = "";
        [XmlElement("LISTFIRSTNAME_KR")]
        public string LISTFIRSTNAME_KR
        {
            get { return _listFirstName_Kr; }
            set { _listFirstName_Kr = value; }
        }
        private string _listLastName_Kr = "";
        [XmlElement("LISTLASTNAME_KR")]
        public string LISTLASTNAME_KR
        {
            get { return _listLastName_Kr; }
            set { _listLastName_Kr = value; }
        }
        #endregion


        #region MS SQLSERVER

        private string _mssql_server = "";
        [XmlElement("MSSQL_SERVER")]
        public string MSSQL_SERVER
        {
            get { return _mssql_server; }
            set { _mssql_server = value; }
        }

        private string _mssql_database = "";
        [XmlElement("MSSQL_DATABASE")]
        public string MSSQL_DATABASE
        {
            get { return _mssql_database; }
            set { _mssql_database = value; }
        }


        private string _mssql_user = "";
        [XmlElement("MSSQL_USER")]
        public string MSSQL_USER
        {
            get { return _mssql_user; }
            set { _mssql_user = value; }
        }


        private string _mssql_pass = "";
        [XmlElement("MSSQL_PASS")]
        public string MSSQL_PASS
        {
            get { return _mssql_pass; }
            set { _mssql_pass = value; }
        }


        #endregion

        
    }
}
