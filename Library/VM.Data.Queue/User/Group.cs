using System;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    /// <summary>
    /// The group that represent the user rights
    /// </summary>
    public class Group
    {
        private String _groupname;
		private String _description;

		/// <summary></summary>
        public Group()
        {
		}

        /// <summary>Name of group.</summary>
        [XmlAttribute("Name")]
        public System.String Name
        {
            get { return _groupname; }
            set { _groupname = value; }
        }

        /// <summary>System.String url attribute.</summary>
        [XmlElement("Description")]
        public System.String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string ToString()
        {
            return _groupname + "(" + _description + ")";
        }
    }
}
