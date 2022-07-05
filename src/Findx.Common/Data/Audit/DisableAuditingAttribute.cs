using System;
namespace Findx.Data
{
    /// <summary>
    /// 标记在审计中忽略的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DisableAuditingAttribute : Attribute
    { }
}

