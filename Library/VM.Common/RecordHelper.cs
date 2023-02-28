using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VM.Common
{
    public class RecordHelper
    {
        /// <summary>
        /// Gets a boolean value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A boolean value</returns>
        public static bool GetBoolean(IDataReader rdr, string columnName, bool defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToBoolean(rdr[index]);
            }
            catch { }

            return defaultValue;
        }

        public static bool GetBoolean(IDataReader rdr, string columnName, bool defaultValue, string strCompare)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToBoolean(rdr[index].ToString() == strCompare);
            }
            catch { }

            return defaultValue;
        }


        public static bool GetBoolean(DataRow rdr, string columnName, bool defaultValue)
        {
            try
            {
                return rdr.Field<bool>(columnName);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Gets a byte array of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A byte array</returns>
        public static byte[] GetBytes(IDataReader rdr, string columnName, byte[] defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return (byte[])rdr[index];
            }
            catch { }

            return defaultValue;
        }


        public static byte[] GetBytes(DataRow rdr, string columnName, byte[] defaultValue)
        {
            try
            {
                return rdr.Field<byte[]>(columnName);
            }
            catch
            {

                return defaultValue;
            }

        }


        /// <summary>
        /// Gets a datetime value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A date time</returns>
        public static DateTime GetDateTime(IDataReader rdr, string columnName, DateTime defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToDateTime(rdr[index]);
            }
            catch { }

            return defaultValue;
        }


        public static Nullable<DateTime> GetDateTime(IDataReader rdr, string columnName, Nullable<DateTime> defaultValue)
        {
            try
            {
                Nullable<DateTime> result = null;

                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }

                result = Convert.ToDateTime(rdr[index]);

                return result;
            }
            catch { }

            return defaultValue;
        }


        public static DateTime GetDateTime(DataRow rdr, string columnName, DateTime defaultValue)
        {
            try
            {
                return rdr.Field<DateTime>(columnName);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Gets a decimal value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A decimal value</returns>
        public static decimal GetDecimal(IDataReader rdr, string columnName, decimal defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToDecimal(rdr[index]);
            }
            catch { }

            return defaultValue;
        }


        public static decimal GetDecimal(DataRow rdr, string columnName, decimal defaultValue)
        {
            try
            {
                return rdr.Field<decimal>(columnName);
            }
            catch
            {
                return defaultValue;

            }
        }


        /// <summary>
        /// Gets a double value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A double value</returns>
        public static Nullable<double> GetDouble(IDataReader rdr, string columnName, Nullable<double> defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToDouble(rdr[index]);
            }
            catch { }

            return defaultValue;
        }



        public static double GetDouble(DataRow rdr, string columnName, double defaultValue)
        {
            try
            {
                return rdr.Field<double>(columnName);
            }
            catch
            {
                return defaultValue;
            }
        }


        /// <summary>
        /// Gets a GUID value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>A GUID value</returns>
        public static Guid GetGuid(IDataReader rdr, string columnName, Guid defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }

                //byte[] buffer = (byte[])rdr[index];
                //Guid gUserKey = new Guid(buffer);
                //return gUserKey;

                byte[] buffer = new byte[0x10];
                rdr.GetBytes(index, 0L, buffer, 0, 0x10);
                string sBuffer = BitConverter.ToString(buffer).Replace("-", "");
                Guid providerUserKey = new Guid(sBuffer);
                return providerUserKey;

                //return rdr.GetGuid(index);
            }
            catch { }
            return defaultValue;
        }
        public static Guid GetGuidVSSQL(IDataReader rdr, string columnName, Guid defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                Guid myGuid = (Guid)rdr[index];
                return myGuid;
            }
            catch { }
            return defaultValue;
        }

        public static Nullable<Guid> GetGuid(IDataReader rdr, string columnName, Nullable<Guid> defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }

                byte[] buffer = new byte[0x10];
                rdr.GetBytes(index, 0L, buffer, 0, 0x10);
                Guid providerUserKey = new Guid(buffer);
                return (Nullable<Guid>)providerUserKey;

                //return rdr.GetGuid(index);
            }
            catch { }
            return defaultValue;
        }

        public static Guid GetGuid(DataRow rdr, string columnName, Guid defaultValue)
        {
            try
            {
                return rdr.Field<Guid>(columnName);
            }
            catch
            {

                return defaultValue;
            }
        }

        /// <summary>
        /// Gets an integer value of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <returns>An integer value</returns>
        public static int GetInt(IDataReader rdr, string columnName, int defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToInt32(rdr[index]);
            }
            catch { }

            return defaultValue;
        }

        public static Nullable<int> GetInt(IDataReader rdr, string columnName, Nullable<int> defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return (Nullable<int>)Convert.ToInt32(rdr[index]);
            }
            catch { }

            return defaultValue;
        }

        public static int GetInt(DataRow rdr, string columnName, int defaultValue)
        {
            try
            {
                return rdr.Field<int>(columnName);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a long value of a data reader by a column name
        /// </summary>
        /// <param name="rdr"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static long GetLong(IDataReader rdr, string columnName, long defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToInt64(rdr[index]);
            }
            catch { }

            return defaultValue;
        }


        public static long GetLong(DataRow rdr, string columnName, long defaultValue)
        {
            try
            {
                return rdr.Field<long>(columnName);
            }
            catch
            {

                return defaultValue;

            }
        }


        /// <summary>
        /// Gets a string of a data reader by a column name
        /// </summary>
        /// <param name="rdr">Data reader</param>
        /// <param name="columnName">Column name</param>
        /// <returns>A string value</returns>
        public static string GetString(IDataReader rdr, string columnName, string defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return Convert.ToString(rdr[index]);
            }
            catch { }
            return defaultValue;
        }



        public static string GetString(DataRow rdr, string columnName, string defaultValue)
        {
            try
            {
                return rdr.Field<string>(columnName);
            }
            catch
            {
                return defaultValue;
            }
        }
        public static List<int> GetListzInt(IDataReader rdr, string columnName, List<int> defaultValue, char cSeparater)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                string[] sArr = Convert.ToString(rdr[index]).Split(cSeparater);
                List<int> iList = new List<int>();
                foreach (string sValue in sArr)
                {
                    int iValue = 0;
                    if (int.TryParse(sValue, out iValue))
                        iList.Add(iValue);
                }
                return iList;
            }
            catch { }
            return defaultValue;
        }
        public static XmlDocument GetXmlDocument(IDataReader rdr, string columnName, XmlDocument defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(rdr[index].ToString());
                return xd;
                //return new XmlDocument(rdr[index].ToString());
            }
            catch { }
            return defaultValue;
        }
        public static XElement GetXElement(IDataReader rdr, string columnName, XElement defaultValue)
        {
            try
            {
                int index = rdr.GetOrdinal(columnName);
                if (rdr.IsDBNull(index))
                {
                    return defaultValue;
                }
                return XElement.Parse(rdr[index].ToString());
            }
            catch { }
            return defaultValue;
        }
    }
}
