namespace Findx.Data;

/// <summary>
///     标记在审计中忽略的属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DisableAuditingAttribute : Attribute;