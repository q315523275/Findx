using System;
using System.Collections.Generic;
using Findx.Data;
using Findx.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     区域的WebApi控制器基类
/// </summary>
[ApiController]
[Route("api/[area]/[controller]/[action]")]
public abstract class AreaApiControllerBase : ControllerBase
{
    /// <summary>
    ///     获取或设置 日志对象
    /// </summary>
    protected ILogger Logger => HttpContext.RequestServices.GetLogger(GetType());

    /// <summary>
    ///     获取服务
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    protected TService GetService<TService>()
    {
        return HttpContext.RequestServices.GetService<TService>();
    }

    /// <summary>
    ///     获取服务集合
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    protected IEnumerable<TService> GetServices<TService>()
    {
        return HttpContext.RequestServices.GetServices<TService>();
    }

    /// <summary>
    ///     获取服务并校验
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    protected TService GetRequiredService<TService>()
    {
        return HttpContext.RequestServices.GetRequiredService<TService>();
    }

    /// <summary>
    ///     获取仓储方法
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    protected IRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IEntity<Guid>
    {
        return Request.HttpContext.RequestServices.GetRequiredService<IRepository<TEntity>>();
    }
    
    /// <summary>
    ///     获取仓储方法
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <returns></returns>
    protected IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class, IEntity<TKey>
    {
        return Request.HttpContext.RequestServices.GetRequiredService<IRepository<TEntity, TKey>>();
    }
}