using System;
using System.Collections;
using System.Xml.Serialization;


namespace VM.Data.Queue
{
    public class GatewayInfo
    {
        private string _ipaddress = "127.0.0.1";
        [XmlElement("IP_ADDRESS")]
        public string IP_ADDRESS 
        {
            get { return _ipaddress; }
            set { _ipaddress = value; }
        }

        private int _port = 8080;
        [XmlElement("PORT")]
        public int PORT
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _username = "miu";
        [XmlElement("USERNAME")]
        public string USERNAME
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password = "miu8369";
        [XmlElement("PASSWORD")]
        public string PASSWORD
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _uri = "/miumt";
        [XmlElement("URI")]
        public string URI
        {
            get { return _uri; }
            set { _uri = value; }
        }


    }
}
