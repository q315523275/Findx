using Microsoft.AspNetCore.Mvc.Filters;

namespace Findx.Module.EleAdmin.Shared.Mvc.Filters;

/// <summary>
///     Ip地址限定器
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IpAddressLimiterAttribute: ActionFilterAttribute;
