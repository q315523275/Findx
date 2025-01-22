using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Findx.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
/// 缓存服务
/// </summary>
[Route("api/cache")]
[Description("缓存服务"), Tags("缓存服务")]
public class CacheController: ApiControllerBase
{
    private readonly ICacheFactory _cacheFactory;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="cacheFactory"></param>
    public CacheController(ICacheFactory cacheFactory)
    {
        _cacheFactory = cacheFactory;
    }

    /// <summary>
    ///     本地缓存服务信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("info")]
    public CommonResult InfoAsync()
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        return CommonResult.Success(cache.ToString());
    }

    /// <summary>
    ///     本地缓存获取
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("get")]
    public async Task<CommonResult> GetAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        var value = await cache.GetAsync<object>(cacheKey, cancellationToken);
        return CommonResult.Success(value);
    }

    /// <summary>
    ///     TryAdd
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("tryAdd")]
    public async Task<CommonResult> TryAddAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var cache = _cacheFactory.Create(CacheType.DefaultMemory);
        var value = SnowflakeIdUtility.Default().NextId();
        var rs = await cache.TryAddAsync(cacheKey, value, cancellationToken);
        var rv = await cache.GetAsync<long>(cacheKey, cancellationToken);
        return CommonResult.Success(rs + "|" + value + "|" + rv);
    }
}