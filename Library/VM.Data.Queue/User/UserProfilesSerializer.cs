using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    public class UserProfilesSerializer
    {
        private XmlSerializer s = null;
		private Type type = null;

		/// <summary>Default constructor.</summary>
        public UserProfilesSerializer()
        {
            this.type = typeof(UserProfiles);
			this.s = new XmlSerializer(this.type);
		}

        /// <summary>Deserializes to an instance of UserProfiles.</summary>
		/// <param name="xml">String xml.</param>
        /// <returns>UserProfiles result.</returns>
        public UserProfiles Deserialize(string xml)
        {
			TextReader reader = new StringReader(xml);
			return Deserialize(reader);
		}

        /// <summary>Deserializes to an instance of UserProfiles.</summary>
		/// <param name="doc">XmlDocument instance.</param>
        /// <returns>UserProfiles result.</returns>
        public UserProfiles Deserialize(XmlDocument doc)
        {
			TextReader reader = new StringReader(doc.OuterXml);
			return Deserialize(reader);
		}

        /// <summary>Deserializes to an instance of UserProfiles.</summary>
		/// <param name="reader">TextReader instance.</param>
        /// <returns>UserProfiles result.</returns>
        public UserProfiles Deserialize(TextReader reader)
        {
			UserProfiles o = (UserProfiles)s.Deserialize(reader);
			reader.Close();
			return o;
		}

		/// <summary>Serializes to an XmlDocument.</summary>
		/// <param name="UserProfiles">UserProfiles to serialize.</param>
		/// <returns>XmlDocument instance.</returns>
		public XmlDocument Serialize(UserProfiles UserProfiles) {
			string xml = StringSerialize(UserProfiles);
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(xml);
			doc = Clean(doc);
			return doc;
		}

		private string StringSerialize(UserProfiles UserProfiles) {
			TextWriter w = WriterSerialize(UserProfiles);
			string xml = w.ToString();
			w.Close();
			return xml;
		}

		private TextWriter WriterSerialize(UserProfiles UserProfiles) {
			TextWriter w = new StringWriter();
			this.s = new XmlSerializer(this.type);
			s.Serialize(w, UserProfiles);
			w.Flush();
			return w;
		}

		private XmlDocument Clean(XmlDocument doc) {
			doc.RemoveChild(doc.FirstChild);
			XmlNode first = doc.FirstChild;
			foreach (XmlNode n in doc.ChildNodes) {
				if (n.NodeType == XmlNodeType.Element) {
					first = n;
					break;
				}
			}
			if (first.Attributes != null) {
				XmlAttribute a = null;
				a = first.Attributes["xmlns:xsd"];
				if (a != null) { first.Attributes.Remove(a); }
				a = first.Attributes["xmlns:xsi"];
				if (a != null) { first.Attributes.Remove(a); }
			}
			return doc;
		}

		/// <summary>Reads config data from config file.</summary>
		/// <param name="file">Config file name.</param>
		/// <returns></returns>
		public static UserProfiles ReadFile(string file) {
			UserProfilesSerializer serializer = new UserProfilesSerializer();
			try {
				string xml = string.Empty;
				using (StreamReader reader = new StreamReader(file)) {
					xml = reader.ReadToEnd();
					reader.Close();
				}
				return serializer.Deserialize(xml);
			} catch {}
			return new UserProfiles();
		}

		/// <summary>Writes config data to config file.</summary>
		/// <param name="file">Config file name.</param>
		/// <param name="config">Config object.</param>
		/// <returns></returns>
		public static bool WriteFile(string file, UserProfiles config) {
			bool ok = false;
			UserProfilesSerializer serializer = new UserProfilesSerializer();
			try {
				string xml = serializer.Serialize(config).OuterXml;
				using (StreamWriter writer = new StreamWriter(file, false)) {
					writer.Write(xml);
					writer.Flush();
					writer.Close();
				}
				ok = true;
			} catch {}
			return ok;
		}
    }
}
