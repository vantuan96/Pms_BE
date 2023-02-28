using System;
using System.IO;
using System.Xml.Serialization;

namespace VM.Data.Queue
{
    /// <summary>    
    /// Represents an xml root UserProfiles document element.
    /// </summary>
    [XmlRoot("UserProfiles")]    
    public class UserProfiles
    {
        private Users user_profiles = new Users();

        /// <summary>Users is user collection xml element.</summary>
        [XmlArrayItem("User", typeof(User))]
        [XmlArray("Users")]
        public Users Users
        {
            get { return user_profiles; }
            set { user_profiles = value; }
        }

        /// <summary>
        /// Sort the user collection by user comparer rule
        /// </summary>
        public void Sort()
        {
            User[] users = new User[user_profiles.Count];
            user_profiles.CopyTo(users, 0);
            UserComparer ucr = new UserComparer();
            Array.Sort(users, ucr);
            user_profiles.Clear();
            user_profiles.AddRange(users);
        }
    }
}
