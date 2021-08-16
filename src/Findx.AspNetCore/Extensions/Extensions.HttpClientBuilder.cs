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

            return httpClientBuilder.AddPolicyHandler(timeoutPolicy);
        }

        /// <summary>
        /// 添加重试策略
        /// </summary>
        /// <typeparam name="TException">触发异常类型</typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="retryCount">重试次数</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddRetryPolicy<TException>(this IHttpClientBuilder httpClientBuilder, int retryCount) where TException : Exception
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (retryCount < 1) return httpClientBuilder;

            var retryPolicy = Policy<HttpResponseMessage>.Handle<TException>().RetryAsync(retryCount);

            return httpClientBuilder.AddPolicyHandler(retryPolicy);
        }

        /// <summary>
        /// 添加熔断策略
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="exceptionsAllowedBeforeBreaking">熔断前连续错误次数</param>
        /// <param name="durationOfBreak">熔断时长,如:30s</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddCircuitBreakerPolicy<TException>(this IHttpClientBuilder httpClientBuilder, int exceptionsAllowedBeforeBreaking, string durationOfBreak) where TException : Exception
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            if (exceptionsAllowedBeforeBreaking < 1 || durationOfBreak.IsNullOrWhiteSpace()) return httpClientBuilder;

            var BreakTimeSpan = Findx.Utils.DateTimeUtils.ToTimeSpan(durationOfBreak);

            var circuitBreakerPolicy = Policy<HttpResponseMessage>.Handle<TException>()
                                                                  .CircuitBreakerAsync(
                                                                    handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                                                                    durationOfBreak: BreakTimeSpan,
                                                                    onBreak: (ex, breakDelay) =>
                                                                    {
                                                                        var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                        var _logger = _loggerFactory.CreateLogger("Microsoft.Extensions.Http.Polly");

                                                                        _logger.LogError(ex.Exception, $"断路器({DateTime.Now})即将熔断" + breakDelay.TotalSeconds + "秒");
                                                                    },
                                                                    onReset: () =>
                                                                    {
                                                                        var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                        var _logger = _loggerFactory.CreateLogger("Microsoft.Extensions.Http.Polly");

                                                                        _logger.LogDebug($"断路器({DateTime.Now})熔断恢复");
                                                                    },
                                                                    onHalfOpen: () =>
                                                                    {
                                                                        var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                        var _logger = _loggerFactory.CreateLogger("Microsoft.Extensions.Http.Polly");

                                                                        _logger.LogDebug(".Breaker logging: Half-open; next call is a trial.");
                                                                    });

            return httpClientBuilder.AddPolicyHandler(circuitBreakerPolicy);
        }

        /// <summary>
        /// 添加降级策略
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="httpClientBuilder"></param>
        /// <param name="httpResponseMessage"></param>
        /// <param name="httpResponseStatus"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddFallbackPolicy<TException>(this IHttpClientBuilder httpClientBuilder, string httpResponseMessage, int httpResponseStatus) where TException : Exception
        {
            Check.NotNull(httpClientBuilder, nameof(httpClientBuilder));
            Check.NotNull(httpResponseMessage, nameof(httpResponseMessage));
            if (httpResponseStatus < 1) return httpClientBuilder;

            HttpResponseMessage fallbackResponse = new() { Content = new StringContent(httpResponseMessage), StatusCode = (HttpStatusCode)httpResponseStatus };

            var fallbackPolicy = Policy<HttpResponseMessage>.HandleInner<TException>()
                                                            .FallbackAsync(fallbackResponse, b =>
                                                            {
                                                                var _loggerFactory = ServiceLocator.GetService<ILoggerFactory>();
                                                                var _logger = _loggerFactory.CreateLogger("Microsoft.Extensions.Http.Polly");

                                                                _logger.LogError(b.Exception, $"服务开始降级({DateTime.Now})");
                                                                return Task.CompletedTask;
                                                            });

            return httpClientBuilder.AddPolicyHandler(fallbackPolicy);
        }
    }
}
