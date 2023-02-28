using DrFee.Utils;
using System.DirectoryServices;
using System.Linq;

namespace DrFee.Models
{
    public class ADUserDetailModel
    {
        public string UserId { get; set; }
        public string Department { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string DisplayName { get; set; }
        public string LoginName { get; set; }
        public string LoginNameWithDomain { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string EmailAddress { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Company { get; set; }
        public ADUserDetailModel Manager
        {
            get
            {
                if (!string.IsNullOrEmpty(this.ManagerName))
                {
                    ActiveDirectoryHelper ad = new ActiveDirectoryHelper();
                    return ad.GetUserByFullName(this.ManagerName);
                }
                return null;
            }
        }
        public string ManagerName { get; set; }

        public ADUserDetailModel() { }
        private ADUserDetailModel(DirectoryEntry directoryUser)
        {
            string domainAddress;
            string domainName;
            this.FirstName = GetProperty(directoryUser, ADProperties.FIRSTNAME);
            this.MiddleName = GetProperty(directoryUser, ADProperties.MIDDLENAME);
            this.LastName = GetProperty(directoryUser, ADProperties.LASTNAME);
            this.FullName = this.LastName + " " + this.FirstName;
            this.DisplayName = GetProperty(directoryUser, ADProperties.DISPLAYNAME);
            this.LoginName = GetProperty(directoryUser, ADProperties.LOGINNAME);
            this.UserId = this.LoginName;
            string userPrincipalName = GetProperty(directoryUser, ADProperties.USERPRINCIPALNAME);
            if (!string.IsNullOrEmpty(userPrincipalName))
            {
                domainAddress = userPrincipalName.Split('@')[1];
            }
            else
            {
                domainAddress = string.Empty;
            }
            if (!string.IsNullOrEmpty(domainAddress))
            {
                domainName = domainAddress.Split('.').First();
            }
            else
            {
                domainName = string.Empty;
            }
            this.LoginNameWithDomain = string.Format(@"{0}\{1}", domainName, LoginName);
            this.StreetAddress = GetProperty(directoryUser, ADProperties.STREETADDRESS);
            this.City = GetProperty(directoryUser, ADProperties.CITY);
            this.State = GetProperty(directoryUser, ADProperties.STATE);
            this.PostalCode = GetProperty(directoryUser, ADProperties.POSTALCODE);
            this.Country = GetProperty(directoryUser, ADProperties.COUNTRY);
            this.Company = GetProperty(directoryUser, ADProperties.COMPANY);
            this.Department = GetProperty(directoryUser, ADProperties.DEPARTMENT);
            this.HomePhone = GetProperty(directoryUser, ADProperties.HOMEPHONE);
            this.Extension = GetProperty(directoryUser, ADProperties.EXTENSION);
            this.Mobile = GetProperty(directoryUser, ADProperties.MOBILE);
            this.Fax = GetProperty(directoryUser, ADProperties.FAX);
            this.EmailAddress = GetProperty(directoryUser, ADProperties.EMAILADDRESS);
            this.Title = GetProperty(directoryUser, ADProperties.TITLE);
            this.Description = GetProperty(directoryUser, ADProperties.DESCRIPTION);
            this.ManagerName = GetProperty(directoryUser, ADProperties.MANAGER);
            if (!string.IsNullOrEmpty(ManagerName))
            {
                string[] managerArray = this.ManagerName.Split(',');
                this.ManagerName = managerArray[0].Replace("CN=", "");
            }
        }
        private static string GetProperty(DirectoryEntry userDetail, string propertyName)
        {
            if (userDetail.Properties.Contains(propertyName))
            {
                return userDetail.Properties[propertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        public static ADUserDetailModel GetUser(DirectoryEntry directoryUser)
        {
            return new ADUserDetailModel(directoryUser);
        }
    }

    public static class ADProperties
    {
        public const string OBJECTCLASS = "objectClass";
        public const string CONTAINERNAME = "cn";
        public const string LASTNAME = "sn";
        public const string COUNTRYNOTATION = "c";
        public const string CITY = "l";
        public const string STATE = "st";
        public const string TITLE = "title";
        public const string DESCRIPTION = "description";
        public const string POSTALCODE = "postalCode";
        public const string PHYSICALDELIVERYOFFICENAME = "physicalDeliveryOfficeName";
        public const string FIRSTNAME = "givenName";
        public const string MIDDLENAME = "initials";
        public const string DISTINGUISHEDNAME = "distinguishedName";
        public const string INSTANCETYPE = "instanceType";
        public const string WHENCREATED = "whenCreated";
        public const string WHENCHANGED = "whenChanged";
        public const string DISPLAYNAME = "displayName";
        public const string USNCREATED = "uSNCreated";
        public const string MEMBEROF = "memberOf";
        public const string USNCHANGED = "uSNChanged";
        public const string COUNTRY = "co";
        public const string DEPARTMENT = "department";
        public const string COMPANY = "company";
        public const string PROXYADDRESSES = "proxyAddresses";
        public const string STREETADDRESS = "streetAddress";
        public const string DIRECTREPORTS = "directReports";
        public const string NAME = "name";
        public const string OBJECTGUID = "objectGUID";
        public const string USERACCOUNTCONTROL = "userAccountControl";
        public const string BADPWDCOUNT = "badPwdCount";
        public const string CODEPAGE = "codePage";
        public const string COUNTRYCODE = "countryCode";
        public const string BADPASSWORDTIME = "badPasswordTime";
        public const string LASTLOGOFF = "lastLogoff";
        public const string LASTLOGON = "lastLogon";
        public const string PWDLASTSET = "pwdLastSet";
        public const string PRIMARYGROUPID = "primaryGroupID";
        public const string OBJECTSID = "objectSid";
        public const string ADMINCOUNT = "adminCount";
        public const string ACCOUNTEXPIRES = "accountExpires";
        public const string LOGONCOUNT = "logonCount";
        public const string LOGINNAME = "sAMAccountName";
        public const string SAMACCOUNTTYPE = "sAMAccountType";
        public const string SHOWINADDRESSBOOK = "showInAddressBook";
        public const string LEGACYEXCHANGEDN = "legacyExchangeDN";
        public const string USERPRINCIPALNAME = "userPrincipalName";
        public const string EXTENSION = "ipPhone";
        public const string SERVICEPRINCIPALNAME = "servicePrincipalName";
        public const string OBJECTCATEGORY = "objectCategory";
        public const string DSCOREPROPAGATIONDATA = "dSCorePropagationData";
        public const string LASTLOGONTIMESTAMP = "lastLogonTimestamp";
        public const string EMAILADDRESS = "mail";
        public const string MANAGER = "manager";
        public const string MOBILE = "mobile";
        public const string PAGER = "pager";
        public const string FAX = "facsimileTelephoneNumber";
        public const string HOMEPHONE = "homePhone";
        public const string MSEXCHUSERACCOUNTCONTROL = "msExchUserAccountControl";
        public const string MDBUSEDEFAULTS = "mDBUseDefaults";
        public const string MSEXCHMAILBOXSECURITYDESCRIPTOR = "msExchMailboxSecurityDescriptor";
        public const string HOMEMDB = "homeMDB";
        public const string MSEXCHPOLICIESINCLUDED = "msExchPoliciesIncluded";
        public const string HOMEMTA = "homeMTA";
        public const string MSEXCHRECIPIENTTYPEDETAILS = "msExchRecipientTypeDetails";
        public const string MAILNICKNAME = "mailNickname";
        public const string MSEXCHHOMESERVERNAME = "msExchHomeServerName";
        public const string MSEXCHVERSION = "msExchVersion";
        public const string MSEXCHRECIPIENTDISPLAYTYPE = "msExchRecipientDisplayType";
        public const string MSEXCHMAILBOXGUID = "msExchMailboxGuid";
        public const string NTSECURITYDESCRIPTOR = "nTSecurityDescriptor";
    }
}