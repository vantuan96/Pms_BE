#region Usings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Config manager
    /// </summary>
    public class ConfigManager : Singleton<ConfigManager>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected ConfigManager()
            : base()
        {
            ConfigFiles = new Dictionary<string, IConfig>();
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Registers a config file
        /// </summary>
        /// <param name="Name">Name to reference by</param>
        /// <param name="ConfigObject">Config object to register</param>
        public void RegisterConfigFile(string Name, IConfig ConfigObject)
        {
            try
            {
                ConfigObject.Load();
                ConfigFiles.Add(Name, ConfigObject);
            }
            catch { throw; }
        }

        /// <summary>
        /// Registers all config files in an assembly
        /// </summary>
        /// <param name="AssemblyContainingConfig">Assembly to search</param>
        public void RegisterConfigFile(Assembly AssemblyContainingConfig)
        {
            try
            {
                List<Type> Types = Reflection.GetTypes(AssemblyContainingConfig, "IConfig");
                foreach (Type Temp in Types)
                {
                    if (!Temp.ContainsGenericParameters)
                    {
                        string Name = "";
                        object[] Attributes = Temp.GetCustomAttributes(typeof(ConfigAttribute), true);
                        if (Attributes.Length > 0)
                        {
                            Name = ((ConfigAttribute)Attributes[0]).Name;
                        }
                        IConfig TempConfig = (IConfig)Temp.Assembly.CreateInstance(Temp.FullName);
                        RegisterConfigFile(Name, TempConfig);
                    }
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Registers all config files in an assembly that is not currently loaded
        /// </summary>
        /// <param name="AssemblyLocation">Location of the assembly</param>
        public void RegisterConfigFile(string AssemblyLocation)
        {
            try
            {
                RegisterConfigFile(Assembly.LoadFile(AssemblyLocation));
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a specified config file
        /// </summary>
        /// <typeparam name="T">Type of the config object</typeparam>
        /// <param name="Name">Name of the config object</param>
        /// <returns>The config object specified</returns>
        public T GetConfigFile<T>(string Name)
        {
            try
            {
                if (!ConfigFiles.ContainsKey(Name))
                    throw new Exception("The config object " + Name + " was not found.");
                if (!(ConfigFiles[Name] is T))
                    throw new Exception("The config object " + Name + " is not the specified type.");
                return (T)ConfigFiles[Name];
            }
            catch { throw; }
        }

        #endregion

        #region Private properties

        private static Dictionary<string, IConfig> ConfigFiles { get; set; }

        #endregion
    }
}
