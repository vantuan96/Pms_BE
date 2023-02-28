using DrFee.Common;
using System;
using System.Collections.Generic;

namespace DrFee.Models.AdminModels
{
    public class ConfigRevenuePercentModel : PagingParameterModel
    {
        public string ConfigName { get; set; }
        public string Name { get; set; }
        public double ChargePercent { get; set; }
        public double ChargePackagePercent { get; set; }
        public double OperationPercent { get; set; }
        public double OperationPackagePercent { get; set; }
        public bool IsHealthCheck { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public List<ServiceShortViewModel> Details { get; set; }

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
    }

    public class ConfigRevenuePercentAllModel: ConfigRevenuePercentModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? HISCode { get; set; }
        public string Groups { get; set; }
        public string Categories { get; set; }
        public bool? IsCalculated { get; set; }

        public string GetFormatedCode()
        {
            return Code.Trim().ToLower();
        }
        public string GetFormatedName()
        {
            return Name.Trim().ToLower();
        }
        public List<Guid?> GetGroups()
        {
            string[] id = Groups.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public List<Guid?> GetCategories()
        {
            string[] id = Categories.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
    }
}