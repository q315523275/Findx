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
    protected override void Append(StringBuilder builder, LogEventInfo logEvent)
    {
        var correlationIdProvider = ServiceLocator.GetService<ICorrelationIdProvider>();
        var httpContextAccessor = ServiceLocator.GetService<IHttpContextAccessor>();
        if (correlationIdProvider != null && correlationIdProvider.Get().IsNotNullOrWhiteSpace())
        {
            builder.Append(correlationIdProvider.Get());
        }
        else if (httpContextAccessor is { HttpContext: not null })
        {
            builder.Append(httpContextAccessor.HttpContext.TraceIdentifier);
        }
    }
}