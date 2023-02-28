using System;
using System.Collections;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    /// <summary>
    /// CP service method
    /// </summary>
    public enum METHOD_ENUM
    {
        WEBSERVICE, HTTP, HTTPS, XML        
    }

    public class CPService
    {        

        private string _ID_CP;
        /// <summary>
        /// The id of Content provider that contains run this service
        /// </summary>
        [XmlAttribute("ID_CP")]
        public string ID_CP
        {
            get { return _ID_CP; }
            set { _ID_CP = value; }
        }

        private string _ID;
        /// <summary>
        /// The id of service 
        /// </summary>
        [XmlAttribute("ID")]
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        private string _NAME;
        /// <summary>
        /// The name of service
        /// </summary>
        [XmlAttribute("NAME")]
        public string NAME
        {
            get { return _NAME; }
            set { _NAME = value; }
        }


        private string _NOTE;
        /// <summary>
        /// Description about the service
        /// </summary>        
        [XmlElement("NOTE")]
        public string NOTE
        {
            get { return _NOTE; }
            set { _NOTE = value; }
        }

        private Keywords _KWS;
        /// <summary>
        /// The command list that service use
        /// </summary>
        [XmlElement("KEYWORDs")]
        public Keywords KEYWORDS //8469 = AT, BT; 8169 = TCTG
        {
            get { return _KWS; }
            set { _KWS = value; }
        }

        private DateTime _SD;
        /// <summary>
        /// The date time service will be started
        /// </summary>      
        [XmlElement("STARTDATE")]
        public DateTime STARTDATE
        {
            get { return _SD; }
            set { _SD = value; }
        }

        private DateTime _ED;
        /// <summary>
        /// The date time service will be stoped
        /// </summary>    
        [XmlElement("ENDDATE")]
        public DateTime ENDDATE
        {
            get { return _ED; }
            set { _ED = value; }
        }

        private METHOD_ENUM _METHOD;
        /// <summary>
        /// The action that Agent will handle on this service.
        /// May be forward message using WEBSERVICE or HTTP method.
        /// May be self processing using PROCESS method
        /// </summary>
        [XmlElement("METHOD")]
        public METHOD_ENUM METHOD
        {
            get { return _METHOD; }
            set { _METHOD = value; }
        }

        private string _URL;
        /// <summary>
        /// The URL of webservice if method = WEBSERVICE
        /// </summary>
        [XmlElement("URL")]
        public string URL  //Url of webservice
        {
            get { return _URL; }
            set { _URL = value; }
        }

        private string _URL_FA;
        /// <summary>
        /// The second URL (when first URI is failed access) of webservice if method = WEBSERVICE
        /// </summary>
        [XmlElement("URL_FA")]
        public string URL_FA
        {
            get { return _URL_FA; }
            set { _URL_FA = value; }
        }

        private string _ADDRESS = "127.0.0.1";
        /// <summary>
        /// The ip address of the server that is listenning if method = HTTP
        /// </summary>
        [XmlElement("ADDRESS")]
        public string ADDRESS
        {
            get { return _ADDRESS; }
            set { _ADDRESS = value; }
        }

        private int _PORT;
        /// <summary>
        /// The port of the servier that is listenning if method = HTTP
        /// </summary>
        [XmlElement("PORT")]
        public int PORT
        {
            get { return _PORT; }
            set { _PORT = value; }
        }

        private string _NAMESPACE;
        /// <summary>
        /// The namespace of the assembly processes CP service if method = PROCESS
        /// </summary>
        [XmlElement("NAMESPACE")]
        public string NAMESPACE
        {
            get { return _NAMESPACE; }
            set { _NAMESPACE = value; }
        }

        private string _CLASS;
        /// <summary>
        /// The class name of an instance that processes CP service if method = PROCESS
        /// </summary>
        [XmlElement("CLASS")]
        public string CLASS
        {
            get { return _CLASS; }
            set { _CLASS = value; }
        }

        private string _USERNAME;
        /// <summary>
        /// The account name for CP assigned in the message data before sending
        /// </summary>
        [XmlElement("USERNAME")]
        public string USERNAME
        {
            get { return _USERNAME; }
            set { _USERNAME = value; }
        }

        private string _PASSWORD;
        /// <summary>
        /// The account name for CP assigned in the message data before sending
        /// </summary>
        [XmlElement("PASSWORD")]
        public string PASSWORD
        {
            get { return _PASSWORD; }
            set { _PASSWORD = value; }
        }


        public override string ToString()
        {
            return _NAME;
        }

    }
}
