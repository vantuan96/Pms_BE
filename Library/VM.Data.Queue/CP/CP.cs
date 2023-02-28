using System;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{   
    public class CP
    {
        private string _ID;
        /// <summary>
        /// ID of this CP, represent by number format
        /// </summary> 
        [XmlAttribute("ID")]
        public string ID
        {
            set { _ID = value; }
            get { return _ID;}
        }

        private string _NAME;
        /// <summary>
        /// The name of CP
        /// </summary>
        [XmlAttribute("NAME")]
        public string NAME
        {
            get { return _NAME; }
            set { _NAME = value; }
        }

        private string _NOTE;
        /// <summary>
        /// Sumary of CP
        /// </summary>
        [XmlElement("NOTE")]
        public string NOTE
        {
            get { return _NOTE; }
            set { _NOTE = value; }
        }

        private CPServices _services;
        /// <summary>
        /// CP services
        /// </summary>
        [XmlElement("SERVICES")]
        public CPServices CPServices
        {
            get { return _services; }
            set { _services = value; }
        }


        private CP _parent;
        /// <summary>
        /// CP services
        /// </summary>
        [XmlElement("PARENT")]
        public CP PARENT
        {
            get { return _parent; }
            set { _parent = value; }
        }


        private bool _enable = true;
        /// <summary>
        /// CP Enable status
        /// </summary>
        [XmlAttribute("ENABLE")]
        public bool ENABLE
        {
            get { return _enable; }
            set { _enable = value; }
        }       

    }
}
