using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using DataAccess.Models;

namespace DataAccess.Repository
{
    public class EfUnitOfWork : DbContext, IUnitOfWork
    {
        private PMSContext context = new PMSContext();

        #region General EfGenericRepository
        private readonly EfGenericRepository<AppConstant> _repoAppConstant;
        private readonly EfGenericRepository<Action> _repoAction;
        private readonly EfGenericRepository<Department> _repoDepartment;
        private readonly EfGenericRepository<Log> _repoLog;
        private readonly EfGenericRepository<LogInFail> _repoLogInFail;
        private readonly EfGenericRepository<LogAction> _repoLogAction;
        private readonly EfGenericRepository<Position> _repoPosition;
        private readonly EfGenericRepository<Role> _repoRole;
        private readonly EfGenericRepository<Module> _repoModule;
        private readonly EfGenericRepository<GroupAction> _repoGroupAction;
        private readonly EfGenericRepository<GroupAction_Map> _repoGroupAction_Map;
        private readonly EfGenericRepository<RoleGroupAction> _repoRoleGroupAction;
        private readonly EfGenericRepository<Site> _repoSite;
        private readonly EfGenericRepository<Specialty> _repoSpecialty;
        private readonly EfGenericRepository<SystemConfig> _repoSystemConfig;
        private readonly EfGenericRepository<SystemNotification> _repoSystemNotification;
        private readonly EfGenericRepository<User> _repoUser;
        private readonly EfGenericRepository<UserPosition> _repoUserPosition;
        private readonly EfGenericRepository<UserRole> _repoUserRole;
        private readonly EfGenericRepository<UserSite> _repoUserSite;

        private readonly EfGenericRepository<PackageGroup> _repoPackageGroup;
        private readonly EfGenericRepository<Package> _repoPackage;
        #region Price Policy setting
        private readonly EfGenericRepository<PackagePrice> _repoPackagePrice;
        private readonly EfGenericRepository<PackagePriceSite> _repoPackagePriceSite;
        private readonly EfGenericRepository<PackagePriceDetail> _repoPackagePriceDetail;
        #endregion .Price Policy setting
        #region Service
        private readonly EfGenericRepository<ServiceGroup> _repoServiceGroup;
        private readonly EfGenericRepository<ServiceCategory> _repoServiceCategory;
        private readonly EfGenericRepository<Service> _repoService;
        private readonly EfGenericRepository<ServiceInPackage> _repoServiceInPackage;
        private readonly EfGenericRepository<ServiceFreeInPackage> _repoServiceFreeInPackage;
        #endregion .Service

        #region Patient & Package manager & setting
        private readonly EfGenericRepository<PatientInformation> _repoPatientInformation;
        private readonly EfGenericRepository<PatientInPackage> _repoPatientInPackage;
        private readonly EfGenericRepository<PatientInPackageChild> _repoPatientInPackageChild;
        private readonly EfGenericRepository<PatientInPackageDetail> _repoPatientInPackageDetail;
        #endregion .Patient & Package manager & setting

        #region Charge & His data
        private readonly EfGenericRepository<HISCharge> _repoHISCharge;
        private readonly EfGenericRepository<HISChargeDetail> _repoHISChargeDetail;
        #endregion .Charge & His data
        #region Temp table
        private readonly EfGenericRepository<Temp_PackageGroup> _repoTemp_PackageGroup;
        private readonly EfGenericRepository<Temp_Package> _repoTemp_Package;
        private readonly EfGenericRepository<Temp_ServiceInPackage> _repoTemp_ServiceInPackage;
        private readonly EfGenericRepository<Temp_PatientInPackage> _repoTemp_PatientInPackage;
        private readonly EfGenericRepository<Temp_ServiceUsing> _repoTemp_ServiceUsing;
        private readonly EfGenericRepository<Temp_ServiceUsingNotCharge> _repoTemp_ServiceUsingNotCharge;
        private readonly EfGenericRepository<Temp_NetAmountLessThanBase> _repoTemp_NetAmountLessThanBase;
        private readonly EfGenericRepository<Temp_UpdateOriginalPrice> _repoTemp_UpdateOriginalPrice;
        #endregion Temp table
        #endregion

        #region General constructor
        public EfUnitOfWork()
        {
            _repoAppConstant = new EfGenericRepository<AppConstant>(context);
            _repoAction = new EfGenericRepository<Action>(context);
            _repoDepartment = new EfGenericRepository<Department>(context);
            _repoLog = new EfGenericRepository<Log>(context);
            _repoLogInFail = new EfGenericRepository<LogInFail>(context);
            _repoLogAction = new EfGenericRepository<LogAction>(context);
            _repoPosition = new EfGenericRepository<Position>(context);
            _repoRole = new EfGenericRepository<Role>(context);
            _repoModule = new EfGenericRepository<Module>(context);
            _repoGroupAction = new EfGenericRepository<GroupAction>(context);
            _repoGroupAction_Map = new EfGenericRepository<GroupAction_Map>(context);
            _repoRoleGroupAction = new EfGenericRepository<RoleGroupAction>(context);
            _repoSite = new EfGenericRepository<Site>(context);
            _repoSpecialty = new EfGenericRepository<Specialty>(context);
            _repoSystemConfig = new EfGenericRepository<SystemConfig>(context);
            _repoSystemNotification = new EfGenericRepository<SystemNotification>(context);
            _repoUser = new EfGenericRepository<User>(context);
            _repoUserPosition = new EfGenericRepository<UserPosition>(context);
            _repoUserRole = new EfGenericRepository<UserRole>(context);
            _repoUserSite = new EfGenericRepository<UserSite>(context);

            _repoPackageGroup = new EfGenericRepository<PackageGroup>(context);
            _repoPackage = new EfGenericRepository<Package>(context);
            _repoPackagePrice = new EfGenericRepository<PackagePrice>(context);
            _repoPackagePriceSite = new EfGenericRepository<PackagePriceSite>(context);
            _repoPackagePriceDetail = new EfGenericRepository<PackagePriceDetail>(context);

            _repoServiceGroup = new EfGenericRepository<ServiceGroup>(context);
            _repoServiceCategory = new EfGenericRepository<ServiceCategory>(context);
            _repoService = new EfGenericRepository<Service>(context);
            _repoServiceInPackage = new EfGenericRepository<ServiceInPackage>(context);
            _repoServiceFreeInPackage = new EfGenericRepository<ServiceFreeInPackage>(context);

            _repoPatientInformation = new EfGenericRepository<PatientInformation>(context);
            _repoPatientInPackage = new EfGenericRepository<PatientInPackage>(context);
            _repoPatientInPackageChild = new EfGenericRepository<PatientInPackageChild>(context);
            _repoPatientInPackageDetail = new EfGenericRepository<PatientInPackageDetail>(context);

            _repoHISCharge = new EfGenericRepository<HISCharge>(context);
            _repoHISChargeDetail = new EfGenericRepository<HISChargeDetail>(context);


            _repoTemp_PackageGroup = new EfGenericRepository<Temp_PackageGroup>(context);
            _repoTemp_Package = new EfGenericRepository<Temp_Package>(context);
            _repoTemp_ServiceInPackage = new EfGenericRepository<Temp_ServiceInPackage>(context);
            _repoTemp_PatientInPackage = new EfGenericRepository<Temp_PatientInPackage>(context);
            _repoTemp_ServiceUsing = new EfGenericRepository<Temp_ServiceUsing>(context);
            _repoTemp_ServiceUsingNotCharge = new EfGenericRepository<Temp_ServiceUsingNotCharge>(context);
            _repoTemp_NetAmountLessThanBase = new EfGenericRepository<Temp_NetAmountLessThanBase>(context);
            _repoTemp_UpdateOriginalPrice = new EfGenericRepository<Temp_UpdateOriginalPrice>(context);
        }
        #endregion

        #region General IGenericRepository
        public IGenericRepository<AppConstant> AppConstantRepository => _repoAppConstant;
        public IGenericRepository<Action> ActionRepository => _repoAction;
        public IGenericRepository<Department> DepartmentRepository => _repoDepartment;
        public IGenericRepository<Log> LogRepository => _repoLog;
        public IGenericRepository<LogInFail> LogInFailRepository => _repoLogInFail;
        public IGenericRepository<LogAction> LogActionRepository => _repoLogAction;
        public IGenericRepository<Position> PositionRepository => _repoPosition;
        public IGenericRepository<Role> RoleRepository => _repoRole;
        public IGenericRepository<Module> ModuleRepository => _repoModule;
        public IGenericRepository<GroupAction> GroupActionRepository => _repoGroupAction;
        public IGenericRepository<GroupAction_Map> GroupAction_MapRepository => _repoGroupAction_Map;
        public IGenericRepository<RoleGroupAction> RoleGroupActionRepository => _repoRoleGroupAction;
        public IGenericRepository<Site> SiteRepository => _repoSite;
        public IGenericRepository<Specialty> SpecialtyRepository => _repoSpecialty;
        public IGenericRepository<SystemConfig> SystemConfigRepository => _repoSystemConfig;
        public IGenericRepository<SystemNotification> SystemNotificationRepository => _repoSystemNotification;
        public IGenericRepository<User> UserRepository => _repoUser;
        public IGenericRepository<UserPosition> UserPositionRepository => _repoUserPosition;
        public IGenericRepository<UserRole> UserRoleRepository => _repoUserRole;
        public IGenericRepository<UserSite> UserSiteRepository => _repoUserSite;
        public IGenericRepository<PackageGroup> PackageGroupRepository => _repoPackageGroup;
        public IGenericRepository<Package> PackageRepository => _repoPackage;
        #region Price Policy setting
        public IGenericRepository<PackagePrice> PackagePriceRepository => _repoPackagePrice;
        public IGenericRepository<PackagePriceSite> PackagePriceSiteRepository => _repoPackagePriceSite;
        public IGenericRepository<PackagePriceDetail> PackagePriceDetailRepository => _repoPackagePriceDetail;
        #endregion .Price Policy setting
        #region Service
        public IGenericRepository<ServiceGroup> ServiceGroupRepository => _repoServiceGroup;
        public IGenericRepository<ServiceCategory> ServiceCategoryRepository => _repoServiceCategory;
        public IGenericRepository<Service> ServiceRepository => _repoService;
        public IGenericRepository<ServiceInPackage> ServiceInPackageRepository => _repoServiceInPackage;
        public IGenericRepository<ServiceFreeInPackage> ServiceFreeInPackageRepository => _repoServiceFreeInPackage;
        #endregion .Service
        #region Patient & Package manager & setting
        public IGenericRepository<PatientInformation> PatientInformationRepository => _repoPatientInformation;
        public IGenericRepository<PatientInPackage> PatientInPackageRepository => _repoPatientInPackage;
        public IGenericRepository<PatientInPackageChild> PatientInPackageChildRepository => _repoPatientInPackageChild;
        public IGenericRepository<PatientInPackageDetail> PatientInPackageDetailRepository => _repoPatientInPackageDetail;
        #endregion .Patient & Package manager & setting

        #region Charge & His data
        public IGenericRepository<HISCharge> HISChargeRepository => _repoHISCharge;
        public IGenericRepository<HISChargeDetail> HISChargeDetailRepository => _repoHISChargeDetail;
        #endregion .Charge & His data
        #region Temp table
        public IGenericRepository<Temp_PackageGroup> Temp_PackageGroupRepository => _repoTemp_PackageGroup;
        public IGenericRepository<Temp_Package> Temp_PackageRepository => _repoTemp_Package;
        public IGenericRepository<Temp_ServiceInPackage> Temp_ServiceInPackageRepository => _repoTemp_ServiceInPackage;
        public IGenericRepository<Temp_PatientInPackage> Temp_PatientInPackageRepository => _repoTemp_PatientInPackage;
        public IGenericRepository<Temp_ServiceUsing> Temp_ServiceUsingRepository => _repoTemp_ServiceUsing;
        public IGenericRepository<Temp_ServiceUsingNotCharge> Temp_ServiceUsingNotChargeRepository => _repoTemp_ServiceUsingNotCharge;
        public IGenericRepository<Temp_NetAmountLessThanBase> Temp_NetAmountLessThanBaseRepository => _repoTemp_NetAmountLessThanBase;
        public IGenericRepository<Temp_UpdateOriginalPrice> Temp_UpdateOriginalPriceRepository => _repoTemp_UpdateOriginalPrice;
        #endregion .Temp table
        #endregion

        public void Commit()
        {
            context.SaveChanges();
        }

        public DataTable ExecStore(string store_name, Dictionary<string,string> param = null)
        {
            DataSet retVal = new DataSet();
            string strCmd = $"[dbo].[{store_name}]";
            SqlConnection sqlConn = (SqlConnection)context.Database.Connection;
            SqlCommand cmd = new SqlCommand(strCmd, sqlConn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            using (cmd)
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if(param != null)
                    foreach(var key in param.Keys)
                        cmd.Parameters.Add(new SqlParameter(key, param[key]));
                da.Fill(retVal);
            }
            return retVal?.Tables[0];
        }
    }
}
