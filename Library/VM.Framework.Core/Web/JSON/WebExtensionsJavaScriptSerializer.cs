﻿#if (true) // WEBEXTENSIONS_REFERENCE

using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Data.SqlClient;

namespace GAPIT.MKT.Framework.Core.JSON
{
    /// <summary>
    /// More text is a basic JSON serializer and deserializer that 
    /// deals with standard .NET types. Unlike the MS Ajax JSONSerializer
    /// parser this parser support serialization and deserialization without 
    /// explicit type markup in the JSON resulting in a simpler two-way model.
    /// 
    /// The inbound model for complex types is based on Reflection parsing
    /// of properties.
    /// </summary>
    internal class WebExtensionsJavaScriptSerializer : JSONSerializerBase, IJSONSerializer
    {
        public WebExtensionsJavaScriptSerializer(JSONSerializer serializer)
            : base(serializer)
        {
 
        }

        public string Serialize(object value)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();

            List<JavaScriptConverter> converters = new List<JavaScriptConverter>();

            if (value != null)
            {
                Type type = value.GetType();
                if (type == typeof(DataTable) || type == typeof(DataRow) || type == typeof(DataSet))
                {
                    converters.Add(new WebExtensionsDataRowConverter());
                    converters.Add(new WebExtensionsDataTableConverter());
                    converters.Add(new WebExtensionsDataSetConverter());
                }
                if (value is IDataReader)
                {
                    converters.Add(new DataReaderConverter());
                }

                if (converters.Count > 0)
                    ser.RegisterConverters(converters);
            }

            return ser.Serialize(value);
        }


        public object Deserialize(string jsonText, Type valueType)
        {
            // Have to use Reflection with a 'dynamic' non constant type instance
            JavaScriptSerializer ser = new JavaScriptSerializer();


            object result = ser.GetType()
                               .GetMethod("Deserialize")
                               .MakeGenericMethod(valueType)
                              .Invoke(ser, new object[1] { jsonText });
            return result;
        }
    }


    internal class WebExtensionsDataTableConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(DataTable) }; }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                                           JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }


        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            DataTable table = obj as DataTable;

            // result 'object'
            Dictionary<string, object> result = new Dictionary<string, object>();

            if (table != null)
            {
                // We'll represent rows as an array/listType
                List<object> rows = new List<object>();

                foreach (DataRow row in table.Rows)
                {
                    rows.Add(row);  // Rely on DataRowConverter to handle
                }

                result["Rows"] = rows;

                return result;
            }

            return new Dictionary<string, object>();
        }
    }




    internal class WebExtensionsDataRowConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(DataRow) }; }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                                           JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            DataRow dataRow = obj as DataRow;
            Dictionary<string, object> propValues = new Dictionary<string, object>();

            if (dataRow != null)
            {
                foreach (DataColumn dc in dataRow.Table.Columns)
                {
                    propValues.Add(dc.ColumnName, dataRow[dc]);
                }
            }

            return propValues;
        }
    }



    internal class WebExtensionsDataSetConverter : JavaScriptConverter
    {
        public override IEnumerable<Type> SupportedTypes
        {
            get { return new Type[] { typeof(DataSet) }; }
        }

        public override object Deserialize(IDictionary<string, object> dictionary, Type type,
                                           JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            DataSet dataSet = obj as DataSet;
            Dictionary<string, object> tables = new Dictionary<string, object>();

            if (dataSet != null)
            {
                foreach (DataTable dt in dataSet.Tables)
                {
                    tables.Add(dt.TableName, dt);
                }
            }

            return tables;
        }
    }



    public class DataReaderConverter : JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {

            IDataReader reader = obj as IDataReader;
            if (reader == null)
                return null;

            Dictionary<string, object> wrapper = new Dictionary<string, object>();

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                Dictionary<string, object> row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount - 1; i++)
                {
                    row.Add(reader.GetName(i), reader[i]);
                }
                rows.Add(row);
            }

            wrapper.Add("Rows", rows);
            return wrapper;
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new Type[3] { typeof(SqlDataReader), 
                                       typeof(System.Data.OleDb.OleDbDataReader), 
                                       typeof(IDataReader) };
            }
        }
    }
}


#endif