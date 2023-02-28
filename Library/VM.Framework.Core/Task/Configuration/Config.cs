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
    /// Config object
    /// </summary>
    [Serializable()]
    public class Config<T> : IConfig
    {
        #region Constructor

        public Config()
        {            
        }

        #endregion

        #region Protected Virtual Properties

        /// <summary>
        /// Location to save/load the config file from.
        /// If blank, it does not save/load but uses any defaults specified.
        /// </summary>
        protected virtual string ConfigFileLocation { get { return ""; } }

        /// <summary>
        /// Encryption password for fields. Used only if set.
        /// </summary>
        protected virtual string EncryptionPassword { get { return ""; } }

        #endregion

        #region IConfig Members

        public void Load()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigFileLocation))
                    return;
                T Temp = default(T);
                try
                {
                    Serialization.XMLToObject<T>(ConfigFileLocation, out Temp);
                }
                catch { }
                LoadProperties(Temp);
                Decrypt();
            }
            catch { throw; }
        }

        public void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigFileLocation))
                    return;
                Encrypt();
                Serialization.ObjectToXML(this, ConfigFileLocation);
                Decrypt();
            }
            catch { throw; }
        }

        #endregion

        #region Private Functions

        private void LoadProperties(T Temp)
        {
            try
            {
                if (Temp == null)
                    return;
                Type ObjectType = Temp.GetType();
                PropertyInfo[] Properties = ObjectType.GetProperties();
                foreach (PropertyInfo Property in Properties)
                {
                    if (Property.CanWrite && Property.CanRead)
                    {
                        Property.SetValue(this, Property.GetValue(Temp, null), null);
                    }
                }
            }
            catch { throw; }
        }

        private void Encrypt()
        {
            try
            {
                if (string.IsNullOrEmpty(EncryptionPassword))
                    return;
                Type ObjectType = this.GetType();
                PropertyInfo[] Properties = ObjectType.GetProperties();
                foreach (PropertyInfo Property in Properties)
                {
                    if (Property.CanWrite && Property.CanRead && Property.PropertyType == typeof(string))
                    {
                        Property.SetValue(this,
                            AESEncryption.Encrypt((string)Property.GetValue(this, null),
                                EncryptionPassword),
                            null);
                    }
                }
            }
            catch { throw; }
        }

        private void Decrypt()
        {
            try
            {
                if (string.IsNullOrEmpty(EncryptionPassword))
                    return;
                Type ObjectType = this.GetType();
                PropertyInfo[] Properties = ObjectType.GetProperties();
                foreach (PropertyInfo Property in Properties)
                {
                    if (Property.CanWrite && Property.CanRead && Property.PropertyType == typeof(string))
                    {
                        string Value = (string)Property.GetValue(this, null);
                        if (!string.IsNullOrEmpty(Value))
                        {
                            Property.SetValue(this,
                                AESEncryption.Decrypt(Value,
                                    EncryptionPassword),
                                null);
                        }
                    }
                }
            }
            catch { throw; }
        }

        #endregion
    }
}
