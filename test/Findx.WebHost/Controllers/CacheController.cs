using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc;
using Findx.Caching;
using Findx.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Findx.WebHost.Controllers;

/// <summary>
/// 缓存服务
/// </summary>
[Description("缓存服务"), Tags("缓存服务")]
public class CacheController: ApiControllerBase
{
    private readonly ICacheFactory _cacheFactory;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="cacheFactory"></param>
    public CacheController(ICacheFactory cacheFactory)
    {
        _cacheFactory = cacheFactory;
    }

    /// <summary>
    ///     缓存信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("info")]
    public CommonResult InfoAsync()
    {
        var info = _cacheFactory.Create(CacheType.DefaultMemory);
        return CommonResult.Success<string>(info.ToString());
    }
    
    /// <summary>
    ///     应用基础信息
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <returns></returns>
    [HttpGet("get")]
    public async Task<CommonResult> GetAsync(string cacheKey)
    {
        return CommonResult.Success(await _cacheFactory.Create(CacheType.DefaultMemory).GetAsync<object>(cacheKey));
    }
}