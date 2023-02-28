using System;
using System.Collections.Generic;
using System.Text;

namespace VM.Data.Queue
{
    /// <summary></summary>
    public enum VMConfigurationComparison
    {
        Name
    }

    public class VMConfigurationComparer : System.Collections.IComparer
    {
        private VMConfigurationComparison cc;

        /// <summary></summary>
        /// <param name="gc"></param>
        public VMConfigurationComparer(VMConfigurationComparison cc)
        {
            this.cc = cc;
        }

        /// <summary></summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            VMConfiguration c1 = x as VMConfiguration;
            VMConfiguration c2 = y as VMConfiguration;
            if (c1 == null && c2 == null)
            {
                return 0;
            }
            else if (c1 == null)
            {
                return -1;
            }
            else if (c2 == null)
            {
                return 1;
            }
            int answer = 0;
            switch (cc)
            {
                case VMConfigurationComparison.Name:
                    return c1.NAME.CompareTo(c2.NAME);                
            }
            return answer;
        }
    }
}
