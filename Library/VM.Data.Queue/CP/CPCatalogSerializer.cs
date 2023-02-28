using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    public class CPCatalogSerializer
    {
        private XmlSerializer s = null;
		private Type type = null;

		
        public CPCatalogSerializer()
        {
            this.type = typeof(CPCatalog);
			this.s = new XmlSerializer(this.type);
		}

        public CPCatalog Deserialize(string xml)
        {
			TextReader reader = new StringReader(xml);
			return Deserialize(reader);
		}

        public CPCatalog Deserialize(XmlDocument doc)
        {
			TextReader reader = new StringReader(doc.OuterXml);
			return Deserialize(reader);
		}

        public CPCatalog Deserialize(TextReader reader)
        {
            CPCatalog o = (CPCatalog)s.Deserialize(reader);
			reader.Close();
			return o;
		}

        public XmlDocument Serialize(CPCatalog CPCatalog)
        {
            string xml = StringSerialize(CPCatalog);
			XmlDocument doc = new XmlDocument();
			doc.PreserveWhitespace = true;
			doc.LoadXml(xml);
			doc = Clean(doc);
			return doc;
		}

        private string StringSerialize(CPCatalog CPCatalog)
        {
            TextWriter w = WriterSerialize(CPCatalog);
			string xml = w.ToString();
			w.Close();
			return xml;
		}

        private TextWriter WriterSerialize(CPCatalog CPCatalog)
        {
			TextWriter w = new StringWriter();
			this.s = new XmlSerializer(this.type);
            s.Serialize(w, CPCatalog);
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

        public static CPCatalog ReadFile(string file)
        {
            CPCatalogSerializer serializer = new CPCatalogSerializer();
			try {
				string xml = string.Empty;
				using (StreamReader reader = new StreamReader(file)) {
					xml = reader.ReadToEnd();
					reader.Close();
				}
				return serializer.Deserialize(xml);
			} catch {}
            return new CPCatalog();
		}

		/// <summary>Writes config data to config file.</summary>
		/// <param name="file">Config file name.</param>
		/// <param name="config">Config object.</param>
		/// <returns></returns>
        public static bool WriteFile(string file, CPCatalog config)
        {
			bool ok = false;
            CPCatalogSerializer serializer = new CPCatalogSerializer();
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
