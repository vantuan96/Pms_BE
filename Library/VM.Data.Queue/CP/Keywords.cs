using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace VM.Data.Queue
{    
    public class Keywords
    {        
        private List<Keyword> kws;

        public Keywords()
        {
            kws = new List<Keyword>();
        }        

        public void ClearKeywords()
        {
            kws.Clear();
        }

        public void AddKeyword(Keyword kw)
        {
            kws.Add(kw);
        }


        public void RemoveKeyword(Keyword kw)
        {
            kws.Remove(kw);
        }


        public bool Contains(Keyword kw)
        {
            foreach (Keyword k in kws)
            {
                if ((k.Name.ToUpper() == kw.Name.ToUpper()) && (k.ShortCode == kw.ShortCode))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsKeywordUsing(Keyword kw, out CP cp, out CPService sv)
        {
            CPCatalog cps = new CPCatalog();
            cp = null;
            sv = null;
            cps = CPCatalogSerializer.ReadFile(Globals.AgentConfigs.CPDataFile);
            if ((cps != null) && (cps.CPs != null))
            {
                foreach (CP curr in cps.CPs)
                {
                    if (curr.CPServices != null)
                    {
                        foreach (CPService service in curr.CPServices.Services)
                        {
                            if (service.KEYWORDS != null)
                            {
                                if (service.KEYWORDS.Contains(kw))
                                {
                                    cp = curr;
                                    sv = service;
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }


        [XmlArrayItem("Keyword", typeof(Keyword))]
        [XmlArray("Keywords")]
        public Keyword[] KWs
        {
            get
            {
                Keyword[] cs = (Keyword[])kws.ToArray();                
                //Array.Sort(cs, new KeywordComparer());
                return cs;
            }
            set
            {
                foreach (Keyword c in value)
                {
                    kws.Add(c);
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            foreach (Keyword k in kws)
            {
                result += k.ToString() + ";";
            }
            if (result!=string.Empty)
                result = result.Substring(0, result.Length - 1);
            return result;
        }
    }

    public class Keyword
    {
        private string kw;
        private string shortcode;

        public Keyword()
        {
        }

        public Keyword(string kw)
            : this()
        {
            this.kw = kw;
        }

        public Keyword(string kw, string shortcode)
            : this(kw)
        {
            this.shortcode = shortcode;
        }

        [XmlAttribute("Name")]
        public string Name
        {
            get { return kw; }
            set { kw = value; }
        }

        [XmlAttribute("ShortCode")]
        public string ShortCode
        {
            get { return shortcode; }
            set { shortcode = value; }
        }

        public override string ToString()
        {
            return kw + "-->" + shortcode;
        }
    }

    public class KeywordComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            Keyword c1 = x as Keyword;
            Keyword c2 = y as Keyword;
            if (x == null || y == null)
            {
                return 0;
            }

            if ((x == null) && (y != null))
            {
                return -1;
            }

            if ((x != null) && (y == null))
            {
                return 1;
            }

            if ((c1.Name.ToUpper() == c2.Name.ToUpper()) && (c1.ShortCode == c2.ShortCode))
            {
                return 0;
            }
            else
            {
                int r = c1.Name.ToUpper().CompareTo(c2.Name.ToUpper());
                if (r != 0)
                {
                    return r;
                }
                else
                {
                    return c1.ShortCode.CompareTo(c2.ShortCode);
                }
            }            
        }
    }



    public class KeywordBySCComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            Keyword c1 = x as Keyword;
            Keyword c2 = y as Keyword;
            if (x == null || y == null)
            {
                return 0;
            }

            if ((x == null) && (y != null))
            {
                return -1;
            }

            if ((x != null) && (y == null))
            {
                return 1;
            }


            return c1.ShortCode.CompareTo(c2.ShortCode);                                                  
           
        }
    }


    public class KeywordByKWComparer : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            Keyword c1 = x as Keyword;
            Keyword c2 = y as Keyword;
            if (x == null || y == null)
            {
                return 0;
            }

            if ((x == null) && (y != null))
            {
                return -1;
            }

            if ((x != null) && (y == null))
            {
                return 1;
            }

            
            return (c1.Name).CompareTo(c2.Name);            
        }
    }

}
