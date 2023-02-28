using System;
using System.Collections;

namespace VM.Data.Queue
{
    /// <summary>
    ///	 <para>
    ///	   A collection that stores <see cref='User'/> objects.
    ///	</para>
    /// </summary>
    /// <seealso cref='User'/>
    [Serializable()]
    public class Users : CollectionBase
    {
        /// <summary>Notifies when the collection has been modified.</summary>
        public event EventHandler OnItemsChanged;

        /// <summary>Notifies that an item has been added.</summary>
        public event UserHandler OnItemAdd;

        /// <summary>Notifies that items have been added.</summary>
        public event UserHandler OnItemsAdd;

        /// <summary>Notifies that an item has been removed.</summary>
        public event UserHandler OnItemRemove;

        /// <summary>
        ///	 <para>
        ///	   Initializes a new instance of <see cref='User'/>.
        ///	</para>
        /// </summary>
        public Users()
        {
        }

        /// <summary>
        ///	 <para>
        ///	   Initializes a new instance of <see cref='User'/> based on another <see cref='Users'/>.
        ///	</para>
        /// </summary>
        /// <param name='value'>
        ///	   A <see cref='Users'/> from which the contents are copied
        /// </param>
        public Users(Users value)
        {
            this.AddRange(value);
        }

        /// <summary>
        ///	 <para>
        ///	   Initializes a new instance of <see cref='Users'/> containing any array of <see cref='User'/> objects.
        ///	</para>
        /// </summary>
        /// <param name='value'>
        ///	   A array of <see cref='User'/> objects with which to intialize the collection
        /// </param>
        public Users(User[] value)
        {
            this.AddRange(value);
        }

        /// <summary>
        /// <para>Represents the entry at the specified index of the <see cref='User'/>.</para>
        /// </summary>
        /// <param name='index'><para>The zero-based index of the entry to locate in the collection.</para></param>
        /// <value>
        ///	<para> The entry at the specified index of the collection.</para>
        /// </value>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='index'/> is outside the valid range of indexes for the collection.</exception>
        public User this[int index]
        {
            get { return ((User)(List[index])); }
            set { List[index] = value; }
        }

        /// <summary>
        ///	<para>Adds a <see cref='User'/> with the specified value to the 
        ///	<see cref='User'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='User'/> to add.</param>
        /// <returns>
        ///	<para>The index at which the new element was inserted.</para>
        /// </returns>
        /// <seealso cref='Users.AddRange'/>
        public int Add(User value)
        {
            int ndx = List.Add(value);
            if (OnItemAdd != null) { OnItemAdd(this, new UserArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
            return ndx;
        }

        /// <summary>
        /// <para>Copies the elements of an array to the end of the <see cref='Users'/>.</para>
        /// </summary>
        /// <param name='value'>
        ///	An array of type <see cref='User'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Users.Add'/>
        public void AddRange(User[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new UserArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        /// <summary>
        ///	 <para>
        ///	   Adds the contents of another <see cref='User'/> to the end of the collection.
        ///	</para>
        /// </summary>
        /// <param name='value'>
        ///	A <see cref='Users'/> containing the objects to add to the collection.
        /// </param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <seealso cref='Users.Add'/>
        public void AddRange(Users value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new UserArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        /// <summary>
        /// <para>Gets a value indicating whether the 
        ///	<see cref='Users'/> contains the specified <see cref='User'/>.</para>
        /// </summary>
        /// <param name='value'>The <see cref='User'/> to locate.</param>
        /// <returns>
        /// <para><see langword='true'/> if the <see cref='User'/> is contained in the collection; 
        ///   otherwise, <see langword='false'/>.</para>
        /// </returns>
        /// <seealso cref='Users.IndexOf'/>
        public bool Contains(User value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// <para>Copies the <see cref='User'/> values to a one-dimensional <see cref='System.Array'/> instance at the 
        ///	specified index.</para>
        /// </summary>
        /// <param name='array'><para>The one-dimensional <see cref='System.Array'/> that is the destination of the values copied from <see cref='User'/> .</para></param>
        /// <param name='index'>The index in <paramref name='array'/> where copying begins.</param>
        /// <returns>
        ///   <para>None.</para>
        /// </returns>
        /// <exception cref='System.ArgumentException'><para><paramref name='array'/> is multidimensional.</para> <para>-or-</para> <para>The number of elements in the <see cref='User'/> is greater than the available space between <paramref name='arrayIndex'/> and the end of <paramref name='array'/>.</para></exception>
        /// <exception cref='System.ArgumentNullException'><paramref name='array'/> is <see langword='null'/>. </exception>
        /// <exception cref='System.ArgumentOutOfRangeException'><paramref name='arrayIndex'/> is less than <paramref name='array'/>'s lowbound. </exception>
        /// <seealso cref='System.Array'/>
        public void CopyTo(User[] array, int index)
        {
            List.CopyTo(array, index);
        }

        /// <summary>
        ///	<para>Returns the index of a <see cref='User'/> in 
        ///	   the <see cref='Users'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='User'/> to locate.</param>
        /// <returns>
        /// <para>The index of the <see cref='User'/> of <paramref name='value'/> in the 
        /// <see cref='User'/>, if found; otherwise, -1.</para>
        /// </returns>
        /// <seealso cref='Users.Contains'/>
        public int IndexOf(User value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// <para>Inserts a <see cref='User'/> into the <see cref='Users'/> at the specified index.</para>
        /// </summary>
        /// <param name='index'>The zero-based index where <paramref name='value'/> should be inserted.</param>
        /// <param name=' value'>The <see cref='User'/> to insert.</param>
        /// <returns><para>None.</para></returns>
        /// <seealso cref='Users.Add'/>
        public void Insert(int index, User value)
        {
            List.Insert(index, value);
            if (OnItemAdd != null) { OnItemAdd(this, new UserArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        /// <summary>
        ///	<para> Removes a specific <see cref='User'/> from the 
        ///	<see cref='Users'/> .</para>
        /// </summary>
        /// <param name='value'>The <see cref='User'/> to remove from the <see cref='Users'/> .</param>
        /// <returns><para>None.</para></returns>
        /// <exception cref='System.ArgumentException'><paramref name='value'/> is not found in the Collection. </exception>
        public void Remove(User value)
        {
            List.Remove(value);
            if (OnItemRemove != null) { OnItemRemove(this, new UserArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        /// Event arguments for the Users collection class.
        public class UserArgs : EventArgs
        {
            private Users t;

            /// Default constructor.
            public UserArgs()
            {
                t = new Users();
            }

            /// Initializes with a User.
            /// Data object.
            public UserArgs(User t)
                : this()
            {
                this.t.Add(t);
            }

            /// Initializes with a collection of User objects.
            /// Collection of data.
            public UserArgs(Users ts)
                : this()
            {
                this.t.AddRange(ts);
            }

            /// Initializes with an array of User objects.
            /// Array of data.
            public UserArgs(User[] ts)
                : this()
            {
                this.t.AddRange(ts);
            }

            /// Gets or sets the data of this argument.
            public Users Users
            {
                get { return t; }
                set { t = value; }
            }
        }

        /// Users event handler.
        public delegate void UserHandler(object sender, UserArgs e);
    }
}
