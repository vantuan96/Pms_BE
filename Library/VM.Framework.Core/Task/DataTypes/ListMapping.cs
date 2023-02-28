using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core.Task
{
    /// <summary>
    /// Maps a key to a list of data
    /// </summary>
    /// <typeparam name="T1">Key value</typeparam>
    /// <typeparam name="T2">Type that the list should contain</typeparam>
    public class ListMapping<T1, T2>
    {
        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public ListMapping()
        {
        }

        #endregion

        #region Private Variables

        protected Dictionary<T1, List<T2>> Items = new Dictionary<T1, List<T2>>();

        #endregion

        #region Public Functions

        /// <summary>
        /// Adds an item to the mapping
        /// </summary>
        /// <param name="Key">Key value</param>
        /// <param name="Value">The value to add</param>
        public virtual void Add(T1 Key, T2 Value)
        {
            try
            {
                if (Items.ContainsKey(Key))
                {
                    Items[Key].Add(Value);
                }
                else
                {
                    Items.Add(Key, new List<T2>());
                    Items[Key].Add(Value);
                }
            }
            catch { throw; }
        }

        /// <summary>
        /// Determines if a key exists
        /// </summary>
        /// <param name="key">Key to check on</param>
        /// <returns>True if it exists, false otherwise</returns>
        public virtual bool ContainsKey(T1 key)
        {
            try
            {
                return Items.ContainsKey(key);
            }
            catch { throw; }
        }

        /// <summary>
        /// The list of keys within the mapping
        /// </summary>
        public virtual ICollection<T1> Keys
        {
            get { try { return Items.Keys; } catch { throw; } }
        }

        /// <summary>
        /// Remove a list of items associated with a key
        /// </summary>
        /// <param name="key">Key to use</param>
        /// <returns>True if the key is found, false otherwise</returns>
        public virtual bool Remove(T1 key)
        {
            try
            {
                return Items.Remove(key);
            }
            catch { throw; }
        }

        /// <summary>
        /// Gets a list of values associated with a key
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>The list of values</returns>
        public virtual List<T2> this[T1 key]
        {
            get
            {
                try
                {
                    return Items[key];
                }
                catch { throw; }
            }
            set
            {
                try
                {
                    Items[key] = value;
                }
                catch { throw; }
            }
        }

        /// <summary>
        /// Clears all items from the listing
        /// </summary>
        public virtual void Clear()
        {
            try
            {
                Items.Clear();
            }
            catch { throw; }
        }

        /// <summary>
        /// The number of items in the listing
        /// </summary>
        public virtual int Count
        {
            get { try { return Items.Count; } catch { throw; } }
        }


        #endregion
    }
}
