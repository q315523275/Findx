﻿namespace Findx.Data;

/// <summary>
///     定义实体更新用户字段
/// </summary>
public interface IUpdateAudited<TUser> : IUpdateTime where TUser : struct
{
    /// <summary>
    ///     更新用户
    /// </summary>
    TUser? LastUpdaterId { get; set; }
}