#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// IConfig interface
    /// </summary>
    public interface IConfig
    {
        #region Functions

        /// <summary>
        /// Loads the config file
        /// </summary>
        void Load();

        /// <summary>
        /// Saves the config file
        /// </summary>
        void Save();

        #endregion
    }
}
