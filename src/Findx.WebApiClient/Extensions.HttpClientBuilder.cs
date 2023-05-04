using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Findx.WebApiClient;

/// <summary>
///     HttpClientBuilder弹性构建扩展
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///     添加超时策略
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="seconds">秒</param>
    /// <returns></returns>
    internal static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder httpClientBuilder, int seconds)
    {
        Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
        if (seconds < 1) return httpClientBuilder;

        return httpClientBuilder.AddPolicyHandler(
            Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(seconds)));
    }

    /// <summary>
    ///     添加重试策略
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="retryCount">重试次数</param>
    /// <returns></returns>
    internal static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder httpClientBuilder, int retryCount)
    {
        Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));

        if (retryCount < 1) return httpClientBuilder;

        httpClientBuilder.AddTransientHttpErrorPolicy(builder =>
        {
            return builder.Or<TaskCanceledException>()
                .Or<OperationCanceledException>()
                .Or<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .OrResult(res => res.StatusCode == HttpStatusCode.TooManyRequests)
                .RetryAsync(retryCount);
        });

        return httpClientBuilder;
    }

    /// <summary>
    ///     添加熔断策略
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="exceptionsAllowedBeforeBreaking">熔断前连续错误次数</param>
    /// <param name="durationOfBreak">熔断时长,如:30s</param>
    /// <returns></returns>
    internal static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder httpClientBuilder,
        int exceptionsAllowedBeforeBreaking, string durationOfBreak)
    {
        Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
        if (exceptionsAllowedBeforeBreaking < 1 || durationOfBreak.IsNullOrWhiteSpace()) return httpClientBuilder;

        var BreakTimeSpan = Time.ToTimeSpan(durationOfBreak);

        httpClientBuilder.AddTransientHttpErrorPolicy(build =>
        {
            return build.Or<HttpRequestException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .OrResult(rsp =>
                    rsp.StatusCode == HttpStatusCode.InternalServerError ||
                    rsp.StatusCode == HttpStatusCode.RequestTimeout)
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking,
                    BreakTimeSpan,
                    (res, ts) =>
                    {
                        var logger = ServiceLocator.GetService<ILoggerFactory>()
                            ?.CreateLogger("Findx.AspNetCore.Extensions");
                        logger?.LogInformation(
                            $"{DateTime.Now}-断路器即将熔断{ts.TotalSeconds}秒,原因:{res?.Exception?.Message}");
                    },
                    () =>
                    {
                        var logger = ServiceLocator.GetService<ILoggerFactory>()
                            ?.CreateLogger("Findx.AspNetCore.Extensions");
                        logger?.LogInformation($"{DateTime.Now}-断路器熔断重置");
                    },
                    () =>
                    {
                        var logger = ServiceLocator.GetService<ILoggerFactory>()
                            ?.CreateLogger("Findx.AspNetCore.Extensions");
                        logger?.LogInformation($"{DateTime.Now}-断路器半开启");
                    });
        });

        return httpClientBuilder;
    }

    /// <summary>
    ///     添加降级策略
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="fallbackRspObj"></param>
    /// <param name="fallbackRspStatus"></param>
    /// <returns></returns>
    internal static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder,
        object fallbackRspObj, int fallbackRspStatus)
    {
        Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
        Check.NotNull(fallbackRspObj, nameof(fallbackRspObj));
        if (fallbackRspStatus < 1) return httpClientBuilder;

        if (!(fallbackRspObj is string fallbackRspResult))
            fallbackRspResult = fallbackRspObj.ToJson();

        httpClientBuilder.AddTransientHttpErrorPolicy(build =>
        {
            return build.Or<HttpRequestException>()
                .Or<BrokenCircuitException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .OrResult(rsp =>
                    rsp.StatusCode == HttpStatusCode.InternalServerError ||
                    rsp.StatusCode == HttpStatusCode.RequestTimeout)
                .FallbackAsync(
                    cancellationToken =>
                    {
                        return Task.FromResult(new HttpResponseMessage
                        {
                            Content = new StringContent(fallbackRspResult),
                            StatusCode = (HttpStatusCode)fallbackRspStatus
                        });
                    },
                    res =>
                    {
                        var logger = ServiceLocator.GetService<ILoggerFactory>()
                            ?.CreateLogger("Findx.AspNetCore.Extensions");
                        logger?.LogInformation($"{DateTime.Now}-服务开始降级,异常消息：{res?.Exception?.Message}");
                        logger?.LogInformation($"{DateTime.Now}-服务降级内容响应：{fallbackRspResult}");
                        return Task.CompletedTask;
                    });
        });

        return httpClientBuilder;
    }

    /// <summary>
    ///     添加降级策略
    /// </summary>
    /// <param name="httpClientBuilder"></param>
    /// <param name="fallbackAction"></param>
    /// <returns></returns>
    internal static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder,
        Func<CancellationToken, Task<HttpResponseMessage>> fallbackAction)
    {
        Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
        Check.NotNull(fallbackAction, nameof(fallbackAction));

        httpClientBuilder.AddTransientHttpErrorPolicy(build =>
        {
            return build.Or<HttpRequestException>()
                .Or<BrokenCircuitException>()
                .Or<TimeoutException>()
                .Or<TimeoutRejectedException>()
                .OrResult(rsp =>
                    rsp.StatusCode == HttpStatusCode.InternalServerError ||
                    rsp.StatusCode == HttpStatusCode.RequestTimeout)
                .FallbackAsync(
                    async cancellationToken => { return await fallbackAction.Invoke(cancellationToken); },
                    async res =>
                    {
                        var logger = ServiceLocator.GetService<ILoggerFactory>()
                            ?.CreateLogger("Findx.AspNetCore.Extensions");
                        logger?.LogInformation($"{DateTime.Now}-服务开始降级,异常消息：{res?.Exception?.Message}");
                        logger?.LogInformation(
                            $"{DateTime.Now}-服务降级内容响应：{await res?.Result?.Content?.ReadAsStringAsync()}");
                    });
        });

        return httpClientBuilder;
    }
}