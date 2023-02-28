using DataAccess.Models;
using DataAccess.Repository;
using DrFee.Models;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace DrFee.Provider
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
        public void SyncDoctorIntoUser(string userName,Guid siteId)
        {
            var user = unitOfWork.UserRepository.FirstOrDefault(s => s.Username.ToLower() == userName.Trim().ToLower());
            if (user != null)
            {
                //Đã tồn tại
                //Gán Site
                //foreach (var site in request["Sites"])
                //    CreateUserSite(user.Id, site);
                //Ktra xem site đã tồn tại chưa
                var userSite_Exist = unitOfWork.UserSiteRepository.AsQueryable().Any(x=>x.SiteId== siteId && x.UserId==user.Id);
                if (!userSite_Exist)
                {
                    var user_site = new UserSite
                    {
                        UserId = user.Id,
                        SiteId = siteId
                    };
                    unitOfWork.UserSiteRepository.Add(user_site);
                    unitOfWork.Commit();
                }
            }
            else
            {
                var result = GetUserADInfo(userName);
                if (result != null)
                {
                    user = new User
                    {
                        Username = userName,
                        //EhosAccount = request["EhosAccount"]?.ToString(),
                        Fullname = result.FullName,
                        DisplayName = result.DisplayName,
                        Department = result.Department,
                        Title = result.Title,
                    };
                    unitOfWork.UserRepository.Add(user);

                    //unitOfWork.UserRoleRepository.HardDeleteRange(user.UserRoles.AsQueryable());
                    //foreach (var role in request["Roles"])
                    //    CreateUserRole(user.Id, role);

                    //Gán Site
                    unitOfWork.UserSiteRepository.HardDeleteRange(user.UserSites.AsQueryable());
                    var user_site = new UserSite
                    {
                        UserId = user.Id,
                        SiteId = siteId
                    };
                    unitOfWork.UserSiteRepository.Add(user_site);

                    unitOfWork.Commit();
                }
            }
        }
    }
}