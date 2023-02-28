using DataAccess.Models;
using DataAccess.Repository;
using PMS.Contract.Models;
using PMS.Contract.Models.MasterData;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using VM.Common;

namespace PMS.Business.Provider
{
    public class UserRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();
        public User ValidateUser(string username)
        {
            return unitOfWork.UserRepository.FirstOrDefault(user => user.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        public void Dispose()
        {
            unitOfWork.Dispose();
        }

        public ADUserDetailModel GetUserADInfo(string userName, string domainName = "vingroup.local", string container = "DC=VINGROUP,DC=LOCAL")
        {
            try
            {
                PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName, container);
                UserPrincipal userPrincipal = new UserPrincipal(domainContext);
                userPrincipal.SamAccountName = userName;
                PrincipalSearcher principleSearch = new PrincipalSearcher();
                principleSearch.QueryFilter = userPrincipal;
                PrincipalSearchResult<Principal> results = principleSearch.FindAll();
                Principal principle = results.ToList()[0];
                DirectoryEntry directory = (DirectoryEntry)principle.GetUnderlyingObject();
                return ADUserDetailModel.GetUser(directory);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<UserSitesModel> GetListUserSiteHaveNoSpecialty()
        {
            var entities= unitOfWork.UserSiteRepository.Find(x=>x.SpecialtyId==null);
            if (entities.Any())
            {
                return entities.Select(x => new UserSitesModel{
                    SiteId=x.SiteId.Value
                    ,UserName=x.User.Username
                    ,UserSiteId = string.Format("{0}_{1}", x.SiteId.Value, x.User.Username)
                }).ToList();
            }
            return null;
        }
    }
}