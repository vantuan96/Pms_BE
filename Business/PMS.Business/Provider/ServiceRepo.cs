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
    public class ServiceRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();

        public void Dispose()
        {
            unitOfWork.Dispose();
        }
        #region Service Master
        public IQueryable<ServiceViewModel> GetServices(ServiceParameterModel request)
        {
            var services = unitOfWork.ServiceRepository.AsQueryable().Where(e => !e.IsDeleted);
            var groups = unitOfWork.ServiceGroupRepository.AsQueryable().Where(e => !e.IsDeleted);
            var categories = unitOfWork.ServiceCategoryRepository.AsQueryable();
            if (!string.IsNullOrEmpty(request.Ids))
            {
                if (request.Ids == "0000")
                    services = services.Where(e => e.Id == null);
                else
                {
                    var service_ids = request.GetIds();
                    services = services.Where(e => service_ids.Contains(e.Id));
                }
            }
            else
            {
                if (request.IsActived != null)
                {
                    services = services.Where(
                        e => e.IsActive == request.IsActived
                    );
                }
                if (!string.IsNullOrEmpty(request.ServiceType))
                {
                    services = services.Where(
                        e => e.ServiceType == request.ServiceType
                    );
                }
                if (!string.IsNullOrEmpty(request.Code))
                    services = services.Where(
                        e => e.Code.ToLower().Contains(request.Code)
                    );

                if (!string.IsNullOrEmpty(request.Name))
                    services = services.Where(
                        e => (e.ViName.ToLower().Contains(request.Name) || e.EnName.ToLower().Contains(request.Name))
                    );
                if (!string.IsNullOrEmpty(request.Search))
                {
                    services = services.Where(
                        e => (e.Code.ToLower().Contains(request.Search) || e.ViName.ToLower().Contains(request.Search) || e.EnName.ToLower().Contains(request.Search))
                    );
                }
            }

            var now = DateTime.Now;
            var results = (from serv_sql in services
                           join grp_sql in groups
                                on serv_sql.ServiceGroupId equals grp_sql.Id into glist
                           from grp_sql in glist.DefaultIfEmpty()
                           join cat_sql in categories
                                on serv_sql.ServiceCategoryId equals cat_sql.Id into clist
                           from cat_sql in clist.DefaultIfEmpty()
                           select new ServiceViewModel
                           {
                               Id = serv_sql.Id,
                               Code = serv_sql.Code,
                               ViName = serv_sql.ViName,
                               EnName = serv_sql.EnName,
                               GroupId = grp_sql.Id,
                               GroupCode = grp_sql.Code,
                               GroupViName = grp_sql.ViName,
                               GroupEnName = grp_sql.EnName,
                               CategoryId = cat_sql.Id,
                               CategoryCode = cat_sql.Code,
                               CategoryViName = cat_sql.ViName,
                               CategoryEnName = cat_sql.EnName
                           }).Distinct();
            if (string.IsNullOrEmpty(request.Ids))
            {
                if (!string.IsNullOrEmpty(request.Groups))
                {
                    if (request.Groups == "0000")
                        results = results.Where(e => e.GroupId == null);
                    else
                    {
                        var group_ids = request.GetGroups();
                        results = results.Where(e => group_ids.Contains(e.GroupId));
                    }
                }

                if (!string.IsNullOrEmpty(request.Categories))
                {
                    if (request.Categories == "0000")
                        results = results.Where(e => e.CategoryId == null);
                    else
                    {
                        var category_ids = request.GetCategories();
                        results = results.Where(e => category_ids.Contains(e.CategoryId));
                    }
                }
            }
            return results;
        }
        #endregion .Service Master

        
    }
}