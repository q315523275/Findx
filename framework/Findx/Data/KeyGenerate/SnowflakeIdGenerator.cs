﻿using Findx.Utilities;

namespace Findx.Data;

/// <summary>
///     雪花算法主键生成器
/// </summary>
public class SnowflakeIdGenerator : IKeyGenerator<long>
{
    /// <summary>
    ///     获取一个<see cref="long" />类型的主键数据
    /// </summary>
    /// <returns></returns>
    public long Create()
    {
        return SnowflakeIdUtility.Default().NextId();
    }
}