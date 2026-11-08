using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Findx.AspNetCore.Mvc.Middlewares;
using Findx.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;

namespace Findx.UnitTests;

/// <summary>
///     跟踪标识中间件回归测试
///     <para>覆盖 options 值快照改造后的行为：请求头缺失时生成、已存在时保留、响应开始时写回响应头、
///     SetResponseHeader=false 时不写响应头。</para>
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private const string HeaderName = "X-Correlation-Id";

    /// <summary>
    ///     可控的响应 Feature：记录 OnStarting 回调，测试里手动触发
    /// </summary>
    private sealed class TestResponseFeature : IHttpResponseFeature
    {
        private readonly List<(Func<object, Task> Callback, object State)> _onStarting = new();

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted { get; private set; }

        public void OnStarting(Func<object, Task> callback, object state) => _onStarting.Add((callback, state));

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public async Task FireOnStartingAsync()
        {
            HasStarted = true;
            foreach (var item in _onStarting)
            {
                if (item.Callback != null) await item.Callback(item.State);
            }
        }
    }

    private static (CorrelationIdMiddleware Middleware, TestResponseFeature ResponseFeature) BuildMiddleware(
        DefaultCorrelationIdProvider provider, CorrelationIdOptions options, Action<string> onNext = null)
    {
        var responseFeature = new TestResponseFeature();
        var middleware = new CorrelationIdMiddleware(provider, _ =>
        {
            onNext?.Invoke(provider.Get());
            return Task.CompletedTask;
        }, options);
        return (middleware, responseFeature);
    }

    [Fact]
    public async Task InvokeAsync_NoHeader_GeneratesIdAndWritesBackToRequest()
    {
        var provider = new DefaultCorrelationIdProvider();
        var options = new CorrelationIdOptions();
        string seenByNext = null;
        var (middleware, responseFeature) = BuildMiddleware(provider, options, id => seenByNext = id);

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);

        await middleware.InvokeAsync(context);

        var generated = context.Request.Headers[HeaderName].ToString();
        Assert.False(string.IsNullOrWhiteSpace(generated));
        Assert.Equal(generated, seenByNext);
    }

    [Fact]
    public async Task InvokeAsync_ExistingHeader_IsPreserved()
    {
        var provider = new DefaultCorrelationIdProvider();
        var options = new CorrelationIdOptions();
        string seenByNext = null;
        var (middleware, responseFeature) = BuildMiddleware(provider, options, id => seenByNext = id);

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Request.Headers[HeaderName] = "trace-abc";

        await middleware.InvokeAsync(context);

        Assert.Equal("trace-abc", context.Request.Headers[HeaderName].ToString());
        Assert.Equal("trace-abc", seenByNext);
    }

    [Fact]
    public async Task InvokeAsync_ResponseStarting_SetsResponseHeader()
    {
        var provider = new DefaultCorrelationIdProvider();
        var options = new CorrelationIdOptions();
        var (middleware, responseFeature) = BuildMiddleware(provider, options);

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Request.Headers[HeaderName] = "trace-response";

        await middleware.InvokeAsync(context);
        await responseFeature.FireOnStartingAsync();

        Assert.Equal("trace-response", context.Response.Headers[HeaderName].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetResponseHeaderDisabled_DoesNotWriteResponseHeader()
    {
        var provider = new DefaultCorrelationIdProvider();
        var options = new CorrelationIdOptions();
        options.SetResponseHeader = false;
        var (middleware, responseFeature) = BuildMiddleware(provider, options);

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Request.Headers[HeaderName] = "trace-no-response";

        await middleware.InvokeAsync(context);
        await responseFeature.FireOnStartingAsync();

        Assert.False(context.Response.Headers.ContainsKey(HeaderName));
    }

    [Fact]
    public async Task InvokeAsync_DisposesProviderScope_RestoresPreviousId()
    {
        // Change 的作用域在请求结束时释放，不应把跟踪标识泄漏到后续执行上下文
        var provider = new DefaultCorrelationIdProvider();
        var options = new CorrelationIdOptions();
        var (middleware, responseFeature) = BuildMiddleware(provider, options);

        var context = new DefaultHttpContext();
        context.Features.Set<IHttpResponseFeature>(responseFeature);
        context.Request.Headers[HeaderName] = "trace-scope";

        await middleware.InvokeAsync(context);

        Assert.Null(provider.Get());
    }
}
