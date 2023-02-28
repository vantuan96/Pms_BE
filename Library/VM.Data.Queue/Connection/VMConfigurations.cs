using System;
using System.Collections;

namespace VM.Data.Queue
{
    
    [Serializable()]
    public class VMConfigurations : CollectionBase
    {
        /// <summary>Notifies when the collection has been modified.</summary>
        public event EventHandler OnItemsChanged;

        /// <summary>Notifies that an item has been added.</summary>
        public event VMConfigurationHandler OnItemAdd;

        /// <summary>Notifies that items have been added.</summary>
        public event VMConfigurationHandler OnItemsAdd;

        /// <summary>Notifies that an item has been removed.</summary>
        public event VMConfigurationHandler OnItemRemove;


        public VMConfigurations()
        {
        }


        public VMConfigurations(VMConfigurations value)
        {
            this.AddRange(value);
        }

        public VMConfigurations(VMConfiguration[] value)
        {
            this.AddRange(value);
        }

        public VMConfiguration this[int index]
        {
            get { return ((VMConfiguration)(List[index])); }
            set { List[index] = value; }
        }

        public int Add(VMConfiguration value)
        {
            int ndx = List.Add(value);
            if (OnItemAdd != null) { OnItemAdd(this, new VMConfigurationArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
            return ndx;
        }

        public void AddRange(VMConfiguration[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new VMConfigurationArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public void AddRange(VMConfigurations value)
        {
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
            if (OnItemsAdd != null) { OnItemsAdd(this, new VMConfigurationArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public bool Contains(VMConfiguration value)
        {
            return List.Contains(value);
        }


        public void CopyTo(VMConfiguration[] array, int index)
        {
            List.CopyTo(array, index);
        }

        public int IndexOf(VMConfiguration value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, VMConfiguration value)
        {
            List.Insert(index, value);
            if (OnItemAdd != null) { OnItemAdd(this, new VMConfigurationArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public void Remove(VMConfiguration value)
        {
            List.Remove(value);
            if (OnItemRemove != null) { OnItemRemove(this, new VMConfigurationArgs(value)); }
            if (OnItemsChanged != null) { OnItemsChanged(value, EventArgs.Empty); }
        }

        public class VMConfigurationArgs : EventArgs
        {
            private VMConfigurations t;

            /// Default constructor.
            public VMConfigurationArgs()
            {
                t = new VMConfigurations();
            }

            public VMConfigurationArgs(VMConfiguration t)
                : this()
            {
                this.t.Add(t);
            }

            public VMConfigurationArgs(VMConfigurations ts)
                : this()
            {
                this.t.AddRange(ts);
            }

            public VMConfigurationArgs(VMConfiguration[] ts)
                : this()
            {
                this.t.AddRange(ts);
            }

            public VMConfigurations VMConfigurations
            {
                get { return t; }
                set { t = value; }
            }
        }

        public delegate void VMConfigurationHandler(object sender, VMConfigurationArgs e);
    }

}
