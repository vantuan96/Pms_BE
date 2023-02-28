using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;


namespace VM.Data.Queue
{
    [XmlRoot("VMConfigurationConfig")]
    public class VMConfigurationConfig
    {
        private VMConfiguration con = new VMConfiguration();
        [XmlElement("INSTANCE")]
        public VMConfiguration INSTANCE
        {
            get { return con; }
            set { con = value; }
        }
    }
}
