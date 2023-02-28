using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Attribute for naming a config file
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConfigAttribute : Attribute
    {
        #region Properties

        /// <summary>
        /// Config's name
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}
