using System;

namespace GAPIT.MKT.Framework.Core.JSON
{
    /// <summary>
    /// Enumeration that determines how JavaScript dates are
    /// generated in JSON output
    /// </summary>
    public enum JsonDateEncodingModes
    {
        NewDateExpression,
        MsAjax,
        ISO
    }
}
