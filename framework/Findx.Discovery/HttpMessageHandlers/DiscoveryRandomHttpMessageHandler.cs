﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Findx.Common;
using Findx.Discovery.Abstractions;
using Findx.Discovery.LoadBalancer;

namespace Findx.Discovery.HttpMessageHandlers;

/// <summary>
/// 随机请求负载计算Http消息处理器
/// </summary>
public class DiscoveryRandomHttpMessageHandler : DelegatingHandler
{
    private readonly ILoadBalancerProvider _loadBalancerManager;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="loadBalancerManager"></param>
    public DiscoveryRandomHttpMessageHandler(ILoadBalancerProvider loadBalancerManager)
    {
        _loadBalancerManager = loadBalancerManager;
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var current = request.RequestUri;
        current.ThrowIfNull();
        
        // ReSharper disable once PossibleNullReferenceException
        var loadBalancer = await _loadBalancerManager.GetAsync(current.Host).ConfigureAwait(false);
        var serviceInfo = await loadBalancer.ResolveServiceEndPointAsync().ConfigureAwait(false);
        request.RequestUri = serviceInfo.ToUri(current.Scheme, current.PathAndQuery);

        var nowTime = DateTime.Now;
        try
        {
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            request.RequestUri = current;
            await loadBalancer.UpdateStatsAsync(serviceInfo, DateTime.Now - nowTime);
        }
    }
}