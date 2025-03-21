﻿using System.ComponentModel.DataAnnotations;

namespace Findx.Module.EleAdmin.Dtos.Auth;

/// <summary>
///     登录参数
/// </summary>
public class LoginDto
{
    /// <summary>
    ///     账号
    /// </summary>
    [Required]
    public string UserName { set; get; }

    /// <summary>
    ///     密码
    /// </summary>
    [Required]
    public string Password { set; get; }

    /// <summary>
    ///     验证码
    /// </summary>
    [Required]
    public string Code { set; get; }

    /// <summary>
    ///     租户
    /// </summary>
    public string TenantId { set; get; }

    /// <summary>
    ///     uuid
    /// </summary>
    [Required]
    public string Uuid { set; get; }
}