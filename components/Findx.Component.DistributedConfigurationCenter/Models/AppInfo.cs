using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Findx.Data;

namespace Findx.Component.DistributedConfigurationCenter.Models;

/// <summary>
///     应用信息表
/// </summary>
[Table("config_apps")]
[EntityExtension(DataSource = "config")]
public class AppInfo : FullAuditedBase<Guid, Guid>, ISoftDeletable, IResponse
{
    /// <summary>
    ///     主键id
    /// </summary>
    [Key]
    [Column("id")]
    public override Guid Id { get; set; }

    /// <summary>
    ///     应用名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     appid
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    ///     密钥
    /// </summary>
    public string Secret { get; set; }

    /// <summary>
    ///     删除时间
    /// </summary>
    public DateTime? DeletionTime { get; set; }

    /// <summary>
    ///     是否删除
    /// </summary>
    public bool IsDeleted { get; set; }
}