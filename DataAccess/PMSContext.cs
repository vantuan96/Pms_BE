using DataAccess.Migrations;
using DataAccess.Models;
using System.Data.Entity;

namespace DataAccess
{
    public class PMSContext: DbContext
    {
        public PMSContext()
            : base("PMSContext")
        {
            Database.Log = null;
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<PMSContext, Configuration>());
        }

        #region General
        public DbSet<AppConstant> AppConstants { get; set; }
        public DbSet<Action> Actions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<LogInFail> LogInFails { get; set; }
        public DbSet<LogAction> LogActions { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<GroupAction> GroupActions { get; set; }
        public DbSet<GroupAction_Map> GroupAction_Maps { get; set; }
        public DbSet<RoleGroupAction> RoleGroupActions { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        public DbSet<SystemNotification> SystemNotifications { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPosition> UserPositions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<PackageGroup> PackageGroups { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackagePrice> PackagePrices { get; set; }
        public DbSet<PackagePriceSite> PackagePriceSites { get; set; }
        public DbSet<PackagePriceDetail> PackagePriceDetails { get; set; }

        public DbSet<ServiceGroup> ServiceGroups { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceInPackage> ServiceInPackages { get; set; }
        public DbSet<ServiceFreeInPackage> ServiceFreeInPackages { get; set; }

        public DbSet<PatientInformation> PatientInformations { get; set; }
        public DbSet<PatientInPackage> PatientInPackages { get; set; }
        public DbSet<PatientInPackageChild> PatientInPackageChilds { get; set; }
        public DbSet<PatientInPackageDetail> PatientInPackageDetails { get; set; }

        public DbSet<HISCharge> HISCharges { get; set; }
        public DbSet<HISChargeDetail> HISChargeDetails { get; set; }

        #region Temp table
        public DbSet<Temp_PackageGroup> Temp_PackageGroups { get; set; }
        public DbSet<Temp_Package> Temp_Packages { get; set; }
        public DbSet<Temp_ServiceInPackage> Temp_ServiceInPackages { get; set; }
        public DbSet<Temp_PatientInPackage> Temp_PatientInPackages { get; set; }
        public DbSet<Temp_ServiceUsing> Temp_ServiceUsings { get; set; }
        public DbSet<Temp_ServiceUsingNotCharge> Temp_ServiceUsingNotCharges { get; set; }
        public DbSet<Temp_NetAmountLessThanBase> Temp_NetAmountLessThanBases { get; set; }
        public DbSet<Temp_UpdateOriginalPrice> Temp_UpdateOriginalPrices { get; set; }
        #endregion .Temp table
        #endregion
    }
}
