using Findx.Threading;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Findx.AspNetCore
{
    public class HttpContextCancellationTokenProvider : ICancellationTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCancellationTokenProvider(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public CancellationToken Token => _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;
    }
}
