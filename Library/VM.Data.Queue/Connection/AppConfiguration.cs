using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{   
    /// <summary>
    /// Represents an xml root VMConfigurationCatalog document element.
    /// </summary>
    [XmlRoot("AppConfiguration")]
    public class AppConfiguration
    {
        private VMConfigurations element_cons = new VMConfigurations();

        /// <summary>VMConfigurations VMConfiguration collection xml element.</summary>
        [XmlArrayItem("VMConfiguration", typeof(VMConfiguration))]
        [XmlArray("VMConfigurations")]
        public VMConfigurations VMConfigurations
        {
            get { return element_cons; }
            set { element_cons = value; }
        }

        /// <summary></summary>
        /// <param name="gc"></param>
        public void Sort(VMConfigurationComparison cc)
        {
            VMConfigurationComparer gcr = new VMConfigurationComparer(cc);
            VMConfiguration[] cons = new VMConfiguration[element_cons.Count];
            element_cons.CopyTo(cons, 0);
            Array.Sort(cons, gcr);
            element_cons.Clear();
            element_cons.AddRange(cons);
        }


        public void RemoveVMConfigurationServiceByID(string serviceID)
        {
            ArrayList services = new ArrayList();
            bool found = false;
            foreach (VMConfiguration cn in VMConfigurations)
            {                
                services.Clear();
                found = false;
                //foreach(string id in cn.SERVICES)
                //{
                //    if (id == serviceID) found = true;
                //    services.Add(id);
                //}

                //if (found)
                //{
                //    services.Remove(serviceID);
                //    cn.SERVICES = (string[])services.ToArray(typeof(string));
                //}
            }
        }
    }
}
