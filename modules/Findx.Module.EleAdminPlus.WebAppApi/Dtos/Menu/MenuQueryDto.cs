﻿using Findx.AspNetCore.Mvc;
using Findx.Data;
using Findx.Expressions;

namespace Findx.Module.EleAdminPlus.WebAppApi.Dtos.Menu;

/// <summary>
///     查询菜单入参
/// </summary>
public class MenuQueryDto : SortCondition
{
    /// <summary>
    ///     名称
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Title { set; get; }

    /// <summary>
    ///     路径
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Path { set; get; }

    /// <summary>
    ///     权限标识
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Contains)]
    public string Authority { set; get; }

    /// <summary>
    ///     父级id
    /// </summary>
    [QueryField(FilterOperate = FilterOperate.Equal)]
    public long? ParentId { set; get; }
}