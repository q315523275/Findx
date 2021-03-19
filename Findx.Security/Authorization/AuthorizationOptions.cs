using Microsoft.Extensions.Options;

namespace Findx.Security.Authorization
{
    public class AuthorizationOptions : IOptions<AuthorizationOptions>
    {
        public AuthorizationOptions Value => this;
    }
}
