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
    public class PackageGroupRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();

        public void Dispose()
        {
            unitOfWork.Dispose();
        }

        public IQueryable<PackageGroup> GetPackageGroups(PackageGroupParameterModel request)
        {
            var groups = unitOfWork.PackageGroupRepository.AsQueryable().Where(e => !e.IsDeleted);

            if (request.Name != null)
                groups = groups.Where(e => e.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                groups = groups.Where(e => e.Code.Contains(request.Code));

            if (request.Status != -1)
                groups = groups.Where(e => e.IsActived == request.Status > 0);
            return groups;
        }
        public PackageGroup GetPackageGroupRoot(PackageGroup child)
        {
            var parrentEntity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == child.ParentId);
            if (parrentEntity != null)
            {
                if (parrentEntity.ParentId != null && (parrentEntity.Id != parrentEntity.ParentId))
                {
                    return GetPackageGroupRoot(parrentEntity);
                }
                return parrentEntity;
            }
            else
            {
                return child;
            }
        }
        public PackageGroup GetPackageGroupRoot(string childCode)
        {
            var entity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Code == childCode);
            if (entity == null)
                return null;
            var parrentEntity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == entity.ParentId);
            if (parrentEntity != null)
            {
                if (parrentEntity.ParentId != null && (parrentEntity.Id != parrentEntity.ParentId))
                {
                    return GetPackageGroupRoot(parrentEntity);
                }
                return parrentEntity;
            }
            else
            {
                return entity;
            }
        }
    }
}