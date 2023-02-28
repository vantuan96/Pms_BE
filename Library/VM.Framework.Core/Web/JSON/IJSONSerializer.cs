using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.JSON
{
    public interface IJSONSerializer
    {
        string Serialize(object value);
        object Deserialize(string jsonString, Type type);
        JsonDateEncodingModes DateSerializationMode { get; set; }
        bool FormatJsonOutput { get; set; }
    }
}
