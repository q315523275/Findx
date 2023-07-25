using System;
using Findx.Data;

namespace Findx.Component.DistributedConfigurationCenter.Dtos;

/// <summary>
///     配置简略信息Dto
/// </summary>
public class ConfigSimpleDto : IResponse
{
    /// <summary>
    ///     记录编号
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     应用编号
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     数据编号
    /// </summary>
    public string DataId { get; set; }

    /// <summary>
    ///     版本号
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    ///     最后更新时间
    /// </summary>
    public DateTime? LastUpdatedTime { get; set; }
}