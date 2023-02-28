using System;
using System.Web;

namespace GAPIT.MKT.Framework.Core
{
    public abstract class BaseHandler : IHttpHandler
    {
        private HttpContext _BaseContext;

        public abstract void ProcessRequest(HttpContext context);

        public HttpContext BaseContext
        {
            get
            {
                return this._BaseContext;
            }
            set
            {
                this._BaseContext = value;             
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return this.BaseContext.Request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return this.BaseContext.Response;
            }
        }

        bool System.Web.IHttpHandler.IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
