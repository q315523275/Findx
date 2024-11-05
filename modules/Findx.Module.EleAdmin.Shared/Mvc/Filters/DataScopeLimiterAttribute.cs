using Microsoft.AspNetCore.Mvc.Filters;

namespace Findx.Module.EleAdmin.Shared.Mvc.Filters;

/// <summary>
///     数据范围限定器
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DataScopeLimiterAttribute: ActionFilterAttribute;