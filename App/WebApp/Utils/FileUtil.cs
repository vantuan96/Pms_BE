using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace DrFee.Utils
{
    public static class FileUtil
    {
        /// <summary>
        /// Create log directory
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists((path)))
            {
                Directory.CreateDirectory((path));
            }
        }

        /// <summary>
        /// Create log file
        /// </summary>
        /// <param name="fileLogPath">file full path</param>
        public static void CreateXmlFile(string fileLogPath, string localName)
        {
            // create only if not extists
            if (!File.Exists(fileLogPath))
            {
                using (XmlWriter xmlWriter = new XmlTextWriter(fileLogPath, Encoding.UTF8))
                {
                    xmlWriter.WriteStartDocument(true);
                    xmlWriter.WriteElementString(localName, "");
                }
            }
        }

        /// <summary>
        /// Remove log file
        /// </summary>
        /// <param name="filePath">Full file path</param>
        /// <returns></returns>
        public static bool RemoveLogFile(string filePath)
        {
            filePath = filePath + ".log";
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool ValidateImageFileExt(string ext)
        {
            string validExts = ",png,jpg,jpeg,rar,";
            if (validExts.IndexOf("," + ext.ToLower() + ",") >= 0)
            {
                return true;
            }
            return false;
        }
        public static byte[] ReadToEnd(System.IO.Stream stream)
        {
            if (stream == null)
                return null;
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[stream.Length];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }
    }
}