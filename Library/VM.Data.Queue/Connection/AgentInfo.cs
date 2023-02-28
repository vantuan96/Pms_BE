using System;
using System.Collections;
using System.Xml.Serialization;


namespace VM.Data.Queue
{
    public class AgentInfo
    {
        private string _ipaddress = "127.0.0.1";
        [XmlElement("IP_ADDRESS")]
        public string IP_ADDRESS
        {
            get { return _ipaddress; }
            set { _ipaddress = value; }
        }

        private int _mo_port = 8080;
        [XmlElement("MO_PORT")]
        public int MO_PORT
        {
            get { return _mo_port; }
            set { _mo_port = value; }
        }

        private int _mt_port = 8180;
        [XmlElement("MT_PORT")]
        public int MT_PORT
        {
            get { return _mt_port; }
            set { _mt_port = value; }
        }

        private string _username = "gapit";
        [XmlElement("USERNAME")]
        public string USERNAME
        {
            get { return _username; }
            set { _username = value; }
        }

        private string _password = "gapit123";
        [XmlElement("PASSWORD")]
        public string PASSWORD
        {
            get { return _password; }
            set { _password = value; }
        }

        private string _uri = "/gapit";
        [XmlElement("URI")]
        public string URI
        {
            get { return _uri; }
            set { _uri = value; }
        }


        private int _data_buffer_size = 10240;
        [XmlElement("DATA_BUFFER_SIZE")]
        public int DATA_BUFFER_SIZE
        {
            get { return _data_buffer_size; }
            set { _data_buffer_size = value; }
        }

        private int _max_socket_client = 1024;
        [XmlElement("MAX_SOCKET_CLIENTS")]
        public int MAX_SOCKET_CLIENTS
        {
            get { return _max_socket_client; }
            set { _max_socket_client = value; }
        }

    }
}
