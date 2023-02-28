using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    /// <summary>
    /// SequenceNumber serializer utility
    /// </summary>
    public class SequenceNumberSerializer
    {
        private XmlSerializer s = null;
        private Type type = null;

        /// <summary>
        /// Default constructor. 
        /// </summary>
        public SequenceNumberSerializer()
        {
            this.type = typeof(SequenceNumber);
            this.s = new XmlSerializer(this.type);
        }

        /// <summary>Deserializes to an instance of SequenceNumber.</summary>
        /// <param name="xml">String xml.</param>
        /// <returns>SequenceNumber result.</returns>
        public SequenceNumber Deserialize(string xml)
        {
            TextReader reader = new StringReader(xml);
            return Deserialize(reader);
        }

        /// <summary>Deserializes to an instance of SequenceNumber.</summary>
        /// <param name="doc">XmlDocument instance.</param>
        /// <returns>SequenceNumber result.</returns>
        public SequenceNumber Deserialize(XmlDocument doc)
        {
            TextReader reader = new StringReader(doc.OuterXml);
            return Deserialize(reader);
        }

        /// <summary>Deserializes to an instance of SequenceNumber.</summary>
        /// <param name="reader">TextReader instance.</param>
        /// <returns>SequenceNumber result.</returns>
        public SequenceNumber Deserialize(TextReader reader)
        {
            SequenceNumber o = (SequenceNumber)s.Deserialize(reader);
            reader.Close();
            return o;
        }

        /// <summary>Serializes to an XmlDocument.</summary>
        /// <param name="GameCatalog">SequenceNumber to serialize.</param>
        /// <returns>XmlDocument instance.</returns>
        public XmlDocument Serialize(SequenceNumber logcatalog)
        {
            string xml = StringSerialize(logcatalog);
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(xml);
            doc = Clean(doc);
            return doc;
        }

        private string StringSerialize(SequenceNumber logcatalog)
        {
            TextWriter w = WriterSerialize(logcatalog);
            string xml = w.ToString();
            w.Close();
            return xml;
        }

        private TextWriter WriterSerialize(SequenceNumber logcatalog)
        {
            TextWriter w = new StringWriter();
            this.s = new XmlSerializer(this.type);
            s.Serialize(w, logcatalog);
            w.Flush();
            return w;
        }

        private XmlDocument Clean(XmlDocument doc)
        {
            doc.RemoveChild(doc.FirstChild);
            XmlNode first = doc.FirstChild;
            foreach (XmlNode n in doc.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    first = n;
                    break;
                }
            }
            if (first.Attributes != null)
            {
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
        public static SequenceNumber ReadFile(string file)
        {
            SequenceNumberSerializer serializer = new SequenceNumberSerializer();
            try
            {
                string xml = string.Empty;
                using (StreamReader reader = new StreamReader(file))
                {
                    xml = reader.ReadToEnd();
                    reader.Close();
                }
                return serializer.Deserialize(xml);
            }
            catch { }
            return new SequenceNumber();
        }

        /// <summary>Writes config data to config file.</summary>
        /// <param name="file">Config file name.</param>
        /// <param name="config">Config object.</param>
        /// <returns></returns>
        public static bool WriteFile(string file, SequenceNumber config)
        {
            bool ok = false;
            SequenceNumberSerializer serializer = new SequenceNumberSerializer();
            try
            {
                string xml = serializer.Serialize(config).OuterXml;
                using (StreamWriter writer = new StreamWriter(file, false))
                {
                    writer.Write(xml);
                    writer.Flush();
                    writer.Close();
                }
                ok = true;
            }
            catch { }
            return ok;
        }
    }
}
