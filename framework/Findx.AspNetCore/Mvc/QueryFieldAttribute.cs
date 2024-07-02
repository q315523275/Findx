using System;
using Findx.Linq;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     查询字段
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class QueryFieldAttribute: Attribute
{
    /// <summary>
    ///     字段名
    /// </summary>
    public string Name { set; get; }
    
    /// <summary>
    ///     过滤类型
    /// </summary>
    public FilterOperate FilterOperate { set; get; }
}