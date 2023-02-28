using System;
using System.Collections.Generic;
using System.Text;

namespace VM.Data.Queue
{
    /// <summary></summary>
    public enum CPComparison
    {
        Name, ID
    }
    
    public class CPComparer : System.Collections.IComparer
    {
        private CPComparison cc;

        /// <summary></summary>
        /// <param name="gc"></param>
        public CPComparer(CPComparison cc)
        {
            this.cc = cc;
        }

        /// <summary></summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            CP c1 = x as CP;
            CP c2 = y as CP;
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
                case CPComparison.Name:
                    return c1.NAME.CompareTo(c2.NAME);
                case CPComparison.ID:                    
                    answer = c1.ID.CompareTo(c2.ID);                                          
                    return answer;                              
            }
            return answer;
        }
    }
}
