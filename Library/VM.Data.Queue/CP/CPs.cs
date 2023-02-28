using System;
using System.Collections;


namespace VM.Data.Queue
{
  
    [Serializable()]
    public class CPs : CollectionBase
    {
        /// <summary>Notifies when the collection has been modified.</summary>
        public event EventHandler OnItemsChanged;

        /// <summary>Notifies that an item has been added.</summary>
        public event CPHandler OnItemAdd;

        /// <summary>Notifies that items have been added.</summary>
        public event CPHandler OnItemsAdd;

        /// <summary>Notifies that an item has been removed.</summary>
        public event CPHandler OnItemRemove;

        
        public CPs()
        {
        }

       
        public CPs(CPs value)
        {
            this.AddRange(value);
        }

        public CPs(CP[] value)
        {
            this.AddRange(value);
        }

        public CP this[int index]
        {
            get { return ((CP)(List[index])); }
            set { List[index] = value; }
        }

        public int Add(CP value)
        {
            int ndx = List.Add(value);
            if (OnItemAdd != null) { OnItemAdd(this, new CPArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
            return ndx;
        }

        public void AddRange(CP[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new CPArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public void AddRange(CPs value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new CPArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public bool Contains(CP value)
        {
            return List.Contains(value);
        }


        public void CopyTo(CP[] array, int index)
        {
            List.CopyTo(array, index);
        }

        public int IndexOf(CP value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, CP value)
        {
            List.Insert(index, value);
            if (OnItemAdd != null) { OnItemAdd(this, new CPArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public void Remove(CP value)
        {
            List.Remove(value);
            if (OnItemRemove != null) { OnItemRemove(this, new CPArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }
       
        public class CPArgs : EventArgs
        {
            private CPs t;

            /// Default constructor.
            public CPArgs()
            {
                t = new CPs();
            }
          
            public CPArgs(CP t)
                : this()
            {
                this.t.Add(t);
            }
           
            public CPArgs(CPs ts)
                : this()
            {
                this.t.AddRange(ts);
            }

            public CPArgs(CP[] ts)
                : this()
            {
                this.t.AddRange(ts);
            }
            
            public CPs CPs
            {
                get { return t; }
                set { t = value; }
            }
        }

        public delegate void CPHandler(object sender, CPArgs e);
    }
}
