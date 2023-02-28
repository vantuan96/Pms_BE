namespace PMS.Contract.Models
{
    public class DepartmentParameterModel: PagingParameterModel
    {
        public string SiteCode { get; set; }
        public string Search { get; set; }
    }
}