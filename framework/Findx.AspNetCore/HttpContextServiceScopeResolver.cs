using System;
using System.Collections.Generic;
using Findx.DependencyInjection;
using Findx.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Findx.AspNetCore;

/// <summary>
///     Http请求上下文作用域服务解析器
/// </summary>
public class HttpContextServiceScopeResolver : IScopedServiceResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public HttpContextServiceScopeResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    ///     是否提供解析器
    /// </summary>
    public bool ResolveEnabled => _httpContextAccessor.HttpContext != null;

    /// <summary>
    ///     域内服务提供器
    /// </summary>
    public IServiceProvider ScopedProvider => _httpContextAccessor.HttpContext?.RequestServices;

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetService<T>()
    {
        return _httpContextAccessor.HttpContext != null
            ? _httpContextAccessor.HttpContext.RequestServices.GetService<T>()
            : default;
    }
    
    /// <summary>
    ///     解析服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetService<T>(string name)
    {
        return _httpContextAccessor.HttpContext != null
            ? _httpContextAccessor.HttpContext.RequestServices.GetService<T>(name)
            : default;
    }

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public object GetService(Type serviceType)
    {
        return _httpContextAccessor.HttpContext?.RequestServices.GetService(serviceType);
    }

    /// <summary>
    ///     解析服务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public object GetService(string name, Type serviceType)
    {
        return _httpContextAccessor.HttpContext?.RequestServices.GetService(name, serviceType);
    }

    /// <summary>
    ///     解析服务集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IEnumerable<T> GetServices<T>()
    {
        return _httpContextAccessor.HttpContext?.RequestServices.GetServices<T>();
    }

    /// <summary>
    ///     解析服务集合
    /// </summary>
    /// <param name="serviceType"></param>
    /// <returns></returns>
    public IEnumerable<object> GetServices(Type serviceType)
    {
        return _httpContextAccessor.HttpContext?.RequestServices.GetServices(serviceType);
    }
}