using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Authentication
{
    public class BusinessHelper
    {
        public IUnitOfWork unitOfWork = new EfUnitOfWork();
        public BusinessHelper(IUnitOfWork entityWork)
        {
            this.unitOfWork = entityWork;
        }
        public void ClearSessionInDBByRoleId(Guid gRoleId)
        {
            #region remove user session
            var users = unitOfWork.UserRoleRepository.Find(
                e =>
                e.UserId != null &&
                e.RoleId != null &&
                e.RoleId == gRoleId
            ).Select(e => e.User);

            foreach (var user in users)
            {
                user.SessionId = null;
                user.Session = null;
            }
            //unitOfWork.Commit();
            #endregion
        }
        public void ClearSessionInDBByUserID(Guid gUId)
        {
            #region remove user session
            var users = unitOfWork.UserRepository.Find(
                e =>
                e.Id == gUId
            );

            foreach (var user in users)
            {
                user.SessionId = null;
                user.Session = null;
            }
            //unitOfWork.Commit();
            #endregion
        }
    }
}