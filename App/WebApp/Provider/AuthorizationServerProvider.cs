using PMS.Business.Provider;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PMS.Provider
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult(0);
        }
        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (UserRepo _repo = new UserRepo())
            {
                var user = _repo.ValidateUser(context.UserName);
                if (user == null)
                {
                    context.SetError("invalid_grant", "Provided username and password is incorrect");
                    return Task.FromResult(0);
                }
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Roles));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                context.Validated(identity);
                return Task.FromResult(0);
            }
        }

    }
}