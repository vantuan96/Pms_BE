using PMS.Authentication;
using PMS.Business.Connection;
using PMS.Controllers.BaseControllers;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Site management
    /// </summary>
    [SessionAuthorize]
    public class AdminSiteController : BaseApiController
    {
        /// <summary>
        /// API get list site
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Site")]
        [Permission()]
        public IHttpActionResult GetSiteApi()
        {
            var sites = unitOfWork.SiteRepository.AsQueryable().Where(x=>x.IsActived).Select(e => new { e.Id, e.Code,e.ApiCode, Name=e.FullNameL });
            return Content(HttpStatusCode.OK, sites);
        }
        /// <summary>
        /// API Get Site info by site code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/SiteInfo/{code}")]
        [Permission()]
        public IHttpActionResult GetSiteInfoApi(string code)
        {
            var sites = OHConnectionAPI.GetSites(code);
            return Content(HttpStatusCode.OK, sites);
        }
    }
}
