using DrFee.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;

namespace DrFee.Utils
{
    public class ActiveDirectoryHelper
    {
        private string LDAPPath
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPath"];
            }
        }
        private string LDAPUser
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPUser"];
            }
        }
        private string LDAPPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["LDAPPassword"];
            }
        }
        DirectoryEntry _directoryEntry;
        private DirectoryEntry SearchRoot
        {
            get
            {
                if (_directoryEntry == null)
                {
                    _directoryEntry = new DirectoryEntry(LDAPPath, LDAPUser, LDAPPassword, AuthenticationTypes.Secure);
                }
                return _directoryEntry;
            }
        }
        public ADUserDetailModel GetUserByFullName(string userName)
        {
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=user)(cn=" + userName + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry user = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    return ADUserDetailModel.GetUser(user);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        public ADUserDetailModel GetUserByLoginName(string userName)
        {
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry user = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    return ADUserDetailModel.GetUser(user);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<ADUserDetailModel> GetUserFromGroup(string groupName)
        {
            List<ADUserDetailModel> userlist = new List<ADUserDetailModel>();
            try
            {
                _directoryEntry = null;
                DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
                directorySearch.Filter = "(&(objectClass=group)(SAMAccountName=" + groupName + "))";
                SearchResult results = directorySearch.FindOne();
                if (results != null)
                {
                    DirectoryEntry deGroup = new DirectoryEntry(results.Path, LDAPUser, LDAPPassword);
                    PropertyCollection pColl = deGroup.Properties;
                    int count = pColl["member"].Count;
                    for (int i = 0; i < count; i++)
                    {
                        string respath = results.Path;
                        string[] pathnavigate = respath.Split("CN".ToCharArray());
                        respath = pathnavigate[0];
                        string objpath = pColl["member"][i].ToString();
                        string path = respath + objpath;
                        DirectoryEntry user = new DirectoryEntry(path, LDAPUser, LDAPPassword);
                        ADUserDetailModel userobj = ADUserDetailModel.GetUser(user);
                        userlist.Add(userobj);
                        user.Close();
                    }
                }
                return userlist;
            }
            catch (Exception)
            {
                return userlist;
            }
        }
        public List<ADUserDetailModel> GetUsersByFirstName(string fName)
        {
            //UserProfile user;  
            List<ADUserDetailModel> userlist = new List<ADUserDetailModel>();
            string filter = "";
            _directoryEntry = null;
            DirectorySearcher directorySearch = new DirectorySearcher(SearchRoot);
            directorySearch.Asynchronous = true;
            directorySearch.CacheResults = true;
            //directorySearch.Filter = "(&(objectClass=user)(SAMAccountName=" + userName + "))";  
            filter = string.Format("(givenName={0}*", fName);
            //filter = "(&(objectClass=user)(objectCategory=person)" + filter + ")";  
            filter = "(&(objectClass=user)(objectCategory=person)(givenName=" + fName + "*))";
            directorySearch.Filter = filter;
            SearchResultCollection userCollection = directorySearch.FindAll();
            foreach (SearchResult users in userCollection)
            {
                DirectoryEntry userEntry = new DirectoryEntry(users.Path, LDAPUser, LDAPPassword);
                ADUserDetailModel userInfo = ADUserDetailModel.GetUser(userEntry);
                userlist.Add(userInfo);
            }
            directorySearch.Filter = "(&(objectClass=group)(SAMAccountName=" + fName + "*))";
            SearchResultCollection results = directorySearch.FindAll();
            if (results != null)
            {
                foreach (SearchResult r in results)
                {
                    DirectoryEntry deGroup = new DirectoryEntry(r.Path, LDAPUser, LDAPPassword);
                    // ADUserDetail dhan = new ADUserDetail();  
                    ADUserDetailModel agroup = ADUserDetailModel.GetUser(deGroup);
                    userlist.Add(agroup);
                }
            }
            return userlist;
        }
    }
}