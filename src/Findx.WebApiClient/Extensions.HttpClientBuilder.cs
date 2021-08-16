using Findx.DependencyInjection;
using Findx.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Findx.Extensions
{
    /// <summary>
    /// HttpClientBuilder弹性构建扩展
    /// </summary>
    internal static partial class Extensions
    {
        /// <summary>
        /// 添加超时策略
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="seconds">秒</param>
        /// <returns></returns>
        internal static IHttpClientBuilder AddTimeoutPolicy(this IHttpClientBuilder httpClientBuilder, int seconds)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (seconds < 1) return httpClientBuilder;

            return httpClientBuilder.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(seconds));
        }

        /// <summary>
        /// 添加重试策略
        /// </summary>
        /// <typeparam name="TException">触发异常类型</typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="retryCount">重试次数</param>
        /// <returns></returns>
        internal static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder httpClientBuilder, int retryCount)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (retryCount < 1) return httpClientBuilder;

            return httpClientBuilder.AddTransientHttpErrorPolicy(build => build.RetryAsync(retryCount));
        }

        /// <summary>
        /// 添加熔断策略
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="exceptionsAllowedBeforeBreaking">熔断前连续错误次数</param>
        /// <param name="durationOfBreak">熔断时长,如:30s</param>
        /// <returns></returns>
        internal static IHttpClientBuilder AddCircuitBreakerPolicy(this IHttpClientBuilder httpClientBuilder, int exceptionsAllowedBeforeBreaking, string durationOfBreak)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (exceptionsAllowedBeforeBreaking < 1 || durationOfBreak.IsNullOrWhiteSpace()) return httpClientBuilder;

            var BreakTimeSpan = Utils.DateTimeUtils.ToTimeSpan(durationOfBreak);

            return httpClientBuilder.AddTransientHttpErrorPolicy(build => build.CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                                                                    durationOfBreak: BreakTimeSpan,
                                                                    onBreak: (ex, breakDelay) =>
                                                                    {
                                                                        var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                        var _logger = _loggerFactory.CreateLogger("Findx.WebApiClient");

                                                                        _logger.LogError(ex.Exception, $"断路器({DateTime.Now})即将熔断" + breakDelay.TotalSeconds + "秒");
                                                                    },
                                                                    onReset: () =>
                                                                    {
                                                                        var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                        var _logger = _loggerFactory.CreateLogger("Findx.WebApiClient");

                                                                        _logger.LogDebug($"断路器({DateTime.Now})熔断恢复");
                                                                    }));
        }

        /// <summary>
        /// 添加降级策略
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="httpResponseMessage"></param>
        /// <param name="httpResponseStatus"></param>
        /// <returns></returns>
        internal static IHttpClientBuilder AddFallbackPolicy(this IHttpClientBuilder httpClientBuilder, string httpResponseMessage, int httpResponseStatus)
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            Check.NotNull(httpResponseMessage, nameof(httpResponseMessage));
            if (httpResponseStatus < 1) return httpClientBuilder;

            HttpResponseMessage fallbackResponse = new() { Content = new StringContent(httpResponseMessage), StatusCode = (HttpStatusCode)httpResponseStatus };

            return httpClientBuilder.AddTransientHttpErrorPolicy(build => build.FallbackAsync(fallbackResponse, b =>
            {
                var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                var _logger = _loggerFactory.CreateLogger("Findx.WebApiClient");

                _logger.LogError(b.Exception, $"服务开始降级({DateTime.Now})");

                return Task.CompletedTask;
            }));
        }
    }
}
