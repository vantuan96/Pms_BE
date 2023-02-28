using DataAccess.Models;
using DataAccess.MSSQL;
using DataAccess.Repository;
using PMS.Contract.Models.AdminModels;
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
    public class HisRevenueRepo : BaseRepository
    {

        #region Initialization
        private HisRevenueDAC _Repository;
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


        public HisRevenueRepo()
        {
            disposedValue = false;
            ConnectionString = ConfigHelper.GetDefaultConnectionString();
            CacheKey = "HisRevenueRepo";
        }

        public HisRevenueRepo(string sConnectionString)
        {
            disposedValue = false;
            ConnectionString = sConnectionString;
            CacheKey = "HisRevenueRepo";
        }

        public HisRevenueDAC Store
        {
            get
            {
                if (null == _Repository)
                {
                    _Repository = new HisRevenueDAC(ConfigHelper.GetDefaultConnectionString());
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
        /// Delete Ads Account
        /// </summary>
        /// <param name="iAccountId"></param>
        /// <returns></returns>
        public bool Delete(int iAccountId)
        {
            return Store.Delete(iAccountId);
        }

        ///// <summary>
        ///// Get Ads Account information
        ///// </summary>        
        //public AdsAccountEntity GetAccount(int iAccountId)
        //{
        //    AdsAccountEntity account = null;
        //    IDataReader myReader = AccountStore.GetById(iAccountId);
        //    if (myReader.Read())
        //    {
        //        account = new AdsAccountEntity();
        //        AdsAccountEntity.FillDataFromReader(account, myReader);
        //    }
        //    myReader.Close();
        //    return account;
        //}
        public List<Temp_ServiceUsing> GetServiceUsingForConfirmApplyPackage(int numberGet)
        {
            using (IUnitOfWork unitOfWorkLocal = new EfUnitOfWork())
            {
                var listEntity = unitOfWorkLocal.Temp_ServiceUsingRepository.Find(x => Constant.NEED_PROCESS_REVENUE_STATUS.Contains(x.StatusForProcess) && x.UsingNumber>0).OrderBy(y => y.NextProcessTime).Take(numberGet).ToList();
                if (listEntity != null)
                {
                    listEntity.ForEach(a => a.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_KEPT4PROCESS"]);
                    unitOfWorkLocal.Commit();

                    return listEntity;
                }
                return null;
            }
        }
        /// <returns></returns>
        public List<HisChargeRevenueModel> GetHISCharge4UpdateDims(string fromDate, string toDate)
        {
            string key = String.Format(CacheKey + "_GetHISCharge4UpdateDims_{0}_{1}", fromDate, toDate);

            //if (EnableCaching && Cache[key] != null)
            //{
            //    return (List<HISRevenueModel>)Cache[key];
            //}

            var listEntity = new List<HisChargeRevenueModel>();
            IDataReader reader = Store.GetHISCharge4UpdateDims(fromDate, toDate);
            if (reader == null)
            {
                return null;
            }
            while (reader.Read())
            {
                var entity = new HisChargeRevenueModel();
                FillDataFromReaderV2(entity, reader);
                listEntity.Add(entity);
            }
            reader.Close();

            //if (EnableCaching)
            //{
            //    CacheData(key, listEntity, CacheDuration);
            //}
            return listEntity;
        }
        public List<HISRevenueModel> GetHISRevenue4Calculate_VMHC(string visitTypeList, string sStatus4Process, int iTakeCount)
        {
            string key = String.Format(CacheKey + "_GetHISRevenue4Calculate_VMHC_{0}_{1}_{2}", visitTypeList, sStatus4Process, iTakeCount.ToString());

            //if (EnableCaching && Cache[key] != null)
            //{
            //    return (List<HISRevenueModel>)Cache[key];
            //}

            var listEntity = new List<HISRevenueModel>();
            IDataReader reader = Store.GetHISRevenue4Calculate_VMHC(visitTypeList, sStatus4Process, iTakeCount);
            if (reader == null)
            {
                return null;
            }
            while (reader.Read())
            {
                var entity = new HISRevenueModel();
                FillDataFromReader(entity, reader);
                listEntity.Add(entity);
            }
            reader.Close();

            //if (EnableCaching)
            //{
            //    CacheData(key, listEntity, CacheDuration);
            //}
            return listEntity;
        }
        /// <summary>
        /// Get All Ads Account from Parrent account
        /// </summary>
        /// <param name="iChanneltype"></param>
        /// <param name="sParrentId"></param>
        /// <param name="iStatus"></param>
        /// <returns></returns>
        //public List<AdsAccountEntity> GetAllFromParrent(int iChanneltype, string sParrentId, int iStatus)
        //{
        //    string key = String.Format(CacheKey + "_GetAllFromParrent_{0}_{1}_{2}", iChanneltype, sParrentId, iStatus);

        //    if (EnableCaching && Cache[key] != null)
        //    {
        //        return (List<AdsAccountEntity>)Cache[key];
        //    }

        //    var listEntity = new List<AdsAccountEntity>();
        //    IDataReader reader = AccountStore.GetAllFromParrent(iChanneltype, sParrentId, iStatus);
        //    if (reader == null)
        //    {
        //        return null;
        //    }
        //    while (reader.Read())
        //    {
        //        var entity = new AdsAccountEntity();
        //        AdsAccountEntity.FillDataFromReader(entity, reader);
        //        listEntity.Add(entity);
        //    }
        //    reader.Close();

        //    if (EnableCaching)
        //    {
        //        CacheData(key, listEntity, CacheDuration);
        //    }
        //    return listEntity;
        //}
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
            entity.OldChargeId = RecordHelper.GetString(reader, "OldChargeId", string.Empty);
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
            entity.ProcessNumber = RecordHelper.GetInt(reader, "ProcessNumber", 0);
        }
        public void FillDataFromReaderV2(HisChargeRevenueModel entity, IDataReader reader)
        {
            entity.ChargeId = RecordHelper.GetGuidVSSQL(reader, "ChargeId", Guid.Empty);
            entity.InPackageType = RecordHelper.GetInt(reader, "InPackageType", 0);
            entity.PackageCode = RecordHelper.GetString(reader, "PackageCode", string.Empty);
            entity.GroupPackageCode = RecordHelper.GetString(reader, "GroupPackageCode", string.Empty);
        }
        #endregion
    }
}
