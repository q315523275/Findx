﻿using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Linq;

namespace Findx.Module.EleAdminPlus.Dtos;

/// <summary>
///     查询组织入参
/// </summary>
public class QueryOrgRequest : PageBase
{
    /// <summary>
    ///     类型
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public string Type { set; get; }

    /// <summary>
    ///     关键词
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Name { set; get; }
}