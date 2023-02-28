using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccess.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        #region General & User/Role
        IGenericRepository<AppConstant> AppConstantRepository { get; }
        IGenericRepository<Models.Action> ActionRepository { get; }
        IGenericRepository<Department> DepartmentRepository { get; }
        IGenericRepository<Log> LogRepository { get; }
        IGenericRepository<LogInFail> LogInFailRepository { get; }
        IGenericRepository<LogAction> LogActionRepository { get; }
        IGenericRepository<Position> PositionRepository { get; }
        IGenericRepository<Role> RoleRepository { get; }
        IGenericRepository<Module> ModuleRepository { get; }
        IGenericRepository<GroupAction> GroupActionRepository { get; }
        IGenericRepository<GroupAction_Map> GroupAction_MapRepository { get; }
        IGenericRepository<RoleGroupAction> RoleGroupActionRepository { get; }
        IGenericRepository<Site> SiteRepository { get; }
        IGenericRepository<Specialty> SpecialtyRepository { get; }
        IGenericRepository<SystemConfig> SystemConfigRepository { get; }
        IGenericRepository<SystemNotification> SystemNotificationRepository { get; }
        IGenericRepository<User> UserRepository { get; }
        IGenericRepository<UserPosition> UserPositionRepository { get; }
        IGenericRepository<UserRole> UserRoleRepository { get; }
        IGenericRepository<UserSite> UserSiteRepository { get; }
        #endregion .General & User/Role
        #region Business
        IGenericRepository<PackageGroup> PackageGroupRepository { get; }
        IGenericRepository<Package> PackageRepository { get; }
        #region Price Policy setting
        IGenericRepository<PackagePrice> PackagePriceRepository { get; }
        IGenericRepository<PackagePriceSite> PackagePriceSiteRepository { get; }
        IGenericRepository<PackagePriceDetail> PackagePriceDetailRepository { get; }
        #endregion .Price Policy setting
        #region Service
        IGenericRepository<ServiceGroup> ServiceGroupRepository { get; }
        IGenericRepository<ServiceCategory> ServiceCategoryRepository { get; }
        IGenericRepository<Service> ServiceRepository { get; }
        IGenericRepository<ServiceInPackage> ServiceInPackageRepository { get; }
        IGenericRepository<ServiceFreeInPackage> ServiceFreeInPackageRepository { get; }
        #endregion .Service
        #region Patient & Package manager & setting
        IGenericRepository<PatientInformation> PatientInformationRepository { get; }
        IGenericRepository<PatientInPackage> PatientInPackageRepository { get; }
        IGenericRepository<PatientInPackageChild> PatientInPackageChildRepository { get; }
        IGenericRepository<PatientInPackageDetail> PatientInPackageDetailRepository { get; }
        #endregion .Patient & Package manager & setting

        #region Charge & His data
        IGenericRepository<HISCharge> HISChargeRepository { get; }
        IGenericRepository<HISChargeDetail> HISChargeDetailRepository { get; }
        #endregion .Charge & His data

        #region Table temp
        IGenericRepository<Temp_PackageGroup> Temp_PackageGroupRepository { get; }
        IGenericRepository<Temp_Package> Temp_PackageRepository { get; }
        IGenericRepository<Temp_ServiceInPackage> Temp_ServiceInPackageRepository { get; }
        IGenericRepository<Temp_PatientInPackage> Temp_PatientInPackageRepository { get; }
        IGenericRepository<Temp_ServiceUsing> Temp_ServiceUsingRepository { get; }
        IGenericRepository<Temp_ServiceUsingNotCharge> Temp_ServiceUsingNotChargeRepository { get; }
        IGenericRepository<Temp_NetAmountLessThanBase> Temp_NetAmountLessThanBaseRepository { get; }
        IGenericRepository<Temp_UpdateOriginalPrice> Temp_UpdateOriginalPriceRepository { get; }
        #endregion .Table temp
        #endregion .Business
        void Commit();

        DataTable ExecStore(string store_name, Dictionary<string, string> param=null);
    }
}
