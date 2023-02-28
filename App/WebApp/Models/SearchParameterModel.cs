namespace DrFee.Models
{
    public class SearchParameterModel: PagingParameterModel
    {
        public string Search { get; set; }
        public string ServiceCode { get; set; }
        public string GetSearch()
        {
            return Search.Trim().ToLower();
        }
    }
}