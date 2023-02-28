using System;
using System.Collections;
using System.Xml.Serialization;

namespace VM.Data.Queue
{   

    public class CPServices
    {
        private ArrayList services;

        public CPServices()
        {
            services = new ArrayList();
        }        

        public void Clear()
        {
            services.Clear();
        }

        public void Add(CPService service)
        {
            try
            {
                services.Add(service);
            }
            catch
            {

            }
        }

        public void Remove(CPService service)
        {
            try
            {
                services.Remove(service);
            }
            catch
            {
            }
        }

        [XmlArrayItem("Service", typeof(CPService))]
        [XmlArray("Services")]
        public CPService[] Services
        {
            get
            {
                CPService[] arr = (CPService[])services.ToArray(typeof(CPService));
                //Array.Sort(arr, new CPServiceComparer());
                return arr;
            }
            set
            {
                foreach (CPService srv in value)
                {
                    services.Add(srv);
                }
            }
        }
    }

    public class CPServiceComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            CPService c1 = x as CPService;
            CPService c2 = y as CPService;
            if (x == null || y == null)
            {
                return 0;
            }
            return c1.ID.CompareTo(c2.ID);
        }
    }
}
