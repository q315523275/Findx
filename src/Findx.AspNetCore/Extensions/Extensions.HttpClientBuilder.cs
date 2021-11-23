using Findx.DependencyInjection;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Findx.Extensions
{
    /// <summary>
    /// HttpClientBuilder弹性构建扩展
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// 添加超时策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="seconds">秒</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder httpClientBuilder, int seconds)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (seconds < 1) return httpClientBuilder;

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(seconds);

            httpClientBuilder = httpClientBuilder.AddPolicyHandler(timeoutPolicy);

            return httpClientBuilder;
        }

        /// <summary>
        /// 添加重试策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="retryCount">重试次数</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder httpClientBuilder, int retryCount)
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
        /// 添加熔断策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="exceptionsAllowedBeforeBreaking">熔断前连续错误次数</param>
        /// <param name="durationOfBreak">熔断时长,如:30s</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder httpClientBuilder, int exceptionsAllowedBeforeBreaking, string durationOfBreak)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (exceptionsAllowedBeforeBreaking < 1 || durationOfBreak.IsNullOrWhiteSpace()) return httpClientBuilder;

            var BreakTimeSpan = Findx.Utils.Time.ToTimeSpan(durationOfBreak);

            httpClientBuilder.AddTransientHttpErrorPolicy(build =>
            {
                return build.Or<HttpRequestException>()
                            .Or<TimeoutException>()
                            .Or<TimeoutRejectedException>()
                            .OrResult(rsp => rsp.StatusCode == HttpStatusCode.InternalServerError || rsp.StatusCode == HttpStatusCode.RequestTimeout)
                            .CircuitBreakerAsync(
                                  handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                                  durationOfBreak: BreakTimeSpan,
                                  onBreak: (res, ts) =>
                                  {
                                      var _logger = ServiceLocator.GetService<ILoggerFactory>()?.CreateLogger("Findx.AspNetCore.Extensions");
                                      _logger?.LogInformation($"{DateTime.Now}-断路器即将熔断{ts.TotalSeconds}秒,原因:{res?.Exception?.Message}");
                                  },
                                  onReset: () =>
                                  {
                                      var _logger = ServiceLocator.GetService<ILoggerFactory>()?.CreateLogger("Findx.AspNetCore.Extensions");
                                      _logger?.LogInformation($"{DateTime.Now}-断路器熔断重置");
                                  },
                                  onHalfOpen: () =>
                                  {
                                      var _logger = ServiceLocator.GetService<ILoggerFactory>()?.CreateLogger("Findx.AspNetCore.Extensions");
                                      _logger?.LogInformation($"{DateTime.Now}-断路器半开启");
                                  });
            });

            return httpClientBuilder;
        }

        /// <summary>
        /// 添加降级策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="fallbackRspObj"></param>
        /// <param name="fallbackRspStatus"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder, object fallbackRspObj, int fallbackRspStatus)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            Check.NotNull(fallbackRspObj, nameof(fallbackRspObj));
            if (fallbackRspStatus < 1) return httpClientBuilder;

            var fallbackRspResult = JsonSerializer.Serialize(fallbackRspObj);

            httpClientBuilder.AddTransientHttpErrorPolicy(build =>
            {
                return build.Or<HttpRequestException>()
                            .Or<TimeoutException>()
                            .Or<TimeoutRejectedException>()
                            .Or<BrokenCircuitException>()
                            .OrResult(rsp => rsp.StatusCode == HttpStatusCode.InternalServerError || rsp.StatusCode == HttpStatusCode.RequestTimeout)
                            .FallbackAsync(
                                  fallbackAction: (cancellationToken) =>
                                  {
                                      return Task.FromResult(new HttpResponseMessage { Content = new StringContent(fallbackRspResult), StatusCode = (HttpStatusCode)fallbackRspStatus });
                                  },
                                  onFallbackAsync: (res) =>
                                  {
                                      var _logger = ServiceLocator.GetService<ILoggerFactory>()?.CreateLogger("Findx.AspNetCore.Extensions");
                                      _logger?.LogInformation($"{DateTime.Now}-服务开始降级,异常消息：{res?.Exception?.Message}");
                                      _logger?.LogInformation($"{DateTime.Now}-服务降级内容响应：{fallbackRspResult}");
                                      return Task.CompletedTask;
                                  });
            });

            return httpClientBuilder;
        }

        /// <summary>
        /// 添加降级策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="fallbackAction"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder, Func<CancellationToken, Task<HttpResponseMessage>> fallbackAction)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            Check.NotNull(fallbackAction, nameof(fallbackAction));

            httpClientBuilder.AddTransientHttpErrorPolicy(build =>
            {
                return build.Or<HttpRequestException>()
                            .Or<TimeoutException>()
                            .Or<TimeoutRejectedException>()
                            .Or<BrokenCircuitException>()
                            .OrResult(rsp => rsp.StatusCode == HttpStatusCode.InternalServerError || rsp.StatusCode == HttpStatusCode.RequestTimeout)
                            .FallbackAsync(
                                  fallbackAction: async (cancellationToken) =>
                                  {
                                      return await fallbackAction.Invoke(cancellationToken);
                                  },
                                  onFallbackAsync: async (res) =>
                                  {
                                      var _logger = ServiceLocator.GetService<ILoggerFactory>()?.CreateLogger("Findx.AspNetCore.Extensions");
                                      _logger?.LogInformation($"{DateTime.Now}-服务开始降级,异常消息：{res?.Exception?.Message}");
                                      _logger?.LogInformation($"{DateTime.Now}-服务降级内容响应：{await res?.Result?.Content?.ReadAsStringAsync()}");
                                  });
            });

            return httpClientBuilder;
        }
    }
}
