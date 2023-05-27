namespace Findx.Data;

/// <summary>
///     标记在审计中忽略的属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DisableAuditingAttribute : Attribute
{
}