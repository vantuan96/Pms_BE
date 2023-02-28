#region Usings
using System;
using System.Reflection;
#endregion

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Base class used for singletons
    /// </summary>
    /// <typeparam name="T">The class type</typeparam>
    public class Singleton<T> where T : class
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected Singleton() { }

        #endregion

        #region Private Variables

        private static T _Instance = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance of the singleton
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (typeof(T))
                    {
                        try
                        {
                            ConstructorInfo Constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                null, new Type[0], null);
                            if (Constructor == null || Constructor.IsAssembly)
                            {
                                throw new Exception("Constructor is not private or protected for type " + typeof(T).Name);
                            }
                            _Instance = (T)Constructor.Invoke(null);
                        }
                        catch { throw; }
                    }
                }
                return _Instance;
            }
        }

        #endregion
    }
}
