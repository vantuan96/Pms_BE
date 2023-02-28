#region Usings
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Xml.Serialization;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Helps with serializing an object to XML and back again.
    /// </summary>
    public static class Serialization
    {
        #region Public Static Functions
        /// <summary>
        /// Serializes an object to binary
        /// </summary>
        /// <param name="Object">Object to serialize</param>
        /// <param name="Output">Binary output of the object</param>
        public static void ObjectToBinary(object Object, out byte[] Output)
        {
            try
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    BinaryFormatter Formatter = new BinaryFormatter();
                    Formatter.Serialize(Stream, Object);
                    Stream.Seek(0, 0);
                    Output = Stream.ToArray();
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Deserializes an object from binary
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="Binary">Binary representation of the object</param>
        /// <param name="Object">Object after it is deserialized</param>
        public static void BinaryToObject<T>(byte[] Binary, out T Object)
        {
            try
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    Stream.Write(Binary, 0, Binary.Length);
                    Stream.Seek(0, 0);
                    BinaryFormatter Formatter = new BinaryFormatter();
                    Object = (T)Formatter.Deserialize(Stream);
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Converts an object to XML
        /// </summary>
        /// <param name="Object">Object to convert</param>
        /// <param name="FileName">File to save the XML to</param>
        /// <returns>string representation of the object in XML format</returns>
        public static string ObjectToXML(object Object, string FileName)
        {
            try
            {
                string XML = ObjectToXML(Object);
                FileManager.SaveFile(XML, FileName);
                return XML;
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Converts an object to XML
        /// </summary>
        /// <param name="Object">Object to convert</param>
        /// <returns>string representation of the object in XML format</returns>
        public static string ObjectToXML(object Object)
        {
            if (Object == null)
            {
                throw new ArgumentException("Object can not be null");
            }
            try
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    XmlSerializer Serializer = new XmlSerializer(Object.GetType());
                    Serializer.Serialize(Stream, Object);
                    Stream.Flush();
                    return UTF8Encoding.UTF8.GetString(Stream.GetBuffer(), 0, (int)Stream.Position);
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Takes an XML file and exports the Object it holds
        /// </summary>
        /// <param name="FileName">File name to use</param>
        /// <param name="Object">Object to export to</param>
        public static void XMLToObject<T>(string FileName, out T Object)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                throw new ArgumentException("File name can not be null/empty");
            }
            if (!FileManager.FileExists(FileName))
            {
                throw new ArgumentException("File does not exist");
            }
            try
            {
                string FileContent = FileManager.GetFileContents(FileName);
                Object = XMLToObject<T>(FileContent);
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Converts an XML string to an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="XML">XML string</param>
        /// <returns>The object of the specified type</returns>
        public static T XMLToObject<T>(string XML)
        {
            if (string.IsNullOrEmpty(XML))
            {
                throw new ArgumentException("XML can not be null/empty");
            }
            try
            {
                using (MemoryStream Stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(XML)))
                {
                    XmlSerializer Serializer = new XmlSerializer(typeof(T));
                    return (T)Serializer.Deserialize(Stream);
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Converts a SOAP string to an object
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="SOAP">SOAP string</param>
        /// <returns>The object of the specified type</returns>
        public static T SOAPToObject<T>(string SOAP)
        {
            if (string.IsNullOrEmpty(SOAP))
            {
                throw new ArgumentException("SOAP can not be null/empty");
            }
            try
            {
                using (MemoryStream Stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(SOAP)))
                {
                    SoapFormatter Formatter = new SoapFormatter();
                    return (T)Formatter.Deserialize(Stream);
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }

        /// <summary>
        /// Converts an object to a SOAP string
        /// </summary>
        /// <param name="Object">Object to serialize</param>
        /// <returns>The serialized string</returns>
        public static string ObjectToSOAP(object Object)
        {
            if (Object == null)
            {
                throw new ArgumentException("Object can not be null");
            }
            try
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    SoapFormatter Serializer = new SoapFormatter();
                    Serializer.Serialize(Stream, Object);
                    Stream.Flush();
                    return UTF8Encoding.UTF8.GetString(Stream.GetBuffer(), 0, (int)Stream.Position);
                }
            }
            catch (Exception a)
            {
                throw a;
            }
        }
        #endregion
    }
}
