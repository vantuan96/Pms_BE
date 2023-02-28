using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using System.Web;

namespace VM.Framework.Core
{
    /// <summary>
    /// Serves as a base class to derive. 
    /// The base repository to manage data.
    /// </summary>
    /// <remarks></remarks>
    public abstract class BaseRepository : IDisposable
    {

        #region Declare Params
        
        private int _cacheDuration = 0;
        private string _cacheKey = "CacheKey";
        private string _connectionString = "Set the ConnectionString";
        private bool _enableCaching = true;

        #endregion

        #region Cache Feature

        protected static void CacheData(string key, object data)
        {
            CacheData(key, data, 120);
        }

        protected static void CacheData(string key, object data, int vCacheDuration)
        {
            if (null != data)
            {
                Cache.Insert(key, data, null, DateTime.Now.AddSeconds(vCacheDuration), TimeSpan.Zero);
            }
        }

        public void PurgeCacheItems(string prefix)
        {
            prefix = prefix.ToLower();
            List<string> itemsToRemove = new List<string>();

            IDictionaryEnumerator enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Key.ToString().ToLower().StartsWith(prefix))
                {
                    itemsToRemove.Add(enumerator.Key.ToString());
                }
            }

            foreach (string itemToRemove in itemsToRemove)
            {
                Cache.Remove(itemToRemove);
            }
        }
        public void RemoveCacheItems(string sKey)
        {
            sKey = sKey.ToLower();
            List<string> itemsToRemove = new List<string>();

            IDictionaryEnumerator enumerator = Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Key.ToString().ToLower().EndsWith(sKey))
                {
                    itemsToRemove.Add(enumerator.Key.ToString());
                }
            }

            foreach (string itemToRemove in itemsToRemove)
            {
                Cache.Remove(itemToRemove);
            }
        }
        #endregion

        #region Object Destroying

        public abstract void Dispose();

        protected abstract void Dispose(bool disposing);


        #endregion

        #region Public Properties

        private Dictionary<string, Exception> _activeExceptions;
        /// <summary>
        /// Contains multi exceptions when operating with the bussines
        /// </summary>
        public Dictionary<string, Exception> ActiveExceptions
        {
            get
            {
                if ((_activeExceptions == null))
                {
                    _activeExceptions = new Dictionary<string, Exception>();
                }
                return _activeExceptions;
            }
            set { _activeExceptions = value; }
        }

        public static System.Web.Caching.Cache Cache
        {
            get
            {
                return HttpContext.Current.Cache;
            }
        }

        public int CacheDuration
        {
            get
            {
                return this._cacheDuration;
            }
            set
            {
                this._cacheDuration = value;
            }
        }

        public string CacheKey
        {
            get
            {
                return this._cacheKey;
            }
            set
            {
                this._cacheKey = value;
            }
        }

        public string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
            set
            {
                this._connectionString = value;
            }
        }


        //public static IPrincipal CurrentUser
        //{
        //    get
        //    {
        //        return CommonHelpers.CurrentUser;
        //    }
        //}


        //public static string CurrentUserIP
        //{
        //    get
        //    {
        //        return CommonHelpers.CurrentUserIP;
        //    }
        //}


        //public static string CurrentUserName
        //{
        //    get
        //    {
        //        return CommonHelpers.CurrentUserName;
        //    }
        //}

        public bool EnableCaching
        {
            get
            {
                return this._enableCaching;
            }
            set
            {
                this._enableCaching = value;
            }
        }


        #endregion

    }
}
