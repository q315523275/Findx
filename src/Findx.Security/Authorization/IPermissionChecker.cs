using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Findx.Security.Authorization
{
    public interface IPermissionChecker
    {
        Task<bool> IsGrantedAsync(IPrincipal principal, HttpContext httpContext);
    }
}
