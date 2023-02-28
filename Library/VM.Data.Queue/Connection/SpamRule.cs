using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VM.Data.Queue
{    
    public class SpamRule
    {
        private string _OPERATORID;
        [XmlAttribute("OPERATORID")]
        public string OPERATORID
        {
            get { return _OPERATORID; }
            set { _OPERATORID = value; }
        }

        private string _NAME;        
        [XmlElement("NAME")]
        public string NAME
        {
            get { return _NAME; }
            set { _NAME = value; }
        }

        private bool _ENABLESPAM = false;
        [XmlElement("ENABLESPAM")]
        public bool ENABLESPAM
        {
            get { return _ENABLESPAM; }
            set { _ENABLESPAM = value; }
        }

        private bool _PREVENTALL = false;
        [XmlElement("PREVENTALL")]
        public bool PREVENTALL
        {
            get { return _PREVENTALL; }
            set { _PREVENTALL = value; }
        }

        private string _operator_prevent_message = "So dien thoai cua ban da bi chung toi ngan chan su dung dich vu, lien he 0982121828 hoac truy cap www.keywordz.vn va www.miu.vn";
        [XmlElement("OPERATOR_PREVENT_MESSAGE")]
        public string OPERATOR_PREVENT_MESSAGE
        {
            get { return _operator_prevent_message; }
            set { _operator_prevent_message = value; }
        }


        private List<string> _services = new List<string>();
        [XmlElement("SERVICES")]
        public List<string> SERVICES
        {
            get { return _services; }
            set { _services = value; }
        }

        private List<string> _services_prevent = new List<string>();
        [XmlElement("SERVICES_PREVENT")]
        public List<string> SERVICES_PREVENT
        {
            get { return _services_prevent; }
            set { _services_prevent = value; }
        }

        public override string ToString()
        {
            return _NAME;
        }
    }

    public class SpamList
    {      
        private List<SpamRule> _list;

        public SpamList()
        {
            _list = new List<SpamRule>();
        }        

        public void Clear()
        {
            _list.Clear();
        }

        public void Add(SpamRule sr)
        {
            _list.Add(sr);
        }


        public void Remove(SpamRule sr)
        {
            _list.Remove(sr);
        }

        public bool Contains(SpamRule sr)
        {
            foreach (SpamRule s in _list)
            {
                if (s.OPERATORID == sr.OPERATORID)
                {
                    return true;
                }
            }
            return false;
        }

        public SpamRule FindSpamRule(string operID)
        {
            foreach (SpamRule s in this._list)
            {
                if (s.OPERATORID == operID)
                {
                    return s;
                }
            }
            return null;
        }

        [XmlArrayItem("SpamRule", typeof(SpamRule))]
        [XmlArray("SpamRules")]
        public SpamRule[] SpamRules
        {
            get
            {
                SpamRule[] cs = (SpamRule[])_list.ToArray();                
                return cs;
            }
            set
            {
                foreach (SpamRule c in value)
                {
                    _list.Add(c);
                }
            }
        }
    }
}
