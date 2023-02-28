using PMS.Authentication;
using PMS.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace PMS.Controllers.AdminControllers
{
    [SessionAuthorize]
    public class AdminPositionController: BaseApiController
    {
        [HttpGet]
        [Route("admin/Position")]
        [Permission()]
        public IHttpActionResult GetListPositionAPI()
        {
            var positions = unitOfWork.PositionRepository.AsQueryable().Select(e => new { e.Id, e.ViName, e.EnName});
            return Content(HttpStatusCode.OK, positions);
        }
    }
}