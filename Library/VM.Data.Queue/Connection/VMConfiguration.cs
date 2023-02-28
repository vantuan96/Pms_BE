using System;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    public class VMConfiguration
    {
        public VMConfiguration()
        {
            _thread = new ThreadInfo();            
        }
        private string _name = "VMEC Config";
        [XmlAttribute("NAME")]
        public string NAME
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _code = "CN_01";
        [XmlElement("CODE")]
        public string CODE
        {
            get { return _code; }
            set { _code = value; }
        }
        private ThreadInfo _thread;
        [XmlElement("THREAD")]
        public ThreadInfo THREAD
        {
            get { return _thread; }
            set { _thread = value; }
        }
    }
}
