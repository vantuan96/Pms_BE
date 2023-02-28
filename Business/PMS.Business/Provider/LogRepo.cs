using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Contract.Models;
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.Enum;
using PMS.Contract.Models.MasterData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using VM.Common;

namespace PMS.Business.Provider
{
    public class LogRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();

        public void Dispose()
        {
            unitOfWork.Dispose();
        }
        #region LogAction business
        public static void AddLogAction(Guid? objectId, string objectName, int actionType, string Notes)
        {
            if (actionType == -1)
                return;
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                LogAction entity = new LogAction()
                {
                    ObjectId = objectId,
                    ObjectName = objectName,
                    ActionType = actionType,
                    Notes = Notes,
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserHelper.CurrentUserName()
                };
                unitOfWork.LogActionRepository.Add(entity);
                unitOfWork.Commit();
            }
        }
        #endregion .LogAction business
    }
}