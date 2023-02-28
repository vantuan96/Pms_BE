using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;

namespace VM.Data.Queue
{
    /// <summary>
    /// Represent a SMS Agent user
    /// </summary>
    public class User
    {
        private String _username;
        private String _fullname;
        private String _description;
        private String _password;
        private Boolean _disabled;

        private Hashtable _groups;

        public User() { _groups = new Hashtable(); }

        /// <summary>
        /// UserName that user uses when login to system.
        /// </summary>
        [XmlAttribute("UserName")]
        public String UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// FullName, the display name of user.
        /// </summary>
        [XmlElement("FullName")]
        public String FullName
        {
            get { return _fullname; }
            set { _fullname = value; }
        }

        /// <summary>
        /// Description, the description about the user.
        /// </summary>
        [XmlElement("Description")]
        public String Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Password that make the security when user login to system
        /// </summary>
        [XmlElement("Password")]
        public String Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// If the user is active status
        /// </summary>
        [XmlElement("Disabled")]
        public bool Disabled
        {
            get { return _disabled; }
            set { _disabled = value; }
        }

        /// <summary>
        /// The groups that user included in
        /// </summary>       
        [XmlArray("Groups")]
        [XmlArrayItem("Group", typeof(Group))]
        public Group[] Groups
        {
            get
            {
                Group[] grs = new Group[_groups.Count];
                int i = 0;
                foreach (string _groupName in _groups.Keys)
                {
                    Group gr = new Group();
                    gr.Name = _groupName;
                    gr.Description = _groups[_groupName].ToString();
                    grs[i++] = gr;
                }
                return grs;
            }
            set
            {
                _groups.Clear();

                foreach (Group gr in value)
                {
                    _groups.Add(gr.Name, gr.Description);
                }
            }
        }

        /// <summary>
        /// Add new group
        /// </summary>
        /// <param name="name">Group name</param>
        /// <param name="desc">Group description</param>
        public void AddGroup(string name, string desc)
        {
            _groups.Add(name, desc);
        }

        /// <summary>
        /// Add new group
        /// </summary>
        /// <param name="g"></param>
        public void AddGroup(Group g)
        {
            _groups.Add(g.Name, g.Description);
        }

        /// <summary>
        /// Remove a group
        /// </summary>
        /// <param name="name"></param>
        public void RemoveGroup(string name)
        {
            _groups.Remove(name);
        }

    }
}
