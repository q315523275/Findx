using System;
using System.Security.Claims;

namespace Findx.Security
{
    public class CurrentUser : ICurrentUser
    {
        public bool IsAuthenticated => throw new NotImplementedException();

        public string UserId => throw new NotImplementedException();

        public string UserName => throw new NotImplementedException();

        public string PhoneNumber => throw new NotImplementedException();

        public bool PhoneNumberVerified => throw new NotImplementedException();

        public string Email => throw new NotImplementedException();

        public bool EmailVerified => throw new NotImplementedException();

        public string TenantId => throw new NotImplementedException();

        public string[] Roles => throw new NotImplementedException();

        public Claim FindClaim(string claimType)
        {
            throw new NotImplementedException();
        }

        public Claim[] FindClaims(string claimType)
        {
            throw new NotImplementedException();
        }

        public Claim[] GetAllClaims()
        {
            throw new NotImplementedException();
        }

        public bool IsInRole(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
