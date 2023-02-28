using System;
using System.Data;

using System.Text;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Collections.Specialized;

using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

using GAPIT.MKT.Helpers;

namespace GAPIT.MKT.Framework.Core.JSON
{
    /// <summary>
    /// The high level JSON Serializer instance that provides 
    /// serialization and deserialization services to the application. This is 
    /// the core serializer that the application or components interact with.
    /// 
    /// This serializer defers operation to the specified JSON parsing implementation.
    /// Supported parsers include:
    /// 
    /// * WestWind Native that's built-in (no dependencies)   (This is the default)
    /// * JSON.NET   (requires JSON.NET assembly to be included and JSONNET_REFERENCE global Define
    /// * JavaScriptSerializer (ASP.NET JavaScript Serializer - requires System.Web.Extension + WEBEXTENSIONS_REFERENCE global Define)
    /// </summary>
    public class JSONSerializer
    {
        /// <summary>
        /// This property determines the default parser that is created when
        /// using the default constructor. This is also the default serializer
        /// used when using the AjaxMethodCallback control.
        /// 
        /// This property should be set only once at application startup typically
        /// </summary>
        public static SupportedJsonParserTypes DefaultJsonParserType = SupportedJsonParserTypes.WestWindJsonSerializer;


        private IJSONSerializer _serializer = null;


        /// <summary>
        /// Encodes Dates as a JSON string value that is compatible
        /// with MS AJAX and is safe for JSON validators. If false
        /// serializes dates as new Date() expression instead.
        /// 
        /// The default is true.
        /// </summary>
        public JsonDateEncodingModes DateSerializationMode
        {
            get { return _SerializeDateAsFormatString; }
            set { _SerializeDateAsFormatString = value; }
        }
        private JsonDateEncodingModes _SerializeDateAsFormatString = JsonDateEncodingModes.ISO;



        /// <summary>
        /// Determines if there are line breaks inserted into the 
        /// JSON to make it more easily human readable.
        /// </summary>
        public bool FormatJsonOutput
        {
            get { return _FormatJsonOutput; }
            set { _FormatJsonOutput = value; }
        }
        private bool _FormatJsonOutput = false;



        /// <summary>
        /// Default Constructor - assigns default 
        /// </summary>
        public JSONSerializer()
            : this(DefaultJsonParserType)
        { }

        public JSONSerializer(IJSONSerializer serializer)
        {
            _serializer = serializer;
        }


        public JSONSerializer(SupportedJsonParserTypes parserType)
        {
            // The Custom Parser is native
            if (parserType == SupportedJsonParserTypes.WestWindJsonSerializer)
                _serializer = new WestwindJsonSerializer(this);

#if (JSONNET_REFERENCE)
            else if (parserType == SupportedJsonParserTypes.JsonNet)
                _serializer = new JsonNetJsonSerializer(this);
#endif
            else if (parserType == SupportedJsonParserTypes.JavaScriptSerializer)
                _serializer = new WebExtensionsJavaScriptSerializer(this);
            else
                throw new InvalidOperationException("Unsupported JSON Serializer specified. JsonNet and System.Web.Extensions must be explicitly compiled in.");
        }


        public string Serialize(object value)
        {
            return _serializer.Serialize(value);
        }

        public object Deserialize(string jsonString, Type type)
        {
            return _serializer.Deserialize(jsonString, type);
        }

        public TType Deserialize<TType>(string jsonString)
        {
            return (TType)Deserialize(jsonString, typeof(TType));
        }

    }
}
