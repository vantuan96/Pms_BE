using DataAccess.Models;
using System;
using System.Collections.Generic;
using VM.Common;

namespace PMS.Contract.Models.AdminModels
{
    public class PackagePricePolicyModel
    {
        /// <summary>
        /// Id Package/Gói
        /// </summary>
        public Guid? PackageId { get; set; }
        /// <summary>
        /// Mã chính sách giá
        /// </summary>
        public string Code { get; set; }
        public List<PackagePriceModel> Policy { get; set; }
        public List<PackagePriceDetailModel> Details { get; set; }
        public List<PackagePriceSitesModel> ListSites { get; set; }
    }
    public class PackagePriceModel
    {
        public Guid? Id { get; set; }
        // <summary>
        /// 1: Người Việt Nam
        /// 2: Người Nước ngoài
        /// </summary>
        public int PersonalType { get; set; }
        /// <summary>
        /// Code site để lấy giá cơ sở
        /// </summary>
        public string SiteBaseCode { get; set; }
        /// <summary>
        /// Giá trị ChargeType trên HIS (OH)
        /// </summary>
        public string ChargeType { get; set; }
        /// <summary>
        /// Giá gói
        /// </summary>
        public double? Amount{ get; set; }
        /// <summary>
        /// tungdd14 thêm hệ số markup vaccine
        /// </summary>
        public double? RateINV { get; set; }
        public bool IsLimitedDrugConsum { get; set; }
        public double? LimitedDrugConsumAmount { get; set; }
        public string StartAt { get; set; }
        public DateTime? GetStartAt()
        {
            if (string.IsNullOrEmpty(StartAt))
                return null;
            try
            {
                return DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
    }
    public class PackagePriceDetailModel
    {
        public Guid? ServiceInPackageId { get; set; }
        public Service Service { get; set; }
        public int? Qty { get; set; }
        /// <summary>
        /// Đơn giá cơ sở 4 VN
        /// </summary>
        public double? BasePrice { get; set; }

        /// <summary>
        /// Thành tiền cơ sơ 4 VN
        /// </summary>
        public double? BaseAmount { get; set; }

        /// <summary>
        /// Đơn giá trong gói 4 VN
        /// </summary>
        public double? PkgPrice { get; set; }

        /// <summary>
        /// Thành tiền trong gói 4 VN
        /// </summary>
        public double? PkgAmount { get; set; }

        /// <summary>
        /// Đơn giá cơ sở 4 Foreign
        /// </summary>
        public double? BasePriceForeign { get; set; }

        /// <summary>
        /// Thành tiền cơ sơ 4 Foreign
        /// </summary>
        public double? BaseAmountForeign { get; set; }

        /// <summary>
        /// Đơn giá trong gói 4 Foreign
        /// </summary>
        public double? PkgPriceForeign { get; set; }

        /// <summary>
        /// Thành tiền trong gói 4 Foreign
        /// </summary>
        public double? PkgAmountForeign { get; set; }
        /// <summary>
        /// Là gói Thuốc/VTTH
        public bool IsPackageDrugConsum { get; set; }
        /// <summary>
        /// Loại dịch vụ:
        /// 0: Total/Tổng
        /// 1: Dịch vụ
        /// 2: Thuốc & VTTH
        /// </summary>
        public int ServiceType { get; set; }
        public bool IsFree { get; set; }
    }
    public class PackagePriceSitesModel
    {
        public Guid? SiteId { get; set; }
        public Site Site { get; set; }
        public string PolicyCode { get; set; }
        public double? PkgAmount { get; set; }
        public double? PkgAmountForeign { get; set; }
        public string StartAt { get; set; }
        public DateTime? GetStartAt()
        {
            if (string.IsNullOrEmpty(StartAt))
                return null;
            try
            {
                return DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        public string EndAt { get; set; }
        public DateTime? GetEndAt()
        {
            if (string.IsNullOrEmpty(EndAt))
                return null;
            try
            {
                return DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        public dynamic Notes { get; set; }
        public bool IsDeleted { get; set; }
    }
}