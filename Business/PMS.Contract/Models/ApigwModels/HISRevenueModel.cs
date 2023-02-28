using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models.ApigwModels
{
    public class HISRevenueModel
    {
        public string QueueCode { get; set; }
        private ObjectId _Id;
        [BsonId]
        public ObjectId Id { get { return this._Id; } set { this._Id = value; } }
        public Guid HisRevenueId { get; set; }
        public int HISCode { get; set; }
        public string HospitalId { get; set; }
        public string Service { get; set; }
        public string SpecimenNumber { get; set; }
        public string ParentChargeId { get; set; }
        public string ChargeId { get; set; }
        public string OldChargeId { get; set; }
        public string ChargeSessionId { get; set; }
        public int? ChargeMonth { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ChargeDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ChargeUpdatedDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ChargeRevenueDate { get; set; }
        public int? ChargeStatus { get; set; }
        public string ChargeDoctorDepartmentCode { get; set; }
        public string ChargeDoctor { get; set; }
        public string OperationId { get; set; }
        public int? OperationMonth { get; set; }
        public string OperationDoctorDepartmentCode { get; set; }
        public string OperationDoctor { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPID { get; set; }
        public string PackageCode { get; set; }
        public double? AmountInPackage { get; set; }
        public bool IsPackage { get; set; }
        public string PatientPackageStatus { get; set; }
        public DateTime? PatientPackageCancelledDate { get; set; }
        public string VisitType { get; set; }
        public string VisitCode { get; set; }
        public string CodeTarget { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? InvoiceDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? InvoiceUpdatedAt { get; set; }
        public string InvoicePaymentStatus { get; set; }
        #region Infor/Fields for Operation
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OperationCreatedAt { get; set; }
        #endregion
        #region Fields for process
        public int ChargeRevenuDateType { get; set; }
        public string PackageType { get; set; }
        public int StatusForProcess { get; set; }
        public int ProcessNumber { get; set; }
        public bool IsCalculated { get; set; }
        /// </summary>
        public int count_fail = 0;

        /// <summary>
        /// Number of the time that the message have been sent
        /// </summary>    
        public int process_fail = 0;
        //The last time that the message have an error
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime last_fail = DateTime.MinValue;
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? NextTimeNeedProcess { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public HISConfigRevenuePercentModel ConfigRevenue { get; set; }
        #endregion .Fields for process
        public string GetDepartmentCodeExt()
        {
            try
            {
                var lst = this.ChargeDoctorDepartmentCode.Split('.');
                return lst[1].Length == 3 ? lst[1] : null;
            }
            catch
            {
                return null;
            }
        }
    }
}