﻿using Findx.Data;

namespace Findx.Module.EleAdmin.Dtos.Role;

/// <summary>
///     查询角色入参
/// </summary>
public class RoleQueryDto : PageBase
{
    /// <summary>
    ///     名称
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     编号
    /// </summary>
    public string Code { set; get; }

    /// <summary>
    ///     备注
    /// </summary>
    public string Comments { set; get; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string ApplicationCode { get; set; }
}