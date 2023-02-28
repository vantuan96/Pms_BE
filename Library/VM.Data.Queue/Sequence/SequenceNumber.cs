using System;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    [XmlRoot("SequenceNumber")]
    public class SequenceNumber
    {
        /// <summary>
        /// May be MO or MT or Bad...
        /// </summary>
        private string _type = "MO";
        [XmlAttribute("SNType")]
        public string SNType
        {
            get { return _type; }
            set { _type = value; }
        }

        private int _number = 0;
        [XmlElement("Value")]
        public int Value
        {
            get { return _number; }
            set { _number = value; }
        }

        public static int GetNextSequenceNumber(string sType, string sFileName)
        {
            SequenceNumber sn = null;
            try
            {
                sn = SequenceNumberSerializer.ReadFile(sFileName);
                sn.SNType = sType;
                sn.Value += 1;
            }
            catch (Exception ex)
            {
                throw  ex;
            }
            try
            {
                SequenceNumberSerializer.WriteFile(sFileName, sn);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (sn != null)
                return sn.Value;
            else
                return 0;
        }

        public static void ResetSequenceNumber(string sType, string sFileName)
        {
            SequenceNumber sn = new SequenceNumber();
            sn.SNType = sType;
            sn.Value = 0;

            try
            {
                SequenceNumberSerializer.WriteFile(sFileName, sn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
