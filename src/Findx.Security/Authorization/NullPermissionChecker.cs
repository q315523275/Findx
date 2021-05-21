using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    public class NullPermissionChecker : IPermissionChecker
    {
        public Task<bool> IsGrantedAsync(IPrincipal principal, HttpContext httpContext)
        {
            return Task.FromResult(true);
        }
    }
}
