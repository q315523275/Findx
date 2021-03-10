using Microsoft.Extensions.Options;

namespace Findx.Authorization
{
    public class AuthorizationOptions : IOptions<AuthorizationOptions>
    {
        public AuthorizationOptions Value => this;
    }
}
