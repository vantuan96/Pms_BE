using System;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
  
    /// <summary>
    /// Represents an xml root CPHistory document element.
    /// </summary>
    [XmlRoot("CPHistory")]
    public class CPHistory
    {
        private CPs element_cp = new CPs();

        /// <summary>CPs CP collection xml element.</summary>
        [XmlArrayItem("CP", typeof(CP))]
        [XmlArray("CPs")]
        public CPs CPs
        {
            get { return element_cp; }
            set { element_cp = value; }
        }

        /// <summary></summary>
        /// <param name="gc"></param>
        public void Sort(CPComparison cc)
        {
            CPComparer gcr = new CPComparer(cc);
            CP[] cps = new CP[element_cp.Count];
            element_cp.CopyTo(cps, 0);
            Array.Sort(cps, gcr);
            element_cp.Clear();
            element_cp.AddRange(cps);
        }

    }

}
