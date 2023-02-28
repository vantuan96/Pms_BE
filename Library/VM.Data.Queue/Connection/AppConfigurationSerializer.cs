using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    public class AppConfigurationSerializer
    {
        private XmlSerializer s = null;
		private Type type = null;


        public AppConfigurationSerializer()
        {
            this.type = typeof(AppConfiguration);
			this.s = new XmlSerializer(this.type);
		}

        public AppConfiguration Deserialize(string xml)
        {
			TextReader reader = new StringReader(xml);
			return Deserialize(reader);
		}

        public AppConfiguration Deserialize(XmlDocument doc)
        {
			TextReader reader = new StringReader(doc.OuterXml);
			return Deserialize(reader);
		}

        public AppConfiguration Deserialize(TextReader reader)
        {
            AppConfiguration o = (AppConfiguration)s.Deserialize(reader);
			reader.Close();
			return o;
		}

        public XmlDocument Serialize(AppConfiguration AppConfiguration)
        {
            string xml = StringSerialize(AppConfiguration);
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(xml);
			doc = Clean(doc);
			return doc;
		}

        private string StringSerialize(AppConfiguration AppConfiguration)
        {
            TextWriter w = WriterSerialize(AppConfiguration);
			string xml = w.ToString();
			w.Close();
			return xml;
		}

        private TextWriter WriterSerialize(AppConfiguration AppConfiguration)
        {
			TextWriter w = new StringWriter();
			this.s = new XmlSerializer(this.type);
            s.Serialize(w, AppConfiguration);
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

        public static AppConfiguration ReadFile(string file)
        {
            AppConfigurationSerializer serializer = new AppConfigurationSerializer();
			try {
				string xml = string.Empty;
				using (StreamReader reader = new StreamReader(file)) {
					xml = reader.ReadToEnd();
					reader.Close();
				}
				return serializer.Deserialize(xml);
			} catch {}
            return new AppConfiguration();
		}

		/// <summary>Writes config data to config file.</summary>
		/// <param name="file">Config file name.</param>
		/// <param name="config">Config object.</param>
		/// <returns></returns>
        public static bool WriteFile(string file, AppConfiguration config)
        {
			bool ok = false;
            AppConfigurationSerializer serializer = new AppConfigurationSerializer();
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
