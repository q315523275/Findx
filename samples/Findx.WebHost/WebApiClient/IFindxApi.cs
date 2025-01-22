using System.Threading.Tasks;
using Findx.WebApiClient;
using WebApiClientCore.Attributes;

namespace Findx.WebHost.WebApiClient;

[WebApiClient("http://Findx.WebHost:8888", UseDiscovery = true, Timeout = 0, Retry = 0, FallbackMessage = "降级策略", FallbackStatus = 200, DurationOfBreak = "5s", ExceptionsAllowedBeforeBreaking = 5)]
public interface IFindxApi
{
    [HttpGet("/applicationInfo")]
    Task<string> ApplicationInfo();

    [HttpGet("/exception")]
    Task<string> Exception();

    [HttpGet("/exception/timeout")]
    Task<string> Timeout();
}