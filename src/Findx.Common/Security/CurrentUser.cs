using Findx.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Findx.Security
{
    public class CurrentUser : ICurrentUser, ITransientDependency
    {
        private readonly IPrincipal _principal;

        public CurrentUser(IPrincipal principal)
        {
            _principal = principal;
        }

        public bool IsAuthenticated => _principal?.Identity?.IsAuthenticated ?? false;

        public string UserId => _principal?.Identity?.GetUserId();

        public string UserName => this.FindClaimValue(ClaimTypes.UserName);

        public string Name => this.FindClaimValue(ClaimTypes.Name);

        public string PhoneNumber => this.FindClaimValue(ClaimTypes.PhoneNumber);

        public bool PhoneNumberVerified => string.Equals(this.FindClaimValue(ClaimTypes.PhoneNumberVerified), "true", StringComparison.InvariantCultureIgnoreCase);

        public string Email => this.FindClaimValue(ClaimTypes.Email);

        public bool EmailVerified => string.Equals(this.FindClaimValue(ClaimTypes.EmailVerified), "true", StringComparison.InvariantCultureIgnoreCase);

        public string TenantId => _principal?.Identity?.GetClaimValueFirstOrDefault(ClaimTypes.TenantId);

        public IEnumerable<string> Roles => FindClaims(ClaimTypes.Role).Select(c => c.Value).Distinct();




        public Claim FindClaim(string claimType)
        {
            return _principal?.Identity?.GetClaimFirstOrDefault(claimType);
        }

        public IEnumerable<Claim> FindClaims(string claimType)
        {
            return _principal?.Identity?.GetClaims(claimType);
        }

        public IEnumerable<Claim> GetAllClaims()
        {
            if (_principal.Identity is not ClaimsIdentity claimsIdentity)
            {
                return new Claim[0];
            }
            return claimsIdentity?.Claims;
        }

        public bool IsInRole(string roleName)
        {
            return _principal?.Identity?.GetClaimValues(ClaimTypes.Role)?.Any(c => c == roleName) ?? false;
        }
    }
}
