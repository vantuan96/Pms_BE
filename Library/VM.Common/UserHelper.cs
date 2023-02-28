using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM.Common
{
    public static class UserHelper
    {
        public static string CurrentUserName()
        {
            try
            {
                var claims = System.Security.Claims.ClaimsPrincipal.Current.Identities.First().Claims.ToList();
                return claims?.FirstOrDefault(x => x.Type.Equals(System.Security.Claims.ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
