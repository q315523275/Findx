using System;
using System.Text;
using Findx.DependencyInjection;
using Findx.Extensions;
using Findx.Tracing;
using Microsoft.AspNetCore.Http;
using NLog;
using NLog.LayoutRenderers;

namespace Findx.NLog.LayoutRenderX;

/// <summary>
///     TraceIdentifier内容读取器
/// </summary>
[LayoutRenderer("TraceIdentifier")]
public class TraceIdentifierLayoutRenderer: LayoutRenderer
{
    // 注册/首次使用时一次性捕获，避免每条日志都做服务定位
    private static readonly Lazy<ICorrelationIdProvider> CorrelationIdProvider =
        new(ServiceLocator.GetService<ICorrelationIdProvider>);

    private static readonly Lazy<IHttpContextAccessor> HttpContextAccessor =
        new(ServiceLocator.GetService<IHttpContextAccessor>);

    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var correlationId = CorrelationIdProvider.Value?.Get();
        if (correlationId.IsNotNullOrWhiteSpace())
        {
            builder.Append(correlationId);
        }
        else if (HttpContextAccessor.Value?.HttpContext is not null)
        {
            builder.Append(HttpContextAccessor.Value.HttpContext.TraceIdentifier);
        }
    }
}