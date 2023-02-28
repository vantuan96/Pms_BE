using DataAccess.MSSQL;
using PMS.Contract.Models.ApigwModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;
using VM.Framework.Core;

namespace PMS.Business.Provider
{
    public class ConfigMasterDataRepo : BaseRepository
    {

        #region Initialization
        private ConfigMasterDataDAC _Repository;
        private bool disposedValue;

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            if ((!disposedValue && disposing) && null != _Repository)
            {
                _Repository.Dispose();
            }
            disposedValue = true;
        }


        public ConfigMasterDataRepo()
        {
            disposedValue = false;
            ConnectionString = ConfigHelper.GetDefaultConnectionString();
            CacheKey = "ConfigMasterDataRepo";
        }

        public ConfigMasterDataRepo(string sConnectionString)
        {
            disposedValue = false;
            ConnectionString = sConnectionString;
            CacheKey = "ConfigMasterDataRepo";
        }

        public ConfigMasterDataDAC Store
        {
            get
            {
                if (null == _Repository)
                {
                    _Repository = new ConfigMasterDataDAC(ConfigHelper.GetDefaultConnectionString());
                }
                return _Repository;
            }
            set
            {
                _Repository = value;
            }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// CheckExistIsNotCalculating
        /// </summary>
        /// <param name="serviceCode"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public bool CheckExistIsNotCalculating(string serviceCode, DateTime startDate, DateTime endDate)
        {
            return Store.CheckExistIsNotCalculating(serviceCode, startDate, endDate);
        }
        /// <summary>
        /// HisRevenueIshaveChild
        /// </summary>
        /// <param name="chargeId"></param>
        /// <returns></returns>
        public bool HisRevenueIshaveChild(string chargeId)
        {
            return Store.HisRevenueIshaveChild(chargeId);
        }
        #endregion
        #region Helper fill Entity
        public void FillDataFromReader(HISRevenueModel entity, IDataReader reader)
        {
            entity.HisRevenueId = RecordHelper.GetGuidVSSQL(reader, "Id", Guid.Empty);
            entity.HISCode = RecordHelper.GetInt(reader, "HISCode", 0);
            entity.HospitalId = RecordHelper.GetString(reader, "HospitalId", string.Empty);
            entity.Service = RecordHelper.GetString(reader, "Service", string.Empty);
            entity.ParentChargeId = RecordHelper.GetString(reader, "ParentChargeId", string.Empty);
            entity.ChargeId = RecordHelper.GetString(reader, "ChargeId", string.Empty);
            entity.ChargeSessionId = RecordHelper.GetString(reader, "ChargeSessionId", string.Empty);
            entity.ChargeDoctor = RecordHelper.GetString(reader, "ChargeDoctor", string.Empty);
            entity.ChargeUpdatedDate = RecordHelper.GetDateTime(reader, "ChargeUpdatedAt", DateTime.MinValue);
            entity.ChargeDoctorDepartmentCode = RecordHelper.GetString(reader, "ChargeDoctorDepartmentCode", string.Empty);
            entity.ChargeStatus = RecordHelper.GetInt(reader, "ChargeStatus", null);
            entity.OperationId = RecordHelper.GetString(reader, "OperationId", string.Empty);
            entity.OperationDoctorDepartmentCode = RecordHelper.GetString(reader, "OperationDoctorDepartmentCode", string.Empty);
            entity.OperationDoctor = RecordHelper.GetString(reader, "OperationDoctor", string.Empty);
            entity.OperationCreatedAt = RecordHelper.GetDateTime(reader, "OperationCreatedAt", null);
            entity.CustomerName = RecordHelper.GetString(reader, "CustomerName", string.Empty);
            entity.CustomerPID = RecordHelper.GetString(reader, "CustomerPID", string.Empty);
            entity.PackageCode = RecordHelper.GetString(reader, "PackageCode", string.Empty);
            entity.AmountInPackage = RecordHelper.GetDouble(reader, "AmountInPackage", null);
            entity.IsPackage = RecordHelper.GetBoolean(reader, "IsPackage", false);
            entity.PatientPackageStatus = RecordHelper.GetString(reader, "PatientPackageStatus", string.Empty);
            entity.PatientPackageCancelledDate = RecordHelper.GetDateTime(reader, "PatientPackageCancelledDate", null);
            entity.VisitType = RecordHelper.GetString(reader, "VisitType", string.Empty);
            entity.VisitCode = RecordHelper.GetString(reader, "VisitCode", string.Empty);
            entity.InvoiceNumber = RecordHelper.GetString(reader, "InvoiceNumber", string.Empty);
            entity.ChargeDate = RecordHelper.GetDateTime(reader, "ChargeDate", null);
            entity.InvoiceDate = RecordHelper.GetDateTime(reader, "InvoiceDate", null);
            entity.InvoiceUpdatedAt = RecordHelper.GetDateTime(reader, "InvoiceUpdatedAt", null);
            entity.InvoicePaymentStatus = RecordHelper.GetString(reader, "InvoicePaymentStatus", string.Empty);
            entity.InvoiceId = RecordHelper.GetString(reader, "InvoiceId", string.Empty);
            entity.SpecimenNumber = RecordHelper.GetString(reader, "SpecimenNumber", string.Empty);
        }
        #endregion
    }
}
