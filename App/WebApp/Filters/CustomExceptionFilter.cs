using System;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http.Filters;
using VM.Common;

namespace PMS.Filters
{
    public class CustomExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                var log = new
                {
                    Ip = GetIp(actionExecutedContext),
                    URI = actionExecutedContext.Request.RequestUri.ToString(),
                    Response = actionExecutedContext.Exception.ToString(),
                    Action = "Error"
                };
                CustomLog.errorlog.Error(log);
            }
            catch (Exception) { }

            var message = "Có lỗi xảy ra";
            var status = HttpStatusCode.InternalServerError;
            actionExecutedContext.Response = new HttpResponseMessage()
            {
                Content = new StringContent(message, System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = status
            };
            base.OnException(actionExecutedContext);
        }
        private string GetIp(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)actionExecutedContext.Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (actionExecutedContext.Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)actionExecutedContext.Request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }
    }
}