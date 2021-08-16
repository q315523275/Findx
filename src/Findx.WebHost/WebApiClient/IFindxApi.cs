using Findx.WebApiClient;
using System.Threading.Tasks;
using WebApiClientCore.Attributes;

namespace Findx.WebHost.WebApiClient
{
    [WebApiClient("http://Findx.WebHost", UseDiscovery = true, Timeout = 1, 
        Retry = 3, FallbackMessage = "降级策略", FallbackStatus = 200, 
        DurationOfBreak = "5s", ExceptionsAllowedBeforeBreaking = 2)]
    public interface IFindxApi
    {
        [HttpGet("/applicationInfo")]
        Task<string> ApplicationInfo();

        [HttpGet("/exception")]
        Task<string> Exception();

        [HttpGet("/exception_timeout")]
        Task<string> Timeout();
    }
}
