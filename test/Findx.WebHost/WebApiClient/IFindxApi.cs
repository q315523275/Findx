using System.Threading.Tasks;
using Findx.WebApiClient;
using WebApiClientCore.Attributes;

namespace Findx.WebHost.WebApiClient;

[WebApiClient("http://127.0.0.1:6666", UseDiscovery = false, Timeout = 1,
    Retry = 1, FallbackMessage = "降级策略", FallbackStatus = 200,
    DurationOfBreak = "5s", ExceptionsAllowedBeforeBreaking = 5)]
public interface IFindxApi
{
    [HttpGet("/applicationInfo")]
    Task<string> ApplicationInfo();

    [HttpGet("/exception")]
    Task<string> Exception();

    [HttpGet("/exception/timeout")]
    Task<string> Timeout();
}