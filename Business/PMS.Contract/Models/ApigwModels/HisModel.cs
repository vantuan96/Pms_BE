using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.ApigwModels
{
    public class ChargeTypeModel
    {
        public string HospitalCode { get; set; }
        public string ChargeTypeCode { get; set; }
        public string ChargeTypeName { get; set; }
    }
    public class ServicePriceModel
    {
        public string ServiceCode { get; set; }
        public double? Price { get; set; }
        public DateTime? EffectTo { get; set; }
        public DateTime? EffectFrom { get; set; }
    }
    public class HISChargeModel
    {
        public Guid? Id { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemCode { get; set; }
        public Guid ChargeId { get; set; }
        public Guid? NewChargeId { get; set; }
        public Guid? ChargeSessionId { get; set; }
        public DateTime? ChargeDate { get; set; }
        public DateTime? ChargeCreatedDate { get; set; }
        public DateTime? ChargeUpdatedDate { get; set; }
        public DateTime? ChargeDeletedDate { get; set; }
        public string ChargeStatus { get; set; }
        public string VisitType { get; set; }
        public string VisitCode { get; set; }
        public DateTime? VisitDate { get; set; }

        #region Invoice info
        public string InvoicePaymentStatus { get; set; }
        #endregion .Invoice info
        public Guid HospitalId { get; set; }
        public string HospitalCode { get; set; }
        #region Customer info basic
        public string PID { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        #endregion .Customer info basic
        public Guid? PatientInPackageId { get; set; }
        #region Price/Amount/Discount information
        public double? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        /// <summary>
        /// 0: Charge on OH
        /// 1: Charge fake
        /// </summary>
        public int ChargeType { get; set; }

        #endregion .Price/Amount/Discount information
        public string PricingClass { get; set; }
        #region properties for Create, Update, Delete info
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        #endregion .properties for Create, Update, Delete info
    }
}
