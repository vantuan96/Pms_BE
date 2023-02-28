using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System;
using Microsoft.Owin.Security.Cookies;
using System.Collections.Generic;
using DataAccess.Repository;
using VM.Common;
using System.Globalization;
using System.Configuration;
using Microsoft.Owin.Security.DataHandler;
using System.Net;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Infrastructure;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using System.Web.Mvc;
using System.Web;
using Microsoft.Owin.Security.DataProtection;

[assembly: OwinStartup(typeof(PMS.Startup))]

namespace PMS
{
    public class Startup
    {
        private static string redirectUri = ConfigurationManager.AppSettings["RedirectUri"];
        private static string clientId = ConfigurationManager.AppSettings["ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["PostLogoutRedirectUri"];
        private static string cookieName = CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationType;
        private static string metadata = string.Format("{0}/{1}/federationmetadata/2007-06/federationmetadata.xml", aadInstance, tenant);
        string authority = string.Format("{0}{1}", aadInstance, tenant);
        public static readonly string Authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        public static string PostLogoutRedirectUri
        {
            get { return postLogoutRedirectUri; }
        }

        public static string AADInstance
        {
            get { return aadInstance; }
        }

        public static string ClientId
        {
            get { return clientId; }
        }

        public static string CheckSessionIFrame
        {
            get;
            set;
        }

        public static string RedirectUri
        {
            get { return redirectUri; }
        }

        public static TicketDataFormat ticketDataFormat
        {
            get;
            set;
        }
        public static string CookieName
        {
            get { return cookieName; }
        }


        public void Configuration(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3; // only allow TLSV1.2 and SSL3
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            CookieAuthenticationOptions cookieOptions = new CookieAuthenticationOptions
            {
                CookieName = CookieName,
            };
            app.UseOpenIdConnectAuthentication(
      new OpenIdConnectAuthenticationOptions
      {
          // Sets the ClientId, authority, RedirectUri as obtained from web.config
          ClientId = ClientId,
          Authority = Authority,
          RedirectUri = RedirectUri,
          // PostLogoutRedirectUri is the page that users will be redirected to after sign-out. In this case, it is using the home page
          PostLogoutRedirectUri = PostLogoutRedirectUri,
          //TokenValidationParameters = new TokenValidationParameters
          //{
          //    // NOTE: Not Good Practice. See https://github.com/AzureADSamples/WebApp-MultiTenant-OpenIdConnect-DotNet
          //    // for proper issues validation in a multi-tenant app.
          //    ValidateIssuer = false,
          //},
          // OpenIdConnectAuthenticationNotifications configures OWIN to send notification of failed authentications to OnAuthenticationFailed method
          Notifications =
          new OpenIdConnectAuthenticationNotifications
          {
              AuthorizationCodeReceived = Startup.AuthorizationCodeRecieved,
              AuthenticationFailed = Startup.AuthenticationFailed,
              RedirectToIdentityProvider = Startup.RedirectToIdentityProvider,
              SecurityTokenValidated = Startup.SecurityTokenValidated,
          },
      }
  );

            // Enable CORS
            if (ConfigurationManager.AppSettings["HiddenError"].Equals("false"))
                app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            //Cookie authenicate
            var session_timeout = Int32.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = "ApplicationCookie",
                CookieName = string.Format("{0}ies", ConfigHelper.AppKey),
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(session_timeout),
                Provider = new CookieAuthenticationProvider
                {
                    OnResponseSignedIn = context =>
                    {
                        var cookies = context.Response.Headers.GetCommaSeparatedValues("Set-Cookie");
                        var cookieValue = GetAuthenCookie(cookies);

                        if (!string.IsNullOrEmpty(cookieValue))
                            UpdateSession(context.Identity.Name, cookieValue);
                    }
                }
            });
            ////Log middleware
            //app.Use(typeof(LogChangeMiddleware));
            IDataProtector dataProtector = app.CreateDataProtector(
            typeof(CookieAuthenticationMiddleware).FullName,
            cookieOptions.AuthenticationType, "v1");
            ticketDataFormat = new TicketDataFormat(dataProtector);
       

      
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }

        private async static Task SecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            OpenIdConnectAuthenticationOptions tenantSpecificOptions = new OpenIdConnectAuthenticationOptions();
            tenantSpecificOptions.Authority = string.Format(aadInstance, notification.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value);
            tenantSpecificOptions.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(tenantSpecificOptions.Authority + "/.well-known/openid-configuration");

            OpenIdConnectConfiguration tenantSpecificConfig = await tenantSpecificOptions.ConfigurationManager.GetConfigurationAsync(notification.Request.CallCancelled);
            notification.AuthenticationTicket.Identity.AddClaim(new Claim("issEndpoint", tenantSpecificConfig.AuthorizationEndpoint, ClaimValueTypes.String, "PMS"));

            CheckSessionIFrame = notification.AuthenticationTicket.Properties.Dictionary[OpenIdConnectSessionProperties.CheckSessionIFrame];
            return;
        }


        public static Task AuthorizationCodeRecieved(AuthorizationCodeReceivedNotification notification)
        {
            // If the successful authorize request was issued by the SingleSignOut javascript
            if (notification.AuthenticationTicket.Properties.RedirectUri.Contains("SessionChanged"))
            {
                // Clear the SingleSignOut Cookie
                ICookieManager cookieManager = new ChunkingCookieManager();
                string cookie = cookieManager.GetRequestCookie(notification.OwinContext, CookieName);
                AuthenticationTicket ticket = ticketDataFormat.Unprotect(cookie);
                if (ticket.Properties.Dictionary != null)
                    ticket.Properties.Dictionary[OpenIdConnectAuthenticationDefaults.AuthenticationType + "SingleSignOut"] = "";
                cookieManager.AppendResponseCookie(notification.OwinContext, CookieName, ticketDataFormat.Protect(ticket), new CookieOptions());

                Claim existingUserObjectId = notification.OwinContext.Authentication.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");
                Claim incomingUserObjectId = notification.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier");

                if (existingUserObjectId.Value != null && incomingUserObjectId != null)
                {
                    // If a different user is logged into AAD
                    if (existingUserObjectId.Value != incomingUserObjectId.Value)
                    {
                        // No need to clear the session state here. It has already been
                        // updated with the new user's session state in SecurityTokenValidated.
                        notification.Response.Redirect("Account/SingleSignOut");
                        notification.HandleResponse();
                    }
                    // If the same user is logged into AAD
                    else if (existingUserObjectId.Value == incomingUserObjectId.Value)
                    {
                        // No need to clear the session state, SecurityTokenValidated will do so.
                        // Simply redirect the iframe to a page other than SingleSignOut to reset
                        // the timer in the javascript.
                        notification.Response.Redirect("/");
                        notification.HandleResponse();
                    }
                }
            }

            return Task.FromResult<object>(null);
        }


        private static Task AuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            string cookieStateValue = null;
            ICookieManager cookieManager = new ChunkingCookieManager();
            string cookie = cookieManager.GetRequestCookie(notification.OwinContext, CookieName);
            AuthenticationTicket ticket = ticketDataFormat.Unprotect(cookie);
            if (ticket?.Properties.Dictionary != null)
                ticket.Properties.Dictionary.TryGetValue(OpenIdConnectAuthenticationDefaults.AuthenticationType + "SingleSignOut", out cookieStateValue);

            // If the failed authentication was a result of a request by the SingleSignOut javascript
            if (cookieStateValue != null && cookieStateValue.Contains(notification.ProtocolMessage.State) && notification.Exception.Message == "login_required")
            {
                // Clear the SingleSignOut cookie, and clear the OIDC session state so 
                //that we don't see any further "Session Changed" messages from the iframe.
                ticket.Properties.Dictionary[OpenIdConnectSessionProperties.SessionState] = "";
                ticket.Properties.Dictionary[OpenIdConnectAuthenticationDefaults.AuthenticationType + "SingleSignOut"] = "";
                cookieManager.AppendResponseCookie(notification.OwinContext, CookieName, ticketDataFormat.Protect(ticket), new CookieOptions());

                notification.Response.Redirect("Authen/SingleSignOut");
                notification.HandleResponse();
            }

            else
            {
                notification.Response.Redirect("Authen/Login");
                notification.HandleResponse();
            }
            //notification.Response.Redirect("Authen/Login");
            //notification.HandleResponse();
            return Task.FromResult<object>(null);

        }

        public static Task RedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        
       {
            // If a challenge was issued by the SingleSignOut javascript
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            if (notification.Request.Uri.AbsolutePath == url.Action("SessionChanged", "Authen"))
            {

                // Store the state in the cookie so we can distinguish OIDC messages that occurred
                // as a result of the SingleSignOut javascript.
                ICookieManager cookieManager = new ChunkingCookieManager();
                string cookie = cookieManager.GetRequestCookie(notification.OwinContext, CookieName);

                AuthenticationTicket ticket = ticketDataFormat.Unprotect(cookie);
                if (ticket?.Properties.Dictionary != null)
                {
                    ticket.Properties.Dictionary[OpenIdConnectAuthenticationDefaults.AuthenticationType + "SingleSignOut"] = notification.ProtocolMessage.State;
                    cookieManager.AppendResponseCookie(notification.OwinContext, CookieName, ticketDataFormat.Protect(ticket), new CookieOptions());


                }
                // Return prompt=none request (to tenant specific endpoint) to SessionChanged controller.
                notification.ProtocolMessage.Prompt = "none";
                notification.ProtocolMessage.IssuerAddress = notification.OwinContext.Authentication.User.FindFirst("issEndpoint").Value;

                string redirectUrl = notification.ProtocolMessage.BuildRedirectUrl();
                notification.Response.Redirect(url.Action("SignOut", "Authen") + "?" + redirectUrl);
                notification.HandleResponse();
            }

            return Task.FromResult<object>(null);
        }


        private string GetAuthenCookie(IList<string> cookies)
        {
            var cookieValue = "";

            foreach (var cookie in cookies)
            {
                var cookieKeyIndex = cookie.IndexOf(string.Format("{0}ies", ConfigHelper.AppKey));
                if (cookieKeyIndex != -1)
                {
                    cookieValue = cookie.Substring(string.Format("{0}ies", ConfigHelper.AppKey).Length + 1);
                    break;
                }
            }
            return cookieValue;
        }
        private void UpdateSession(string username, string cookieValue)
        {
            try
            {
                //VM.Common.CustomLog.accesslog.Error(string.Format("Begin UpdateSession"));
                using (var unitOfWork = new EfUnitOfWork())
                {
                    var user = unitOfWork.UserRepository.FirstOrDefault(m => !m.IsDeleted && m.Username == username);
                    user.SessionId = cookieValue.Substring(0, 20);
                    user.Session = cookieValue;
                    unitOfWork.UserRepository.Update(user);
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("UpdateSession fail. Ex: {0}", ex));
            }
        }
    }
}
