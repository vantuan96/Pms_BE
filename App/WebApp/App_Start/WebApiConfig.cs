using PMS.Filters;
using System.Configuration;
using System.Web.Http;

namespace PMS
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors();
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            if (ConfigurationManager.AppSettings["HiddenError"].Equals("true"))
            {
                config.Filters.Add(new CustomExceptionFilter());
                config.MessageHandlers.Add(new CustomModifyingErrorMessageDelegatingHandler());
            }
        }
    }
}
